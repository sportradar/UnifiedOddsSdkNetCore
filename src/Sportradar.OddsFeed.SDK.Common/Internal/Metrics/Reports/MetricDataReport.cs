/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System.Collections.Generic;
using System.Linq;
using Common.Logging;
using Metrics.MetricData;

namespace Sportradar.OddsFeed.SDK.Common.Internal.Metrics.Reports
{
    internal class MetricDataReport : BaseReport<MetricsData>
    {
        public MetricDataReport(string context = null, ILog log = null, MetricsReportPrintMode printMode = MetricsReportPrintMode.Normal, int decimals = 2)
            : base(context, log, printMode, decimals)
        {
        }

        public override void ReportList(IEnumerable<MetricsData> items)
        {
            var listItems = items as IReadOnlyList<MetricsData> ?? items.ToList();
            if (!listItems.Any())
            {
                return;
            }

            //QueueAdd(FormatHelper.SectionName("MetricsData(s)", ContextName));

            base.ReportList(listItems);
        }

        public override void Report(MetricsData item, bool suppressPrint = false)
        {
            if (!suppressPrint)
            {
                SetLog(item.Context);
                SetContextName(item.Context);
                QueueAdd($"{FormatHelper.SectionName("MetricsData", ContextName)}");
            }

            PrintSelector(item);

            if (!suppressPrint)
            {
                PrintQueue();
            }
        }

        protected override void Print(MetricsData item)
        {
            var cr = new CounterReport(item.Context, Log, PrintMode, Decimals);
            var mr = new MeterReport(item.Context, Log, PrintMode, Decimals);
            var tr = new TimerReport(item.Context, Log, PrintMode, Decimals);
            var gr = new GaugeReport(item.Context, Log, PrintMode, Decimals);
            var hr = new HistogramReport(item.Context, Log, PrintMode, Decimals);

            cr.ReportList(item.Counters);
            tr.ReportList(item.Timers);
            mr.ReportList(item.Meters);
            gr.ReportList(item.Gauges);
            hr.ReportList(item.Histograms);

            ReportList(item.ChildMetrics);
        }

        protected override void PrintMinimal(MetricsData item)
        {
            var cr = new CounterReport(item.Context, Log, PrintMode, Decimals);
            var mr = new MeterReport(item.Context, Log, PrintMode, Decimals);
            var tr = new TimerReport(item.Context, Log, PrintMode, Decimals);

            cr.ReportList(item.Counters);
            tr.ReportList(item.Timers);
            mr.ReportList(item.Meters);
        }

        protected override void PrintCompact(MetricsData item)
        {
            PrintMinimal(item);

            ReportList(item.ChildMetrics);
        }

        protected override void PrintFull(MetricsData item)
        {
            Print(item);

            var er = new EnvironmentReport(item.Context, Log, PrintMode, Decimals);
            er.ReportList(item.Environment);
        }
    }
}
