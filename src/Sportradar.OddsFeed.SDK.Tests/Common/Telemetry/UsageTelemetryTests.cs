// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System.Linq;
using System.Net;
using System.Text;
using Moq;
using OpenTelemetry.Metrics;
using Shouldly;
using Sportradar.OddsFeed.SDK.Api.Config;
using Sportradar.OddsFeed.SDK.Api.Internal.Managers;
using Sportradar.OddsFeed.SDK.Common.Enums;
using Sportradar.OddsFeed.SDK.Common.Internal;
using Sportradar.OddsFeed.SDK.Common.Internal.Telemetry;
using Sportradar.OddsFeed.SDK.Entities.Rest;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;
using xRetry;
using Xunit;

namespace Sportradar.OddsFeed.SDK.Tests.Common.Telemetry;

[Collection(NonParallelCollectionFixture.NonParallelTestCollection)]
public class UsageTelemetryTests
{
    private const string MetricNameForTestHistogram = UofSdkTelemetry.MetricNamePrefix + "test-histogram";
    private const int AnyRecordedValue = 1234;
    private const int BookmakerId = 1111;
    private const int NodeId = 2222;
    private const string AccessToken = "my-access-token";

    [Fact]
    public void WhenSetupIsNotRunThenMeterIsInitialized()
    {
        UofSdkTelemetry.DefaultMeter.ShouldNotBeNull();
    }

    [RetryFact(3, 5000)]
    public void WhenExportEndpointReachableThenCanExport()
    {
        var wireMockServer = SetupWireMockServerWithEndpoint();

        var mockConfig = MockUofConfigurationWithStatisticsInterval(wireMockServer, true);

        var meterProvider = UsageTelemetry.SetupUsageTelemetry(mockConfig.Object);
        var testHistogram = UofSdkTelemetry.DefaultMeter.CreateHistogram<long>(MetricNameForTestHistogram);
        testHistogram.Record(AnyRecordedValue);

        var flushSucceeded = meterProvider.ForceFlush();
        flushSucceeded.ShouldBeTrue();

        var logEntries = wireMockServer.LogEntries.ToList();
        logEntries.ShouldHaveSingleItem();
        logEntries.First().ResponseMessage.StatusCode.ShouldBe(202);
    }

    [RetryFact(3, 5000)]
    public void WhenExportEndpointNotReachableThenCanNotExport()
    {
        var wireMockServer = SetupWireMockServerWithEndpoint("/non-existing-endpoint");

        var mockConfig = MockUofConfigurationWithStatisticsInterval(wireMockServer, true);

        var meterProvider = UsageTelemetry.SetupUsageTelemetry(mockConfig.Object);
        var testHistogram = UofSdkTelemetry.DefaultMeter.CreateHistogram<long>(MetricNameForTestHistogram);
        testHistogram.Record(AnyRecordedValue);

        var flushSucceeded = meterProvider.ForceFlush();
        flushSucceeded.ShouldBeFalse();

        var logEntries = wireMockServer.LogEntries.ToList();
        logEntries.ShouldHaveSingleItem();
        logEntries.First().ResponseMessage.StatusCode.ShouldBe(404);
    }

    [Fact]
    public void WhenMetricsExportDisabledThenMeterIsNotConfigured()
    {
        var wireMockServer = SetupWireMockServerWithEndpoint();
        var mockConfig = MockUofConfigurationWithStatisticsInterval(wireMockServer, false);

        var meterProvider = UsageTelemetry.SetupUsageTelemetry(mockConfig.Object);

        meterProvider.ShouldBeNull();
    }

    [Fact]
    public void WhenMetricsExportDisabledThenMeterObjectCanStillBeUsed()
    {
        var wireMockServer = SetupWireMockServerWithEndpoint();
        var mockConfig = MockUofConfigurationWithStatisticsInterval(wireMockServer, false);

        var meterProvider = UsageTelemetry.SetupUsageTelemetry(mockConfig.Object);
        var testHistogram = UofSdkTelemetry.DefaultMeter.CreateHistogram<long>(MetricNameForTestHistogram);
        testHistogram.Record(AnyRecordedValue);

        meterProvider.ShouldBeNull();
    }

