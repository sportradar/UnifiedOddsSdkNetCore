// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using Sportradar.OddsFeed.SDK.Common.Extensions;

namespace Sportradar.OddsFeed.SDK.Common.Internal.Telemetry
{
    internal class TelemetryTracker : IDisposable
    {
        private bool _isDisposed;
        private readonly DateTime _startTime;
        private readonly Histogram<long> _histogram;
        private readonly TagList _tags;

        public TimeSpan Elapsed => DateTime.UtcNow - _startTime;

        public TagList Tags => _tags;

        public bool IsDisposed => _isDisposed;

        public TelemetryTracker(Histogram<long> histogram)
        {
            _histogram = histogram ?? throw new ArgumentNullException(nameof(histogram));
            _startTime = DateTime.UtcNow;
            _tags = new TagList();
        }

        public TelemetryTracker(Histogram<long> histogram, string tagKey, object tagValue)
        {
            _histogram = histogram;
            _startTime = DateTime.UtcNow;
            _tags = new TagList { { tagKey, tagValue } };
        }

        public TelemetryTracker(Histogram<long> histogram, IDictionary<string, string> inputTags)
        {
            _histogram = histogram;
            _startTime = DateTime.UtcNow;

            _tags = new TagList();
            if (!inputTags.IsNullOrEmpty())
            {
                foreach (var tag in inputTags)
                {
                    _tags.Add(tag.Key, tag.Value);
                }
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_isDisposed)
            {
                return;
            }
            if (disposing)
            {
                var duration = DateTime.UtcNow - _startTime;
                _histogram.Record((long)duration.TotalMilliseconds, _tags);
            }

            _isDisposed = true;
        }
    }
}
