using System;

namespace JeffPires.BacklogChatGPTAssistant.Models
{
    /// <summary>
    /// Represents a Azure Devops project.
    /// </summary>
    public class AzureDevopsProject
    {
        /// <summary>
        /// Gets or sets the unique identifier.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        public string Name { get; set; }
    }
}