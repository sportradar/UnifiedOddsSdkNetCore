// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using System.Collections.Generic;
using System.Diagnostics;
using OpenTelemetry;
using OpenTelemetry.Exporter;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using Sportradar.OddsFeed.SDK.Api.Config;

namespace Sportradar.OddsFeed.SDK.Common.Internal.Telemetry
{
    internal static class UsageTelemetry
    {
        internal const string EndpointUrl = "/v1/metrics";

        public static MeterProvider SetupUsageTelemetry(IUofConfiguration config)
        {
            return config.Usage.IsExportEnabled
                ? SetupUsageExporter(config)
                : null;
        }

        private static MeterProvider SetupUsageExporter(IUofConfiguration config)
        {
            var resourceServiceName = $"odds-feed-sdk_usage_{config.Environment}".ToLowerInvariant();
            const string resourceServiceNamespace = "Sportradar.OddsFeed.SDKCore";

            var resourceBuilder = ResourceBuilder.CreateDefault()
                                                 .AddService(resourceServiceName, resourceServiceNamespace, SdkInfo.GetVersion())
                                                 .AddAttributes(new[]
                                                                    {
                                                                        new KeyValuePair<string, object>("nodeId", config.NodeId.ToString()),
                                                                        new KeyValuePair<string, object>("environment", config.Environment.ToString().ToLowerInvariant()),
                                                                        new KeyValuePair<string, object>("metricsVersion", UofSdkTelemetry.MetricsVersion),
                                                                        new KeyValuePair<string, object>("bookmakerId", config.BookmakerDetails.BookmakerId.ToString())
                                                                    });

            var usageMeterProvider = Sdk
                                    .CreateMeterProviderBuilder()
                                    .SetupUsageMetricsForExporter()
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
                                                         exporterOptions.Headers = $"x-access-token={config.AccessToken},x-environment={config.Environment.ToString().ToLowerInvariant()},x-node-id={config.NodeId},x-sdk-version={SdkInfo.GetVersion()}";
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

        private static MeterProviderBuilder SetupUsageMetricsForExporter(this MeterProviderBuilder builder)
        {
            return builder
                  .AddMeter(UofSdkTelemetry.ServiceName)
                  .AddView(UofSdkTelemetry.MetricNameForProducerStatus, UofSdkTelemetry.MetricNameForProducerStatus)
                  .AddView("*", MetricStreamConfiguration.Drop);
        }
    }
}
