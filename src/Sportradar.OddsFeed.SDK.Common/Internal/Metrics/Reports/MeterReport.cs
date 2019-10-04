/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System.Collections.Generic;
using System.Linq;
using Common.Logging;
using Metrics;
using Metrics.MetricData;

namespace Sportradar.OddsFeed.SDK.Common.Internal.Metrics.Reports
{
    internal class MeterReport : BaseReport<MeterValueSource>
    {
        public MeterReport(string context = null, ILog log = null, MetricsReportPrintMode printMode = MetricsReportPrintMode.Normal, int decimals = 2)
            : base(context, log, printMode, decimals)
        {
        }

        public override void ReportList(IEnumerable<MeterValueSource> items)
        {
            var listItems = items as IReadOnlyList<MeterValueSource> ?? items.ToList();
            if (!listItems.Any())
            {
                return;
            }

            QueueAdd(FormatHelper.SectionName("Meters", ContextName));

            base.ReportList(listItems);
        }

        public override void Report(MeterValueSource item, bool suppressPrint = false)
        {
            if (!suppressPrint)
            {
                SetLog(item.Name);
                SetContextName(item.Name);
            }

            PrintSelector(item);

            if (!suppressPrint)
            {
                PrintQueue();
            }
        }

        protected override void Print(MeterValueSource item)
        {
            var fh = new FormatHelper(Decimals, MetricsReportPrintMode.Compact);
            QueueAdd(item.Name);
            QueueAdd("Count", item.Value.Count + " " + item.Unit);
            QueueAdd("Mean value", $"{fh.Dec(item.Value.MeanRate)} {fh.U(item.Unit, item.RateUnit, false)}");
            QueueAdd("1 minute rate", $"{fh.Dec(item.Value.OneMinuteRate)} {fh.U(item.Unit, item.RateUnit, false)}");
            QueueAdd("5 minute rate", $"{fh.Dec(item.Value.FiveMinuteRate)} {fh.U(item.Unit, item.RateUnit, false)}");
            QueueAdd("15 minute rate", $"{fh.Dec(item.Value.FifteenMinuteRate)} {fh.U(item.Unit, item.RateUnit, false)}");
        }

        protected override void PrintFull(MeterValueSource item)
        {
            Print(item);

            PrintItems(item.Value.Items, item.Unit);
        }

        private void PrintItems(IEnumerable<MeterValue.SetItem> items, Unit unit)
        {
            if (items == null)
            {
                return;
            }

            var fh = new FormatHelper(Decimals, MetricsReportPrintMode.Compact);
            foreach (var i in items)
            {
                QueueAdd(i.Item);
                QueueAdd("Count", i.Value.Count + " " + unit);
                QueueAdd("Percent", fh.Dec(i.Percent) + "%");
                QueueAdd("Mean value", $"{fh.Dec(i.Value.MeanRate)} {fh.U(unit, i.Value.RateUnit, false)}");
                QueueAdd("1 minute rate", $"{fh.Dec(i.Value.OneMinuteRate)} {fh.U(unit, i.Value.RateUnit, false)}");
                QueueAdd("5 minute rate", $"{fh.Dec(i.Value.FiveMinuteRate)} {fh.U(unit, i.Value.RateUnit, false)}");
                QueueAdd("15 minute rate", $"{fh.Dec(i.Value.FifteenMinuteRate)} {fh.U(unit, i.Value.RateUnit, false)}");
            }
        }
    }
}
