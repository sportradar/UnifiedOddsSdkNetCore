// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using OpenTelemetry.Metrics;
using Shouldly;

namespace Sportradar.OddsFeed.SDK.Tests.Common.Extensions.Assertions;

public static class MetricAssertionExtensions
{
    public static void ShouldHaveTag(this Metric metric, string tagKey, string tagValue)
    {
        var tagFound = false;
        foreach (var metricPoint in metric.GetMetricPoints())
        {
            foreach (var tag in metricPoint.Tags)
            {
                if (tag.Key != tagKey || !string.Equals(tag.Value?.ToString(), tagValue, StringComparison.Ordinal))
                {
                    continue;
                }

                tagFound = true;
                break;
            }

            if (tagFound)
            {
                break;
            }
        }

        tagFound.ShouldBeTrue($"Expected metric to have tag with key '{tagKey}' and value '{tagValue}'");
    }
}
