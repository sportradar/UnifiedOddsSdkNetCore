// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using Microsoft.Extensions.Logging;
using Sportradar.OddsFeed.SDK.Common.Internal;
using Sportradar.OddsFeed.SDK.Messages.Rest;

namespace Sportradar.OddsFeed.SDK.Api.Internal.ApiAccess
{
    /// <summary>
    /// A implementation of <see cref="IDataFetcher"/> and <see cref="IDataPoster"/> which uses the HTTP requests to fetch or post the requested data. All request are logged.
    /// </summary>
    /// <seealso cref="IDataFetcher" />
    /// <remarks>ALL, DEBUG, INFO, WARN, ERROR, FATAL, OFF - the levels are defined in order of increasing priority</remarks>
    internal class LogHttpDataFetcherRecovery : LogHttpDataFetcher, ILogHttpDataFetcherRecovery
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LogHttpDataFetcherRecovery"/> class.
        /// </summary>
        /// <param name="sdkHttpClient">A <see cref="ISdkHttpClientRecovery"/> used to invoke HTTP requests</param>
        /// <param name="responseDeserializer">The deserializer for unexpected response</param>
        /// <param name="logger">Logger to log rest requests</param>
        /// <param name="connectionFailureLimit">Indicates the limit of consecutive request failures, after which it goes in "blocking mode"</param>
        /// <param name="connectionFailureTimeout">indicates the timeout after which comes out of "blocking mode" (in seconds)</param>
        public LogHttpDataFetcherRecovery(ISdkHttpClientRecovery sdkHttpClient,
                                        IDeserializer<response> responseDeserializer,
                                        ILogger logger,
                                        int connectionFailureLimit = 5,
                                        int connectionFailureTimeout = 15)
            : base(sdkHttpClient, responseDeserializer, logger, connectionFailureLimit, connectionFailureTimeout)
        {
        }
    }
}
