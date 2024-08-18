using JeffPires.BacklogChatGPTAssistant.Utils;
using System.Collections.Generic;

namespace JeffPires.BacklogChatGPTAssistant.Models
{
    /// <summary>
    /// Represents a epic, inheriting from WorkItemBase.
    /// </summary>
    public class Epic : WorkItemBase
    {
        /// <summary>
        /// Default Constructor.
        /// </summary>
        public Epic()
        {
            Type = WorkItemType.Epic;
        }

        /// <summary>
        /// Gets or sets the list of features.
        /// </summary>
        public List<Feature> Features { get; set; }
    }
}