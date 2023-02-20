/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using App.Metrics;
using App.Metrics.Gauge;
using App.Metrics.Meter;
using App.Metrics.Timer;

namespace Sportradar.OddsFeed.SDK.Common.Internal.Metrics
{
    internal static class MetricsSettings
    {
        /// <summary>
        /// Default context for UfSdkSemaphorePool
        /// </summary>
        private const string McUfSemaphorePool = "UfSdkSemaphorePool";
        public static readonly TimerOptions TimerSemaphorePool = new TimerOptions { Context = McUfSemaphorePool, Name = "SemaphorePoolAcquire", MeasurementUnit = Unit.Items };
        public static readonly GaugeOptions GaugeSemaphorePool = new GaugeOptions { Context = McUfSemaphorePool, Name = "SemaphorePoolAcquireSize", MeasurementUnit = Unit.Items };

        /// <summary>
        /// Default context for UfSdkRabbitMessageReceiver
        /// </summary>
        private const string McUfRabbitMqMessageReceiver = "UfSdkRabbitMessageReceiver";
        public static readonly TimerOptions TimerMessageDeserialize = new TimerOptions { Context = McUfRabbitMqMessageReceiver, Name = "Message deserialization time", MeasurementUnit = Unit.Items };
        public static readonly TimerOptions TimerRawFeedDataDispatch = new TimerOptions { Context = McUfRabbitMqMessageReceiver, Name = "Raw message dispatched", MeasurementUnit = Unit.Items };
        public static readonly MeterOptions MeterMessageConsume = new MeterOptions { Context = McUfRabbitMqMessageReceiver, Name = "Message received" };
        public static readonly MeterOptions MeterMessageDeserializeException = new MeterOptions { Context = McUfRabbitMqMessageReceiver, Name = "Message deserialization exception" };
        public static readonly MeterOptions MeterMessageConsumeException = new MeterOptions { Context = McUfRabbitMqMessageReceiver, Name = "Message consuming exception" };

        /// <summary>
        /// Default context for UfSdkSportEvent
        /// </summary>
        private const string McUfSportEvent = "UfSdkSportEvent";
        public static readonly TimerOptions TimerFetchMissingSummary = new TimerOptions { Context = McUfSportEvent, Name = "FetchMissingSummary", MeasurementUnit = Unit.Items };
        public static readonly TimerOptions TimerFetchMissingFixtures = new TimerOptions { Context = McUfSportEvent, Name = "FetchMissingFixtures", MeasurementUnit = Unit.Items };
    }
}
