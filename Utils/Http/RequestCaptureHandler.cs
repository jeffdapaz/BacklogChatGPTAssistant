﻿using System.Collections.Generic;
    /// <summary>
    /// Represents a custom HTTP client handler that captures the request data.
    /// </summary>
    public class RequestCaptureHandler : HttpClientHandler

        /// <summary>
        /// Initializes a new instance of the RequestCaptureHandler class.
        /// </summary>
        /// <param name="logRequests">A boolean value indicating whether requests should be logged.</param>
        /// <param name="logResponses">A boolean value indicating whether responses should be logged.</param>
        public RequestCaptureHandler(bool logRequests, bool logResponses)

        /// <summary>
        /// Overrides the SendAsync method to log the request and response information.
        /// </summary>
        /// <param name="request">The HTTP request message.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The HTTP response message.</returns>
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)

            return response;