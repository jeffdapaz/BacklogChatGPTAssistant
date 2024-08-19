using System.Collections.Generic;

namespace JeffPires.BacklogChatGPTAssistant.Models
{
    /// <summary>
    /// Represents the result of a Backlog Items generation process.
    /// </summary>
    public class GenerateResult
    {
        /// <summary>
        /// Gets or sets the selected Azure DevOps project.
        /// </summary>
        public AzureDevopsProject SelectedProject { get; set; }

        /// <summary>
        /// Gets or sets the selected iteration path.
        /// </summary>
        public string SelectedIterationPath { get; set; }

        /// <summary>
        /// Gets or sets the existing work item used to generate his children.
        /// </summary>
        public WorkItem ExistentWorkItem { get; set; }

        /// <summary>
        /// Gets or sets the list of generated work items.
        /// </summary>
        public List<WorkItem> GeneratedWorkItems { get; set; }
    }
}