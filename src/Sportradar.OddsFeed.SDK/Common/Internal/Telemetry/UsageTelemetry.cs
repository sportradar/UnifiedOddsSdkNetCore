// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using OpenTelemetry;
using OpenTelemetry.Exporter;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using Sportradar.OddsFeed.SDK.Api.Config;

namespace Sportradar.OddsFeed.SDK.Common.Internal.Telemetry
{
    internal static class UsageTelemetry
    {
        private const string MetricsVersion = "v1";
        internal const string EndpointUrl = "/v1/metrics";
        private const string UsageServiceName = "UofSdk-NetStd-Usage";
        internal static readonly Meter UsageMeter = new Meter(UsageServiceName, SdkInfo.GetVersion());

        public static MeterProvider SetupUsageTelemetry(IUofConfiguration config)
        {
            SetupTelemetryObjects();

            return config.Usage.IsExportEnabled
                ? SetupUsageExporter(config)
                : null;
        }

        private static void SetupTelemetryObjects()
        {
            // setup all histograms and counters
        }

        private static MeterProvider SetupUsageExporter(IUofConfiguration config)
        {
            var resourceServiceName = $"odds-feed-sdk_usage_{config.Environment}".ToLowerInvariant();
            const string resourceServiceNamespace = "Sportradar.OddsFeed.SDKCore";

            var resourceBuilder = ResourceBuilder.CreateDefault()
                                                 .AddService(resourceServiceName, resourceServiceNamespace, SdkInfo.GetVersion())
                                                 .AddAttributes(new[]
                                                                    {
                                                                        new KeyValuePair<string, object>("nodeId", config.NodeId),
                                                                        new KeyValuePair<string, object>("environment", config.Environment.ToString().ToLowerInvariant()),
                                                                        new KeyValuePair<string, object>("metricsVersion", MetricsVersion)
                                                                    });

            var usageMeterProvider = Sdk
                                    .CreateMeterProviderBuilder()
                                    .AddMeter(UsageServiceName)
                                    .AddOtlpExporter((exporterOptions, metricReaderOptions) =>
                                                     {
                                                         exporterOptions.Endpoint = new Uri(config.Usage.Host + EndpointUrl);
                                                         exporterOptions.Protocol = OtlpExportProtocol.HttpProtobuf;
                                                         exporterOptions.ExportProcessorType = ExportProcessorType.Batch; // set to Batch for periodic exporting
                                                         exporterOptions.BatchExportProcessorOptions = new BatchExportProcessorOptions<Activity>
                                                         {
                                                             MaxExportBatchSize = 102400,
                                                             MaxQueueSize = 100000,
                                                             ScheduledDelayMilliseconds = config.Usage.ExportIntervalInSec * 1000,
                                                             ExporterTimeoutMilliseconds = config.Usage.ExportTimeoutInSec * 1000
                                                         };
                                                         exporterOptions.Headers = $"x-access-token={config.AccessToken},x-environment={config.Environment},x-node-id={config.NodeId},x-sdk-version={SdkInfo.GetVersion()}";
                                                         exporterOptions.TimeoutMilliseconds = config.Usage.ExportTimeoutInSec * 1000;
                                                         metricReaderOptions.TemporalityPreference = MetricReaderTemporalityPreference.Delta;
                                                         metricReaderOptions.PeriodicExportingMetricReaderOptions = new PeriodicExportingMetricReaderOptions
                                                         {
                                                             ExportIntervalMilliseconds = config.Usage.ExportIntervalInSec * 1000,
                                                             ExportTimeoutMilliseconds = config.Usage.ExportTimeoutInSec * 1000
                                                         };
                                                     })
                                    .SetResourceBuilder(resourceBuilder)
                                    .Build();

            return usageMeterProvider;
        }
    }
}
