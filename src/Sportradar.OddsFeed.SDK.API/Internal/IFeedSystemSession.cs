/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Entities.Internal.EventArguments;

namespace Sportradar.OddsFeed.SDK.API.Internal
{
    /// <summary>
    /// Specifies a contract defining events used for notification from system session
    /// </summary>
    internal interface IFeedSystemSession : IOpenable, IDisposable
    {
        /// <summary>
        /// Raised when a alive message is received from the feed
        /// </summary>
        event EventHandler<FeedMessageReceivedEventArgs> AliveReceived;
    }
}
