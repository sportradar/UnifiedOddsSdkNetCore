/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using System.Threading;
using Common.Logging;
using Metrics;
using Metrics.MetricData;
using Metrics.Reporters;
using Sportradar.OddsFeed.SDK.Common.Internal.Metrics.Reports;

namespace Sportradar.OddsFeed.SDK.Common.Internal.Metrics
{
    /// <summary>
    /// Implementation of <see cref="MetricsReport" /> using <see cref="ILog" /> for printing collected data
    /// </summary>
    /// <seealso cref="MetricsReport" />
    public class MetricsReporter : MetricsReport
    {
        private ILog _log;
        private readonly MetricsReportPrintMode _printMode;
        private readonly int _decimals;
        private readonly bool _runHealthStatus;

        /// <summary>
        /// Initializes a new instance of the <see cref="MetricsReporter"/> class.
        /// </summary>
        /// <param name="printMode">The print mode.</param>
        /// <param name="decimals">The number decimals used for printing numbers</param>
        /// <param name="runHealthStatus">Indicates if it should execute <see cref="RunHealthStatus"/> before completing <see cref="RunReport"/></param>
        public MetricsReporter(MetricsReportPrintMode printMode = MetricsReportPrintMode.Normal, int decimals = 2, bool runHealthStatus = true)
        {
            _log = null;
            _printMode = printMode;
            _decimals = decimals;
            _runHealthStatus = runHealthStatus;
        }

        /// <summary>
        /// Runs the report of <see cref="MetricsData"/>
        /// </summary>
        /// <param name="metricsData">The metrics data.</param>
        /// <param name="healthStatus">The health status.</param>
        /// <param name="token">The token.</param>
        public void RunReport(MetricsData metricsData, Func<HealthStatus> healthStatus, CancellationToken token)
        {
            if (_log == null)
            {
                _log = SdkLoggerFactory.GetLoggerForStats(typeof(MetricsReporter));
            }

            if (_log != null)
            {
                _log.Info("===========================");
                _log.Info("===  UF SDK .NET Stats  ===");
                _log.Info("===========================");
            }

            var mdr = new MetricDataReport(metricsData?.Context, _log, _printMode, _decimals);
            mdr.Report(metricsData);

            if (_runHealthStatus)
            {
                RunHealthStatus(metricsData?.Context);
            }
        }

        /// <summary>
        /// Gets the health status and log it
        /// </summary>
        /// <param name="context">The context.</param>
        private void RunHealthStatus(string context = null)
        {
            var hs = HealthChecks.GetStatus();
            var report = new HealthStatusReport(context, _log, _printMode, _decimals);
            report.Report(hs);
        }
    }
}
