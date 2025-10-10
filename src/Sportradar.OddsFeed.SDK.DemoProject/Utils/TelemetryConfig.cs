using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace Sportradar.OddsFeed.SDK.DemoProject.Utils;

public class TelemetryConfig
{
    // .NET runtime libraries have metric names using '-' if a separator is needed.
    private const string ServiceName = "demoproject";
    private const string MetricNamePrefix = ServiceName + "-";

    public static readonly ActivitySource ActivitySource = new ActivitySource(ServiceName);

    private static readonly Meter DefaultMeter = new Meter(ServiceName);

    private const string MetricNameForOddsChangeReceived = MetricNamePrefix + "oddschangereceived";
    private const string MetricNameForBetStopReceived = MetricNamePrefix + "betstopreceived";
    private const string MetricNameForFixtureChangeReceived = MetricNamePrefix + "fixturechangereceived";
    private const string MetricNameForBetSettlementReceived = MetricNamePrefix + "betsettlementreceived";
    private const string MetricNameForBetCancelReceived = MetricNamePrefix + "betcancelreceived";
    private const string MetricNameForRollbackBetSettlementReceived = MetricNamePrefix + "rollbackbetsettlementreceived";
    private const string MetricNameForRollbackBetCancelReceived = MetricNamePrefix + "rollbackbetcancelreceived";
    private const string MetricNameForWriteMarketData = MetricNamePrefix + "writemarketdata";
    private const string MetricNameForWriteSportData = MetricNamePrefix + "writesportdata";
    public static readonly Histogram<long> OddsChangeReceived = DefaultMeter.CreateHistogram<long>(MetricNameForOddsChangeReceived);
    public static readonly Histogram<long> BetStopReceived = DefaultMeter.CreateHistogram<long>(MetricNameForBetStopReceived);
    public static readonly Histogram<long> FixtureChangeReceived = DefaultMeter.CreateHistogram<long>(MetricNameForFixtureChangeReceived);
    public static readonly Histogram<long> BetSettlementReceived = DefaultMeter.CreateHistogram<long>(MetricNameForBetSettlementReceived);
    public static readonly Histogram<long> BetCancelReceived = DefaultMeter.CreateHistogram<long>(MetricNameForBetCancelReceived);
    public static readonly Histogram<long> RollbackBetSettlementReceived = DefaultMeter.CreateHistogram<long>(MetricNameForRollbackBetSettlementReceived);
    public static readonly Histogram<long> RollbackBetCancelReceived = DefaultMeter.CreateHistogram<long>(MetricNameForRollbackBetCancelReceived);
    public static readonly Histogram<long> WriteMarketData = DefaultMeter.CreateHistogram<long>(MetricNameForWriteMarketData);
    public static readonly Histogram<long> WriteSportData = DefaultMeter.CreateHistogram<long>(MetricNameForWriteSportData);
}
