// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Sportradar.OddsFeed.SDK.Api.Internal.Handlers
{
    /// <summary>
    /// Custom message handler which adds a unique request id to the request headers
    /// </summary>
    public class HttpRequestDecoratorHandler : DelegatingHandler
    {
        private readonly IRequestDecorator _requestDecorator;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="requestDecorator">Http request decorator</param>
        /// <param name="innerHandler">Inner message handler</param>
        public HttpRequestDecoratorHandler(IRequestDecorator requestDecorator, HttpMessageHandler innerHandler) : base(innerHandler)
        {
            _requestDecorator = requestDecorator;
        }

        /// <summary>
        /// Adds a unique request id to the request headers
        /// </summary>
        /// <param name="request">request being sent</param>
        /// <param name="cancellationToken">cancellation token</param>
        /// <returns></returns>
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            _requestDecorator.Decorate(request);

            return await base.SendAsync(request, cancellationToken);
        }
    }
}
