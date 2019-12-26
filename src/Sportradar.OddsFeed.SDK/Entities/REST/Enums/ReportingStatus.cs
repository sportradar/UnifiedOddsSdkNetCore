/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
namespace Sportradar.OddsFeed.SDK.Entities.REST.Enums
{
    /// <summary>
    /// An enum describing the reporting status of a sport event
    /// </summary>
    public enum ReportingStatus
    {
        /// <summary>
        /// The reporting status of the sport event is not available
        /// </summary>
        NotAvailable = 0,

        /// <summary>
        /// The reporting status of the sport event is currently live
        /// </summary>
        Live = 1,

        /// <summary>
        /// The reporting status of the sport event is suspended or temporary lost contact
        /// </summary>
        Suspended = -1,

        /// <summary>
        /// The reporting status of the sport event is unknown
        /// </summary>
        Unknown = 99
    }
}
