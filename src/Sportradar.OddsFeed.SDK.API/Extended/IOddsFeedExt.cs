/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/

using System;
using Sportradar.OddsFeed.SDK.Messages.EventArguments;

namespace Sportradar.OddsFeed.SDK.API.Extended
{
    /// <summary>
    /// Represent an extended unified odds feed
    /// </summary>
    public interface IOddsFeedExt : IOddsFeedV2
    {
        /// <summary>
        /// Occurs when any feed message arrives
        /// </summary>
        event EventHandler<RawFeedMessageEventArgs> RawFeedMessageReceived;

        /// <summary>
        /// Occurs when data from Sports API arrives
        /// </summary>
        event EventHandler<RawApiDataEventArgs> RawApiDataReceived;
    }
}