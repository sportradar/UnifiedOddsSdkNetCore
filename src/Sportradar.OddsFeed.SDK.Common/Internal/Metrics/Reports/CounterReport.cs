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
    internal class CounterReport : BaseReport<CounterValueSource>
    {
        public CounterReport(string context = null, ILog log = null, MetricsReportPrintMode printMode = MetricsReportPrintMode.Normal, int decimals = 2)
            : base(context, log, printMode, decimals)
        {
        }

        public override void ReportList(IEnumerable<CounterValueSource> items)
        {
            var listItems = items as IReadOnlyList<CounterValueSource> ?? items.ToList();
            if (!listItems.Any())
            {
                return;
            }

            QueueAdd(FormatHelper.SectionName("Counters", ContextName));

            base.ReportList(listItems);
        }

        public override void Report(CounterValueSource item, bool suppressPrint = false)
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

        protected override void Print(CounterValueSource item)
        {
            QueueAdd(item.Name, item.Value.Count + " " + item.Unit);
        }

        protected override void PrintFull(CounterValueSource item)
        {
            Print(item);
            QueueAdd("Items:");
            PrintItems(item.Value.Items, item.Unit);
        }

        private void PrintItems(IEnumerable<CounterValue.SetItem> items, Unit unit)
        {
            if (items == null)
            {
                return;
            }

            var fh = new FormatHelper(Decimals, MetricsReportPrintMode.Compact);
            foreach (var i in items)
            {
                QueueAdd(i.Item, $"{i.Count} {unit} ({fh.Dec(i.Percent)}%)");
            }
        }
    }
}
