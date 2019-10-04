/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System.Collections.Generic;
using System.Linq;
using Common.Logging;
using Metrics.MetricData;

namespace Sportradar.OddsFeed.SDK.Common.Internal.Metrics.Reports
{
    internal class GaugeReport : BaseReport<GaugeValueSource>
    {
        private readonly FormatHelper _fh;

        public GaugeReport(string context = null, ILog log = null, MetricsReportPrintMode printMode = MetricsReportPrintMode.Normal, int decimals = 2)
            : base(context, log, printMode, decimals)
        {
            _fh = new FormatHelper(Decimals, MetricsReportPrintMode.Compact);
        }

        public override void ReportList(IEnumerable<GaugeValueSource> items)
        {
            var listItems = items as IReadOnlyList<GaugeValueSource> ?? items.ToList();
            if (!listItems.Any())
            {
                return;
            }

            QueueAdd(FormatHelper.SectionName("Gauges", ContextName));

            base.ReportList(listItems);
        }

        public override void Report(GaugeValueSource item, bool suppressPrint = false)
        {
            if (!suppressPrint)
            {
                QueueAdd(FormatHelper.SectionName("Gauge", ContextName));
                SetLog(item.Name);
                SetContextName(item.Name);
            }

            PrintSelector(item);

            if (!suppressPrint)
            {
                PrintQueue();
            }
        }

        protected override void Print(GaugeValueSource item)
        {
            QueueAdd($"{item.Name}", $"{_fh.Dec(item.Value)} {_fh.U(item.Unit)}");
        }
    }
}
