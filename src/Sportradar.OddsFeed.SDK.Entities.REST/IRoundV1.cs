/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/

using Sportradar.OddsFeed.SDK.Messages;

namespace Sportradar.OddsFeed.SDK.Entities.REST
{
    /// <summary>
    /// Defines a contract implemented by classes providing basic tournament round information
    /// </summary>
    public interface IRoundV1 : IRound
    {
        /// <summary>
        /// Gets the id of the group associated with the current round
        /// </summary>
        URN GroupId { get; }
    }
}