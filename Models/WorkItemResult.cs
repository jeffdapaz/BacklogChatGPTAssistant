using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using static JeffPires.BacklogChatGPTAssistant.Models.WorkItem;

namespace JeffPires.BacklogChatGPTAssistant.Models
{
    /// <summary>
    /// Represents the result of a work item, implementing the INotifyPropertyChanged interface 
    /// to support property change notifications for data binding scenarios.
    /// </summary>
    public class WorkItemResult : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private double? id;
        private double? parentId;
        private WorkItemType type;
        private string title;
        private string description;
        private string acceptanceCriteria;
        private double? remainingWork;
        private bool isExpanded;

        /// <summary>
        /// Gets or sets the unique identifier.
        /// </summary>
        public double? Id
        {
            get => id;
            set => SetProperty(ref id, value);
        }        

        /// <summary>
        /// Gets or sets the ID of the parent work item. This property is nullable.
        /// </summary>
        public double? ParentId
        {
            get => parentId;
            set => SetProperty(ref parentId, value);
        }
        
        /// <summary>
        /// Gets or sets the type of the work item.
        /// </summary>
        public WorkItemType Type
        {
            get => type;
            set => SetProperty(ref type, value);
        }
        
        /// <summary>
        /// Gets or sets the title.
        /// </summary>
        public string Title
        {
            get => title;
            set => SetProperty(ref title, value);
        }
        
        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        public string Description
        {
            get => description;
            set => SetProperty(ref description, value);
        }
        
        /// <summary>
        /// Gets or sets the acceptance criteria.
        /// </summary>
        public string AcceptanceCriteria
        {
            get => acceptanceCriteria;
            set => SetProperty(ref acceptanceCriteria, value);
        }
        
        /// <summary>
        /// Gets or sets the remaining work.
        /// </summary>
        public double? RemainingWork
        {
            get => remainingWork;
            set => SetProperty(ref remainingWork, value);
        }

        /// <summary>
        /// Gets or sets the collection of child work items associated with the current work item.
        /// </summary>
        public List<WorkItemResult> Children { get; set; } = new();

        /// <summary>
        /// Gets the source of the icon associated with the work item.
        /// </summary>
        public string IconSource => Type switch
        {
            WorkItemType.Epic => "pack://application:,,,/BacklogChatGPTAssistant;component/Resources/epic.png",
            WorkItemType.Feature => "pack://application:,,,/BacklogChatGPTAssistant;component/Resources/feature.png",
            WorkItemType.ProductBacklogItem => "pack://application:,,,/BacklogChatGPTAssistant;component/Resources/backlog.png",
            _ => "pack://application:,,,/BacklogChatGPTAssistant;component/Resources/task.png"
        };

        /// <summary>
        /// Gets the visibility of the acceptance criteria based on the work item type.
        /// If the type is Task, the visibility is set to Collapsed; otherwise, it is set to Visible.
        /// </summary>
        public Visibility AcceptanceCriteriaVisibility => Type switch
        {
            WorkItemType.Task => Visibility.Collapsed,
            _ => Visibility.Visible
        };

        /// <summary>
        /// Gets the visibility of the remaining work based on the type of work item.
        /// If the work item type is Task, it returns Visible; otherwise, it returns Collapsed.
        /// </summary>
        public Visibility RemainingWorkVisibility => Type switch
        {
            WorkItemType.Task => Visibility.Visible,
            _ => Visibility.Collapsed
        };        

        /// <summary>
        /// Gets or sets a value indicating whether the item is expanded.
        /// </summary>
        /// <remarks>
        /// When set, if the current value differs from the provided value, it triggers a property changed notification.
        /// </remarks>
        public bool IsExpanded
        {
            get => isExpanded;
            set => SetProperty(ref isExpanded, value);
        }        

        /// <summary>
        /// Sets a property value and raises the PropertyChanged event if the value has changed.
        /// </summary>
        /// <param name="storage">The backing field for the property.</param>
        /// <param name="value">The new value to set.</param>
        /// <param name="propertyName">The name of the property being set, automatically provided by the compiler.</param>
        /// <returns>
        /// True if the value was changed; otherwise, false.
        /// </returns>
        protected bool SetProperty<T>(ref T storage, T value, [System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(storage, value))
            {
                return false;
            }

            storage = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        /// <summary>
        /// Raises the PropertyChanged event for the specified property name, 
        /// notifying any listeners that the property value has changed.
        /// </summary>
        /// <param name="propertyName">The name of the property that changed.</param>
        protected void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
