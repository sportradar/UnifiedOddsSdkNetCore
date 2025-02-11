// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System.Linq;
using System.Net;
using System.Text;
using FluentAssertions;
using Moq;
using OpenTelemetry.Metrics;
using Sportradar.OddsFeed.SDK.Api.Config;
using Sportradar.OddsFeed.SDK.Api.Internal.Managers;
using Sportradar.OddsFeed.SDK.Common.Enums;
using Sportradar.OddsFeed.SDK.Common.Internal.Telemetry;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;
using Xunit;
using Xunit.Abstractions;

namespace Sportradar.OddsFeed.SDK.Tests.Common.Telemetry;

public class UsageTelemetryTests
{
    private const string MetricNameForTestHistogram = UofSdkTelemetry.MetricNamePrefix + "test-histogram";
    private readonly ITestOutputHelper _outputHelper;

    // ReSharper disable once ConvertToPrimaryConstructor
    public UsageTelemetryTests(ITestOutputHelper outputHelper)
    {
        _outputHelper = outputHelper;
    }

    [Fact]
    public void UsageTelemetryMeterWhenSetupIsNotRunThenMeterIsInitialized()
    {
        UsageTelemetry.UsageMeter.Should().NotBeNull();
    }

    [Fact]
    public void UsageTelemetryMeterWhenExportEndpointReachableThenCanExport()
    {
        var wireMockServer = SetupWireMockServerWithEndpoint();

        var mockConfig = MockUofConfigurationWithStatisticsInterval(wireMockServer, true);

        var meterProvider = UsageTelemetry.SetupUsageTelemetry(mockConfig.Object);
        var testHistogram = UsageTelemetry.UsageMeter.CreateHistogram<long>(MetricNameForTestHistogram);
        testHistogram.Record(1234);

        var flushSucceeded = meterProvider.ForceFlush();
        var logEntries = wireMockServer.LogEntries.ToList();

        flushSucceeded.Should().BeTrue();
        logEntries.Should().HaveCount(1);
        logEntries.First().ResponseMessage.StatusCode.Should().Be(202);
    }

    [Fact]
    public void UsageTelemetryMeterWhenExportEndpointNotReachableThenCanNotExport()
    {
        var wireMockServer = SetupWireMockServerWithEndpoint("/non-existing-endpoint");

        var mockConfig = MockUofConfigurationWithStatisticsInterval(wireMockServer, true);

        var meterProvider = UsageTelemetry.SetupUsageTelemetry(mockConfig.Object);
        var testHistogram = UsageTelemetry.UsageMeter.CreateHistogram<long>(MetricNameForTestHistogram);
        testHistogram.Record(1234);

        var flushSucceeded = meterProvider.ForceFlush();
        var logEntries = wireMockServer.LogEntries.ToList();

        flushSucceeded.Should().BeFalse();
        logEntries.Should().HaveCount(1);
        logEntries.First().ResponseMessage.StatusCode.Should().Be(404);
    }

    [Fact]
    public void UsageTelemetryMeterWhenMetricsExportDisabledThenMeterIsNotConfigured()
    {
        var wireMockServer = SetupWireMockServerWithEndpoint();
        var mockConfig = MockUofConfigurationWithStatisticsInterval(wireMockServer, false);

        var meterProvider = UsageTelemetry.SetupUsageTelemetry(mockConfig.Object);

        meterProvider.Should().BeNull();
    }

    [Fact]
    public void UsageTelemetryMeterWhenMetricsExportDisabledThenMeterObjectCanStillBeUsed()
    {
        var wireMockServer = SetupWireMockServerWithEndpoint();
        var mockConfig = MockUofConfigurationWithStatisticsInterval(wireMockServer, false);

        var meterProvider = UsageTelemetry.SetupUsageTelemetry(mockConfig.Object);
        var testHistogram = UsageTelemetry.UsageMeter.CreateHistogram<long>(MetricNameForTestHistogram);
        testHistogram.Record(1234);

        meterProvider.Should().BeNull();
    }

    [Fact]
    public void UsageTelemetryMeterWhenExportedProducerStatusIsIncluded()
    {
        var wireMockServer = SetupWireMockServerWithEndpoint();
        var mockConfig = MockUofConfigurationWithStatisticsInterval(wireMockServer, true);
        var mockProducerConfig = new Mock<IUofProducerConfiguration>();
        mockProducerConfig.Setup(s => s.DisabledProducers).Returns([]);
        mockProducerConfig.Setup(s => s.Producers).Returns(TestConfiguration.GetConfig().Producer.Producers);
        mockConfig.Setup(s => s.Producer).Returns(mockProducerConfig.Object);
        var meterProvider = UsageTelemetry.SetupUsageTelemetry(mockConfig.Object);
        _ = new ProducerManager(mockConfig.Object);

        var flushSucceeded = meterProvider.ForceFlush();

        var logEntries = wireMockServer.LogEntries.ToList();
        flushSucceeded.Should().BeTrue();
        logEntries.Should().HaveCount(1);
        var usageRequestBody = Encoding.UTF8.GetString(logEntries.First().RequestMessage.BodyAsBytes!);
        _outputHelper.WriteLine(usageRequestBody);
        usageRequestBody.Should().Contain(UofSdkTelemetry.MetricNameForProducerStatus);
    }

    private static WireMockServer SetupWireMockServerWithEndpoint(string endpoint = UsageTelemetry.EndpointUrl)
    {
        var wireMockServer = WireMockServer.Start();
        wireMockServer.Given(Request.Create()
                .WithPath(endpoint)
                .UsingPost())
            .RespondWith(Response.Create().WithBody("data-received").WithStatusCode(HttpStatusCode.Accepted));
        return wireMockServer;
    }

    private static Mock<IUofConfiguration> MockUofConfigurationWithStatisticsInterval(WireMockServer wireMockServer, bool enableUsageExport)
    {
        var mockApiConfig = new Mock<IUofApiConfiguration>();
        mockApiConfig.Setup(s => s.Host).Returns(wireMockServer.Urls[0]);
        var mockUsageConfig = new Mock<IUofUsageConfiguration>();
        mockUsageConfig.Setup(s => s.IsExportEnabled).Returns(enableUsageExport);
        mockUsageConfig.Setup(s => s.Host).Returns(wireMockServer.Urls[0]);
        mockUsageConfig.Setup(s => s.ExportIntervalInSec).Returns(10);
        mockUsageConfig.Setup(s => s.ExportTimeoutInSec).Returns(5);
        var mockConfig = new Mock<IUofConfiguration>();
        mockConfig.Setup(s => s.AccessToken).Returns("MyAccessToken");
        mockConfig.Setup(s => s.NodeId).Returns(123);
        mockConfig.Setup(s => s.Environment).Returns(SdkEnvironment.Integration);
        mockConfig.Setup(s => s.Api).Returns(mockApiConfig.Object);
        mockConfig.Setup(s => s.Usage).Returns(mockUsageConfig.Object);
        return mockConfig;
    }
}
