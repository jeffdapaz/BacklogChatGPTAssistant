using JeffPires.BacklogChatGPTAssistantShared.Utils;
using System.Collections.Generic;

namespace JeffPires.BacklogChatGPTAssistantShared.Models
{
    /// <summary>
    /// Represents a product backlog item, inheriting from WorkItemBase.
    /// </summary>
    public class ProductBacklogItem : WorkItemBase
    {
        /// <summary>
        /// Default Constructor.
        /// </summary>
        public ProductBacklogItem()
        {
            Type = WorkItemType.ProductBacklogItem;
        }

        /// <summary>
        /// Gets or sets the list of tasks.
        /// </summary>
        public List<Task> Tasks { get; set; }
    }
}
