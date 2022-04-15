/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/

using App.Metrics;
using Sportradar.OddsFeed.SDK.Common.Internal.Metrics;
using System;
using System.Diagnostics;

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

        /// <summary>
        /// Record system/app metrics
        /// </summary>
        public static void SystemMeasures()
        {
            if (MetricsRoot == null)
            {
                return;
            }


            try
            {
                var process = Process.GetCurrentProcess();
                var lastTotalProcessorTime = TimeSpan.Zero;
                var lastUserProcessorTime = TimeSpan.Zero;
                var lastPrivilegedProcessorTime = TimeSpan.Zero;
                var lastTimeStamp = process.StartTime;
                process.Refresh();

                var totalCpuTimeUsed = process.TotalProcessorTime.TotalMilliseconds - lastTotalProcessorTime.TotalMilliseconds;
                var privilegedCpuTimeUsed = process.PrivilegedProcessorTime.TotalMilliseconds - lastPrivilegedProcessorTime.TotalMilliseconds;
                var userCpuTimeUsed = process.UserProcessorTime.TotalMilliseconds - lastUserProcessorTime.TotalMilliseconds;

                lastTotalProcessorTime = process.TotalProcessorTime;
                lastPrivilegedProcessorTime = process.PrivilegedProcessorTime;
                lastUserProcessorTime = process.UserProcessorTime;

                var cpuTimeElapsed = (DateTime.UtcNow - lastTimeStamp).TotalMilliseconds * Environment.ProcessorCount;
                lastTimeStamp = DateTime.UtcNow;

                MetricsRoot.Measure.Gauge.SetValue(SystemUsageMetricsRegistry.Gauges.TotalCpuUsed, totalCpuTimeUsed * 100 / cpuTimeElapsed);
                MetricsRoot.Measure.Gauge.SetValue(SystemUsageMetricsRegistry.Gauges.PrivilegedCpuUsed, privilegedCpuTimeUsed * 100 / cpuTimeElapsed);
                MetricsRoot.Measure.Gauge.SetValue(SystemUsageMetricsRegistry.Gauges.UserCpuUsed, userCpuTimeUsed * 100 / cpuTimeElapsed);

                MetricsRoot.Measure.Gauge.SetValue(SystemUsageMetricsRegistry.Gauges.MemoryWorkingSet, process.WorkingSet64);
                MetricsRoot.Measure.Gauge.SetValue(SystemUsageMetricsRegistry.Gauges.NonPagedSystemMemory, process.NonpagedSystemMemorySize64);
                MetricsRoot.Measure.Gauge.SetValue(SystemUsageMetricsRegistry.Gauges.PagedMemory, process.PagedMemorySize64);
                MetricsRoot.Measure.Gauge.SetValue(SystemUsageMetricsRegistry.Gauges.PagedSystemMemory, process.PagedSystemMemorySize64);
                MetricsRoot.Measure.Gauge.SetValue(SystemUsageMetricsRegistry.Gauges.PrivateMemory, process.PrivateMemorySize64);
                MetricsRoot.Measure.Gauge.SetValue(SystemUsageMetricsRegistry.Gauges.VirtualMemory, process.VirtualMemorySize64);

                MetricsRoot.Measure.Gauge.SetValue(GcMetricsRegistry.Gauges.Gen0Collections, GC.CollectionCount(0));
                MetricsRoot.Measure.Gauge.SetValue(GcMetricsRegistry.Gauges.Gen1Collections, GC.CollectionCount(1));
                MetricsRoot.Measure.Gauge.SetValue(GcMetricsRegistry.Gauges.Gen2Collections, GC.CollectionCount(2));
                MetricsRoot.Measure.Gauge.SetValue(GcMetricsRegistry.Gauges.LiveObjectsSize, GC.GetTotalMemory(false));
            }
            catch (Exception)
            {
                // ignored
            }
        }
    }
}
