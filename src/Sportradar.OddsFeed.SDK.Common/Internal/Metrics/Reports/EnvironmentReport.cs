/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System.Collections.Generic;
using System.Linq;
using Common.Logging;
using Metrics.MetricData;

namespace Sportradar.OddsFeed.SDK.Common.Internal.Metrics.Reports
{
    internal class EnvironmentReport : BaseReport<EnvironmentEntry>
    {
        public EnvironmentReport(string context = null, ILog log = null, MetricsReportPrintMode printMode = MetricsReportPrintMode.Normal, int decimals = 2)
            : base(context, log, printMode, decimals)
        {
        }

        public override void ReportList(IEnumerable<EnvironmentEntry> items)
        {
            var listItems = items as IReadOnlyList<EnvironmentEntry> ?? items.ToList();
            if (!listItems.Any())
            {
                return;
            }

            QueueAdd(FormatHelper.SectionName("Environments", ContextName));

            base.ReportList(listItems);
        }

        public override void Report(EnvironmentEntry item, bool suppressPrint = false)
        {
            if (!suppressPrint)
            {
                QueueAdd(FormatHelper.SectionName("Environment", ContextName));
                SetLog(item.Name);
                SetContextName(item.Name);
            }

            PrintSelector(item);

            if (!suppressPrint)
            {
                PrintQueue();
            }
        }

        protected override void Print(EnvironmentEntry item)
        {
            QueueAdd(item.Name, item.Value);
        }
    }
}
