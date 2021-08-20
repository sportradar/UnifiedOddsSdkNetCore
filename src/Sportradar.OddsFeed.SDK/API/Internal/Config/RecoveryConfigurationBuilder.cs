/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using Sportradar.OddsFeed.SDK.Common.Internal;

namespace Sportradar.OddsFeed.SDK.API.Internal.Config
{
    /// <summary>
    /// Class RecoveryConfigurationBuilder
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <seealso cref="ConfigurationBuilderBase{T}" />
    /// <seealso cref="IRecoveryConfigurationBuilder{T}" />
    internal abstract class RecoveryConfigurationBuilder<T> : ConfigurationBuilderBase<T>, IRecoveryConfigurationBuilder<T> where T : class
    {
        /// <summary>
        /// The inactivity seconds
        /// </summary>
        protected int? InactivitySeconds;

        /// <summary>
        /// The maximum recovery time in seconds
        /// </summary>
        protected int? MaxRecoveryTimeInSeconds;

        /// <summary>
        /// The minimal interval between recovery requests initiated by alive messages (seconds)
        /// </summary>
        protected int? MinIntervalBetweenRecoveryRequests;

        /// <summary>
        /// The value indicating if the after age should be adjusted
        /// </summary>
        protected bool? AdjustAfterAge;

        /// <summary>
        /// Value specifying timeout set for recovery HTTP responses
        /// </summary>
        protected int? RecoveryHttpClientTimeout;

        /// <summary>
        /// Construct RecoveryConfigurationBuilder
        /// </summary>
        /// <param name="accessToken">An access token used to authenticate with the feed</param>
        /// <param name="sectionProvider">A <see cref="IConfigurationSectionProvider"/> used to access <see cref="IOddsFeedConfigurationSection"/></param>
        internal RecoveryConfigurationBuilder(string accessToken, IConfigurationSectionProvider sectionProvider)
            : base(accessToken, sectionProvider)
        {
        }

        /// <summary>
        /// Sets the recovery configuration properties to values read from configuration file. Only values which can be set
        /// through <see cref="IRecoveryConfigurationBuilder{T}" /> methods are set.
        /// Any values already set by methods on the current instance are overridden
        /// </summary>
        /// <param name="section">A <see cref="IOddsFeedConfigurationSection"/> from which to load the config</param>
        /// <returns>T.</returns>
        internal override void LoadFromConfigFile(IOddsFeedConfigurationSection section)
        {
            base.LoadFromConfigFile(section);

            InactivitySeconds = section.InactivitySeconds;
            MaxRecoveryTimeInSeconds = section.MaxRecoveryTime;
            MinIntervalBetweenRecoveryRequests = section.MinIntervalBetweenRecoveryRequests;
            AdjustAfterAge = section.AdjustAfterAge;
            RecoveryHttpClientTimeout = section.RecoveryHttpClientTimeout;
        }

        /// <summary>
        /// Sets the max time window between two consecutive alive messages before the associated producer is marked as down
        /// </summary>
        /// <param name="inactivitySeconds">the max time window between two consecutive alive messages</param>
        /// <returns>A <see cref="IRecoveryConfigurationBuilder{T}" /> derived instance used to set general configuration properties</returns>
        public T SetInactivitySeconds(int inactivitySeconds)
        {
            if (inactivitySeconds < SdkInfo.MinInactivitySeconds)
            {
                throw new ArgumentException($"Value must be at least {SdkInfo.MinInactivitySeconds}.");
            }
            if (inactivitySeconds > SdkInfo.MaxInactivitySeconds)
            {
                throw new ArgumentException($"Value must be less then {SdkInfo.MaxInactivitySeconds}.");
            }

            InactivitySeconds = inactivitySeconds;
            return this as T;
        }

        /// <summary>
        /// Sets the maximum time in seconds in which recovery must be completed (minimum 600 seconds)
        /// </summary>
        /// <param name="maxRecoveryTimeInSeconds">Maximum recovery time in seconds</param>
        /// <returns>A <see cref="IRecoveryConfigurationBuilder{T}" /> instance used to set general configuration properties</returns>
        public T SetMaxRecoveryTime(int maxRecoveryTimeInSeconds)
        {
            if (maxRecoveryTimeInSeconds < SdkInfo.MinRecoveryExecutionInSeconds)
            {
                throw new ArgumentException($"Value must be at least {SdkInfo.MinRecoveryExecutionInSeconds}.");
            }
            if (maxRecoveryTimeInSeconds > SdkInfo.MaxRecoveryExecutionInSeconds)
            {
                throw new ArgumentException($"Value must be less then {SdkInfo.MaxRecoveryExecutionInSeconds}.");
            }

            MaxRecoveryTimeInSeconds = maxRecoveryTimeInSeconds;
            return this as T;
        }

        /// <summary>
        /// Sets the minimal interval between recovery requests initiated by alive messages (between 20 and 180 seconds)
        /// </summary>
        /// <param name="minIntervalBetweenRecoveryRequests">The minimal interval between recovery requests initiated by alive messages (seconds)</param>
        /// <returns>A <see cref="IRecoveryConfigurationBuilder{T}" /> instance used to set general configuration properties</returns>
        public T SetMinIntervalBetweenRecoveryRequests(int minIntervalBetweenRecoveryRequests)
        {
            if (minIntervalBetweenRecoveryRequests < SdkInfo.MinIntervalBetweenRecoveryRequests)
            {
                throw new ArgumentException($"Value must be at least {SdkInfo.MinIntervalBetweenRecoveryRequests}.");
            }
            if (minIntervalBetweenRecoveryRequests > SdkInfo.MaxIntervalBetweenRecoveryRequests)
            {
                throw new ArgumentException($"Value must be less then {SdkInfo.MaxIntervalBetweenRecoveryRequests}.");
            }

            MinIntervalBetweenRecoveryRequests = minIntervalBetweenRecoveryRequests;
            return this as T;
        }

        /// <summary>
        /// Sets the value indicating whether the after age should be enforced before executing recovery request
        /// </summary>
        /// <param name="adjustAfterAge">True if age should be enforced; False otherwise</param>
        /// <returns>The <see cref="IRecoveryConfigurationBuilder{T}"/> instance used to set additional values</returns>
        public T SetAdjustAfterAge(bool adjustAfterAge)
        {
            AdjustAfterAge = adjustAfterAge;
            return this as T;
        }

        /// <summary>
        /// Sets the timeout for recovery HTTP responses for this instance of the sdk
        /// </summary>
        /// <param name="recoveryHttpClientTimeout">The timeout for recovery HTTP responses</param>
        /// <returns>A <see cref="IConfigurationBuilderBase{T}"/> derived instance used to set general configuration properties</returns>
        public T SetRecoveryHttpClientTimeout(int recoveryHttpClientTimeout)
        {
            RecoveryHttpClientTimeout = recoveryHttpClientTimeout;
            return this as T;
        }
    }
}
