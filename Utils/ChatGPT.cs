using JeffPires.BacklogChatGPTAssistant.OpenAIOverride;
using JeffPires.BacklogChatGPTAssistant.Options;
using JeffPires.BacklogChatGPTAssistant.Utils.Http;
using Newtonsoft.Json.Schema;
using OpenAI_API;
using OpenAI_API.Chat;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace JeffPires.BacklogChatGPTAssistant.Utils
{
    /// <summary>
    /// Static class containing methods for interacting with the ChatGPT API.
    /// </summary>
    static class ChatGPT
    {
        private static OpenAIAPI openAiAPI;
        private static OpenAIAPI azureAPI;
        private static ChatGPTHttpClientFactory chatGPTHttpClient;
        private static readonly TimeSpan timeout = new(0, 0, 120);

        /// <summary>
        /// Asynchronously gets a response from a chatbot based on the provided options, system messages, user input, stop sequences, and JSON schema.
        /// </summary>
        /// <param name="options">The options for the chatbot.</param>
        /// <param name="systemMessages">The system messages to send to the chatbot.</param>
        /// <param name="userInput">The user input to send to the chatbot.</param>
        /// <param name="stopSequences">The stop sequences to use for ending the conversation.</param>
        /// <param name="jsonSchema">The JSON schema for the response format.</param>
        /// <param name="jsonSchemaName">The name of the JSON schema.</param>
        /// <param name="cancellationToken">The cancellation token to cancel the operation.</param>
        /// <returns>A task that represents the asynchronous operation, containing the response from the chatbot.</returns>
        public static async Task<string> GetResponseAsync(OptionPageGridGeneral options,
                                                          List<string> systemMessages,
                                                          string userInput,
                                                          string[] stopSequences,
                                                          JSchema jsonSchema,
                                                          string jsonSchemaName,
                                                          CancellationToken cancellationToken)
        {
            ConversationOverride chat = CreateConversationForCompletions(options, systemMessages, userInput, stopSequences, jsonSchema, jsonSchemaName);

            Task<string> task = chat.GetResponseFromChatbotAsync();

            await Task.WhenAny(task, Task.Delay(timeout, cancellationToken)).ConfigureAwait(false);

            if (task.IsFaulted)
            {
                throw task.Exception.InnerException ?? task.Exception;
            }

            cancellationToken.ThrowIfCancellationRequested();

            return await task;
        }

        /// <summary>
        /// Creates a conversation for chatbot interactions based on the provided options, system messages, and JSON schema.
        /// </summary>
        /// <param name="options">The options for configuring the chatbot service.</param>
        /// <param name="systemMessages">A list of system messages to be included in the conversation.</param>
        /// <param name="jsonSchema">The JSON schema defining the expected response format.</param>
        /// <param name="jsonSchemaName">The name of the JSON schema.</param>
        /// <returns>A <see cref="ConversationOverride"/> instance configured for the chatbot interaction.</returns>
        private static ConversationOverride CreateConversation(OptionPageGridGeneral options, List<string> systemMessages, JSchema jsonSchema, string jsonSchemaName)
        {
            ConversationOverride chat;

            if (options.Service == OpenAIService.OpenAI)
            {
                CreateOpenAIApiHandler(options);

                chat = new ConversationOverride((ChatEndpoint)openAiAPI.Chat);
            }
            else
            {
                CreateAzureApiHandler(options);

                chat = new ConversationOverride((ChatEndpoint)azureAPI.Chat);
            }

            foreach (string systemMessage in systemMessages)
            {
                chat.AppendSystemMessage(systemMessage);
            }            

            chat.AutoTruncateOnContextLengthExceeded = true;
            chat.RequestParameters.Model = string.IsNullOrWhiteSpace(options.CustomModel) ? options.Model.GetStringValue() : options.CustomModel;
            chat.RequestParameters.Temperature = options.Temperature;
            chat.RequestParameters.MaxTokens = options.MaxTokens;
            chat.RequestParameters.TopP = options.TopP;
            chat.RequestParameters.FrequencyPenalty = options.FrequencyPenalty;
            chat.RequestParameters.PresencePenalty = options.PresencePenalty;
            chat.RequestParameters.ResponseFormat = new() { Type = "text", JsonSchema = new() { Name = jsonSchemaName, Schema = jsonSchema } };

            return chat;
        }

        /// <summary>
        /// Creates a conversation for completions with the specified options, system messages, user input, stop sequences, JSON schema, and schema name.
        /// </summary>
        /// <param name="options">The options for the conversation.</param>
        /// <param name="systemMessages">The system messages to append to the conversation.</param>
        /// <param name="userInput">The user input to send to the conversation.</param>
        /// <param name="stopSequences">The stop sequences to use for ending the conversation.</param>
        /// <param name="jsonSchema">The JSON schema for the response format.</param>
        /// <param name="jsonSchemaName">The name of the JSON schema.</param>
        /// <returns>
        /// The created conversation override instance.
        /// </returns>
        private static ConversationOverride CreateConversationForCompletions(OptionPageGridGeneral options, List<string> systemMessages, string userInput, string[] stopSequences, JSchema jsonSchema, string jsonSchemaName)
        {
            ConversationOverride chat = CreateConversation(options, systemMessages, jsonSchema, jsonSchemaName);

            if (options.MinifyRequests)
            {
                userInput = TextFormat.MinifyText(userInput);
            }

            userInput = TextFormat.RemoveCharactersFromText(userInput, options.CharactersToRemoveFromRequests.Split(','));

            chat.AppendUserInput(userInput);

            if (stopSequences != null && stopSequences.Length > 0)
            {
                chat.RequestParameters.MultipleStopSequences = stopSequences;
            }

            return chat;
        }

        /// <summary>
        /// Creates an OpenAI API handler based on the provided options.
        /// </summary>
        /// <param name="options">The options to use for creating the OpenAI API handler.</param>
        private static void CreateOpenAIApiHandler(OptionPageGridGeneral options)
        {
            if (openAiAPI == null)
            {
                chatGPTHttpClient = new(options);

                if (!string.IsNullOrWhiteSpace(options.Proxy))
                {
                    chatGPTHttpClient.SetProxy(options.Proxy);
                }

                APIAuthentication auth;

                if (!string.IsNullOrWhiteSpace(options.OpenAIOrganization))
                {
                    auth = new(options.ApiKey, options.OpenAIOrganization);
                }
                else
                {
                    auth = new(options.ApiKey);
                }

                openAiAPI = new(auth);

                if (!string.IsNullOrWhiteSpace(options.BaseAPI))
                {
                    openAiAPI.ApiUrlFormat = options.BaseAPI + "/{0}/{1}";
                }

                openAiAPI.HttpClientFactory = chatGPTHttpClient;
            }
            else if (IsOptionsParametersModified(options))
            {
                openAiAPI = null;
                CreateOpenAIApiHandler(options);
            }

            if (openAiAPI.Auth.ApiKey != options.ApiKey)
            {
                openAiAPI.Auth.ApiKey = options.ApiKey;
            }

            if ((openAiAPI.Auth.OpenAIOrganization ?? string.Empty) != (options.OpenAIOrganization ?? string.Empty))
            {
                openAiAPI.Auth.OpenAIOrganization = options.OpenAIOrganization;
            }
        }

        /// <summary>
        /// Checks if the options parameters have been modified.
        /// </summary>
        /// <param name="options">The options page containing general settings.</param>
        /// <returns>
        /// True if settings have been modified; otherwise, false.
        /// </returns>
        private static bool IsOptionsParametersModified(OptionPageGridGeneral options)
        {
            return IsProxyModified(options) || IsBaseApiModified(options);
        }

        /// <summary>
        /// Checks if the proxy setting has been modified in the options.
        /// </summary>
        /// <param name="options">The general options page grid containing the proxy settings.</param>
        /// <returns>
        /// True if the proxy setting has been modified; otherwise, false.
        /// </returns>
        private static bool IsProxyModified(OptionPageGridGeneral options)
        {
            return (chatGPTHttpClient.Proxy ?? string.Empty) != (options.Proxy ?? string.Empty);
        }

        /// <summary>
        /// Checks if the base API URL has been modified from the default value.
        /// </summary>
        /// <param name="options">The general options containing the base API URL to check against.</param>
        /// <returns>
        /// True if the base API URL has been modified; otherwise, false.
        /// </returns>
        private static bool IsBaseApiModified(OptionPageGridGeneral options)
        {
            if (string.IsNullOrWhiteSpace(options.BaseAPI))
            {
                return openAiAPI.ApiUrlFormat != "https://api.openai.com/{0}/{1}";
            }

            return openAiAPI.ApiUrlFormat != options.BaseAPI + "/{0}/{1}";
        }

        /// <summary>
        /// Creates an Azure API handler based on the provided options. 
        /// </summary>
        /// <param name="options">The options to use for creating/updating the Azure API handler.</param>
        private static void CreateAzureApiHandler(OptionPageGridGeneral options)
        {
            if (azureAPI == null)
            {
                chatGPTHttpClient = new(options);

                if (!string.IsNullOrWhiteSpace(options.Proxy))
                {
                    chatGPTHttpClient.SetProxy(options.Proxy);
                }

                azureAPI = OpenAIAPI.ForAzure(options.AzureResourceName, options.AzureDeploymentId, options.ApiKey);

                azureAPI.HttpClientFactory = chatGPTHttpClient;
            }
            else if ((chatGPTHttpClient.Proxy ?? string.Empty) != (options.Proxy ?? string.Empty) || !azureAPI.ApiUrlFormat.Contains(options.AzureResourceName) || !azureAPI.ApiUrlFormat.Contains(options.AzureDeploymentId))
            {
                azureAPI = null;
                CreateAzureApiHandler(options);
            }

            if (azureAPI.Auth.ApiKey != options.ApiKey)
            {
                azureAPI.Auth.ApiKey = options.ApiKey;
            }

            if ((azureAPI.ApiVersion ?? string.Empty) != (options.AzureApiVersion ?? string.Empty))
            {
                azureAPI.ApiVersion = options.AzureApiVersion;
            }
        }
    }

    /// <summary>
    /// Enum containing the different types of model languages.
    /// </summary>
    public enum ModelLanguageEnum
    {
        [EnumStringValue("gpt-4o-2024-08-06")]
        GPT_4o,
        [EnumStringValue("gpt-4o-mini")]
        GPT_4o_Mini
    }

    /// <summary>
    /// Enum to represent the different OpenAI services available.
    /// </summary>
    public enum OpenAIService
    {
        OpenAI,
        AzureOpenAI
    }
}
