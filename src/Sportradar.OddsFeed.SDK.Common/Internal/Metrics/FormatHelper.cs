/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using System.Collections.Generic;
using System.Linq;
using Metrics;
using Sportradar.OddsFeed.SDK.Common.Internal.Metrics.Reports;

namespace Sportradar.OddsFeed.SDK.Common.Internal.Metrics
{
    internal class FormatHelper
    {
        private readonly int _decimals;
        private readonly MetricsReportPrintMode _printMode;

        public FormatHelper(int decimals, MetricsReportPrintMode printMode)
        {
            _decimals = decimals;
            _printMode = printMode;
        }

        public string U(Unit unit)
        {
            return $"{unit}";
        }

        public string U(Unit unit, TimeUnit time, bool useSpace)
        {
            string t = _printMode == 0 ? time.ToString() : TimeShort(time);

            return useSpace ? $"{unit} {t}" : $"{unit}/{t}";
        }

        public string Dec(double input)
        {
            return Math.Round(input, _decimals).ToString($"F{_decimals}");
        }

        public string Dec(decimal input)
        {
            return Math.Round(input, _decimals).ToString($"F{_decimals}");
        }

        public static string Dec(decimal input, int decimals)
        {
            return Math.Round(input, decimals).ToString($"F{decimals}");
        }

        public string Time(TimeUnit unit)
        {
            return Time(unit, _printMode);
        }

        public static string Time(TimeUnit unit, MetricsReportPrintMode printMode)
        {
            return (printMode == MetricsReportPrintMode.Normal || printMode == MetricsReportPrintMode.Full) ? unit.ToString() : TimeShort(unit);
        }

        private static string TimeShort(TimeUnit unit)
        {
            switch (unit)
            {
                case TimeUnit.Nanoseconds:
                    return "ns";
                case TimeUnit.Microseconds:
                    return "µs";
                case TimeUnit.Milliseconds:
                    return "ms";
                case TimeUnit.Seconds:
                    return "s";
                case TimeUnit.Minutes:
                    return "min";
                case TimeUnit.Hours:
                    return "h";
                case TimeUnit.Days:
                    return "d";
                default:
                    throw new ArgumentOutOfRangeException(nameof(unit), unit, null);
            }
        }

        public static string SectionName(string name, string context)
        {
            var cnt = string.IsNullOrEmpty(context) ? string.Empty : $"[{context}]";
            return $"**** {cnt} {name}   [{DateTime.Now}] ****";
        }

        public static string DictionaryToString(IReadOnlyDictionary<string, string> input)
        {
            if (input == null || !input.Any())
            {
                return string.Empty;
            }
            var result = input.Aggregate(string.Empty, (current, pair) => current + $"{pair.Key}={pair.Value}|");
            return result.Remove(result.Length - 1);
        }
    }
}
