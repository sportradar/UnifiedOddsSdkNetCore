/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using Sportradar.OddsFeed.SDK.Api.EventArguments;

namespace Sportradar.OddsFeed.SDK.Api.Extended
{
    /// <summary>
    /// Represent an extended unified odds sdk
    /// </summary>
    public interface IUofSdkExtended : IUofSdk
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
