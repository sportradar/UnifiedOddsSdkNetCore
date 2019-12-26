/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/

// ReSharper disable InconsistentNaming
namespace Sportradar.OddsFeed.SDK.Entities.REST.Enums
{
    /// <summary>
    /// Enumerates values providing information on the booking status of an event
    /// </summary>
    public enum BookingStatus
    {
        /// <summary>
        /// Indicates if the associated event is buyable
        /// </summary>
        Buyable,

        /// <summary>
        /// Indicates that the associated event is not booked and information associated with it will not be provided, but the event could be booked
        /// </summary>
        Bookable,

        /// <summary>
        /// Indicates that the associated event is booked and information associated with it will be provided
        /// </summary>
        Booked,

        /// <summary>
        /// Indicates that the associated event is not available for booking
        /// </summary>
        Unavailable
    }
}
