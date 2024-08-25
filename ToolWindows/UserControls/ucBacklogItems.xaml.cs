using JeffPires.BacklogChatGPTAssistant.Models;
using JeffPires.BacklogChatGPTAssistant.Options;
using JeffPires.BacklogChatGPTAssistant.Utils;
using System;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using UserControl = System.Windows.Controls.UserControl;

namespace JeffPires.BacklogChatGPTAssistant.ToolWindows
{
    /// <summary>
    /// User control responsible to show the Backlog Items generated.
    /// </summary>
    public partial class ucBacklogItems : UserControl
    {
        #region Events

        public delegate void CanceledDelegate();
        public event CanceledDelegate Canceled;

        #endregion Events

        #region Properties

        private readonly GenerateResult generateResult;
        private readonly ObservableCollection<WorkItemResult> workItems = [];
        private bool savingWorkItems = false;
        private bool workItemControlIsInProcessing = false;
        private CancellationTokenSource workItemControlInProcessigCancellationToken;
        private int countWorkItemsSaved = 0;

        #endregion Properties

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ucBacklogItems"/> class.
        /// Sets the data source for the backlog items using the provided work items and options.
        /// </summary>
        /// <param name="workItems">The generated result containing work items.</param>
        /// <param name="options">The app options instance.</param>
        public ucBacklogItems(GenerateResult workItems, OptionPageGridGeneral options)
        {
            this.InitializeComponent();

            this.generateResult = workItems;

            SetDataSource(workItems, options);
        }

        #endregion Constructors

        #region Event Handlers

        /// <summary>
        /// Handles the click event for the "Expand All" button, expanding all items.
        /// </summary>
        private void btnExpandAll_Click(object sender, RoutedEventArgs e)
        {
            ExpandCollapseAllItems(workItems, true);
        }

