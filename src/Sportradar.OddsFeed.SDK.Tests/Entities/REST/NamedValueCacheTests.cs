// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using Moq;
using Shouldly;
using Sportradar.OddsFeed.SDK.Api.Internal.ApiAccess;
using Sportradar.OddsFeed.SDK.Api.Internal.Caching;
using Sportradar.OddsFeed.SDK.Api.Internal.Config;
using Sportradar.OddsFeed.SDK.Common.Enums;
using Sportradar.OddsFeed.SDK.Common.Internal;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal;
using Sportradar.OddsFeed.SDK.Messages.Rest;
using Sportradar.OddsFeed.SDK.Tests.Common;
using Sportradar.OddsFeed.SDK.Tests.Common.Dsl.Api.Markets;
using Xunit;

namespace Sportradar.OddsFeed.SDK.Tests.Entities.Rest;

[Collection(NonParallelCollectionFixture.NonParallelTestCollection)]
public class NamedValueCacheTests
{
    private Mock<IDataFetcher> _fetcherMock;
    private Uri _betStopReasonsUri;
    private NamedValueCache _cache;

    private void Setup(ExceptionHandlingStrategy exceptionStrategy, ISdkTimer cacheSdkTimer = null)
    {
        var allBetStopReasons = BetstopReasonsDescriptionsEndpoint.GetDescriptionWithAllBetstopReasons();
        _fetcherMock = new Mock<IDataFetcher>();

        _betStopReasonsUri = new Uri("http://localhost/betstop_reasons.xml", UriKind.Absolute);
        _fetcherMock.Setup(args => args.GetDataAsync(_betStopReasonsUri)).ReturnsAsync(GetMatchStatusStream(allBetStopReasons));

        const string uriFormat = "http://localhost/betstop_reasons.xml";
        var nameCacheSdkTimer = cacheSdkTimer ?? SdkTimer.Create(UofSdkBootstrap.TimerForNamedValueCache, TimeSpan.FromMilliseconds(10), TimeSpan.Zero);
        _cache = new NamedValueCache("BetstopReasons", exceptionStrategy, new NamedValueDataProvider(UofSdkBootstrap.DataProviderForNamedValueCacheBetStopReason, uriFormat, _fetcherMock.Object, "betstop_reason"), nameCacheSdkTimer);
    }

    [Fact]
    public void ConstructingWithNullCacheNameThrows()
    {
        var sdkTimer = SdkTimer.Create(UofSdkBootstrap.TimerForNamedValueCache, TimeSpan.FromMilliseconds(10), TimeSpan.Zero);
        var dataProvider = new NamedValueDataProvider(UofSdkBootstrap.DataProviderForNamedValueCacheMatchStatus, "any", new TestDataFetcher(), "match_status");

        Should.Throw<ArgumentNullException>(() => new NamedValueCache(null, ExceptionHandlingStrategy.Catch, dataProvider, sdkTimer));
    }

    [Fact]
    public void ConstructingWithEmptyCacheNameThrows()
    {
        var sdkTimer = SdkTimer.Create(UofSdkBootstrap.TimerForNamedValueCache, TimeSpan.FromMilliseconds(10), TimeSpan.Zero);
        var dataProvider = new NamedValueDataProvider(UofSdkBootstrap.DataProviderForNamedValueCacheMatchStatus, "any", new TestDataFetcher(), "match_status");

        Should.Throw<ArgumentNullException>(() => new NamedValueCache(string.Empty, ExceptionHandlingStrategy.Catch, dataProvider, sdkTimer));
    }

    [Fact]
    public void ConstructingWithNullDataProviderThrows()
    {
        var sdkTimer = SdkTimer.Create(UofSdkBootstrap.TimerForNamedValueCache, TimeSpan.FromMilliseconds(10), TimeSpan.Zero);

        Should.Throw<ArgumentNullException>(() => new NamedValueCache("AnyCacheName", ExceptionHandlingStrategy.Catch, null, sdkTimer));
    }

    [Fact]
    public void ConstructingWithNullSdkTimerThrows()
    {
        var dataProvider = new NamedValueDataProvider(UofSdkBootstrap.TimerForNamedValueCache, "any", new TestDataFetcher(), "match_status");

        Should.Throw<ArgumentNullException>(() => new NamedValueCache("AnyCacheName", ExceptionHandlingStrategy.Catch, dataProvider, null));
    }

    [Fact]
    public void DataIsFetchedOnlyOnce()
    {
        Setup(ExceptionHandlingStrategy.Catch);
        var item = _cache.GetNamedValue(0);
        _cache.GetNamedValue(1);
        _cache.GetNamedValue(2);
        _cache.GetNamedValue(1000);

        item.ShouldNotBeNull();

        _fetcherMock.Verify(x => x.GetDataAsync(_betStopReasonsUri), Times.Once);
    }

    [Fact]
    public void InitialDataFetchDoesNotBlockConstructor()
    {
        Setup(ExceptionHandlingStrategy.Catch, SdkTimer.Create(UofSdkBootstrap.TimerForNamedValueCache, TimeSpan.FromSeconds(10), TimeSpan.Zero));

        _fetcherMock.Verify(x => x.GetDataAsync(_betStopReasonsUri), Times.Never);
    }

    [Fact]
    public void InitialDataFetchStartedByConstructor()
    {
        Setup(ExceptionHandlingStrategy.Catch, SdkTimer.Create(UofSdkBootstrap.TimerForNamedValueCache, TimeSpan.FromMilliseconds(10), TimeSpan.Zero));
        var finished = TestExecutionHelper.WaitToComplete(() => _fetcherMock.Verify(x => x.GetDataAsync(_betStopReasonsUri), Times.Once), 15000);

        finished.ShouldBeTrue();
    }

    [Theory]
    [MemberData(nameof(GetAllBetstopReasonIdsFrom0To79Included))]
    public void CorrectValueIsLoaded(int id)
    {
        Setup(ExceptionHandlingStrategy.Throw);
        var namedValue = _cache.GetNamedValue(id);

        namedValue.ShouldNotBeNull();
        namedValue.Id.ShouldBe(id);
        namedValue.Description.ShouldNotBeNullOrEmpty();
    }

    [Fact]
    public void ThrowingExceptionStrategyIsRespected()
    {
        Setup(ExceptionHandlingStrategy.Throw);

        Action action = () => _cache.GetNamedValue(1000);
        action.ShouldThrow<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void CatchingExceptionStrategyIsRespected()
    {
        Setup(ExceptionHandlingStrategy.Catch);
        var value = _cache.GetNamedValue(1000);

        value.Id.ShouldBe(1000);
        value.Description.ShouldBeNull();
    }

    private static MemoryStream GetMatchStatusStream(betstop_reasons_descriptions betStopReasonsDescriptions)
    {
        var serializer = new XmlSerializer(typeof(betstop_reasons_descriptions));

        var memoryStream = new MemoryStream();
        var writer = new StreamWriter(memoryStream, new UTF8Encoding(false), leaveOpen: true);
        serializer.Serialize(writer, betStopReasonsDescriptions);
        writer.Flush();
        memoryStream.Position = 0;

        return memoryStream;
    }

    public static IEnumerable<object[]> GetAllBetstopReasonIdsFrom0To79Included()
    {
        return Enumerable.Range(0, 80).Select(id => new object[] { id });
    }
}
