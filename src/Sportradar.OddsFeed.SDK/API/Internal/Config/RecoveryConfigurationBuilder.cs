/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using System.Diagnostics.CodeAnalysis;
using Sportradar.OddsFeed.SDK.Api.Config;

namespace Sportradar.OddsFeed.SDK.Api.Internal.Config
{
    /// <summary>
    /// Class RecoveryConfigurationBuilder
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <seealso cref="ConfigurationBuilderBase{T}" />
    /// <seealso cref="IRecoveryConfigurationBuilder{T}" />
    [SuppressMessage("ReSharper", "ComplexConditionExpression")]
    internal abstract class RecoveryConfigurationBuilder<T> : ConfigurationBuilderBase<T>, IRecoveryConfigurationBuilder<T> where T : class
    {
        /// <summary>
        /// Construct RecoveryConfigurationBuilder
        /// </summary>
        /// <param name="configuration">Current <see cref="IUofConfiguration"/></param>
        /// <param name="sectionProvider">A <see cref="IUofConfigurationSectionProvider"/> used to access <see cref="IUofConfigurationSection"/></param>
        /// <param name="bookmakerDetailsProvider">Provider for bookmaker details</param>
        /// <param name="producersProvider">Provider for available producers</param>
        internal RecoveryConfigurationBuilder(UofConfiguration configuration,
            IUofConfigurationSectionProvider sectionProvider,
            IBookmakerDetailsProvider bookmakerDetailsProvider,
            IProducersProvider producersProvider)
            : base(configuration, sectionProvider, bookmakerDetailsProvider, producersProvider)
        {
        }

        /// <summary>
        /// Sets the max time window between two consecutive alive messages before the associated producer is marked as down
        /// </summary>
        /// <param name="inactivitySeconds">the max time window between two consecutive alive messages</param>
        /// <returns>A <see cref="IRecoveryConfigurationBuilder{T}" /> derived instance used to set general configuration properties</returns>
        public T SetInactivitySeconds(int inactivitySeconds)
        {
            if (inactivitySeconds < ConfigLimit.InactivitySecondsMin)
            {
                throw new ArgumentException($"Value must be at least {ConfigLimit.InactivitySecondsMin}.");
            }
            if (inactivitySeconds > ConfigLimit.InactivitySecondsMax)
            {
                throw new ArgumentException($"Value must be less then {ConfigLimit.InactivitySecondsMax}.");
            }

            var producerConfig = (UofProducerConfiguration)UofConfiguration.Producer;
            producerConfig.InactivitySeconds = TimeSpan.FromSeconds(inactivitySeconds);
            UofConfiguration.Producer = producerConfig;
            return this as T;
        }

        public T SetInactivitySecondsPrematch(int inactivitySecondsPrematch)
        {
            if (inactivitySecondsPrematch < ConfigLimit.InactivitySecondsPrematchMin)
            {
                throw new ArgumentException($"Value must be at least {ConfigLimit.InactivitySecondsPrematchMin}.");
            }
            if (inactivitySecondsPrematch > ConfigLimit.InactivitySecondsPrematchMax)
            {
                throw new ArgumentException($"Value must be less then {ConfigLimit.InactivitySecondsPrematchMax}.");
            }

            var producerConfig = (UofProducerConfiguration)UofConfiguration.Producer;
            producerConfig.InactivitySecondsPrematch = TimeSpan.FromSeconds(inactivitySecondsPrematch);
            UofConfiguration.Producer = producerConfig;
            return this as T;
        }

        /// <summary>
        /// Sets the maximum time in seconds in which recovery must be completed (minimum 600 seconds)
        /// </summary>
        /// <param name="maxRecoveryTimeInSeconds">Maximum recovery time in seconds</param>
        /// <returns>A <see cref="IRecoveryConfigurationBuilder{T}" /> instance used to set general configuration properties</returns>
        public T SetMaxRecoveryTime(int maxRecoveryTimeInSeconds)
        {
            if (maxRecoveryTimeInSeconds < ConfigLimit.MaxRecoveryTimeMin)
            {
                throw new ArgumentException($"Value must be at least {ConfigLimit.MaxRecoveryTimeMin}.");
            }
            if (maxRecoveryTimeInSeconds > ConfigLimit.MaxRecoveryTimeMax)
            {
                throw new ArgumentException($"Value must be less then {ConfigLimit.MaxRecoveryTimeMax}.");
            }

            var producerConfig = (UofProducerConfiguration)UofConfiguration.Producer;
            producerConfig.MaxRecoveryTime = TimeSpan.FromSeconds(maxRecoveryTimeInSeconds);
            UofConfiguration.Producer = producerConfig;
            return this as T;
        }

