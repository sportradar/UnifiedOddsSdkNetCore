/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using System.Collections.Generic;
using Common.Logging;

namespace Sportradar.OddsFeed.SDK.Common.Internal.Metrics.Reports
{
    /// <summary>
    /// Enum MetricsReportPrintMode used to define mode of printing of information into log
    /// </summary>
    public enum MetricsReportPrintMode
    {
        /// <summary>
        /// The normal
        /// </summary>
        Normal = 0,
        /// <summary>
        /// The minimal
        /// </summary>
        Minimal = 1,
        /// <summary>
        /// The compact
        /// </summary>
        Compact = 2,
        /// <summary>
        /// The full
        /// </summary>
        Full = 3
    };

    internal abstract class BaseReport<T>
    {
        private string _contextName;
        private string _logFormat;
        protected readonly int Decimals;
        protected readonly MetricsReportPrintMode PrintMode;

        protected ILog Log;
        protected string ContextName => _contextName;
        protected readonly Queue<KeyValuePair<string, string>> LogQueue;

        public string LogFormat => _logFormat;

        protected BaseReport(string context = null, ILog log = null, MetricsReportPrintMode printMode = MetricsReportPrintMode.Normal, int decimals = 2)
        {
            Log = log;
            PrintMode = printMode;
            Decimals = decimals;
            _contextName = context;
            LogQueue = new Queue<KeyValuePair<string, string>>();
        }

        public virtual void ReportList(IEnumerable<T> items)
        {
            if (items == null)
            {
                return;
            }
            foreach (var md in items)
            {
                Report(md, true);
            }

            PrintQueue();
        }

        public abstract void Report(T item, bool suppressPrint = false);

        protected virtual void SetContextName(string context)
        {
            _contextName = context;
        }

        protected virtual void SetFormat(string format)
        {
            _logFormat = format;
        }

        protected virtual void SetLog(string loggerName)
        {
            if (Log == null && !string.IsNullOrEmpty(loggerName))
            {
                Log = LogManager.GetLogger(loggerName);
            }
        }

        protected virtual void QueueAdd(string key, string value = null)
        {
            LogQueue.Enqueue(new KeyValuePair<string, string>(key, value));
        }

        protected virtual void PrintQueue()
        {
            lock (LogQueue)
            {
                Log.Info(string.Empty);
                while (LogQueue.Count > 0)
                {
                    var kv = LogQueue.Dequeue();
                    if (string.IsNullOrEmpty(kv.Value))
                    {
                        var space = (kv.Key.Length / 2) + 30;
                        var text = "{0," + space + "}";
                        Log.Info(string.Format(text, kv.Key));
                    }
                    else
                    {
                        Log.Info($"{kv.Key,25} = {kv.Value}");
                    }
                }
                Log.Info(string.Empty);
            }
        }

        protected virtual void PrintSelector(T item)
        {
            switch (PrintMode)
            {
                case MetricsReportPrintMode.Normal:
                    Print(item);
                    break;
                case MetricsReportPrintMode.Minimal:
                    PrintMinimal(item);
                    break;
                case MetricsReportPrintMode.Compact:
                    PrintCompact(item);
                    break;
                case MetricsReportPrintMode.Full:
                    PrintFull(item);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        protected abstract void Print(T item);

        protected virtual void PrintMinimal(T item)
        {
            Print(item);
        }

        protected virtual void PrintCompact(T item)
        {
            Print(item);
        }

        protected virtual void PrintFull(T item)
        {
            Print(item);
        }
    }
}
