// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;

namespace Sportradar.OddsFeed.SDK.Api.Config
{
    /// <summary>
    /// Defines a contract implemented by classes representing api connection configuration / settings
    /// </summary>
    public interface IUofApiConfiguration
    {
        /// <summary>
        /// Gets a host name of the Sports API
        /// </summary>
        string Host { get; }

        /// <summary>
        /// Gets the representation of Sports API URI
        /// </summary>
        string BaseUrl { get; }

        /// <summary>
        /// Gets a value indicating whether the connection to Sports API should use SSL
        /// </summary>
        bool UseSsl { get; }

        /// <summary>
        /// Gets a value specifying timeout set for HTTP requests
        /// </summary>
        TimeSpan HttpClientTimeout { get; }

        /// <summary>
        /// Gets a value specifying timeout set for HTTP request for recovery endpoints
        /// </summary>
        TimeSpan HttpClientRecoveryTimeout { get; }

        /// <summary>
        /// Gets a value specifying timeout set for fast failing HTTP requests
        /// </summary>
        /// <remarks>Applies for API calls to endpoints: summary, competitor and player profile, draw summary, single variant markets</remarks>
        TimeSpan HttpClientFastFailingTimeout { get; }

        /// <summary>
        /// Gets the URL of the feed's Replay Server REST interface
        /// </summary>
        string ReplayHost { get; }

        /// <summary>
        /// Gets a representation of Replay Server API base url
        /// </summary>
        string ReplayBaseUrl { get; }

        /// <summary>
        /// Gets the maximum number of concurrent connections (per server endpoint) allowed by an HttpClientHandler object. (default: int.Max)
        /// </summary>
        int MaxConnectionsPerServer { get; }
    }
}
