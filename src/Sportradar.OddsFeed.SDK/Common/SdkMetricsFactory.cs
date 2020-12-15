/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/

using System.Configuration;
using App.Metrics;

namespace Sportradar.OddsFeed.SDK.Common
{
    /// <summary>
    /// Provides methods to get <see cref="IMetricsRoot"/> to record sdk metrics
    /// </summary>
    public class SdkMetricsFactory
    {
        private static IMetricsRoot _metricsRoot;

        /// <summary>
        /// Used to set <see cref="IMetricsRoot"/> used within sdk
        /// </summary>
        /// <param name="metricsRoot">The <see cref="IMetricsRoot"/> used within sdk</param>
        public SdkMetricsFactory(IMetricsRoot metricsRoot)
        {
            _metricsRoot = metricsRoot;
        }

        /// <summary>
        /// The <see cref="IMetricsRoot"/> used within sdk
        /// </summary>
        public static IMetricsRoot MetricsRoot
        {
            get
            {
                if (_metricsRoot == null)
                {
                    _metricsRoot = AppMetrics.CreateDefaultBuilder().Build();
                }
                return _metricsRoot;
            }
        }

        /// <summary>
        /// Method for getting <see cref="IMeasureMetrics"/>
        /// </summary>
        /// <returns>Returns <see cref="IMetricsRoot"/> </returns>
        public static IMeasureMetrics GetMeasure()
        {
            return _metricsRoot?.Measure;
        }
    }
}
