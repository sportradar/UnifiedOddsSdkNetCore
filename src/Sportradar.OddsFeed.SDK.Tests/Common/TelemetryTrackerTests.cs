// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Threading.Tasks;
using Sportradar.OddsFeed.SDK.Common.Internal.Telemetry;
using Xunit;

namespace Sportradar.OddsFeed.SDK.Tests.Common;
public class TelemetryTrackerTests
{
    private readonly Meter _meter;

    public TelemetryTrackerTests()
    {
        _meter = new Meter("TestMeter");
    }

    [Fact]
    public void ConstructorWhenWithNullHistogramThenThrows()
    {
        Assert.Throws<ArgumentNullException>(() => new TelemetryTracker(null));
    }

    [Fact]
    public void ConstructorWhenInitializesFieldsCorrectlyThenSucceed()
    {
        var tracker = new TelemetryTracker(_meter.CreateHistogram<long>("some-name"));

        Assert.NotNull(tracker);
        Assert.Empty(tracker.Tags);
        Assert.True(tracker.Elapsed.TotalMilliseconds > 0);
    }

    [Fact]
    public void ConstructorWhenWithSingleTagsThenInitializesFieldsCorrectly()
    {
        var tagKey = "key";
        var tagValue = "value";

        var tracker = new TelemetryTracker(_meter.CreateHistogram<long>("some-name"), tagKey, tagValue);

        Assert.NotNull(tracker);
        Assert.NotEmpty(tracker.Tags);
        Assert.Equal(tagKey, tracker.Tags[0].Key);
        Assert.Equal(tagValue, tracker.Tags[0].Value);
        Assert.True(tracker.Elapsed.TotalMilliseconds > 0);
    }

    [Fact]
    public void ConstructorWhenWithSingleDictionaryTagsThenInitializesFieldsCorrectly()
    {
        var tags = new Dictionary<string, string> { { "key1", "value1" } };

        var tracker = new TelemetryTracker(_meter.CreateHistogram<long>("some-name"), tags);

        Assert.NotNull(tracker);
        Assert.NotEmpty(tracker.Tags);
        Assert.Single(tracker.Tags);
        Assert.Equal(tags.Keys.First(), tracker.Tags[0].Key);
        Assert.Equal(tags.Values.First(), tracker.Tags[0].Value);
        Assert.True(tracker.Elapsed.TotalMilliseconds > 0);
    }

    [Fact]
    public void ConstructorWhenWithDictionaryTagsThenInitializesFieldsCorrectly()
    {
        var tags = new Dictionary<string, string> { { "key1", "value1" }, { "key2", "value2" } };

        var tracker = new TelemetryTracker(_meter.CreateHistogram<long>("some-name"), tags);

        Assert.NotNull(tracker);
        Assert.NotEmpty(tracker.Tags);
        Assert.Equal(2, tracker.Tags.Count);
        Assert.Equal(tags.Keys.First(), tracker.Tags[0].Key);
        Assert.Equal(tags.Values.First(), tracker.Tags[0].Value);
        Assert.Equal(tags.Keys.Skip(1).First(), tracker.Tags[1].Key);
        Assert.Equal(tags.Values.Skip(1).First(), tracker.Tags[1].Value);
        Assert.True(tracker.Elapsed.TotalMilliseconds > 0);
    }

    [Fact]
    public async Task WhenDisposeThenRecordsToHistogram()
    {
        var hist = _meter.CreateHistogram<long>("some-name");
        using var tracker = new TelemetryTracker(hist);
        await Task.Delay(TimeSpan.FromMilliseconds(500));

        Assert.NotNull(hist);
        Assert.NotNull(tracker);
        Assert.True(tracker.Elapsed.TotalMilliseconds >= 450, $"Expected: {tracker.Elapsed.TotalMilliseconds} >= 450");
    }

    [Fact]
    [SuppressMessage("Major Code Smell", "S3966:Objects should not be disposed more than once", Justification = "Allowed in this test")]
    public void WhenDisposedTwiceThenDoesNotThrow()
    {
        var hist = _meter.CreateHistogram<long>("some-name");
        var tracker = new TelemetryTracker(hist);
        Assert.NotNull(tracker);

        tracker.Dispose();
        tracker.Dispose();

        Assert.True(tracker.IsDisposed);
    }
}
