// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Sportradar.OddsFeed.SDK.Api.Internal.Handlers
{
    internal sealed class RequestCircuitHandler : DelegatingHandler
    {
        private readonly IRequestCircuitBreaker _guard;
        private readonly ICircuitTimeTracker _timeTracker;

        public RequestCircuitHandler(
            IRequestCircuitBreaker guard,
            ICircuitTimeTracker timeTracker)
        {
            _guard = guard;
            _timeTracker = timeTracker;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
        {
            if (_guard.IsOpen())
            {
                return new HttpResponseMessage(HttpStatusCode.ServiceUnavailable);
            }

            HttpResponseMessage response;
            try
            {
                response = await base.SendAsync(request, ct).ConfigureAwait(false);
                response.EnsureSuccessStatusCode();

                ResetFailedRequests();
            }
            catch (HttpRequestException)
            {
                SetNextCooldown();
                throw;
            }

            return response;
        }

        private void ResetFailedRequests()
        {
            _timeTracker.ResetFailedRequestsCount();
            _guard.Close();
        }

        private void SetNextCooldown()
        {
            _timeTracker.IncrementFailedRequestsCount();
            var nextCooldown = _timeTracker.NextOpenDuration();
            _guard.Open(nextCooldown);
        }
    }
}