        /// <summary>
        /// Sets the minimal interval between recovery requests initiated by alive messages (between 20 and 180 seconds)
        /// </summary>
        /// <param name="minIntervalBetweenRecoveryRequests">The minimal interval between recovery requests initiated by alive messages (seconds)</param>
        /// <returns>A <see cref="IRecoveryConfigurationBuilder{T}" /> instance used to set general configuration properties</returns>
        public T SetMinIntervalBetweenRecoveryRequests(int minIntervalBetweenRecoveryRequests)
        {
            if (minIntervalBetweenRecoveryRequests < ConfigLimit.MinIntervalBetweenRecoveryRequestMin)
            {
                throw new ArgumentException($"Value must be at least {ConfigLimit.MinIntervalBetweenRecoveryRequestMin}.");
            }
            if (minIntervalBetweenRecoveryRequests > ConfigLimit.MinIntervalBetweenRecoveryRequestMax)
            {
                throw new ArgumentException($"Value must be less then {ConfigLimit.MinIntervalBetweenRecoveryRequestMax}.");
            }

            var producerConfig = (UofProducerConfiguration)UofConfiguration.Producer;
            producerConfig.MinIntervalBetweenRecoveryRequests = TimeSpan.FromSeconds(minIntervalBetweenRecoveryRequests);
            UofConfiguration.Producer = producerConfig;
            return this as T;
        }

        /// <summary>
        /// Sets the value indicating whether the after age should be enforced before executing recovery request
        /// </summary>
        /// <param name="adjustAfterAge">True if age should be enforced; False otherwise</param>
        /// <returns>The <see cref="IRecoveryConfigurationBuilder{T}"/> instance used to set additional values</returns>
        public T SetAdjustAfterAge(bool adjustAfterAge)
        {
            var producerConfig = (UofProducerConfiguration)UofConfiguration.Producer;
            producerConfig.AdjustAfterAge = adjustAfterAge;
            UofConfiguration.Producer = producerConfig;
            return this as T;
        }

        /// <summary>
        /// Sets the timeout for HTTP responses for this instance of the sdk
        /// </summary>
        /// <param name="httpClientTimeout">The timeout for HTTP responses</param>
        /// <returns>A <see cref="IConfigurationBuilderBase{T}"/> derived instance used to set general configuration properties</returns>
        public T SetHttpClientTimeout(int httpClientTimeout)
        {
            if (httpClientTimeout >= ConfigLimit.HttpClientTimeoutMin && httpClientTimeout <= ConfigLimit.HttpClientTimeoutMax)
            {
                var apiConfig = (UofApiConfiguration)UofConfiguration.Api;
                apiConfig.HttpClientTimeout = TimeSpan.FromSeconds(httpClientTimeout);
                UofConfiguration.Api = apiConfig;
                return this as T;
            }
            throw new ArgumentException($"Invalid timeout value for HttpClientTimeout: {httpClientTimeout} seconds.");
        }

