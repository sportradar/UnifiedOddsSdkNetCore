// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System.Net.Http;

namespace Sportradar.OddsFeed.SDK.Api.Internal
{
    /// <summary>
    /// Request decorator used to decorate the <see cref="HttpRequestMessage"/> before it is sent
    /// </summary>
    public interface IRequestDecorator
    {
        /// <summary>
        /// Decorates the provided <see cref="HttpRequestMessage"/>
        /// </summary>
        /// <param name="request">Http request</param>
        void Decorate(HttpRequestMessage request);
    }
}
