using JeffPires.BacklogChatGPTAssistant.Options;
using JeffPires.BacklogChatGPTAssistant.Utils;
using JeffPires.BacklogChatGPTAssistant.Utils.Http;
using OpenAI;
using OpenAI.Managers;
using OpenAI.ObjectModels.RequestModels;
using OpenAI.ObjectModels.ResponseModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace JeffPires.BacklogChatGPTAssistantShared.Utils
{
    public static class OpenAI
    {
        #region Attributes

        private static OpenAIService openAiService;
        private static OpenAIService azureService;

        private static OpenAiOptions openAiAPIOptions;
        private static OpenAiOptions azureAPIOptions;

        private static ChatGPTHttpClientFactory chatGPTHttpClient;

        #endregion Attributes

        #region Public Methods

        /// <summary>
        /// Sends a request to the OpenAI or Azure service based on the provided options and returns the response.
        /// </summary>
        /// <param name="options">Configuration options for the service.</param>
        /// <param name="systemMessages">List of system messages to include in the request.</param>
        /// <param name="userMessages">List of user messages to include in the request.</param>
        /// <param name="isJsonResponseFormat">Indicates whether the response should be in JSON format.</param>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <returns>
        /// The response from the service as a string.
        /// </returns>
        /// <exception cref="Exception">Thrown when the service returns an error or an unknown error occurs.</exception>
        public static async Task<string> RequestAsync(OptionPageGridGeneral options, List<string> systemMessages, List<string> userMessages, bool isJsonResponseFormat, CancellationToken cancellationToken)
        {
            OpenAIService service;

            if (options.Service == ServiceType.OpenAI)
            {
                CreateOpenAIApiHandler(options);

                service = openAiService;
            }
            else
            {
                CreateAzureApiHandler(options);

                service = azureService;
            }

            List<ChatMessage> chatMessages = [];

            foreach (string systemMessage in systemMessages)
            {
                chatMessages.Add(ChatMessage.FromSystem(systemMessage));
            }

            foreach (string userMessage in userMessages)
            {
                chatMessages.Add(ChatMessage.FromUser(userMessage));
            }

            ChatCompletionCreateResponse completionResult = await service.ChatCompletion.CreateCompletion(new ChatCompletionCreateRequest()
            {
                Messages = chatMessages,
                Model = string.IsNullOrWhiteSpace(options.CustomModel) ? options.Model.GetStringValue() : options.CustomModel,
                FrequencyPenalty = options.FrequencyPenalty.HasValue ? (float)options.FrequencyPenalty : null,
                MaxTokens = options.MaxTokens,
                PresencePenalty = options.PresencePenalty.HasValue ? (float)options.PresencePenalty : null,
                StopAsList = options.StopSequences.Split(),
                Temperature = options.Temperature.HasValue ? (float)options.Temperature : null,
                TopP = options.TopP.HasValue ? (float)options.TopP : null,
                User = Constants.EXTENSION_NAME,
                ResponseFormat = isJsonResponseFormat ? new ResponseFormat() { Type = "json_object" } : null
            }, cancellationToken: cancellationToken);

            if (completionResult.Successful)
            {
                return completionResult.Choices.FirstOrDefault().Message.Content;
            }
            else
            {
                string message = "Unknown Error. Try again.";

                if (completionResult.Error != null)
                {
                    message = $"{completionResult.Error.Code}: {completionResult.Error.Message}";
                }

                throw new Exception(message);
            }
        }

        #endregion Public Methods

        #region Private Methods        

        /// <summary>
        /// Creates and configures an Azure API handler using the provided options. If the handler already exists and the options have been modified, it reinitializes the handler.
        /// </summary>
        private static void CreateAzureApiHandler(OptionPageGridGeneral options)
        {
            if (azureService == null)
            {
                chatGPTHttpClient = new(options);

                if (!string.IsNullOrWhiteSpace(options.Proxy))
                {
                    chatGPTHttpClient.SetProxy(options.Proxy);
                }

                azureAPIOptions = new OpenAiOptions()
                {
                    ApiKey = options.ApiKey,
                    ProviderType = ProviderType.Azure,
                    ApiVersion = options.AzureApiVersion,
                    DeploymentId = options.AzureDeploymentId,
                    ResourceName = options.AzureResourceName
                };

                azureService = new OpenAIService(azureAPIOptions, chatGPTHttpClient.CreateClient(Constants.EXTENSION_NAME));
            }
            else if (IsAzureOptionsParametersModified(options))
            {
                azureService = null;
                CreateAzureApiHandler(options);
            }
        }

        /// <summary>
        /// Creates and initializes the OpenAI API handler with the provided options. If the handler already exists and the options have been modified, it reinitializes the handler.
        /// </summary>
        private static void CreateOpenAIApiHandler(OptionPageGridGeneral options)
        {
            if (openAiService == null)
            {
                chatGPTHttpClient = new(options);

                if (!string.IsNullOrWhiteSpace(options.Proxy))
                {
                    chatGPTHttpClient.SetProxy(options.Proxy);
                }

                openAiAPIOptions = new OpenAiOptions()
                {
                    ApiKey = options.ApiKey,
                    ProviderType = ProviderType.OpenAi,
                    BaseDomain = !string.IsNullOrWhiteSpace(options.BaseAPI) ? options.BaseAPI : null,
                    Organization = !string.IsNullOrWhiteSpace(options.OpenAIOrganization) ? options.OpenAIOrganization : null
                };

                openAiService = new OpenAIService(openAiAPIOptions, chatGPTHttpClient.CreateClient(BacklogChatGPTAssistant.Utils.Constants.EXTENSION_NAME));
            }
            else if (IsOpenAIOptionsParametersModified(options))
            {
                openAiService = null;
                CreateOpenAIApiHandler(options);
            }
        }

        /// <summary>
        /// Checks if the Azure options parameters have been modified by comparing the current options with the provided options.
        /// </summary>
        /// <param name="options">The options to compare against the current Azure API options.</param>
        /// <returns>
        /// True if any of the Azure options parameters have been modified; otherwise, false.
        /// </returns>
        private static bool IsAzureOptionsParametersModified(OptionPageGridGeneral options)
        {
            return IsProxyModified(options) ||
                azureAPIOptions.ApiKey != options.ApiKey ||
                azureAPIOptions.DeploymentId != options.AzureDeploymentId ||
                azureAPIOptions.ResourceName != options.AzureResourceName;
        }

        /// <summary>
        /// Checks if any of the OpenAI options parameters have been modified.
        /// </summary>
        /// <param name="options">The current option settings to compare against.</param>
        /// <returns>
        /// True if any of the OpenAI options parameters have been modified; otherwise, false.
        /// </returns>
        private static bool IsOpenAIOptionsParametersModified(OptionPageGridGeneral options)
        {
            return IsProxyModified(options) || IsBaseApiModified(options) || openAiAPIOptions.ApiKey != options.ApiKey || openAiAPIOptions.Organization != options.OpenAIOrganization;
        }

        /// <summary>
        /// Determines if the proxy settings have been modified by comparing the current proxy with the provided options.
        /// </summary>
        /// <param name="options">The options containing the proxy settings to compare against.</param>
        /// <returns>
        /// True if the proxy settings have been modified; otherwise, false.
        /// </returns>
        private static bool IsProxyModified(OptionPageGridGeneral options)
        {
            return (chatGPTHttpClient.Proxy ?? string.Empty) != (options.Proxy ?? string.Empty);
        }

        /// <summary>
        /// Determines if the BaseAPI option has been modified by comparing it with the openAiOptions BaseDomain.
        /// </summary>
        /// <param name="options">The general options containing the BaseAPI to be checked.</param>
        /// <returns>
        /// True if the BaseAPI has been modified; otherwise, false.
        /// </returns>
        private static bool IsBaseApiModified(OptionPageGridGeneral options)
        {
            if (string.IsNullOrWhiteSpace(options.BaseAPI) && string.IsNullOrWhiteSpace(openAiAPIOptions.BaseDomain))
            {
                return false;
            }

            return options.BaseAPI != openAiAPIOptions.BaseDomain;
        }

        #endregion Private Methods
    }

    /// <summary>
    /// Enum containing the different types of model languages.
    /// </summary>
    public enum ModelLanguageEnum
    {
        [EnumStringValue("gpt-3.5-turbo")]
        GPT_3_5_Turbo,
        [EnumStringValue("gpt-3.5-turbo-1106")]
        GPT_3_5_Turbo_1106,
        [EnumStringValue("gpt-4")]
        GPT_4,
        [EnumStringValue("gpt-4-32k")]
        GPT_4_32K,
        [EnumStringValue("gpt-4-turbo")]
        GPT_4_Turbo,
        [EnumStringValue("gpt-4o")]
        GPT_4o,
        [EnumStringValue("gpt-4o-mini")]
        GPT_4o_Mini
    }

    /// <summary>
    /// Enum to represent the different OpenAI services available.
    /// </summary>
    public enum ServiceType
    {
        OpenAI,
        AzureOpenAI
    }
}