        /// <summary>
        /// Sets the timeout for recovery HTTP responses for this instance of the sdk
        /// </summary>
        /// <param name="httpClientRecoveryTimeout">The timeout for recovery HTTP responses</param>
        /// <returns>A <see cref="IConfigurationBuilderBase{T}"/> derived instance used to set general configuration properties</returns>
        public T SetHttpClientRecoveryTimeout(int httpClientRecoveryTimeout)
        {
            if (httpClientRecoveryTimeout >= ConfigLimit.HttpClientRecoveryTimeoutMin && httpClientRecoveryTimeout <= ConfigLimit.HttpClientRecoveryTimeoutMax)
            {
                var apiConfig = (UofApiConfiguration)UofConfiguration.Api;
                apiConfig.HttpClientRecoveryTimeout = TimeSpan.FromSeconds(httpClientRecoveryTimeout);
                UofConfiguration.Api = apiConfig;
                return this as T;
            }
            throw new ArgumentException($"Invalid timeout value for HttpClientRecoveryTimeout: {httpClientRecoveryTimeout} seconds.");
        }
        /// <summary>
        /// Sets a timeout for HttpClient for fast api request (in seconds).
        /// </summary>
        /// <param name="httpClientFastFailingTimeout">The timeout to be set (in seconds)</param>
        /// <remarks>Between 1 and 30 (default 5s) - set before connection is made.</remarks>
        public T SetHttpClientFastFailingTimeout(int httpClientFastFailingTimeout)
        {
            if (httpClientFastFailingTimeout >= ConfigLimit.HttpClientFastFailingTimeoutMin && httpClientFastFailingTimeout <= ConfigLimit.HttpClientFastFailingTimeoutMax)
            {
                var apiConfig = (UofApiConfiguration)UofConfiguration.Api;
                apiConfig.HttpClientFastFailingTimeout = TimeSpan.FromSeconds(httpClientFastFailingTimeout);
                UofConfiguration.Api = apiConfig;
                return this as T;
            }

            throw new ArgumentException($"Invalid timeout value for HttpClientFastFailingTimeout: {httpClientFastFailingTimeout} seconds.");
        }

        /// <summary>
        /// Sets the maximum number of concurrent connections (per server endpoint) allowed by an HttpClient object. (default: int.Max)
        /// </summary>
        /// <param name="maxConnectionsPerServer">The new maximum number of concurrent connections (per server endpoint) allowed by an HttpClient object.</param>
        public T SetMaxConnectionsPerServer(int maxConnectionsPerServer)
        {
            if (maxConnectionsPerServer > 0)
            {
                ((UofApiConfiguration)UofConfiguration.Api).MaxConnectionsPerServer = maxConnectionsPerServer;
                return this as T;
            }

            throw new ArgumentException($"Invalid value for MaxConnectionsPerServer: {maxConnectionsPerServer}.");
        }

        public T SetSportEventCacheTimeout(int timeoutInHours)
        {
            if (timeoutInHours >= ConfigLimit.SportEventCacheTimeoutMin && timeoutInHours <= ConfigLimit.SportEventCacheTimeoutMax)
            {
                ((UofCacheConfiguration)UofConfiguration.Cache).SportEventCacheTimeout = TimeSpan.FromHours(timeoutInHours);
                return this as T;
            }

            throw new ArgumentException($"Invalid timeout value for SportEventStatusTimeout: {timeoutInHours} hours.");
        }

        /// <summary>
        /// Sets the sport event status cache timeout
        /// </summary>
        /// <param name="timeoutInMinutes">The timeout.</param>
        public T SetSportEventStatusCacheTimeout(int timeoutInMinutes)
        {
            if (timeoutInMinutes >= ConfigLimit.SportEventStatusCacheTimeoutMinutesMin && timeoutInMinutes <= ConfigLimit.SportEventStatusCacheTimeoutMinutesMax)
            {
                ((UofCacheConfiguration)UofConfiguration.Cache).SportEventStatusCacheTimeout = TimeSpan.FromMinutes(timeoutInMinutes);
                return this as T;
            }

            throw new ArgumentException($"Invalid timeout value for SportEventStatusCacheTimeout: {timeoutInMinutes} min.");
        }

        /// <summary>
        /// Sets the profile cache timeout.
        /// </summary>
        /// <param name="timeoutInHours">The timeout.</param>
        public T SetProfileCacheTimeout(int timeoutInHours)
        {
            if (timeoutInHours >= ConfigLimit.ProfileCacheTimeoutMin && timeoutInHours <= ConfigLimit.ProfileCacheTimeoutMax)
            {
                ((UofCacheConfiguration)UofConfiguration.Cache).ProfileCacheTimeout = TimeSpan.FromHours(timeoutInHours);
                return this as T;
            }

            throw new ArgumentException($"Invalid timeout value for ProfileCacheTimeout: {timeoutInHours} hours.");
        }

        /// <summary>
        /// Sets the variant market description cache timeout
        /// </summary>
        /// <param name="timeoutInHours">The timeout.</param>
        public T SetVariantMarketDescriptionCacheTimeout(int timeoutInHours)
        {
            if (timeoutInHours >= ConfigLimit.SingleVariantMarketTimeoutMin && timeoutInHours <= ConfigLimit.SingleVariantMarketTimeoutMax)
            {
                ((UofCacheConfiguration)UofConfiguration.Cache).VariantMarketDescriptionCacheTimeout = TimeSpan.FromHours(timeoutInHours);
                return this as T;
            }

            throw new ArgumentException($"Invalid timeout value for VariantMarketDescriptionCacheTimeout: {timeoutInHours} hours.");
        }

