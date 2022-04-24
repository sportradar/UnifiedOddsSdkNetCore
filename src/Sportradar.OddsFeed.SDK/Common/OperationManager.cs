using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using System;

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
        /// Gets a rabbit timeout setting for connection attempts (in seconds).
        /// </summary>
        /// <value>A rabbit timeout setting for connection attempts (in seconds).</value>
        /// <remarks>Between 10 and 120 (default 30s)</remarks>
        public static int RabbitConnectionTimeout { get; private set; }

        /// <summary>
        /// Gets a heartbeat timeout to use when negotiating with the server (in seconds).
        /// </summary>
        /// <value>A heartbeat timeout to use when negotiating with the server (in seconds).</value>
        /// <remarks>Between 10 and 180 (default 60s)</remarks>
        public static int RabbitHeartbeat { get; private set; }

        /// <summary>
        /// Gets a timeout for HttpClient for fast api request (in seconds).
        /// </summary>
        /// <value>A timeout for HttpClient for fast api request (in seconds).</value>
        /// <remarks>Between 1 and 30 (default 5s). Used for summary, competitor profile, player profile and variant description endpoint. Must be set before feed instance is created.</remarks>
        public static TimeSpan FastHttpClientTimeout { get; private set; }

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
            RabbitConnectionTimeout = ConnectionFactory.DefaultConnectionTimeout / 1000;
            RabbitHeartbeat = ConnectionFactory.DefaultHeartbeat;
            FastHttpClientTimeout = TimeSpan.FromSeconds(5);
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

        /// <summary>
        /// Sets the rabbit timeout setting for connection attempts (in seconds).
        /// </summary>
        /// <param name="rabbitConnectionTimeout">The rabbit timeout setting for connection attempts (in seconds)</param>
        /// <remarks>Between 10 and 120 (default 30s) - set before connection is made</remarks>
        public static void SetRabbitConnectionTimeout(int rabbitConnectionTimeout)
        {
            if (rabbitConnectionTimeout >= 10 && rabbitConnectionTimeout <= 120)
            {
                RabbitConnectionTimeout = rabbitConnectionTimeout;
                InteractionLog.LogInformation($"Set RabbitConnectionTimeout to {rabbitConnectionTimeout}s.");
            }
        }

        /// <summary>
        /// Sets a heartbeat timeout to use when negotiating with the rabbit server (in seconds).
        /// </summary>
        /// <param name="rabbitHeartbeat">The heartbeat timeout to use when negotiating with the rabbit server (in seconds).</param>
        /// <remarks>Between 10 and 180 (default 60s) - set before connection is made</remarks>
        public static void SetRabbitHeartbeat(int rabbitHeartbeat)
        {
            if (rabbitHeartbeat >= 10 && rabbitHeartbeat <= 120)
            {
                RabbitHeartbeat = rabbitHeartbeat;
                InteractionLog.LogInformation($"Set RabbitHeartbeat to {rabbitHeartbeat}s.");
            }
        }

        /// <summary>
        /// Sets a timeout for HttpClient for fast api request (in seconds).
        /// </summary>
        /// <param name="timeout">The timeout to be set</param>
        /// /// <remarks>Between 1 and 30 (default 5s) - set before connection is made.</remarks>
        public static void SetFastHttpClientTimeout(TimeSpan timeout)
        {
            if (timeout >= TimeSpan.FromSeconds(1) && timeout <= TimeSpan.FromSeconds(30))
            {
                FastHttpClientTimeout = timeout;
                InteractionLog.LogInformation($"Set FastHttpClientTimeout to {timeout.TotalSeconds} seconds.");
                return;
            }

            throw new InvalidOperationException($"Invalid timeout value for FastHttpClientTimeout: {timeout.TotalSeconds} seconds.");
        }
    }
}
