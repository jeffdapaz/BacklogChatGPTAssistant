using JeffPires.BacklogChatGPTAssistant.Options;
using JeffPires.BacklogChatGPTAssistant.Utils;
using JeffPires.BacklogChatGPTAssistant.Models;
using Microsoft.TeamFoundation.Core.WebApi;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;
using Microsoft.VisualStudio.Services.Client;
using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.WebApi;
using Microsoft.VisualStudio.Services.WebApi.Patch.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Operation = Microsoft.VisualStudio.Services.WebApi.Patch.Operation;
using WorkItemType = JeffPires.BacklogChatGPTAssistant.Models.WorkItemBase.WorkItemType;

namespace JeffPires.BacklogChatGPTAssistant.Utils
{
    /// <summary>
    /// Static class for interacting with Azure DevOps services and APIs.
    /// </summary>
    static class AzureDevops
    {
        #region Attributes

        private static OptionPageGridGeneral options;
        private static VssConnection vssConnection;

        #endregion Attributes

        #region Public Methods

        /// <summary>
        /// Logs in to Azure DevOps using the provided options.
        /// </summary>
        /// <param name="optionPageGridGeneral">The general options for the connection, including credentials and URL.</param>
        public static void Login(OptionPageGridGeneral optionPageGridGeneral)
        {
            options = optionPageGridGeneral;

            if (!string.IsNullOrWhiteSpace(options.AzureDevopsPAT))
            {
                VssBasicCredential credentials = new(string.Empty, options.AzureDevopsPAT);

                vssConnection = new VssConnection(new Uri(options.AzureDevopsUrl), credentials);
            }
            else
            {
                VssClientCredentials credentials = new(new WindowsCredential(false), new VssFederatedCredential(false), CredentialPromptType.PromptIfNeeded);

                vssConnection = new VssConnection(new Uri(options.AzureDevopsUrl), credentials);
            }

            vssConnection.ConnectAsync().SyncResult();
        }

        /// <summary>
        /// Retrieves a list of team projects.
        /// </summary>
        /// <returns>
        /// Contains an IEnumerable of TeamProjectReference objects.
        /// </returns>
        public static List<Project> ListProjects()
        {
            ProjectHttpClient projectClient = vssConnection.GetClient<ProjectHttpClient>();

            IEnumerable<TeamProjectReference> projects = projectClient.GetProjects().Result;

            List<Project> result = [];

            foreach (TeamProjectReference projectReference in projects)
            {
                Project project = new()
                {
                    Id = projectReference.Id,
                    Name = projectReference.Name
                };

                result.Add(project);
            }

            return result.OrderBy(r => r.Name).ToList();
        }

        /// <summary>
        /// Retrieves and returns a list of iteration paths for a specified project.
        /// </summary>
        /// <param name="projectName">The name of the project for which to retrieve iteration paths.</param>
        /// <returns>
        /// Contains a list of iteration paths sorted in ascending order.
        /// </returns>
        public static async Task<List<string>> ListInterationPathsAsync(string projectName)
        {
            WorkItemTrackingHttpClient workItemTrackingClient = vssConnection.GetClient<WorkItemTrackingHttpClient>();

            //Obtain all the areas and iterations of the project
            WorkItemClassificationNode classificationNodes = await workItemTrackingClient.GetClassificationNodeAsync(projectName, TreeStructureGroup.Iterations, depth: int.MaxValue);

            List<string> iterationPaths = [];

            GetIterationPaths(classificationNodes, string.Empty, iterationPaths);

            return iterationPaths.OrderBy(x => x).ToList();
        }

