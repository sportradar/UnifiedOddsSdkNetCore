using System;
using Microsoft.Extensions.Logging;

namespace Sportradar.OddsFeed.SDK.Common
{
    /// <summary>
    /// Defines methods used to get or set various values for sdk operations
    /// </summary>
    public static class OperationManager
    {
        private static readonly ILogger InteractionLog = SdkLoggerFactory.GetLoggerForClientInteraction(typeof(OperationManager));

        /// <summary>
        /// Gets the sport event status cache timeout - how long status is cached
        /// </summary>
        /// <value>The sport event status cache timeout.</value>
        /// <remarks>Can be between 1 min and 60 min - default 5 min (absolute expiration)</remarks>
        public static TimeSpan SportEventStatusCacheTimeout { get; private set; }

        /// <summary>
        /// Gets the competitor/player cache timeout - how long cache item is cached
        /// </summary>
        /// <value>The competitor/player cache timeout.</value>
        /// <remarks>Can be between 1 hour and 48 hours - default 24 hours (sliding expiration)</remarks>
        public static TimeSpan ProfileCacheTimeout { get; private set; }

        /// <summary>
        /// Gets the variant market description cache timeout - how long cache item is cached
        /// </summary>
        /// <value>The variant market description cache timeout.</value>
        /// <remarks>Can be between 1 hour and 24 hours - default 3 hours (sliding expiration)</remarks>
        public static TimeSpan VariantMarketDescriptionCacheTimeout { get; private set; }

        /// <summary>
        /// Gets the ignore BetPal timeline sport event status cache timeout - how long cache item is cached. How long should the event id from BetPal producer be cached. SportEventStatus from timeline endpoint for these events are ignored.
        /// </summary>
        /// <value>The ignore BetPal timeline sport event status timeout.</value>
        /// <remarks>Can be between 1 hour and 24 hours - default 3 hours (sliding expiration)</remarks>
        public static TimeSpan IgnoreBetPalTimelineSportEventStatusCacheTimeout { get; private set; }

        /// <summary>
        /// Gets a value indicating whether to ignore sport event status from timeline endpoint for sport events on BetPal producer
        /// </summary>
        /// <value><c>true</c> if sport event status from timeline endpoint should be ignored, otherwise <c>false</c>.</value>
        /// <remarks>Default <c>false</c></remarks>
        public static bool IgnoreBetPalTimelineSportEventStatus { get; private set; }

        /// <summary>
        /// Initialization of default values of the <see cref="OperationManager"/>
        /// </summary>
        static OperationManager()
        {
            SportEventStatusCacheTimeout = TimeSpan.FromMinutes(5);
            ProfileCacheTimeout = TimeSpan.FromHours(24);
            VariantMarketDescriptionCacheTimeout = TimeSpan.FromHours(3);
            IgnoreBetPalTimelineSportEventStatusCacheTimeout = TimeSpan.FromHours(3);
            IgnoreBetPalTimelineSportEventStatus = false;
        }

        /// <summary>
        /// Sets the sport event status cache timeout
        /// </summary>
        /// <param name="timeout">The timeout.</param>
        public static void SetSportEventStatusCacheTimeout(TimeSpan timeout)
        {
            if (timeout >= TimeSpan.FromMinutes(1) && timeout <= TimeSpan.FromMinutes(60))
            {
                SportEventStatusCacheTimeout = timeout;
                InteractionLog.LogInformation($"Set SportEventStatusCacheTimeout to {timeout.TotalMinutes} min.");
                return;
            }

            throw new InvalidOperationException($"Invalid timeout value for SportEventStatusCacheTimeout: {timeout.TotalMinutes} min.");
        }

        /// <summary>
        /// Sets the profile cache timeout.
        /// </summary>
        /// <param name="timeout">The timeout.</param>
        public static void SetProfileCacheTimeout(TimeSpan timeout)
        {
            if (timeout >= TimeSpan.FromHours(1) && timeout <= TimeSpan.FromHours(48))
            {
                ProfileCacheTimeout = timeout;
                InteractionLog.LogInformation($"Set ProfileCacheTimeout to {timeout.TotalHours} hours.");
                return;
            }

            throw new InvalidOperationException($"Invalid timeout value for ProfileCacheTimeout: {timeout.TotalHours} hours.");
        }

        /// <summary>
        /// Sets the variant market description cache timeout
        /// </summary>
        /// <param name="timeout">The timeout.</param>
        public static void SetVariantMarketDescriptionCacheTimeout(TimeSpan timeout)
        {
            if (timeout >= TimeSpan.FromHours(1) && timeout <= TimeSpan.FromHours(24))
            { 
                VariantMarketDescriptionCacheTimeout = timeout;
                InteractionLog.LogInformation($"Set VariantMarketDescriptionCacheTimeout to {timeout.TotalHours} hours.");
                return;
            }

            throw new InvalidOperationException($"Invalid timeout value for VariantMarketDescriptionCacheTimeout: {timeout.TotalHours} hours.");
        }

        /// <summary>
        /// Sets the ignore BetPal timeline sport event status cache timeout. How long should the event id from BetPal be cached. SportEventStatus from timeline endpoint for these events are ignored.
        /// </summary>
        /// <param name="timeout">The timeout.</param>
        public static void SetIgnoreBetPalTimelineSportEventStatusCacheTimeout(TimeSpan timeout)
        {
            if (timeout >= TimeSpan.FromHours(1) && timeout <= TimeSpan.FromHours(24))
            { 
                IgnoreBetPalTimelineSportEventStatusCacheTimeout = timeout;
                InteractionLog.LogInformation($"Set IgnoreBetPalTimelineSportEventStatusCacheTimeout to {timeout.TotalHours} hours.");
                return;
            }

            throw new InvalidOperationException($"Invalid timeout value for IgnoreBetPalTimelineSportEventStatusCacheTimeout: {timeout.TotalHours} hours.");
        }

        /// <summary>
        /// Sets the value indicating whether to ignore sport event status from timeline endpoint for sport events on BetPal producer
        /// </summary>
        /// <param name="ignore">if set to <c>true</c> ignore</param>
        public static void SetIgnoreBetPalTimelineSportEventStatus(bool ignore)
        {
            IgnoreBetPalTimelineSportEventStatus = ignore;
            InteractionLog.LogInformation($"Set IgnoreBetPalTimelineSportEventStatus to {ignore}.");
        }
    }
}
