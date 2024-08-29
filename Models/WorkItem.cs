using JeffPires.BacklogChatGPTAssistant.Utils;
using System.Collections.Generic;

namespace JeffPires.BacklogChatGPTAssistant.Models
{
    /// <summary>
    /// Represents the class for a work item.
    /// </summary>
    public class WorkItem
    {
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
        public List<WorkItem> Children { get; set; }
    }

    /// <summary>
    /// Represents the schema for a work item, defining its structure and properties.
    /// </summary>
    public class WorkItemSchema
    {
        /// <summary>
        /// Gets or sets the collection of work items.
        /// </summary>
        public List<WorkItem> WorkItems { get; set; }
    }

    /// <summary>
    /// Enumeration representing different types of work items.
    /// </summary>
    public enum WorkItemType
    {
        [EnumStringValue("Task")]
        Task = 0,
        [EnumStringValue("Product Backlog Item")]
        ProductBacklogItem = 1,
        [EnumStringValue("Feature")]
        Feature = 2,
        [EnumStringValue("Epic")]
        Epic = 3
    }
}