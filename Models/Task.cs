using JeffPires.BacklogChatGPTAssistant.Utils;

namespace JeffPires.BacklogChatGPTAssistant.Models
{
    /// <summary>
    /// Represents a task that inherits from the WorkItemBase class.
    /// </summary>
    public class Task : WorkItemBase
    {
        /// <summary>
        /// Default Constructor.
        /// </summary>
        public Task()
        {
            Type = WorkItemType.Task;
        }
    }
}
