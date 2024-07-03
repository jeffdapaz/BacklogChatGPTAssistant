using JeffPires.BacklogChatGPTAssistantShared.Utils;

namespace JeffPires.BacklogChatGPTAssistantShared.Models
{
    /// <summary>
    /// Represents a Azure Devops Work Item.
    /// </summary>
    public class WorkItem
    {
        /// <summary>
        /// Gets or sets the unique identifier.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the ID of the parent work item. This property is nullable.
        /// </summary>
        public int? ParentId { get; set; }

        /// <summary>
        /// Gets or sets the iteration path, which represents the hierarchical path of the iteration within a project.
        /// </summary>
        public string IterationPath { get; set; }

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
        public int? RemainingWork { get; set; }
    }
}