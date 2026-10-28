// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

namespace Sportradar.OddsFeed.SDK.Entities.Rest.Enums
{
    /// <summary>
    /// Indicates whether an each-way outcome is settled as a win or a place
    /// </summary>
    public enum EachWayResult
    {
        /// <summary>
        /// The outcome is settled as a winner/place
        /// </summary>
        WinnerPlace,

        /// <summary>
        /// The outcome is settled as a place
        /// </summary>
        Place,

        /// <summary>
        /// Indicating that the each-way result was present in message, but SDK does not support the each-way value
        /// </summary>
        UnsupportedBySdk
    }
}