    [RetryFact(3, 5000)]
    public void WhenExportedProducerStatusIsIncluded()
    {
        var wireMockServer = SetupWireMockServerWithEndpoint();
        var meterProvider = PrepareMeterProviderWithProducerManager(wireMockServer);

        var flushSucceeded = meterProvider.ForceFlush();
        flushSucceeded.ShouldBeTrue();

        EnsureAttributeInUsageRequestBody(UofSdkTelemetry.MetricNameForProducerStatus, wireMockServer);
    }

    [RetryFact(3, 5000)]
    public void WhenExportedNonUsageMetricsAreNotIncluded()
    {
        var wireMockServer = SetupWireMockServerWithEndpoint();
        var meterProvider = PrepareMeterProviderWithProducerManager(wireMockServer);

        var flushSucceeded = meterProvider.ForceFlush();
        flushSucceeded.ShouldBeTrue();

        EnsureAttributeNotInUsageRequestBody(UofSdkTelemetry.MetricNameForSemaphorePoolAcquireSize, wireMockServer);
    }

    [RetryFact(3, 5000)]
    public void GivenNonUsageHistogramHasRecordWhenExportedNonUsageMetricsAreNotIncluded()
    {
        var wireMockServer = SetupWireMockServerWithEndpoint();
        var meterProvider = PrepareMeterProviderWithProducerManager(wireMockServer);

        UofSdkTelemetry.SportEventCacheGetAll.Record(AnyRecordedValue);

        var flushSucceeded = meterProvider.ForceFlush();
        flushSucceeded.ShouldBeTrue();

        EnsureAttributeNotInUsageRequestBody(UofSdkTelemetry.SportEventCacheGetAll.Name, wireMockServer);
    }

    [RetryFact(3, 5000)]
    public void GivenNonUsageCounterHasRecordWhenExportedNonUsageMetricsAreNotIncluded()
    {
        var wireMockServer = SetupWireMockServerWithEndpoint();
        var meterProvider = PrepareMeterProviderWithProducerManager(wireMockServer);

        UofSdkTelemetry.RabbitMessageReceiverMessageReceived.Add(1);

        var flushSucceeded = meterProvider.ForceFlush();
        flushSucceeded.ShouldBeTrue();

        EnsureAttributeNotInUsageRequestBody(UofSdkTelemetry.RabbitMessageReceiverMessageReceived.Name, wireMockServer);
    }

    [RetryFact(3, 5000)]
    public void UofSdkServiceNameIsExportedOnlyWhenAtLeastOneUsageMetricHasRecordInTheBody()
    {
        var wireMockServer = SetupWireMockServerWithEndpoint();
        var meterProvider = PrepareMeterProviderWithProducerManager(wireMockServer);

        var flushSucceeded = meterProvider.ForceFlush();
        flushSucceeded.ShouldBeTrue();

        EnsureAttributeInUsageRequestBody("UofSdk-Net", wireMockServer);
    }

    [RetryTheory(3, 5000)]
    [InlineData("nodeId")]
    [InlineData(NodeId)]
    [InlineData("environment")]
    [InlineData("integration")]
    [InlineData("metricsVersion")]
    [InlineData("v1")]
    [InlineData("bookmakerId")]
    [InlineData(BookmakerId)]
    [InlineData("Sportradar.OddsFeed.SDKCore")]
    [InlineData("odds-feed-sdk_usage_integration")]
    [InlineData("service.instance.id")]
    [InlineData("service.namespace")]
    [InlineData("service.name")]
    [InlineData("UofSdk-Net")]
    public void WhenExportedThenExporterResourceAttributesAreIncludedInTheBody(string attributeName)
    {
        var wireMockServer = SetupWireMockServerWithEndpoint();
        var meterProvider = PrepareMeterProviderWithProducerManager(wireMockServer);

        var flushSucceeded = meterProvider.ForceFlush();
        flushSucceeded.ShouldBeTrue();

        EnsureAttributeInUsageRequestBody(attributeName, wireMockServer);
    }

    [RetryTheory(3, 5000)]
    [InlineData("producerId")]
    [InlineData("UofSdk-NetStd-Usage")]
    [InlineData("UofSdk-NetStd")]
    [InlineData("UofSdk-NETStd")]
    public void WhenExportedThenExporterResourceAttributesAreNotIncludedInTheBody(string attributeName)
    {
        var wireMockServer = SetupWireMockServerWithEndpoint();
        var meterProvider = PrepareMeterProviderWithProducerManager(wireMockServer);

        var flushSucceeded = meterProvider.ForceFlush();
        flushSucceeded.ShouldBeTrue();

        EnsureAttributeNotInUsageRequestBody(attributeName, wireMockServer);
    }

