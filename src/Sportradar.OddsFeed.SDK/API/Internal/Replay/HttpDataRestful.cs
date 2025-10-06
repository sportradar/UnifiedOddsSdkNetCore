// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Dawn;
using Microsoft.Extensions.Logging;
using Sportradar.OddsFeed.SDK.Api.Internal.ApiAccess;
using Sportradar.OddsFeed.SDK.Common.Exceptions;
using Sportradar.OddsFeed.SDK.Common.Internal;
using Sportradar.OddsFeed.SDK.Common.Internal.Telemetry;
using Sportradar.OddsFeed.SDK.Messages.Rest;

namespace Sportradar.OddsFeed.SDK.Api.Internal.Replay
{
    /// <summary>
    /// A <see cref="IDataRestful"/> which uses the HTTP requests to post/get/put/patch and delete the data
    /// </summary>
    internal class HttpDataRestful : HttpDataFetcher, IDataRestful
    {
        private readonly ILogger _log = SdkLoggerFactory.GetLoggerForRestTraffic(typeof(HttpDataRestful));

        /// <summary>
        /// A <see cref="ISdkHttpClient"/> used to invoke HTTP requests
        /// </summary>
        private readonly ISdkHttpClient _sdkHttpClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpDataRestful"/> class.
        /// </summary>
        /// <param name="sdkHttpClient">A <see cref="ISdkHttpClient" /> used to invoke HTTP requests</param>
        /// <param name="responseDeserializer">The deserializer for unexpected response</param>
        /// <param name="connectionFailureLimit">Indicates the limit of consecutive request failures, after which it goes in "blocking mode"</param>
        /// <param name="connectionFailureTimeout">indicates the timeout after which comes out of "blocking mode" (in seconds)</param>
        public HttpDataRestful(ISdkHttpClient sdkHttpClient, IDeserializer<response> responseDeserializer, int connectionFailureLimit = 5, int connectionFailureTimeout = 15)
            : base(sdkHttpClient, responseDeserializer, connectionFailureLimit, connectionFailureTimeout)
        {
            Guard.Argument(sdkHttpClient, nameof(sdkHttpClient)).NotNull();
            Guard.Argument(sdkHttpClient.DefaultRequestHeaders, nameof(sdkHttpClient)).NotNull();
            Guard.Argument(connectionFailureLimit, nameof(connectionFailureLimit)).Positive();
            Guard.Argument(connectionFailureTimeout, nameof(connectionFailureTimeout)).Positive();

            _sdkHttpClient = sdkHttpClient;
        }

        /// <summary>
        /// Asynchronously gets a <see cref="HttpResponseMessage"/> as a result of PUT request send to the provided <see cref="Uri"/>
        /// </summary>
        /// <param name="uri">The <see cref="Uri"/> of the resource to be send to</param>
        /// <param name="content">A <see cref="HttpContent"/> to be posted to the specific <see cref="Uri"/></param>
        /// <returns>A <see cref="Task"/> which, when completed will return a <see cref="HttpResponseMessage"/> containing status code and data</returns>
        /// <exception cref="CommunicationException">Failed to execute http post</exception>
        public virtual async Task<HttpResponseMessage> PutDataAsync(Uri uri, HttpContent content = null)
        {
            ValidateConnection(uri);
            var responseMessage = new HttpResponseMessage(HttpStatusCode.ServiceUnavailable);
            try
            {
                _log.LogInformation("PutDataAsync url: {AbsoluteUri}", uri.AbsoluteUri);
                responseMessage = await _sdkHttpClient.PutAsync(uri, content ?? new StringContent(string.Empty)).ConfigureAwait(false);
                RecordSuccess();
                return responseMessage;
            }
            catch (Exception ex)
            {
                if (ex is HttpRequestException)
                {
                    RecordFailure();
                    throw new CommunicationException("Failed to execute http PUT request", uri.ToString(), responseMessage.StatusCode, ex);
                }
                throw;
            }
        }

        /// <summary>
        /// Asynchronously gets a <see cref="HttpResponseMessage"/> as a result of DELETE request send to the provided <see cref="Uri"/>
        /// </summary>
        /// <param name="uri">The <see cref="Uri"/> of the resource to be send to</param>
        /// <returns>A <see cref="Task"/> which, when completed will return a <see cref="HttpResponseMessage"/> containing status code and data</returns>
        /// <exception cref="CommunicationException">Failed to execute http post</exception>
        public virtual async Task<HttpResponseMessage> DeleteDataAsync(Uri uri)
        {
            ValidateConnection(uri);
            var responseMessage = new HttpResponseMessage(HttpStatusCode.ServiceUnavailable);
            try
            {
                _log.LogInformation("DeleteDataAsync url: {AbsoluteUri}", uri.AbsoluteUri);
                responseMessage = await _sdkHttpClient.DeleteAsync(uri).ConfigureAwait(false);
                RecordSuccess();
                return responseMessage;
            }
            catch (Exception ex)
            {
                if (ex is HttpRequestException)
                {
                    RecordFailure();
                    throw new CommunicationException("Failed to execute http DELETE request", uri.ToString(), responseMessage.StatusCode, ex);
                }
                throw;
            }
        }
    }
}
