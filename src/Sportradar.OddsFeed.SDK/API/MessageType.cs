/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/

// ReSharper disable InconsistentNaming
namespace Sportradar.OddsFeed.SDK.API
{
    /// <summary>
    /// Enumerates the types of the messages received from the feed
    /// </summary>
    public enum MessageType
    {
        /// <summary>
        /// Indicating the type of the message could not be determined
        /// </summary>
        UNKNOWN,

        /// <summary>
        /// A message indicating that a producer associated with the feed went down
        /// </summary>
        PRODUCER_DOWN,

        /// <summary>
        /// A message indicating all messages with a specific snapshot request were delivered
        /// </summary>
        SNAPSHOT_COMPLETE,

        /// <summary>
        /// A message periodically send by all feed associated producers indicating their status
        /// </summary>
        ALIVE,

        /// <summary>
        /// A message indicating that a status of a specific fixture has changed
        /// </summary>
        FIXTURE_CHANGE,

        /// <summary>
        /// A message indicating that betting on specified markets should be stopped
        /// </summary>
        BET_STOP,

        /// <summary>
        /// A message indicating that bets placed on specified markets should be canceled
        /// </summary>
        BET_CANCEL,

        /// <summary>
        /// A message specifying that changes made by the associated bet cancel should be un-done
        /// </summary>
        ROLLBACK_BET_CANCEL,

        /// <summary>
        /// A message specifying that bets associated with specified markets should be settled
        /// </summary>
        BET_SETTLEMENT,

        /// <summary>
        /// A message specifying that changes made by the associated bet settlement should be un-done
        /// </summary>
        ROLLBACK_BET_SETTLEMENT,

        /// <summary>
        /// A message specifying that odds for specified markets have changed
        /// </summary>
        ODDS_CHANGE
    }
}