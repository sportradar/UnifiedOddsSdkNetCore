// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Sportradar.OddsFeed.SDK.Api.Internal.ApiAccess;

namespace Sportradar.OddsFeed.SDK.Api.Internal.Authentication
{
    /// <summary>
    /// Requests an OAuth access token using a client-assertion JWT token.
    /// </summary>
    internal sealed class AuthenticationClient : IAuthenticationClient
    {
        private readonly string _nameOfHttpClientForAuthentication;
        private const string TokenPath = "oauth/token";

        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<RestTraffic> _restLog;

        public AuthenticationClient(IHttpClientFactory httpClientFactory, string httpClientName, ILoggerFactory loggerFactory)
        {
            _httpClientFactory = httpClientFactory;
            _nameOfHttpClientForAuthentication = httpClientName;
            _restLog = loggerFactory.CreateLogger<RestTraffic>();
        }

        /// <summary>
        /// Performs the OAuth token request using a client-assertion JWT token.
        /// Returns the access_token string from the JSON response.
        /// </summary>
        public async Task<AuthenticationTokenApiResponse> RequestAccessTokenAsync(string clientAssertionJwt, string audience, CancellationToken cancellationToken)
        {
            using (var httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, TokenPath))
            {
                httpRequestMessage.Content = CreateJwtRequestContent(clientAssertionJwt, audience);

                return await SendRequestAsync(httpRequestMessage, cancellationToken);
            }
        }

        private async Task<AuthenticationTokenApiResponse> SendRequestAsync(HttpRequestMessage requestMessage, CancellationToken cancellationToken)
        {
            var httpClient = _httpClientFactory.CreateClient(_nameOfHttpClientForAuthentication);

            var watch = Stopwatch.StartNew();
            _restLog.LogInformation("POST: {BaseAddress}{PostUri} initiated", httpClient.BaseAddress, requestMessage.RequestUri);

            using (var httpResponseMessage = await httpClient.SendAsync(requestMessage, cancellationToken).ConfigureAwait(false))
            {
                var traceId = requestMessage.GetTraceId();

                try
                {
                    _ = httpResponseMessage.EnsureSuccessStatusCode();
                }
                catch (Exception ex)
                {
                    _restLog.LogError(ex, "TraceId: {TraceId} POST: {PostUri} took {Elapsed} ms", traceId, requestMessage.RequestUri.AbsoluteUri, watch.ElapsedMilliseconds.ToString(CultureInfo.InvariantCulture));
                    throw;
                }

                var tokenResponseRawString = await httpResponseMessage.Content.ReadAsStringAsync().ConfigureAwait(false);
                var tokenResponse = DeserializeTokenEndpointResponse(tokenResponseRawString);

                if (_restLog.IsEnabled(LogLevel.Debug))
                {
                    const string msgTemplate = "TraceId: {TraceId} POST: {PostUri} took {Elapsed} ms. Response: {ResponseStatusCode}-{ResponseReasonPhrase} {ResponseContent}";
                    _restLog.LogDebug(msgTemplate,
                                      traceId,
                                      requestMessage.RequestUri.AbsoluteUri,
                                      watch.ElapsedMilliseconds.ToString(CultureInfo.InvariantCulture),
                                      ((int)httpResponseMessage.StatusCode).ToString(CultureInfo.InvariantCulture),
                                      httpResponseMessage.ReasonPhrase,
                                      tokenResponse.ToSanitizedString());
                }
                else
                {
                    _restLog.LogInformation("TraceId: {TraceId} POST: {PostUri} took {Elapsed} ms",
                                            traceId,
                                            requestMessage.RequestUri.AbsoluteUri,
                                            watch.ElapsedMilliseconds.ToString(CultureInfo.InvariantCulture));
                }

                return tokenResponse;
            }
        }

        private static FormUrlEncodedContent CreateJwtRequestContent(string clientAssertionJwt, string audience)
        {
            return new FormUrlEncodedContent(new[]
                                                 {
                                                     new KeyValuePair<string, string>("grant_type", "client_credentials"),
                                                     new KeyValuePair<string, string>("client_assertion_type", "urn:ietf:params:oauth:client-assertion-type:jwt-bearer"),
                                                     new KeyValuePair<string, string>("client_assertion", clientAssertionJwt),
                                                     new KeyValuePair<string, string>("audience", audience)
                                                 });
        }

        private static readonly JsonSerializerOptions JsonSerializerOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

        private static AuthenticationTokenApiResponse DeserializeTokenEndpointResponse(string tokenRawResponse)
        {
            try
            {
                return JsonSerializer.Deserialize<AuthenticationTokenApiResponse>(tokenRawResponse, JsonSerializerOptions);
            }
            catch (Exception)
            {
                throw new JsonException($"Failed to deserialize token response. Content: {tokenRawResponse}");
            }
        }
    }
}
