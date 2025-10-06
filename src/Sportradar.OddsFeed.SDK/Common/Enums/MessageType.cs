// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

namespace Sportradar.OddsFeed.SDK.Common.Enums
{
    /// <summary>
    /// Enumerates the types of the messages received from the feed
    /// </summary>
    public enum MessageType
    {
        /// <summary>
        /// Indicating the type of the message could not be determined
        /// </summary>
        Unknown,

        /// <summary>
        /// A message indicating that a producer associated with the feed went down
        /// </summary>
        ProducerDown,

        /// <summary>
        /// A message indicating all messages with a specific snapshot request were delivered
        /// </summary>
        SnapshotComplete,

        /// <summary>
        /// A message periodically send by all feed associated producers indicating their status
        /// </summary>
        Alive,

        /// <summary>
        /// A message indicating that a status of a specific fixture has changed
        /// </summary>
        FixtureChange,

        /// <summary>
        /// A message indicating that betting on specified markets should be stopped
        /// </summary>
        BetStop,

        /// <summary>
        /// A message indicating that bets placed on specified markets should be canceled
        /// </summary>
        BetCancel,

        /// <summary>
        /// A message specifying that changes made by the associated bet cancel should be un-done
        /// </summary>
        RollbackBetCancel,

        /// <summary>
        /// A message specifying that bets associated with specified markets should be settled
        /// </summary>
        BetSettlement,

        /// <summary>
        /// A message specifying that changes made by the associated bet settlement should be un-done
        /// </summary>
        RollbackBetSettlement,

        /// <summary>
        /// A message specifying that odds for specified markets have changed
        /// </summary>
        OddsChange
    }
}
