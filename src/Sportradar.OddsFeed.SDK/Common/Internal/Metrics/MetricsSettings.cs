using App.Metrics;
using App.Metrics.Gauge;
using App.Metrics.Timer;

namespace Sportradar.OddsFeed.SDK.Common.Internal.Metrics
{
    internal static class MetricsSettings
    {
        /// <summary>
        /// Default context for metrics - UFGlobalEventProcessor
        /// </summary>
        private const string McUfSemaphorePool = "UfSdkSemaphorePool";

        public static readonly TimerOptions TimerSemaphorePool = new TimerOptions { Context = McUfSemaphorePool, Name = "SemaphorePoolAcquire", MeasurementUnit = Unit.Items };
        public static readonly GaugeOptions GaugeSemaphorePool = new GaugeOptions { Context = McUfSemaphorePool, Name = "SemaphorePoolAcquire", MeasurementUnit = Unit.Items };
    }
}
