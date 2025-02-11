// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

namespace Sportradar.OddsFeed.SDK.Api.Config
{
    /// <summary>
    /// Defines a contract implemented by classes used to set recovery related configuration properties
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IRecoveryConfigurationBuilder<out T> : IConfigurationBuilderBase<T>
    {
        /// <summary>
        /// Sets the max time window between two consecutive alive messages before the associated producer is marked as down
        /// </summary>
        /// <param name="inactivitySeconds">the max time window between two consecutive alive messages</param>
        /// <returns>A <see cref="IRecoveryConfigurationBuilder{T}"/> derived instance used to set general configuration properties</returns>
        T SetInactivitySeconds(int inactivitySeconds);

        /// <summary>
        /// Sets the max time window between two consecutive alive messages before the associated producer is marked as down (for prematch producer)
        /// </summary>
        /// <param name="inactivitySecondsPrematch">the max time window between two consecutive alive messages</param>
        /// <returns>A <see cref="IRecoveryConfigurationBuilder{T}"/> derived instance used to set general configuration properties</returns>
        T SetInactivitySecondsPrematch(int inactivitySecondsPrematch);

        /// <summary>
        /// Sets the maximum time in seconds in which recovery must be completed (minimum 600 seconds)
        /// </summary>
        /// <param name="maxRecoveryTimeInSeconds">Maximum recovery time in seconds</param>
        /// <returns>A <see cref="IRecoveryConfigurationBuilder{T}"/> instance used to set general configuration properties</returns>
        T SetMaxRecoveryTime(int maxRecoveryTimeInSeconds);

        /// <summary>
        /// Sets the minimal interval between recovery requests initiated by alive messages (between 20 and 180 seconds)
        /// </summary>
        /// <param name="minIntervalBetweenRecoveryRequests">The minimal interval between recovery requests initiated by alive messages (seconds)</param>
        /// <returns>A <see cref="IRecoveryConfigurationBuilder{T}" /> instance used to set general configuration properties</returns>
        T SetMinIntervalBetweenRecoveryRequests(int minIntervalBetweenRecoveryRequests);

        /// <summary>
        /// Sets the timeout for HTTP requests for this instance of the sdk
        /// </summary>
        /// <param name="httpClientTimeout">The timeout for recovery HTTP requests</param>
        /// <returns>A <see cref="IRecoveryConfigurationBuilder{T}"/> derived instance used to set general configuration properties</returns>
        T SetHttpClientTimeout(int httpClientTimeout);

        /// <summary>
        /// Sets the timeout for recovery HTTP requests for this instance of the sdk
        /// </summary>
        /// <param name="httpClientRecoveryTimeout">The timeout for recovery HTTP requests</param>
        /// <returns>A <see cref="IRecoveryConfigurationBuilder{T}"/> derived instance used to set general configuration properties</returns>
        T SetHttpClientRecoveryTimeout(int httpClientRecoveryTimeout);

        /// <summary>
        /// Sets a timeout for HttpClient for fast api request (in seconds).
        /// </summary>
        /// <param name="httpClientFastFailingTimeout">The timeout to be set (in seconds)</param>
        /// <remarks>Between 1 and 30 (default 5s) - set before connection is made.</remarks>
        /// <returns>A <see cref="IRecoveryConfigurationBuilder{T}"/> derived instance used to set general configuration properties</returns>
        T SetHttpClientFastFailingTimeout(int httpClientFastFailingTimeout);

        /// <summary>
        /// Sets the maximum number of concurrent connections (per server endpoint) allowed by an HttpClient object. (default: int.Max)
        /// </summary>
        /// <param name="maxConnectionsPerServer">The new maximum number of concurrent connections (per server endpoint) allowed by an HttpClient object.</param>
        /// <returns>A <see cref="IRecoveryConfigurationBuilder{T}"/> derived instance used to set general configuration properties</returns>
        T SetMaxConnectionsPerServer(int maxConnectionsPerServer);

        /// <summary>
        /// Sets the timeout for cache items in SportEventCache (in hours)
        /// </summary>
        /// <param name="timeoutInHours">The timeout.</param>
        /// <remarks>Can be between 1 hour and 48 hours - default 12 hours (sliding expiration)</remarks>
        /// <returns>A <see cref="IRecoveryConfigurationBuilder{T}"/> derived instance used to set general configuration properties</returns>
        T SetSportEventCacheTimeout(int timeoutInHours);

        /// <summary>
        /// Sets the timeout for cache items in SportEventCacheStatus (in minutes)
        /// </summary>
        /// <param name="timeoutInMinutes">The timeout.</param>
        /// <returns>A <see cref="IRecoveryConfigurationBuilder{T}"/> derived instance used to set general configuration properties</returns>
        T SetSportEventStatusCacheTimeout(int timeoutInMinutes);

        /// <summary>
        /// Sets the profile cache timeout.
        /// </summary>
        /// <param name="timeoutInHours">The timeout.</param>
        /// <returns>A <see cref="IRecoveryConfigurationBuilder{T}"/> derived instance used to set general configuration properties</returns>
        T SetProfileCacheTimeout(int timeoutInHours);

        /// <summary>
        /// Sets the timeout for cache items in  variant market description cache (in hours)
        /// </summary>
        /// <param name="timeoutInHours">The timeout.</param>
        /// <returns>A <see cref="IRecoveryConfigurationBuilder{T}"/> derived instance used to set general configuration properties</returns>
        T SetVariantMarketDescriptionCacheTimeout(int timeoutInHours);

        /// <summary>
        /// Sets the ignore BetPal timeline sport event status cache timeout. How long should the event id from BetPal be cached. SportEventStatus from timeline endpoint for these events are ignored.
        /// </summary>
        /// <param name="timeoutInHours">The timeout.</param>
        /// <returns>A <see cref="IRecoveryConfigurationBuilder{T}"/> derived instance used to set general configuration properties</returns>
        T SetIgnoreBetPalTimelineSportEventStatusCacheTimeout(int timeoutInHours);

        /// <summary>
        /// Sets the value indicating whether to ignore sport event status from timeline endpoint for sport events on BetPal producer
        /// </summary>
        /// <param name="ignore">if set to <c>true</c> ignore</param>
        /// <returns>A <see cref="IRecoveryConfigurationBuilder{T}"/> derived instance used to set general configuration properties</returns>
        T SetIgnoreBetPalTimelineSportEventStatus(bool ignore);

        /// <summary>
        /// Sets the rabbit timeout setting for connection attempts (in seconds)
        /// </summary>
        /// <param name="rabbitConnectionTimeout">The rabbit timeout setting for connection attempts (in seconds)</param>
        /// <remarks>Between 10 and 120 (default 30s) - set before connection is made</remarks>
        /// <returns>A <see cref="IRecoveryConfigurationBuilder{T}"/> derived instance used to set general configuration properties</returns>
        T SetRabbitConnectionTimeout(int rabbitConnectionTimeout);

        /// <summary>
        /// Sets a heartbeat timeout to use when negotiating with the rabbit server (in seconds)
        /// </summary>
        /// <param name="heartbeatInSeconds">The heartbeat timeout to use when negotiating with the rabbit server (in seconds)</param>
        /// <remarks>Between 10 and 180 (default 60s) - set before connection is made</remarks>
        /// <returns>A <see cref="IRecoveryConfigurationBuilder{T}"/> derived instance used to set general configuration properties</returns>
        T SetRabbitHeartbeat(int heartbeatInSeconds);

        /// <summary>
        /// Sets the interval for automatically collecting statistics (in minutes)
        /// </summary>
        /// <remarks>Setting to 0 indicates it is disabled</remarks>
        /// <param name="intervalInMinutes">The timeout for automatically collecting statistics (in minutes)</param>
        /// <returns>The <see cref="IRecoveryConfigurationBuilder{T}"/> instance used to set additional values</returns>
        T SetStatisticsInterval(int intervalInMinutes);

        /// <summary>
        /// Sets the value indicating whether to ignore market mappings when fetching market descriptions from API
        /// </summary>
        /// <param name="omit">if set to <c>true</c> omit, otherwise include market mapping data</param>
        /// <returns>A <see cref="IRecoveryConfigurationBuilder{T}"/> derived instance used to set general configuration properties</returns>
        T OmitMarketMappings(bool omit);

        /// <summary>
        /// Sets the value indicating whether sdk usage (metrics) can be exported
        /// </summary>
        /// <param name="enableUsageExport">if set to <c>true</c> export is permitted, otherwise not</param>
        /// <returns>A <see cref="IRecoveryConfigurationBuilder{T}"/> derived instance used to set general configuration properties</returns>
        T EnableUsageExport(bool enableUsageExport);
    }
}
