using JeffPires.BacklogChatGPTAssistant.Models;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Input;
using UserControl = System.Windows.Controls.UserControl;

namespace JeffPires.BacklogChatGPTAssistant.ToolWindows
{
    /// <summary>
    /// User control responsible to generate the Backlog Items
    /// </summary>
    public partial class ucBacklogItems : UserControl
    {
        #region Events

        /// <summary>
        /// Represents a delegate that is called when work items are generated.
        /// </summary>
        /// <param name="result">The result of the generated work items.</param>
        public delegate void WorkItemsSavedDelegate();
        public event WorkItemsSavedDelegate WorkItemsSaved;

        #endregion Events

        #region Properties

        private readonly List<WorkItemResult> workItems = [];

        #endregion Properties

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ucGenerate"/> class.
        /// Sets up the UI components and event handlers for project and iteration selection.
        /// </summary>
        /// <param name="options">The options for the general settings of the command user control.</param>
        public ucBacklogItems(GenerateResult workItems)
        {
            this.InitializeComponent();

            SetTreeViewDataSource(workItems);
        }

        #endregion Constructors

        #region Event Handlers

        /// <summary>
        /// Handles the SizeChanged event for the UserControl, adjusting the size of controls based on the new width.
        /// </summary>
        private void UserControl_SizeChanged(object sender, System.Windows.SizeChangedEventArgs e)
        {
            SetControlsSize(workItems, e.NewSize.Width);
        }

        /// <summary>
        /// Handles the mouse wheel preview event for the work items tree view,
        /// allowing for horizontal or vertical scrolling based on the shift key state.
        /// </summary>
        private void trvWorkItems_PreviewMouseWheel(object sender, System.Windows.Input.MouseWheelEventArgs e)
        {
            if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
            {
                scrollViewer.ScrollToHorizontalOffset(scrollViewer.HorizontalOffset - e.Delta);
            }
            else
            {
                scrollViewer.ScrollToVerticalOffset(scrollViewer.VerticalOffset - e.Delta);
            }

            e.Handled = true;
        }

        /// <summary>
        /// Handles the click event for the "Expand All" button, expanding all items in the work items tree view.
        /// </summary>
        private void btnExpandAll_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            foreach (object item in trvWorkItems.Items)
            {
                TreeViewItem treeViewItem = trvWorkItems.ItemContainerGenerator.ContainerFromItem(item) as TreeViewItem;

                ExpandCollapseAllItems(treeViewItem, true);
            }
        }

        /// <summary>
        /// Handles the click event for the "Collapse All" button, collapsing all items in the work items tree view.
        /// </summary>
        private void btnCollapseAll_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            foreach (object item in trvWorkItems.Items)
            {
                TreeViewItem treeViewItem = trvWorkItems.ItemContainerGenerator.ContainerFromItem(item) as TreeViewItem;

                ExpandCollapseAllItems(treeViewItem, false);
            }
        }

        #endregion Event Handlers

        #region Methods

        /// <summary>
        /// Sets the data source for the tree view by converting generated work items 
        /// from the provided GenerateResult and adding them to the work items collection.
        /// </summary>
        /// <param name="generateResult">The result containing generated work items to be displayed.</param>
        private void SetTreeViewDataSource(GenerateResult generateResult)
        {
            foreach (WorkItem workItemResult in generateResult.GeneratedWorkItems)
            {
                workItems.Add(ConvertResult(workItemResult));
            }

            trvWorkItems.ItemsSource = workItems;
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
        /// Expands or collapses all items in a given TreeViewItem recursively.
        /// </summary>
        /// <param name="treeViewItem">The TreeViewItem to expand or collapse.</param>
        /// <param name="isExpanded">True to expand all items; false to collapse them.</param>
        private void ExpandCollapseAllItems(TreeViewItem treeViewItem, bool isExpanded)
        {
            if (treeViewItem == null)
            {
                return;
            }

            treeViewItem.IsExpanded = isExpanded;

            for (int i = 0; i < treeViewItem.Items.Count; i++)
            {
                TreeViewItem childItem = treeViewItem.ItemContainerGenerator.ContainerFromIndex(i) as TreeViewItem;

                ExpandCollapseAllItems(childItem, isExpanded);
            }
        }

        private void SetControlsSize(List<WorkItemResult> workItems, double size)
        {
            foreach (WorkItemResult workItem in workItems)
            {
                workItem.TextWidth = workItem.Type switch
                {
                    WorkItem.WorkItemType.Epic => size - 85,
                    WorkItem.WorkItemType.Feature => size - 104,
                    WorkItem.WorkItemType.ProductBacklogItem => size - 123,
                    _ => size - 142
                };

                SetControlsSize(workItem.Children, size);
            }
        }

        #endregion Methods          
    }
}