        /// <summary>
        /// Handles the click event for the "Collapse All" button, collapsing all items.
        /// </summary>
        private void btnCollapseAll_Click(object sender, RoutedEventArgs e)
        {
            ExpandCollapseAllItems(workItems, false);
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            string message;

            if (savingWorkItems)
            {
                message = $"Do you confirm the cancellation?{Environment.NewLine + Environment.NewLine}If you cancel, the recording of Work Items in Azure Devops will stop.";

                if (MessageBox.Show(message, Constants.EXTENSION_NAME, MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    ResetPage();
                }
            }
            else if (workItemControlIsInProcessing)
            {
                message = "Do you confirm the cancellation?";

                if (MessageBox.Show(message, Constants.EXTENSION_NAME, MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    workItemControlInProcessigCancellationToken.Cancel();

                    ResetPage();
                }
            }
            else
            {
                message = $"Do you confirm the cancellation?{Environment.NewLine + Environment.NewLine}When canceling, the generated Backlog Items will be lost and you will return to the start window.";

                if (MessageBox.Show(message, Constants.EXTENSION_NAME, MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    Canceled?.Invoke();
                }
            }
        }

        /// <summary>
        /// Handles the click event for the save button, prompting the user for confirmation 
        /// before saving work items to Azure DevOps. It updates the UI to reflect the saving process 
        /// and initiates the asynchronous save operation.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The event data.</param>
        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Do you confirm the saving of Work Items in Azure Devops?", Constants.EXTENSION_NAME, MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No)
            {
                return;
            }

            workItemsList.IsEnabled = false;
            btnSave.IsEnabled = false;
            txtProgress.Text = "Saving the Work Items to Azure Devops...";
            grdProgress.IsIndeterminate = false;
            grdProgress.Minimum = 0;
            grdProgress.Maximum = CountWorkItems(workItems);
            grdProgress.Visibility = Visibility.Visible;
            txtProgress.Visibility = Visibility.Visible;
            savingWorkItems = true;

            SaveWorkItems();
        }

        #endregion Event Handlers

        #region Methods

        /// <summary>
        /// Starts the processing of work item items with the specified cancellation token and message.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token source to control the operation.</param>
        /// <param name="processMessage">The message to display during processing.</param>
        public void StartWorkItemProcessing(CancellationTokenSource cancellationToken, string processMessage)
        {
            workItemControlInProcessigCancellationToken = cancellationToken;
            workItemControlIsInProcessing = true;
            workItemsList.IsEnabled = false;
            btnSave.IsEnabled = false;
            txtProgress.Text = processMessage;
            grdProgress.IsIndeterminate = true;
            grdProgress.Visibility = Visibility.Visible;
            txtProgress.Visibility = Visibility.Visible;
        }

        /// <summary>
        /// Resets the page state.
        /// </summary>
        public void StopWorkItemProcessing()
        {
            ResetPage();
        }

        /// <summary>
        /// Deletes the specified work item from the collection of work items.
        /// </summary>
        /// <param name="workItem">The work item to be deleted.</param>
        public void DeleteWorkItem(WorkItemResult workItem)
        {
            workItems.Remove(workItem);
        }

        /// <summary>
        /// Sets the data source for the work items by converting the generated work items 
        /// from the provided GenerateResult and adding them to the work items collection.
        /// </summary>
        /// <param name="generateResult">The result containing generated work items.</param>
        /// <param name="options">App options instance.</param>
        private void SetDataSource(GenerateResult generateResult, OptionPageGridGeneral options)
        {
            workItems.Clear();

            foreach (WorkItem workItemResult in generateResult.GeneratedWorkItems)
            {
                workItems.Add(ConvertToResult(workItemResult, null, options));
            }

            workItemsList.ItemsSource = workItems;
        }

        /// <summary>
        /// Converts a given WorkItem into a WorkItemResult, including its children.
        /// </summary>
        /// <param name="workItem">The WorkItem to be converted.</param>
        /// <param name="options">The app options instance.</param>
        /// <returns>A WorkItemResult that represents the converted WorkItem.</returns>
        private WorkItemResult ConvertToResult(WorkItem workItem, WorkItemResult parentWorkitem, OptionPageGridGeneral options)
        {
            WorkItemResult result = new()
            {
                AcceptanceCriteria = workItem.AcceptanceCriteria,
                Description = workItem.Description,
                Id = workItem.Id,
                ParentId = workItem.ParentId,
                RemainingWork = workItem.RemainingWork,
                Title = workItem.Title,
                Type = workItem.Type,
                Children = [],
                OptionsInstance = options,
                BacklogItemsControlInstance = this,
                ParentWorkitem = parentWorkitem
            };

            if (workItem.Children != null && workItem.Children.Count > 0)
            {
                foreach (WorkItem workItemChild in workItem.Children)
                {
                    result.Children.Add(ConvertToResult(workItemChild, result, options));
                }
            }

            return result;
        }

        /// <summary>
        /// Converts a WorkItemResult object to a WorkItem object.
        /// </summary>
        /// <param name="workItemResult">The WorkItemResult object to convert.</param>
        /// <returns>A WorkItem object populated with data from the WorkItemResult.</returns>
        private WorkItem ConvertToWorkItem(WorkItemResult workItemResult)
        {
            WorkItem workItem = new()
            {
                AcceptanceCriteria = workItemResult.AcceptanceCriteria,
                Description = workItemResult.Description,
                RemainingWork = workItemResult.RemainingWork,
                Title = workItemResult.Title,
                Type = workItemResult.Type,
                Children = []
            };

            if (workItemResult.Children != null && workItemResult.Children.Count > 0)
            {
                foreach (WorkItemResult child in workItemResult.Children)
                {
                    workItem.Children.Add(ConvertToWorkItem(child));
                }
            }

            return workItem;
        }

        /// <summary>
        /// Expands or collapses all items in the provided list of work items recursively.
        /// </summary>
        /// <param name="workItems">The list of work items to expand or collapse.</param>
        /// <param name="isExpanded">True to expand all items; false to collapse them.</param>
        private void ExpandCollapseAllItems(ObservableCollection<WorkItemResult> workItems, bool isExpanded)
        {
            foreach (WorkItemResult workItem in workItems)
            {
                workItem.IsExpanded = isExpanded;

                ExpandCollapseAllItems(workItem.Children, isExpanded);
            }
        }

        /// <summary>
        /// Resets the state of the page by disabling controls, hiding progress indicators and setting processing flags to false.
        /// </summary>
        private void ResetPage()
        {
            savingWorkItems = false;
            workItemControlIsInProcessing = false;
            workItemsList.IsEnabled = true;
            btnSave.IsEnabled = true;
            grdProgress.Visibility = Visibility.Collapsed;
            txtProgress.Visibility = Visibility.Collapsed;
            countWorkItemsSaved = 0;
        }

        private async Task SaveWorkItems()
        {
            try
            {
                foreach (WorkItemResult workItemResult in workItems)
                {
                    if (!savingWorkItems)
                    {
                        return;
                    }

                    WorkItem workItem = ConvertToWorkItem(workItemResult);

                    if (generateResult.ExistentWorkItem != null)
                    {
                        workItem.ParentId = generateResult.ExistentWorkItem.Id;
                    }

                    await SaveWorkItem(workItem);
                }

                MessageBox.Show("Work Items successfully saved.", Constants.EXTENSION_NAME, MessageBoxButton.OK, MessageBoxImage.Information);

                Canceled?.Invoke();
            }
            catch (Exception ex)
            {
                Logger.Log(ex);

                ResetPage();

                MessageBox.Show(ex.Message, Constants.EXTENSION_NAME, MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }
        }

        /// <summary>
        /// Asynchronously saves a work item and its children to Azure DevOps.
        /// </summary>
        /// <param name="workItem">The work item to be saved, which may contain child work items.</param>
        private async Task SaveWorkItem(WorkItem workItem)
        {
            int id = await AzureDevops.SaveWorkItemAsync(generateResult.SelectedProject, workItem, generateResult.SelectedIterationPath);

            countWorkItemsSaved++;
            grdProgress.Value = countWorkItemsSaved;

            if (workItem.Children != null && workItem.Children.Count > 0)
            {
                foreach (WorkItem child in workItem.Children)
                {
                    if (!savingWorkItems)
                    {
                        return;
                    }

                    child.ParentId = id;

                    await SaveWorkItem(child);
                }
            }
        }

        /// <summary>
        /// Counts the total number of work items in a collection, including nested child work items.
        /// </summary>
        /// <param name="workItems">The collection of work items to count.</param>
        /// <returns>The total count of work items, including children.</returns>
        private int CountWorkItems(ObservableCollection<WorkItemResult> workItems)
        {
            int count = 0;

            foreach (WorkItemResult item in workItems)
            {
                count++;

                if (item.Children != null && item.Children.Count > 0)
                {
                    count += CountWorkItems(item.Children);
                }
            }

            return count;
        }

        #endregion Methods   
    }
}