    [RetryFact(3, 5000)]
    public void WhenExportedThenExporterResourceHeaderAttributesAreIncluded()
    {
        var wireMockServer = SetupWireMockServerWithEndpoint();
        var meterProvider = PrepareMeterProviderWithProducerManager(wireMockServer);

        var flushSucceeded = meterProvider.ForceFlush();
        flushSucceeded.ShouldBeTrue();

        EnsureAttributeInUsageRequestHeader("x-access-token", AccessToken, wireMockServer);
        EnsureAttributeInUsageRequestHeader("x-environment", "integration", wireMockServer);
        EnsureAttributeInUsageRequestHeader("x-node-id", NodeId.ToString(), wireMockServer);
        EnsureAttributeInUsageRequestHeader("x-sdk-version", SdkInfo.GetVersion(), wireMockServer);
    }

    private static MeterProvider PrepareMeterProviderWithProducerManager(WireMockServer wireMockServer)
    {
        var mockConfig = MockUofConfigurationWithStatisticsInterval(wireMockServer, true);
        var mockProducerConfig = new Mock<IUofProducerConfiguration>();
        mockProducerConfig.Setup(s => s.DisabledProducers).Returns([]);
        mockProducerConfig.Setup(s => s.Producers).Returns(TestConfiguration.GetConfig().Producer.Producers);
        mockConfig.Setup(s => s.Producer).Returns(mockProducerConfig.Object);
        var meterProvider = UsageTelemetry.SetupUsageTelemetry(mockConfig.Object);
        _ = new ProducerManager(mockConfig.Object);
        return meterProvider;
    }

    private static WireMockServer SetupWireMockServerWithEndpoint(string endpoint = UsageTelemetry.EndpointUrl)
    {
        var wireMockServer = WireMockServer.Start();
        wireMockServer.Given(Request.Create().WithPath(endpoint).UsingPost())
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

        var mockBookmakerDetails = new Mock<IBookmakerDetails>();
        mockBookmakerDetails.Setup(s => s.BookmakerId).Returns(BookmakerId);

        var mockConfig = new Mock<IUofConfiguration>();
        mockConfig.Setup(s => s.AccessToken).Returns(AccessToken);
        mockConfig.Setup(s => s.NodeId).Returns(NodeId);
        mockConfig.Setup(s => s.Environment).Returns(SdkEnvironment.Integration);
        mockConfig.Setup(s => s.Api).Returns(mockApiConfig.Object);
        mockConfig.Setup(s => s.Usage).Returns(mockUsageConfig.Object);
        mockConfig.Setup(s => s.BookmakerDetails).Returns(mockBookmakerDetails.Object);
        return mockConfig;
    }

    private static void EnsureAttributeInUsageRequestHeader(string headerKey, string headerValue, WireMockServer wireMockServer)
    {
        var logEntries = wireMockServer.LogEntries.ToList();
        logEntries.ShouldHaveSingleItem();

        var requestMessageHeaders = logEntries.First().RequestMessage.Headers;
        requestMessageHeaders.ShouldNotBeNull();
        requestMessageHeaders.ShouldContainKey(headerKey);
        requestMessageHeaders[headerKey].ShouldBe(headerValue);
    }

    private static void EnsureAttributeInUsageRequestBody(string attributeName, WireMockServer wireMockServer)
    {
        var logEntries = wireMockServer.LogEntries.ToList();
        logEntries.ShouldHaveSingleItem();

        var usageRequestBody = Encoding.UTF8.GetString(logEntries.First().RequestMessage.BodyAsBytes!);
        usageRequestBody.ShouldContain(attributeName);
    }

    private static void EnsureAttributeNotInUsageRequestBody(string attributeName, WireMockServer wireMockServer)
    {
        var logEntries = wireMockServer.LogEntries.ToList();
        logEntries.ShouldHaveSingleItem();

        var usageRequestBody = Encoding.UTF8.GetString(logEntries.First().RequestMessage.BodyAsBytes!);
        usageRequestBody.ShouldNotContain(attributeName);
    }
}
