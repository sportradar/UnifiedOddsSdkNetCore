using System;
using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace Sportradar.OddsFeed.SDK.DemoProject.Utils;

internal class TelemetryTracker : IDisposable
{
    private bool _isDisposed;
    private readonly DateTime _startTime;
    private readonly Histogram<long> _histogram;
    private readonly TagList _tags;
    public TimeSpan Elapsed => DateTime.UtcNow - _startTime;

    public TelemetryTracker(Histogram<long> histogram)
    {
        _histogram = histogram;
        _startTime = DateTime.UtcNow;
        _tags = [];
    }

    public TelemetryTracker(Histogram<long> histogram, string tagKey, object tagValue)
    {
        _histogram = histogram;
        _startTime = DateTime.UtcNow;
        _tags = new TagList { { tagKey, tagValue } };
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
