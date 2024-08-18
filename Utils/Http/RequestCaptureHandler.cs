﻿using System.Collections.Generic;using System.Net.Http;using System.Threading;using System.Threading.Tasks;namespace JeffPires.BacklogChatGPTAssistant.Utils.Http{
    /// <summary>
    /// Represents a custom HTTP client handler that captures the request data.
    /// </summary>
    public class RequestCaptureHandler : HttpClientHandler    {        private readonly bool logRequests;        private readonly bool logResponses;

        /// <summary>
        /// Initializes a new instance of the RequestCaptureHandler class.
        /// </summary>
        /// <param name="logRequests">A boolean value indicating whether requests should be logged.</param>
        /// <param name="logResponses">A boolean value indicating whether responses should be logged.</param>
        public RequestCaptureHandler(bool logRequests, bool logResponses)        {            this.logRequests = logRequests;            this.logResponses = logResponses;        }

        /// <summary>
        /// Overrides the SendAsync method to log the request and response information.
        /// </summary>
        /// <param name="request">The HTTP request message.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The HTTP response message.</returns>
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)        {            string content;            if (logRequests)            {                Logger.Log($"Request URI: {request.RequestUri}");                Logger.Log($"Request Method: {request.Method}");                Logger.Log("Request Headers:");                foreach (KeyValuePair<string, IEnumerable<string>> header in request.Headers)                {                    Logger.Log($"{header.Key}: {string.Join(", ", header.Value)}");                }                if (request.Content != null)                {                    content = await request.Content.ReadAsStringAsync();                    Logger.Log("Request Content: " + content);                }                Logger.Log(new string('_', 100));            }            HttpResponseMessage response = await base.SendAsync(request, cancellationToken);            if (response != null && logResponses)            {                Logger.Log("Response Headers:");                foreach (KeyValuePair<string, IEnumerable<string>> header in response.Headers)                {                    Logger.Log($"{header.Key}: {string.Join(", ", header.Value)}");                }                Logger.Log($"Response Status Code: {response.StatusCode}");                if (response.Content != null)                {                    content = await response.Content.ReadAsStringAsync();                    Logger.Log("Response Content: " + content);                }                Logger.Log(new string('_', 100));            }

            return response;        }    }}