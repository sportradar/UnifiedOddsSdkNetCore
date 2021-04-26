/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using Sportradar.OddsFeed.SDK.Messages.REST;

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
        /// Initializes a new instance of the <see cref="RawApiDataEventArgs" /> class
        /// </summary>
        /// <param name="uri">The <see cref="Uri"/> used to get the api data</param>
        /// <param name="restMessage">The rest message</param>
        public RawApiDataEventArgs(Uri uri, RestMessage restMessage)
        {
            Uri = uri;
            RestMessage = restMessage;
        }
    }
}
