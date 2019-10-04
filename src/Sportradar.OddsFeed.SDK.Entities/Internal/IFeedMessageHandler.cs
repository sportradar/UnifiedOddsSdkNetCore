/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/

using Sportradar.OddsFeed.SDK.Messages.Feed;

namespace Sportradar.OddsFeed.SDK.Entities.Internal
{
    /// <summary>
    /// Defines a contract for handling special cases for processing feed messages
    /// </summary>
    public interface IFeedMessageHandler
    {
        /// <summary>
        /// Stops the processing of fixture change if the same message was already processed
        /// </summary>
        /// <param name="fixtureChange">The fixture change</param>
        /// <returns><c>true</c> if already processed then <c>true</c>, <c>false</c> otherwise</returns>
        bool StopProcessingFixtureChange(fixture_change fixtureChange);
    }
}
