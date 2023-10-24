/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/

using System;

namespace Sportradar.OddsFeed.SDK.Api.Config
{
    /// <summary>
    /// Defines a contract implemented by classes representing sdk internal caches configuration / settings
    /// </summary>
    public interface IUofCacheConfiguration
    {
        /// <summary>
        /// Gets the sport event cache timeout - how long cache item is cached
        /// </summary>
        /// <value>The event cache timeout.</value>
        /// <remarks>Can be between 1 hour and 48 hours - default 12 hours (sliding expiration)</remarks>
        TimeSpan SportEventCacheTimeout { get; }

        /// <summary>
        /// Gets the sport event status cache timeout - how long status is cached
        /// </summary>
        /// <value>The sport event status cache timeout.</value>
        /// <remarks>Can be between 1 min and 60 min - default 5 min (absolute expiration)</remarks>
        TimeSpan SportEventStatusCacheTimeout { get; }

        /// <summary>
        /// Gets the competitor/player cache timeout - how long cache item is cached
        /// </summary>
        /// <value>The competitor/player cache timeout.</value>
        /// <remarks>Can be between 1 hour and 48 hours - default 24 hours (sliding expiration)</remarks>
        TimeSpan ProfileCacheTimeout { get; }

        /// <summary>
        /// Gets the variant market description cache timeout - how long cache item is cached
        /// </summary>
        /// <value>The variant market description cache timeout.</value>
        /// <remarks>Can be between 1 hour and 24 hours - default 3 hours (sliding expiration)</remarks>
        TimeSpan VariantMarketDescriptionCacheTimeout { get; }

        /// <summary>
        /// Gets the ignore BetPal timeline sport event status cache timeout - how long cache item is cached. How long should the event id from BetPal producer be cached. SportEventStatus from timeline endpoint for these events are ignored.
        /// </summary>
        /// <value>The ignore BetPal timeline sport event status timeout.</value>
        /// <remarks>Can be between 1 hour and 24 hours - default 3 hours (sliding expiration)</remarks>
        TimeSpan IgnoreBetPalTimelineSportEventStatusCacheTimeout { get; }

        /// <summary>
        /// Gets a value indicating whether to ignore sport event status from timeline endpoint for sport events on BetPal producer
        /// </summary>
        /// <value><c>true</c> if sport event status from timeline endpoint should be ignored, otherwise <c>false</c>.</value>
        /// <remarks>Default <c>false</c></remarks>
        bool IgnoreBetPalTimelineSportEventStatus { get; }
    }
}
