/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using System.Diagnostics;
using App.Metrics;
using Sportradar.OddsFeed.SDK.Common.Internal.Metrics;

namespace Sportradar.OddsFeed.SDK.Common
{
    /// <summary>
    /// Provides methods to get <see cref="IMetricsRoot"/> to record sdk metrics
    /// </summary>
    public static class SdkMetricsFactory
    {
        private static IMetricsRoot MetricsRootInternal;

        /// <summary>
        /// The <see cref="IMetricsRoot"/> used within sdk
        /// </summary>
        public static IMetricsRoot MetricsRoot
        {
            get
            {
                MetricsRootInternal ??= AppMetrics.CreateDefaultBuilder().Build();
                return MetricsRootInternal;
            }
        }

        /// <summary>
        /// Set the <see cref="IMetricsRoot"/>
        /// </summary>
        /// <param name="metricsRoot">An <see cref="IMetricsRoot"/> to be used</param>
        public static void SetMetricsFactory(IMetricsRoot metricsRoot)
        {
            MetricsRootInternal = metricsRoot;
        }

        /// <summary>
        /// Method for getting <see cref="IMeasureMetrics"/>
        /// </summary>
        /// <returns>Returns <see cref="IMetricsRoot"/> </returns>
        public static IMeasureMetrics GetMeasure()
        {
            return MetricsRootInternal?.Measure;
        }

        /// <summary>
        /// Record system/application metrics
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Roslynator", "RCS1075:Avoid empty catch clause that catches System.Exception.", Justification = "Ignore all metrics setup issues")]
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

                var cpuTimeElapsed = (DateTime.UtcNow - lastTimeStamp).TotalMilliseconds * Environment.ProcessorCount;

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
