using System.Globalization;
using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using OpenTelemetry;
using OpenTelemetry.Metrics;

namespace Sportradar.OddsFeed.SDK.Common.Internal.Telemetry
{
    internal class MetricsLogExporter : BaseExporter<Metric>
    {
        private readonly ILogger _logger;

        public MetricsLogExporter(ILogger logger)
        {
            _logger = logger;
        }

        public MetricsLogExporter(LogExporterOptions options)
        {
            _logger = new NullLoggerFactory().CreateLogger(options.LoggerName);
        }

        public override ExportResult Export(in Batch<Metric> batch)
        {
            using var scope = SuppressInstrumentationScope.Begin();

            foreach (var metric in batch)
            {
                var msg = new StringBuilder("\nExport ");
                msg.Append(metric.Name);
                if (metric.Description != string.Empty)
                {
                    msg.Append(", ");
                    msg.Append(metric.Description);
                }

                if (metric.Unit != string.Empty)
                {
                    msg.Append($", Unit: {metric.Unit}");
                }

                if (!string.IsNullOrEmpty(metric.MeterName))
                {
                    msg.Append($", Meter: {metric.MeterName}");

                    if (!string.IsNullOrEmpty(metric.MeterVersion))
                    {
                        msg.Append($"/{metric.MeterVersion}");
                    }
                }

                _logger.LogInformation(msg.ToString());

                foreach (ref readonly var metricPoint in metric.GetMetricPoints())
                {
                    var valueDisplay = string.Empty;
                    var tagsBuilder = new StringBuilder();
                    foreach (var tag in metricPoint.Tags)
                    {
                        if (LogTagTransformer.Instance.TryTransformTag(tag, out var result))
                        {
                            tagsBuilder.Append(result);
                            tagsBuilder.Append(' ');
                        }
                    }

                    var tags = tagsBuilder.ToString().TrimEnd();

                    var metricType = metric.MetricType;

                    if (metricType == MetricType.Histogram)
                    {
                        var bucketsBuilder = new StringBuilder();
                        var sum = metricPoint.GetHistogramSum();
                        var count = metricPoint.GetHistogramCount();
                        bucketsBuilder.Append($"Sum: {sum.ToString()} Count: {count.ToString()} ");
                        if (metricPoint.TryGetHistogramMinMaxValues(out var min, out var max))
                        {
                            bucketsBuilder.Append($"Min: {min.ToString()} Max: {max.ToString()} ");
                        }

                        bucketsBuilder.AppendLine();

                        if (metricType == MetricType.Histogram)
                        {
                            var isFirstIteration = true;
                            double previousExplicitBound = default;
                            foreach (var histogramMeasurement in metricPoint.GetHistogramBuckets())
                            {
                                if (isFirstIteration)
                                {
                                    bucketsBuilder.Append("(-Infinity,");
                                    bucketsBuilder.Append(histogramMeasurement.ExplicitBound);
                                    bucketsBuilder.Append(']');
                                    bucketsBuilder.Append(':');
                                    bucketsBuilder.Append(histogramMeasurement.BucketCount);
                                    previousExplicitBound = histogramMeasurement.ExplicitBound;
                                    isFirstIteration = false;
                                }
                                else
                                {
                                    bucketsBuilder.Append('(');
                                    bucketsBuilder.Append(previousExplicitBound);
                                    bucketsBuilder.Append(',');
                                    if (histogramMeasurement.ExplicitBound != double.PositiveInfinity)
                                    {
                                        bucketsBuilder.Append(histogramMeasurement.ExplicitBound);
                                        previousExplicitBound = histogramMeasurement.ExplicitBound;
                                    }
                                    else
                                    {
                                        bucketsBuilder.Append("+Infinity");
                                    }

                                    bucketsBuilder.Append(']');
                                    bucketsBuilder.Append(':');
                                    bucketsBuilder.Append(histogramMeasurement.BucketCount);
                                }

                                bucketsBuilder.AppendLine();
                            }
                        }
                        else
                        {
                            // TODO: Consider how/if to display buckets for exponential histograms.
                            bucketsBuilder.AppendLine("Buckets are not displayed for exponential histograms.");
                        }

                        valueDisplay = bucketsBuilder.ToString();
                    }
                    else if (metricType.IsDouble())
                    {
                        if (metricType.IsSum())
                        {
                            valueDisplay = metricPoint.GetSumDouble().ToString(CultureInfo.InvariantCulture);
                        }
                        else
                        {
                            valueDisplay = metricPoint.GetGaugeLastValueDouble().ToString(CultureInfo.InvariantCulture);
                        }
                    }
                    else if (metricType.IsLong())
                    {
                        if (metricType.IsSum())
                        {
                            valueDisplay = metricPoint.GetSumLong().ToString(CultureInfo.InvariantCulture);
                        }
                        else
                        {
                            valueDisplay = metricPoint.GetGaugeLastValueLong().ToString(CultureInfo.InvariantCulture);
                        }
                    }

                    var exemplarString = new StringBuilder();
                    foreach (var exemplar in metricPoint.GetExemplars())
                    {
                        if (exemplar.Timestamp != default)
                        {
                            exemplarString.Append("Value: ");
                            exemplarString.Append(exemplar.DoubleValue);
                            exemplarString.Append(" Timestamp: ");
                            exemplarString.Append(exemplar.Timestamp.ToString("yyyy-MM-ddTHH:mm:ss.fffffffZ", CultureInfo.InvariantCulture));
                            exemplarString.Append(" TraceId: ");
                            exemplarString.Append(exemplar.TraceId);
                            exemplarString.Append(" SpanId: ");
                            exemplarString.Append(exemplar.SpanId);

                            if (exemplar.FilteredTags != null && exemplar.FilteredTags.Count > 0)
                            {
                                exemplarString.Append(" Filtered Tags : ");

                                foreach (var tag in exemplar.FilteredTags)
                                {
                                    if (LogTagTransformer.Instance.TryTransformTag(tag, out var result))
                                    {
                                        exemplarString.Append(result);
                                        exemplarString.Append(' ');
                                    }
                                }
                            }

                            exemplarString.AppendLine();
                        }
                    }

                    msg = new StringBuilder();
                    msg.Append('(');
                    msg.Append(metricPoint.StartTime.ToString("yyyy-MM-ddTHH:mm:ss.fffffffZ", CultureInfo.InvariantCulture));
                    msg.Append(", ");
                    msg.Append(metricPoint.EndTime.ToString("yyyy-MM-ddTHH:mm:ss.fffffffZ", CultureInfo.InvariantCulture));
                    msg.Append("] ");
                    msg.Append(tags);
                    if (tags != string.Empty)
                    {
                        msg.Append(' ');
                    }

                    msg.Append(metric.MetricType);
                    msg.AppendLine();
                    msg.Append($"Value: {valueDisplay}");

                    if (exemplarString.Length > 0)
                    {
                        msg.AppendLine();
                        msg.AppendLine("Exemplars");
                        msg.Append(exemplarString.ToString());
                    }

                    _logger.LogInformation(msg.ToString());
                }
            }

            return ExportResult.Success;
        }
    }
}
