using JeffPires.BacklogChatGPTAssistant.Models;
using JeffPires.BacklogChatGPTAssistant.Options;
using JeffPires.BacklogChatGPTAssistant.Utils;
using Newtonsoft.Json.Linq;
using NJsonSchema;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using UserControl = System.Windows.Controls.UserControl;

namespace JeffPires.BacklogChatGPTAssistant.ToolWindows
{
    /// <summary>
    /// User control containing the template to show the Backlog Items
    /// </summary>
    public partial class ucWorkItem : UserControl
    {
        private CancellationTokenSource cancellationTokenSource;

        #region Constructors

        /// <summary>
        /// Default constructor.
        /// </summary>
        public ucWorkItem()
        {
            this.InitializeComponent();
        }

        #endregion Constructors

        #region Event Handlers

        /// <summary>
        /// Handles the PreviewTextInput event for the txtEstimateProjectHours control, restricting input to numeric characters only.
        /// </summary>
        private void txtRemainingWork_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            Regex regex = new("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        /// <summary>
        /// Handles the mouse down event for the delete button, prompting the user for confirmation 
        /// before invoking the WorkItemDeleted event to remove the specified work item and its children.
        /// </summary>
        private void btnDelete_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Image button = sender as Image;

            WorkItemResult workItem = button.DataContext as WorkItemResult;

            string message = $"Confirm remove the following Work Item and all its children:{Environment.NewLine + Environment.NewLine}\"{workItem.Title}\" ";

            if (MessageBox.Show(message, Constants.EXTENSION_NAME, MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                if (workItem.ParentWorkitem != null)
                {
                    workItem.ParentWorkitem.Children.Remove(workItem);
                }
                else
                {
                    workItem.BacklogItemsControlInstance.DeleteWorkItem(workItem);
                }
            }
        }

        /// <summary>
        /// Handles the click event for the improve button. Validates input, serializes the work item, 
        /// generates a schema, and retrieves a response from ChatGPT to update the work item details.
        /// </summary>
        private async void btnImprove_Click(object sender, RoutedEventArgs e)
        {
            WorkItemResult workItem = null;

            try
            {
                if (string.IsNullOrWhiteSpace(txtImprove.Text))
                {
                    MessageBox.Show("Please, write the suggestions to improve.", Constants.EXTENSION_NAME, MessageBoxButton.OK, MessageBoxImage.Exclamation);

                    return;
                }

                Image button = sender as Image;

                workItem = button.DataContext as WorkItemResult;

                cancellationTokenSource = new CancellationTokenSource();

                workItem.BacklogItemsControlInstance.StartWorkItemProcessing(cancellationTokenSource, "Getting the improve for the Work Item...");

                string workItemAsJson = JsonSerializer.Serialize(ConvertToWorkItem(workItem));

                List<string> systemMessages = [];

                if (!string.IsNullOrWhiteSpace(workItem.OptionsInstance.InstructionImprove))
                {
                    systemMessages.Add($"{workItem.OptionsInstance.InstructionImprove} {Environment.NewLine + workItemAsJson}");
                }

                if (!string.IsNullOrWhiteSpace(workItem.OptionsInstance.InstructionWorkItemType))
                {
                    systemMessages.Add($"{workItem.OptionsInstance.InstructionWorkItemType} {workItem.Type.GetStringValue()}");
                }

                systemMessages.Add(string.Format(Constants.COMMAND_JSON_SCHEMA, Environment.NewLine + workItemAsJson));

                JsonSchema schema = JsonSchema.FromType<WorkItem>();

                string response = await ChatGPT.GetResponseAsync(workItem.OptionsInstance, systemMessages, txtImprove.Text, null, schema, nameof(WorkItem), cancellationTokenSource.Token);

                response = TextFormat.RemoveLanguageIdentifier(response);

                WorkItem workItemGenerate = JsonSerializer.Deserialize<WorkItem>(response);

                workItem.Title = workItemGenerate.Title;
                workItem.Description = workItemGenerate.Description;
                workItem.AcceptanceCriteria = workItemGenerate.AcceptanceCriteria;
                workItem.RemainingWork = workItemGenerate.RemainingWork;
            }
            catch (OperationCanceledException ex)
            {
                Logger.Log(ex);
            }
            catch (Exception ex)
            {
                Logger.Log(ex);

                MessageBox.Show($"Please try again:{Environment.NewLine + Environment.NewLine + ex.Message}", Constants.EXTENSION_NAME, MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            finally
            {
                workItem?.BacklogItemsControlInstance.StopWorkItemProcessing();
            }
        }

        /// <summary>
        /// Handles the click event for the Add Children button. 
        /// It retrieves the associated work item, serializes it to JSON, 
        /// and sends a request to generate child work items based on the provided instructions. 
        /// The response is processed to add the generated children to the original work item. 
        /// </summary>
        private async void btnAddChildren_Click(object sender, RoutedEventArgs e)
        {
            WorkItemResult workItem = null;

            try
            {
                if (string.IsNullOrWhiteSpace(txtAddChildren.Text))
                {
                    MessageBox.Show("Please, write instructions to create the children.", Constants.EXTENSION_NAME, MessageBoxButton.OK, MessageBoxImage.Exclamation);

                    return;
                }

                Image button = sender as Image;

                workItem = button.DataContext as WorkItemResult;

                cancellationTokenSource = new CancellationTokenSource();

                workItem.BacklogItemsControlInstance.StartWorkItemProcessing(cancellationTokenSource, "Creating children for the Work Item...");

                string workItemAsJson = JsonSerializer.Serialize(ConvertToWorkItem(workItem));

                List<string> systemMessages = [];

                if (!string.IsNullOrWhiteSpace(workItem.OptionsInstance.InstructionDefault))
                {
                    systemMessages.Add(workItem.OptionsInstance.InstructionDefault);
                }

                WorkItemType workItemChildrenType = GetWorkItemChildrenType(workItem.Type);

                if (!string.IsNullOrWhiteSpace(workItem.OptionsInstance.InstructionWorkItemType))
                {
                    systemMessages.Add($"{workItem.OptionsInstance.InstructionWorkItemType} {workItemChildrenType.GetStringValue()}");
                }

                if (!string.IsNullOrWhiteSpace(workItem.OptionsInstance.InstructionChildren) && workItem.Type != WorkItemType.ProductBacklogItem)
                {
                    systemMessages.Add(workItem.OptionsInstance.InstructionChildren);
                }

                systemMessages.Add($"{workItem.OptionsInstance.InstructionParentWork} Parent item:{Environment.NewLine + workItemAsJson}");

                systemMessages.Add(string.Format(Constants.COMMAND_JSON_SCHEMA, Environment.NewLine + workItemAsJson));

                JsonSchema schema = JsonSchema.FromType<List<WorkItem>>();

                string response = await ChatGPT.GetResponseAsync(workItem.OptionsInstance, systemMessages, txtAddChildren.Text, null, schema, nameof(WorkItem), cancellationTokenSource.Token);

                response = TextFormat.RemoveLanguageIdentifier(response);

                JToken token = JToken.Parse(response);

                if (token.Type == JTokenType.Object)
                {
                    WorkItem workItemGenerated = JsonSerializer.Deserialize<WorkItem>(response);

                    if (workItem.Id == workItemGenerated.Id)
                    {
                        foreach (WorkItem child in workItemGenerated.Children)
                        {
                            workItem.Children.Add(ConvertWorkItemToResult(child, workItemChildrenType, workItem.OptionsInstance, workItem));
                        }
                    }
                    else
                    {
                        workItem.Children.Add(ConvertWorkItemToResult(workItemGenerated, workItemChildrenType, workItem.OptionsInstance, workItem));
                    }
                }
                else
                {
                    List<WorkItem> workItemsGenerated = JsonSerializer.Deserialize<List<WorkItem>>(response);

                    foreach (WorkItem workItemGenerated in workItemsGenerated)
                    {
                        workItem.Children.Add(ConvertWorkItemToResult(workItemGenerated, workItemChildrenType, workItem.OptionsInstance, workItem));
                    }
                }
            }
            catch (OperationCanceledException ex)
            {
                Logger.Log(ex);
            }
            catch (Exception ex)
            {
                Logger.Log(ex);

                MessageBox.Show($"Please try again:{Environment.NewLine + Environment.NewLine + ex.Message}", Constants.EXTENSION_NAME, MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            finally
            {
                workItem?.BacklogItemsControlInstance.StopWorkItemProcessing();
            }
        }

        #endregion Event Handlers

        #region Methods        

        /// <summary>
        /// Converts a WorkItemResult object to a WorkItem object.
        /// </summary>
        /// <param name="workItemResult">The WorkItemResult object to convert.</param>
        /// <returns>A WorkItem object populated with data from the WorkItemResult.</returns>
        private WorkItem ConvertToWorkItem(WorkItemResult workItemResult)
        {
            return new()
            {
                AcceptanceCriteria = workItemResult.AcceptanceCriteria,
                Description = workItemResult.Description,
                Id = workItemResult.Id,
                ParentId = workItemResult.ParentId,
                RemainingWork = workItemResult.RemainingWork,
                Title = workItemResult.Title,
                Type = workItemResult.Type
            };
        }

        /// <summary>
        /// Converts a given WorkItem into a WorkItemResult, including its children.
        /// </summary>
        /// <param name="workItem">The WorkItem to convert.</param>
        /// <param name="workItemResultType">The type of the WorkItemResult.</param>
        /// <param name="options">The app options instance.</param>
        /// <param name="parentWorkItem">The parent Work Item.</param>
        /// <returns>A WorkItemResult representing the converted WorkItem.</returns>
        private WorkItemResult ConvertWorkItemToResult(WorkItem workItem, WorkItemType workItemResultType, OptionPageGridGeneral options, WorkItemResult parentWorkItem)
        {
            WorkItemResult result = new()
            {
                AcceptanceCriteria = workItem.AcceptanceCriteria,
                Description = workItem.Description,
                Id = workItem.Id,
                ParentId = workItem.ParentId,
                RemainingWork = workItem.RemainingWork,
                Title = workItem.Title,
                Type = workItemResultType,
                Children = [],
                OptionsInstance = options,
                ParentWorkitem = parentWorkItem
            };

            if (workItem.Children != null && workItem.Children.Count > 0)
            {
                foreach (WorkItem workItemChild in workItem.Children)
                {
                    result.Children.Add(ConvertWorkItemToResult(workItemChild, workItemResultType, options, result));
                }
            }

            return result;
        }

        /// <summary>
        /// Retrieves the child work item type for a given parent work item type.
        /// </summary>
        /// <param name="workItemParentType">The parent work item type for which to find the child type.</param>
        /// <returns>
        /// The corresponding child work item type based on the provided parent type.
        /// </returns>
        private WorkItemType GetWorkItemChildrenType(WorkItemType workItemParentType)
        {
            return workItemParentType switch
            {
                WorkItemType.Epic => WorkItemType.Feature,
                WorkItemType.Feature => WorkItemType.ProductBacklogItem,
                WorkItemType.ProductBacklogItem => WorkItemType.Task,
                _ => WorkItemType.Task
            };
        }

        #endregion Methods            
    }
}