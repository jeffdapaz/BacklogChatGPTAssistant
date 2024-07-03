using System;

namespace JeffPires.BacklogChatGPTAssistantShared.Models
{
    /// <summary>
    /// Represents a Azure Devops project.
    /// </summary>
    public class Project
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