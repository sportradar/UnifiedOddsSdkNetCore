/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using Sportradar.OddsFeed.SDK.Messages;

namespace Sportradar.OddsFeed.SDK.API
{
    /// <summary>
    /// Defines a contract for classes implementing booking manager to perform various booking calendar operations
    /// </summary>
    public interface IBookingManager
    {
        /// <summary>
        /// Books the live odds event associated with the provided {@link URN} identifier
        /// </summary>
        /// <param name="eventId">The event id</param>
        /// <returns><c>true</c> if event was successfully booked, <c>false</c> otherwise</returns>
        bool BookLiveOddsEvent(URN eventId);
    }
}
