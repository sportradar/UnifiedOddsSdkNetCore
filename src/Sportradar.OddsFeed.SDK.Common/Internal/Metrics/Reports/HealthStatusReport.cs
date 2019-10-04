/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using Common.Logging;
using Metrics;

namespace Sportradar.OddsFeed.SDK.Common.Internal.Metrics.Reports
{
    internal class HealthStatusReport : BaseReport<HealthStatus>
    {
        public HealthStatusReport(string context = null, ILog log = null, MetricsReportPrintMode printMode = MetricsReportPrintMode.Normal, int decimals = 2)
            : base(context, log, printMode, decimals)
        {
        }

        public override void Report(HealthStatus item, bool suppressPrint = false)
        {
            PrintSelector(item);

            if (!suppressPrint)
            {
                PrintQueue();
            }
        }

        protected override void Print(HealthStatus item)
        {
            if (!item.HasRegisteredChecks)
            {
                QueueAdd(FormatHelper.SectionName("No HealthChecks registered.", ContextName));
                return;
            }

            var result = item.IsHealthy ? "OK" : "NOK";
            QueueAdd(FormatHelper.SectionName("HealthStatus", ContextName));
            QueueAdd("Status", result);

            foreach (var s in item.Results)
            {
                result = s.Check.IsHealthy ? "OK" : "NOK";
                var msg = string.IsNullOrEmpty(s.Check.Message) ? string.Empty : $"({s.Check.Message})";
                QueueAdd($"{s.Name} => Status: {result}. {msg}");
            }
            QueueAdd(string.Empty);
        }
    }
}
