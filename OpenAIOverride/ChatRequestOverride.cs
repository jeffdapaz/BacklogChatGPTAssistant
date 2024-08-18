using Newtonsoft.Json;
using OpenAI_API.Chat;

namespace JeffPires.BacklogChatGPTAssistant.OpenAIOverride
{
    /// <summary>
    /// Represents an overridden chat request that includes a response format property.
    /// </summary>
    public class ChatRequestOverride : ChatRequest
    {
        #region Properties

        /// <summary>
        /// Represents the format of the response from the chatbot.
        /// </summary>
        [JsonProperty("response_format")]
        public ResponseFormat ResponseFormat { get; set; }

        #endregion Properties

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ChatRequestOverride"/> class based on another instance.
        /// </summary>
        /// <param name="basedOn">The instance to base the new instance on. If null, the properties will not be copied.</param>
        public ChatRequestOverride(ChatRequestOverride basedOn) : base(basedOn)
        {
            if (basedOn == null)
            {
                return;
            }

            ResponseFormat = basedOn.ResponseFormat;
        }

        #endregion Constructors
    }

    /// <summary>
    /// Represents the format of the response from the chatbot, including the type and JSON schema.
    /// </summary>
    public class ResponseFormat
    {
        /// <summary>
        /// Represents the format of the response from the chatbot, including the type and JSON schema.
        /// </summary>
        [JsonProperty("type")]
        public string Type { get; set; }

        /// <summary>
        /// Represents the response format for a chat request, including the type and JSON schema.
        /// </summary>
        [JsonProperty("json_schema")]
        public JsonSchema JsonSchema { get; set; }
    }

    /// <summary>
    /// Represents a JSON schema for defining the structure of JSON data.
    /// </summary>
    public class JsonSchema
    {
        /// <summary>
        /// Represents a JSON schema for a work item, including its name and schema definition.
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; set; }

        /// <summary>
        /// Represents the JSON schema for a work item, including its name and schema definition.
        /// </summary>
        [JsonProperty("schema")]
        public object Schema { get; set; }
    }
}