        /// <summary>
        /// Asynchronously retrieves a list of work items from a specified project and iteration path, filtered by work item type.
        /// </summary>
        /// <param name="projectName">The name of the project from which to retrieve work items.</param>
        /// <param name="iterationPath">The iteration path to filter the work items.</param>
        /// <param name="workItemType">The type of work items to retrieve.</param>
        /// <returns>
        /// Result contains a list of work items matching the specified criteria.
        /// </returns>
        public static async Task<List<WorkItemBase>> ListWorkItemsAsync(string projectName, string iterationPath, WorkItemType workItemType)
        {
            List<WorkItemBase> result = [];

            string wiqlQuery = $@"
               SELECT [System.Id], [System.Title], [System.State]
               FROM workitems
               WHERE [System.TeamProject] = '{projectName}'
               AND [System.IterationPath] = '{iterationPath}'
               AND [System.WorkItemType] = '{workItemType.GetStringValue()}'
               ORDER BY [System.Id]";

            WorkItemTrackingHttpClient workItemTrackingClient = vssConnection.GetClient<WorkItemTrackingHttpClient>();

            Wiql wiql = new() { Query = wiqlQuery };

            WorkItemQueryResult queryResult = await workItemTrackingClient.QueryByWiqlAsync(wiql);

            if (!queryResult.WorkItems.Any())
            {
                return result;
            }

            int[] ids = queryResult.WorkItems.Select(wi => wi.Id).ToArray();

            List<WorkItem> workItems = await workItemTrackingClient.GetWorkItemsAsync(ids);

            foreach (WorkItem workItem in workItems)
            {
                WorkItemBase workItemResult = new() { Id = workItem.Id.Value, Type = workItemType };

                if (workItem.Fields.TryGetValue("System.Title", out string title))
                {
                    workItemResult.Title = title;
                }

                if (workItem.Fields.TryGetValue("System.Description", out string description))
                {
                    workItemResult.Description = description;
                }

                if (workItem.Fields.TryGetValue("Microsoft.VSTS.Common.AcceptanceCriteria", out string acceptanceCriteria))
                {
                    workItemResult.AcceptanceCriteria = acceptanceCriteria;
                }

                result.Add(workItemResult);
            }

            return result;
        }

        /// <summary>
        /// Creates a new work item in the specified project with the provided details.
        /// </summary>
        /// <param name="project">The project in which the work item will be created.</param>
        /// <param name="workItem">The work item details to be created.</param>
        /// <returns>
        /// The ID of the newly created work item.
        /// </returns>
        public static async Task<int> CreateWorkItemAsync(Project project, WorkItemBase workItem)
        {
            JsonPatchDocument patchDocument = new();

            patchDocument.Add(
                new JsonPatchOperation()
                {
                    Operation = Operation.Add,
                    Path = "/fields/System.IterationPath",
                    Value = workItem.IterationPath
                }
             );

            patchDocument.Add(
                new JsonPatchOperation()
                {
                    Operation = Operation.Add,
                    Path = "/fields/System.Title",
                    Value = workItem.Title
                }
            );

            patchDocument.Add(
                new JsonPatchOperation()
                {
                    Operation = Operation.Add,
                    Path = "/fields/System.Description",
                    Value = workItem.Description
                }
            );

            if (workItem.Type == WorkItemType.Task)
            {
                patchDocument.Add(
                    new JsonPatchOperation()
                    {
                        Operation = Operation.Add,
                        Path = "/fields/System.RemainingWork",
                        Value = workItem.RemainingWork
                    }
                );
            }
            else
            {
                patchDocument.Add(
                    new JsonPatchOperation()
                    {
                        Operation = Operation.Add,
                        Path = "/fields/System.AcceptanceCriteria",
                        Value = workItem.AcceptanceCriteria
                    }
                );
            }

            if (workItem.ParentId.HasValue)
            {
                patchDocument.Add(
                    new JsonPatchOperation()
                    {
                        Operation = Operation.Add,
                        Path = "/relations/-",
                        Value = new
                        {
                            rel = "System.LinkTypes.Hierarchy-Reverse",
                            url = $"{options.AzureDevopsUrl}/{project.Name}/_workItems/{workItem.ParentId}"
                        }
                    }
                );
            }

            WorkItemTrackingHttpClient workItemTrackingClient = vssConnection.GetClient<WorkItemTrackingHttpClient>();

            WorkItem newWorkItem = await workItemTrackingClient.CreateWorkItemAsync(patchDocument, project.Id, workItem.Type.GetStringValue());

            return newWorkItem.Id.Value;
        }

        #endregion Public Methods

        #region Private Methods

        /// <summary>
        /// Recursively retrieves all iteration paths from a given WorkItemClassificationNode and adds them to a list.
        /// </summary>
        /// <param name="node">The current WorkItemClassificationNode being processed.</param>
        /// <param name="path">The accumulated path string up to the current node.</param>
        /// <param name="iterationPaths">The list to which the iteration paths are added.</param>
        private static void GetIterationPaths(WorkItemClassificationNode node, string path, List<string> iterationPaths)
        {
            string currentPath = string.IsNullOrWhiteSpace(path) ? node.Name : $"{path}\\{node.Name}";

            if (node.Children != null && node.Children.Any())
            {
                foreach (WorkItemClassificationNode child in node.Children)
                {
                    GetIterationPaths(child, currentPath, iterationPaths);
                }
            }
            else
            {
                iterationPaths.Add(currentPath);
            }
        }

        #endregion Private Methods
    }
}