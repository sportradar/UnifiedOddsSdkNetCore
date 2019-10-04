/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System.Collections.Generic;
using System.Linq;
using Common.Logging;
using Metrics.MetricData;

namespace Sportradar.OddsFeed.SDK.Common.Internal.Metrics.Reports
{
    internal class TimerReport : BaseReport<TimerValueSource>
    {
        private readonly FormatHelper _fh;

        public TimerReport(string context = null, ILog log = null, MetricsReportPrintMode printMode = MetricsReportPrintMode.Normal, int decimals = 2)
            : base(context, log, printMode, decimals)
        {
            _fh = new FormatHelper(Decimals, MetricsReportPrintMode.Compact);
        }

        public override void ReportList(IEnumerable<TimerValueSource> items)
        {
            var listItems = items as IList<TimerValueSource> ?? items.ToList();
            if (!listItems.Any())
            {
                return;
            }

            QueueAdd(FormatHelper.SectionName("Timers", ContextName));

            base.ReportList(listItems);
        }

        public override void Report(TimerValueSource item, bool suppressPrint = false)
        {
            if (!suppressPrint)
            {
                QueueAdd(FormatHelper.SectionName("Timer", ContextName));
                SetLog(item.Name);
                SetContextName(item.Name);
            }

            PrintSelector(item);

            if (!suppressPrint)
            {
                PrintQueue();
            }
        }

        protected override void Print(TimerValueSource item)
        {
            if (!string.IsNullOrEmpty(item.Name))
            {
                QueueAdd(item.Name);
            }

            var u = _fh.U(item.Unit, item.RateUnit, false);
            QueueAdd("Active sessions", $"{item.Value.ActiveSessions}");
            QueueAdd("Total time", $"{item.Value.TotalTime} {_fh.Time(item.DurationUnit)}");
            QueueAdd("Count", $"{item.Value.Rate.Count} {item.Unit}");
            QueueAdd("Mean value", $"{_fh.Dec(item.Value.Rate.MeanRate)} {u}");
            QueueAdd("1 minute rate", $"{_fh.Dec(item.Value.Rate.OneMinuteRate)} {u}");
            QueueAdd("5 minute rate", $"{_fh.Dec(item.Value.Rate.FiveMinuteRate)} {u}");
            QueueAdd("15 minute rate", $"{_fh.Dec(item.Value.Rate.FifteenMinuteRate)} {u}");
        }

        protected override void PrintFull(TimerValueSource item)
        {
            Print(item);

            var hist = item.Value.Histogram;
            var t = _fh.Time(item.DurationUnit);
            QueueAdd("Mean", $"{_fh.Dec(hist.Mean)} {t}");
            QueueAdd("75%", $"{_fh.Dec(hist.Percentile75)} {t}");
            QueueAdd("95%", $"{_fh.Dec(hist.Percentile95)} {t}");
            QueueAdd("98%", $"{_fh.Dec(hist.Percentile98)} {t}");
            QueueAdd("99%", $"{_fh.Dec(hist.Percentile99)} {t}");
            QueueAdd("99.9%", $"{_fh.Dec(hist.Percentile999)} {t}");
            QueueAdd("StdDev", $"{_fh.Dec(hist.StdDev)} {t}");
            QueueAdd("Median", $"{_fh.Dec(hist.Median)} {t}");
            QueueAdd("Min", $"{_fh.Dec(hist.Min)} {t}");
            QueueAdd("Max", $"{_fh.Dec(hist.Max)} {t}");
            QueueAdd("Last", $"{_fh.Dec(hist.LastValue)} {t}");
            QueueAdd("Min", $"{hist.MinUserValue}");
            QueueAdd("Max", $"{hist.MaxUserValue}");
            QueueAdd("Last", $"{hist.LastUserValue}");
        }
    }
}
