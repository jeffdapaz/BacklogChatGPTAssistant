using System.Collections.Generic;
using System.ComponentModel;
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

        /// <summary>
        /// Gets or sets the unique identifier.
        /// </summary>
        public double? Id { get; set; }

        /// <summary>
        /// Gets or sets the ID of the parent work item. This property is nullable.
        /// </summary>
        public double? ParentId { get; set; }

        /// <summary>
        /// Gets or sets the type of the work item.
        /// </summary>
        public WorkItemType Type { get; set; }

        /// <summary>
        /// Gets or sets the title.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the acceptance criteria.
        /// </summary>
        public string AcceptanceCriteria { get; set; }

        /// <summary>
        /// Gets or sets the remaining work.
        /// </summary>
        public double? RemainingWork { get; set; }

        /// <summary>
        /// Gets or sets the collection of child work items associated with the current work item.
        /// </summary>
        public List<WorkItemResult> Children { get; set; }

        /// <summary>
        /// Gets the source of the icon associated with the work item.
        /// </summary>
        public string IconSource
        {
            get => Type switch
            {
                WorkItemType.Epic => "pack://application:,,,/BacklogChatGPTAssistant;component/Resources/epic.png",
                WorkItemType.Feature => "pack://application:,,,/BacklogChatGPTAssistant;component/Resources/feature.png",
                WorkItemType.ProductBacklogItem => "pack://application:,,,/BacklogChatGPTAssistant;component/Resources/backlog.png",
                _ => "pack://application:,,,/BacklogChatGPTAssistant;component/Resources/task.png"
            };
            private set { }
        }

        private double textWidth;

        /// <summary>
        /// Gets or sets the width of the text.
        /// </summary>
        public double TextWidth
        {
            get
            {
                return textWidth;
            }
            set
            {
                if (textWidth != value)
                {
                    textWidth = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(textWidth)));
                }
            }
        }
    }
}
