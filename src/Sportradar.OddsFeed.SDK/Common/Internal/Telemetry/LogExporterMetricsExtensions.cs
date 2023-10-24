using System;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OpenTelemetry;
using OpenTelemetry.Metrics;

namespace Sportradar.OddsFeed.SDK.Common.Internal.Telemetry
{
    /// <summary>
    /// Extension methods to simplify registering of the Log exporter.
    /// </summary>
    internal static class LogExporterMetricsExtensions
    {
        private const int DefaultExportIntervalMilliseconds = 60000;
        private const int DefaultExportTimeoutMilliseconds = Timeout.Infinite;

        /// <summary>
        /// Adds <see cref="MetricsLogExporter"/> to the <see cref="MeterProviderBuilder"/> using default options.
        /// </summary>
        /// <param name="builder"><see cref="MeterProviderBuilder"/> builder to use.</param>
        /// <returns>The instance of <see cref="MeterProviderBuilder"/> to chain the calls.</returns>
        public static MeterProviderBuilder AddLogExporter(this MeterProviderBuilder builder) => AddLogExporter(builder, name: null, configureExporter: null);

        /// <summary>
        /// Adds <see cref="MetricsLogExporter"/> to the <see cref="MeterProviderBuilder"/>.
        /// </summary>
        /// <param name="builder"><see cref="MeterProviderBuilder"/> builder to use.</param>
        /// <param name="configureExporter">Callback action for configuring <see cref="LogExporterOptions"/>.</param>
        /// <returns>The instance of <see cref="MeterProviderBuilder"/> to chain the calls.</returns>
        public static MeterProviderBuilder AddLogExporter(this MeterProviderBuilder builder, Action<LogExporterOptions> configureExporter) => AddLogExporter(builder, name: null, configureExporter);

        /// <summary>
        /// Adds <see cref="MetricsLogExporter"/> to the <see cref="MeterProviderBuilder"/>.
        /// </summary>
        /// <param name="builder"><see cref="MeterProviderBuilder"/> builder to use.</param>
        /// <param name="name">Name which is used when retrieving options.</param>
        /// <param name="configureExporter">Callback action for configuring <see cref="LogExporterOptions"/>.</param>
        /// <returns>The instance of <see cref="MeterProviderBuilder"/> to chain the calls.</returns>
        public static MeterProviderBuilder AddLogExporter(
            this MeterProviderBuilder builder,
            string name,
            Action<LogExporterOptions> configureExporter)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            name = name ?? Options.DefaultName;

            if (configureExporter != null)
            {
                builder.ConfigureServices(services => services.Configure(name, configureExporter));
            }

            return builder.AddReader(sp =>
            {
                return BuildLogExporterMetricReader(
                    sp.GetRequiredService<IOptionsMonitor<LogExporterOptions>>().Get(name),
                    sp.GetRequiredService<IOptionsMonitor<MetricReaderOptions>>().Get(name));
            });
        }

        /// <summary>
        /// Adds <see cref="MetricsLogExporter"/> to the <see cref="MeterProviderBuilder"/>.
        /// </summary>
        /// <param name="builder"><see cref="MeterProviderBuilder"/> builder to use.</param>
        /// <param name="configureExporterAndMetricReader">Callback action for
        /// configuring <see cref="LogExporterOptions"/> and <see
        /// cref="MetricReaderOptions"/>.</param>
        /// <returns>The instance of <see cref="MeterProviderBuilder"/> to chain the calls.</returns>
        public static MeterProviderBuilder AddLogExporter(this MeterProviderBuilder builder, Action<LogExporterOptions, MetricReaderOptions> configureExporterAndMetricReader) => AddLogExporter(builder, name: null, configureExporterAndMetricReader);

        /// <summary>
        /// Adds <see cref="MetricsLogExporter"/> to the <see cref="MeterProviderBuilder"/>.
        /// </summary>
        /// <param name="builder"><see cref="MeterProviderBuilder"/> builder to use.</param>
        /// <param name="name">Name which is used when retrieving options.</param>
        /// <param name="configureExporterAndMetricReader">Callback action for
        /// configuring <see cref="LogExporterOptions"/> and <see
        /// cref="MetricReaderOptions"/>.</param>
        /// <returns>The instance of <see cref="MeterProviderBuilder"/> to chain the calls.</returns>
        public static MeterProviderBuilder AddLogExporter(
            this MeterProviderBuilder builder,
            string name,
            Action<LogExporterOptions, MetricReaderOptions> configureExporterAndMetricReader)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            name = name ?? Options.DefaultName;

            return builder.AddReader(sp =>
            {
                var exporterOptions = sp.GetRequiredService<IOptionsMonitor<LogExporterOptions>>().Get(name);
                var metricReaderOptions = sp.GetRequiredService<IOptionsMonitor<MetricReaderOptions>>().Get(name);

                configureExporterAndMetricReader?.Invoke(exporterOptions, metricReaderOptions);

                return BuildLogExporterMetricReader(exporterOptions, metricReaderOptions);
            });
        }

        private static MetricReader BuildLogExporterMetricReader(
            LogExporterOptions exporterOptions,
            MetricReaderOptions metricReaderOptions)
        {
            var metricExporter = new MetricsLogExporter(exporterOptions);

            return CreatePeriodicExportingMetricReader(
                metricExporter,
                metricReaderOptions,
                DefaultExportIntervalMilliseconds,
                DefaultExportTimeoutMilliseconds);
        }

        private static PeriodicExportingMetricReader CreatePeriodicExportingMetricReader(
            BaseExporter<Metric> exporter,
            MetricReaderOptions options,
            int defaultExportIntervalMilliseconds = DefaultExportIntervalMilliseconds,
            int defaultExportTimeoutMilliseconds = DefaultExportTimeoutMilliseconds)
        {
            var exportInterval =
                options.PeriodicExportingMetricReaderOptions.ExportIntervalMilliseconds ?? defaultExportIntervalMilliseconds;

            var exportTimeout =
                options.PeriodicExportingMetricReaderOptions.ExportTimeoutMilliseconds ?? defaultExportTimeoutMilliseconds;

            var metricReader = new PeriodicExportingMetricReader(exporter, exportInterval, exportTimeout)
            {
                TemporalityPreference = options.TemporalityPreference,
            };

            return metricReader;
        }
    }
}
