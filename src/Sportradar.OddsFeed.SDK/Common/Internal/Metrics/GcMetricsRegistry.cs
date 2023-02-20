/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using App.Metrics;
using App.Metrics.Gauge;

namespace Sportradar.OddsFeed.SDK.Common.Internal.Metrics
{
    //TODO: should be replaced with App.Metrics.App.All
    internal static class GcMetricsRegistry
    {
        public static readonly string ContextName = "Dotnet.Gc";

        public static class Gauges
        {
            public static readonly GaugeOptions Gen0Collections = new GaugeOptions
            {
                Context = ContextName,
                Name = "Gen 0 Collections",
                MeasurementUnit = Unit.Events
            };

            public static readonly GaugeOptions Gen1Collections = new GaugeOptions
            {
                Context = ContextName,
                Name = "Gen 1 Collections",
                MeasurementUnit = Unit.Events
            };

            public static readonly GaugeOptions Gen2Collections = new GaugeOptions
            {
                Context = ContextName,
                Name = "Gen 2 Collections",
                MeasurementUnit = Unit.Events
            };

            public static readonly GaugeOptions LiveObjectsSize = new GaugeOptions
            {
                Context = ContextName,
                Name = "Live Objects Size",
                MeasurementUnit = Unit.Bytes
            };

            public static readonly GaugeOptions Gen0HeapSize = new GaugeOptions
            {
                Context = ContextName,
                Name = "Gen 0 Heap Size",
                MeasurementUnit = Unit.Bytes
            };

            public static readonly GaugeOptions Gen1HeapSize = new GaugeOptions
            {
                Context = ContextName,
                Name = "Gen 1 Heap Size",
                MeasurementUnit = Unit.Bytes
            };

            public static readonly GaugeOptions Gen2HeapSize = new GaugeOptions
            {
                Context = ContextName,
                Name = "Gen 2 Heap Size",
                MeasurementUnit = Unit.Bytes
            };

            public static readonly GaugeOptions LargeObjectHeapSize = new GaugeOptions
            {
                Context = ContextName,
                Name = "Large Object Heap Size",
                MeasurementUnit = Unit.Bytes
            };

            public static readonly GaugeOptions BytesPromotedFromGen0 = new GaugeOptions
            {
                Context = ContextName,
                Name = "Bytes Promoted From Gen 0",
                MeasurementUnit = Unit.Bytes
            };

            public static readonly GaugeOptions BytesPromotedFromGen1 = new GaugeOptions
            {
                Context = ContextName,
                Name = "Bytes Promoted From Gen 1",
                MeasurementUnit = Unit.Bytes
            };

            public static readonly GaugeOptions BytesSurvivedFromGen2 = new GaugeOptions
            {
                Context = ContextName,
                Name = "Bytes Survived From Gen 2",
                MeasurementUnit = Unit.Bytes
            };

            public static readonly GaugeOptions BytesSurvivedLargeObjectHeap = new GaugeOptions
            {
                Context = ContextName,
                Name = "Bytes Survived Large Object Heap",
                MeasurementUnit = Unit.Bytes
            };
        }
    }
}
