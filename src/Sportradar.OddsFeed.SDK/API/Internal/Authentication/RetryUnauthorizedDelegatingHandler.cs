// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Polly;
using Polly.Retry;

namespace Sportradar.OddsFeed.SDK.Api.Internal.Authentication
{
    internal sealed class RetryUnauthorizedDelegatingHandler : DelegatingHandler
    {
        private readonly ResiliencePipeline<HttpResponseMessage> _pipeline;

        public RetryUnauthorizedDelegatingHandler(IAuthenticationTokenCache tokenCache)
        {
            var retryStrategyOptions = CreateRetryStrategyOptions(tokenCache);
            _pipeline = new ResiliencePipelineBuilder<HttpResponseMessage>()
                       .AddRetry(retryStrategyOptions)
                       .Build();
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return _pipeline.ExecuteAsync(async _ => await base.SendAsync(request, cancellationToken), cancellationToken).AsTask();
        }

        private static RetryStrategyOptions<HttpResponseMessage> CreateRetryStrategyOptions(IAuthenticationTokenCache tokenCache)
        {
            return new RetryStrategyOptions<HttpResponseMessage>
            {
                MaxRetryAttempts = 1,
                Delay = TimeSpan.Zero,
                BackoffType = DelayBackoffType.Constant,
                OnRetry = async args =>
                          {
                              var bearerToken = GetRequestBearerToken(args.Outcome.Result?.RequestMessage);
                              await tokenCache.RefreshApiTokenAsync(bearerToken).ConfigureAwait(false);
                          },
                ShouldHandle = args => new ValueTask<bool>(args.Outcome.Result != null &&
                                                           args.Outcome.Result.StatusCode == HttpStatusCode.Unauthorized),
            };
        }

        private static string GetRequestBearerToken(HttpRequestMessage requestMessage)
        {
            var bearerToken = string.Empty;
            if (requestMessage.Headers.Authorization.Scheme.Equals("Bearer", StringComparison.OrdinalIgnoreCase))
            {
                bearerToken = requestMessage.Headers.Authorization.Parameter;
            }

            return bearerToken;
        }
    }
}
