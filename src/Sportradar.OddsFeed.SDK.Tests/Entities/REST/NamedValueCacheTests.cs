/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/

using System;
using System.Xml.Linq;
using FluentAssertions;
using Moq;
using Sportradar.OddsFeed.SDK.Api.Internal.ApiAccess;
using Sportradar.OddsFeed.SDK.Api.Internal.Caching;
using Sportradar.OddsFeed.SDK.Api.Internal.Config;
using Sportradar.OddsFeed.SDK.Common.Enums;
using Sportradar.OddsFeed.SDK.Common.Internal;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal;
using Sportradar.OddsFeed.SDK.Tests.Common;
using Xunit;

namespace Sportradar.OddsFeed.SDK.Tests.Entities.Rest;

public class NamedValueCacheTests
{
    private Mock<IDataFetcher> _fetcherMock;
    private Uri _betStopReasonsUri;
    private NamedValueCache _cache;

    private void Setup(ExceptionHandlingStrategy exceptionStrategy, SdkTimer cacheSdkTimer = null)
    {
        var dataFetcher = new TestDataFetcher();
        _fetcherMock = new Mock<IDataFetcher>();

        _betStopReasonsUri = new Uri($"{TestData.RestXmlPath}/betstop_reasons.xml", UriKind.Absolute);
        _fetcherMock.Setup(args => args.GetDataAsync(_betStopReasonsUri)).Returns(dataFetcher.GetDataAsync(_betStopReasonsUri));

        var uriFormat = $"{TestData.RestXmlPath}/betstop_reasons.xml";
        var nameCacheSdkTimer = cacheSdkTimer ?? SdkTimer.Create(UofSdkBootstrap.TimerForNamedValueCache, TimeSpan.FromMilliseconds(10), TimeSpan.Zero);
        _cache = new NamedValueCache("BetstopReasons", exceptionStrategy, new NamedValueDataProvider(UofSdkBootstrap.DataProviderForNamedValueCacheBetStopReason, uriFormat, _fetcherMock.Object, "betstop_reason"), nameCacheSdkTimer);
    }

    [Fact]
    public void ConstructingWithNullCacheNameThrows()
    {
        var sdkTimer = SdkTimer.Create(UofSdkBootstrap.TimerForNamedValueCache, TimeSpan.FromMilliseconds(10), TimeSpan.Zero);
        var dataProvider = new NamedValueDataProvider(UofSdkBootstrap.DataProviderForNamedValueCacheMatchStatus, "any", new TestDataFetcher(), "match_status");
        Assert.Throws<ArgumentNullException>(() => new NamedValueCache(null, ExceptionHandlingStrategy.Catch, dataProvider, sdkTimer));
    }

    [Fact]
    public void ConstructingWithEmptyCacheNameThrows()
    {
        var sdkTimer = SdkTimer.Create(UofSdkBootstrap.TimerForNamedValueCache, TimeSpan.FromMilliseconds(10), TimeSpan.Zero);
        var dataProvider = new NamedValueDataProvider(UofSdkBootstrap.DataProviderForNamedValueCacheMatchStatus, "any", new TestDataFetcher(), "match_status");
        Assert.Throws<ArgumentNullException>(() => new NamedValueCache(string.Empty, ExceptionHandlingStrategy.Catch, dataProvider, sdkTimer));
    }

    [Fact]
    public void ConstructingWithNullDataProviderThrows()
    {
        var sdkTimer = SdkTimer.Create(UofSdkBootstrap.TimerForNamedValueCache, TimeSpan.FromMilliseconds(10), TimeSpan.Zero);
        Assert.Throws<ArgumentNullException>(() => new NamedValueCache("AnyCacheName", ExceptionHandlingStrategy.Catch, null, sdkTimer));
    }

    [Fact]
    public void ConstructingWithNullSdkTimerThrows()
    {
        var dataProvider = new NamedValueDataProvider(UofSdkBootstrap.TimerForNamedValueCache, "any", new TestDataFetcher(), "match_status");
        Assert.Throws<ArgumentNullException>(() => new NamedValueCache("AnyCacheName", ExceptionHandlingStrategy.Catch, dataProvider, null));
    }

    [Fact]
    public void DataIsFetchedOnlyOnce()
    {
        Setup(ExceptionHandlingStrategy.Catch);
        var item = _cache.GetNamedValue(0);
        _cache.GetNamedValue(1);
        _cache.GetNamedValue(2);
        _cache.GetNamedValue(1000);

        Assert.NotNull(item);

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
        var finished = ExecutionHelper.WaitToComplete(() => _fetcherMock.Verify(x => x.GetDataAsync(_betStopReasonsUri), Times.Once), 15000);
        Assert.True(finished);
    }

    [Fact]
    public void CorrectValuesAreLoaded()
    {
        Setup(ExceptionHandlingStrategy.Throw);
        var doc = XDocument.Load($"{TestData.RestXmlPath}/betstop_reasons.xml");

        foreach (var xElement in doc.Element("betstop_reasons_descriptions")?.Elements("betstop_reason")!)
        {
            var id = int.Parse(xElement.Attribute("id")!.Value);
            var namedValue = _cache.GetNamedValue(id);

            Assert.NotNull(namedValue);
            Assert.Equal(id, namedValue.Id);
            Assert.False(string.IsNullOrEmpty(namedValue.Description));
        }
    }

    [Fact]
    public void ThrowingExceptionStrategyIsRespected()
    {
        Setup(ExceptionHandlingStrategy.Throw);

        Action action = () => _cache.GetNamedValue(1000);
        action.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void CatchingExceptionStrategyIsRespected()
    {
        Setup(ExceptionHandlingStrategy.Catch);
        var value = _cache.GetNamedValue(1000);

        Assert.Equal(1000, value.Id);
        Assert.Null(value.Description);
    }
}
