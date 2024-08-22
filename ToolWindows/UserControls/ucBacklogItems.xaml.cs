using JeffPires.BacklogChatGPTAssistant.Models;
using System.Collections.Generic;
using UserControl = System.Windows.Controls.UserControl;

namespace JeffPires.BacklogChatGPTAssistant.ToolWindows
{
    /// <summary>
    /// User control responsible to show the Backlog Items generated.
    /// </summary>
    public partial class ucBacklogItems : UserControl
    {
        #region Events

        /// <summary>
        /// Represents a delegate that is called when work items are saved on Azure Devops.
        /// </summary>
        public delegate void WorkItemsSavedDelegate();
        public event WorkItemsSavedDelegate WorkItemsSaved;

        #endregion Events

        #region Properties

        private readonly List<WorkItemResult> workItems = [];

        #endregion Properties

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ucBacklogItems"/> class.
        /// Sets up the UI components and data source for the backlog items.
        /// </summary>
        /// <param name="workItems">The generated result containing work items to be displayed.</param>
        public ucBacklogItems(GenerateResult workItems)
        {
            this.InitializeComponent();

            SetDataSource(workItems);
        }

        #endregion Constructors

        #region Event Handlers

        /// <summary>
        /// Handles the click event for the "Expand All" button, expanding all items.
        /// </summary>
        private void btnExpandAll_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            ExpandCollapseAllItems(workItems, true);
        }

        /// <summary>
        /// Handles the click event for the "Collapse All" button, collapsing all items.
        /// </summary>
        private void btnCollapseAll_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            ExpandCollapseAllItems(workItems, false);
        }

        #endregion Event Handlers

        #region Methods

        /// <summary>
        /// Sets the data source by converting generated work items 
        /// from the provided GenerateResult and adding them to the work items collection.
        /// </summary>
        /// <param name="generateResult">The result containing generated work items to be displayed.</param>
        private void SetDataSource(GenerateResult generateResult)
        {
            foreach (WorkItem workItemResult in generateResult.GeneratedWorkItems)
            {
                workItems.Add(ConvertResult(workItemResult));
            }

            workItemsList.ItemsSource = workItems;
        }

        /// <summary>
        /// Converts a given WorkItem into a WorkItemResult, including its properties and recursively processing its children.
        /// </summary>
        /// <param name="workItem">The WorkItem to be converted.</param>
        /// <returns>A WorkItemResult containing the converted properties of the WorkItem.</returns>
        private WorkItemResult ConvertResult(WorkItem workItem)
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
                Children = []
            };

            foreach (WorkItem workItemChild in workItem.Children)
            {
                result.Children.Add(ConvertResult(workItemChild));
            }

            return result;
        }

        /// <summary>
        /// Expands or collapses all items in the provided list of work items recursively.
        /// </summary>
        /// <param name="workItems">The list of work items to expand or collapse.</param>
        /// <param name="isExpanded">True to expand all items; false to collapse them.</param>
        private void ExpandCollapseAllItems(List<WorkItemResult> workItems, bool isExpanded)
        {
            foreach (WorkItemResult workItem in workItems)
            {
                workItem.IsExpanded = isExpanded;

                ExpandCollapseAllItems(workItem.Children, isExpanded);
            }
        }

        #endregion Methods          
    }
}