using JeffPires.BacklogChatGPTAssistantShared.Utils;
using System.Collections.Generic;

namespace JeffPires.BacklogChatGPTAssistantShared.Models
{
    /// <summary>
    /// Represents a feature, inheriting from WorkItemBase.
    /// </summary>
    public class Feature : WorkItemBase
    {
        /// <summary>
        /// Default Constructor.
        /// </summary>
        public Feature()
        {
            Type = WorkItemType.Feature;
        }

        /// <summary>
        /// Gets or sets the list of product backlog items.
        /// </summary>
        public List<ProductBacklogItem> ProductBacklogItems { get; set; }
    }
}