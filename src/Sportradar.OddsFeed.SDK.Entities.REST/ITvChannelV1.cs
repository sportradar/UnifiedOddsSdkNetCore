/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/

namespace Sportradar.OddsFeed.SDK.Entities.REST
{
    /// <summary>
    /// Defines a contract implemented by classes representing TV channels
    /// </summary>
    public interface ITvChannelV1 : ITvChannel
    {
        /// <summary>
        /// Gets the stream url of the channel represented by the current <see cref="ITvChannelV1"/> instance
        /// </summary>
        string StreamUrl { get; }
    }
}