        /// <summary>
        /// Sets the ignore BetPal timeline sport event status cache timeout. How long should the event id from BetPal be cached. SportEventStatus from timeline endpoint for these events are ignored.
        /// </summary>
        /// <param name="timeoutInHours">The timeout.</param>
        public T SetIgnoreBetPalTimelineSportEventStatusCacheTimeout(int timeoutInHours)
        {
            if (timeoutInHours >= ConfigLimit.IgnoreBetpalTimelineTimeoutMin && timeoutInHours <= ConfigLimit.IgnoreBetpalTimelineTimeoutMax)
            {
                ((UofCacheConfiguration)UofConfiguration.Cache).IgnoreBetPalTimelineSportEventStatusCacheTimeout = TimeSpan.FromHours(timeoutInHours);
                return this as T;
            }

            throw new ArgumentException($"Invalid timeout value for IgnoreBetPalTimelineSportEventStatusCacheTimeout: {timeoutInHours} hours.");
        }

        /// <summary>
        /// Sets the value indicating whether to ignore sport event status from timeline endpoint for sport events on BetPal producer
        /// </summary>
        /// <param name="ignore">if set to <c>true</c> ignore</param>
        public T SetIgnoreBetPalTimelineSportEventStatus(bool ignore)
        {
            ((UofCacheConfiguration)UofConfiguration.Cache).IgnoreBetPalTimelineSportEventStatus = ignore;
            return this as T;
        }

        /// <summary>
        /// Sets the rabbit timeout setting for connection attempts (in seconds).
        /// </summary>
        /// <param name="rabbitConnectionTimeout">The rabbit timeout setting for connection attempts (in seconds)</param>
        /// <remarks>Between 10 and 120 (default 30s) - set before connection is made</remarks>
        public T SetRabbitConnectionTimeout(int rabbitConnectionTimeout)
        {
            if (rabbitConnectionTimeout >= ConfigLimit.RabbitConnectionTimeoutMin && rabbitConnectionTimeout <= ConfigLimit.RabbitConnectionTimeoutMax)
            {
                ((UofRabbitConfiguration)UofConfiguration.Rabbit).ConnectionTimeout = TimeSpan.FromSeconds(rabbitConnectionTimeout);
                return this as T;
            }
            throw new ArgumentException($"Invalid timeout value for RabbitConnectionTimeout: {rabbitConnectionTimeout} seconds.");
        }

        /// <summary>
        /// Sets a heartbeat timeout to use when negotiating with the rabbit server (in seconds).
        /// </summary>
        /// <param name="heartbeatInSeconds">The heartbeat timeout to use when negotiating with the rabbit server (in seconds).</param>
        /// <remarks>Between 10 and 180 (default 60s) - set before connection is made</remarks>
        public T SetRabbitHeartbeat(int heartbeatInSeconds)
        {
            if (heartbeatInSeconds >= ConfigLimit.RabbitHeartbeatMin && heartbeatInSeconds <= ConfigLimit.RabbitHeartbeatMax)
            {
                ((UofRabbitConfiguration)UofConfiguration.Rabbit).Heartbeat = TimeSpan.FromSeconds(heartbeatInSeconds);
                return this as T;
            }
            throw new ArgumentException($"Invalid timeout value for RabbitHeartBeat: {heartbeatInSeconds} seconds.");
        }

        public T SetStatisticsInterval(int intervalInMinutes)
        {
            if (intervalInMinutes >= ConfigLimit.StatisticsIntervalMinutesMin && intervalInMinutes <= ConfigLimit.StatisticsIntervalMinutesMax)
            {
                ((UofAdditionalConfiguration)UofConfiguration.Additional).StatisticsInterval = TimeSpan.FromMinutes(intervalInMinutes);
                return this as T;
            }

            throw new ArgumentException($"Invalid timeout value for StatisticsInterval: {intervalInMinutes} minutes.");
        }

        public T OmitMarketMappings(bool omit)
        {
            ((UofAdditionalConfiguration)UofConfiguration.Additional).OmitMarketMappings = omit;
            return this as T;
        }
    }
}
