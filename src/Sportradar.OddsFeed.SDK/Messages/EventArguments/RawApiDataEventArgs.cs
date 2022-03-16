/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using Sportradar.OddsFeed.SDK.Messages.REST;
using System;

// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace Sportradar.OddsFeed.SDK.Messages.EventArguments
{
    /// <summary>
    /// Event arguments for the RawApiDataReceived events
    /// </summary>
    public class RawApiDataEventArgs : EventArgs
    {
        /// <summary>
        /// The associated event identifier
        /// </summary>
        public Uri Uri { get; }

        /// <summary>
        /// The rest message
        /// </summary>
        public RestMessage RestMessage { get; }

        /// <summary>
        /// The parameters associated with the request (if present)
        /// </summary>
        public string RequestParams { get; }

        /// <summary>
        ///The time it took for the request to execute
        /// </summary>
        public TimeSpan RequestTime { get; }

        /// <summary>
        /// The language associated with the request
        /// </summary>
        public string Language { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="RawApiDataEventArgs" /> class
        /// </summary>
        /// <param name="uri">The <see cref="Uri"/> used to get the api data</param>
        /// <param name="restMessage">The rest message</param>
        /// <param name="requestParams">The parameters associated with the request</param>
        /// <param name="requestTime">The time needed to execute request</param>
        /// <param name="language">The language associated with the request</param>
        public RawApiDataEventArgs(Uri uri, RestMessage restMessage, string requestParams, TimeSpan requestTime, string language)
        {
            Uri = uri;
            RestMessage = restMessage;
            RequestParams = requestParams;
            RequestTime = requestTime;
            Language = language;
        }
    }
}
