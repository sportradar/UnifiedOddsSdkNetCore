// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.Serialization;
using FluentAssertions;
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
using Sportradar.OddsFeed.SDK.Tests.Common.Dsl.Api.Endpoints;
using Xunit;

namespace Sportradar.OddsFeed.SDK.Tests.Entities.Rest;

public class LocalizedNamedValueCacheTests
{
    private readonly Uri _deMatchStatusUri;
    private readonly Uri _enMatchStatusUri;
    private readonly Uri _huMatchStatusUri;
    private readonly Uri _nlMatchStatusUri;
    private readonly match_status_descriptions _matchStatusDescriptionEn;
    private readonly match_status_descriptions _matchStatusDescriptionDe;
    private readonly match_status_descriptions _matchStatusDescriptionHu;
    private readonly match_status_descriptions _matchStatusDescriptionNl;

    private Mock<IDataFetcher> _fetcherMock;

    public LocalizedNamedValueCacheTests()
    {
        _matchStatusDescriptionEn = MatchStatusDescriptionsEndpoint.GetMatchStatusDescriptionForStatus01En();
        _matchStatusDescriptionDe = MatchStatusDescriptionsEndpoint.GetMatchStatusDescriptionForStatus01De();
        _matchStatusDescriptionHu = MatchStatusDescriptionsEndpoint.GetMatchStatusDescriptionForStatus01Hu();
        _matchStatusDescriptionNl = MatchStatusDescriptionsEndpoint.GetMatchStatusDescriptionForStatus01Nl();
        _enMatchStatusUri = new Uri("http://localhost/match_status_descriptions_en.xml", UriKind.Absolute);
        _deMatchStatusUri = new Uri("http://localhost/match_status_descriptions_de.xml", UriKind.Absolute);
        _huMatchStatusUri = new Uri("http://localhost/match_status_descriptions_hu.xml", UriKind.Absolute);
        _nlMatchStatusUri = new Uri("http://localhost/match_status_descriptions_nl.xml", UriKind.Absolute);
    }

    private LocalizedNamedValueCache Setup(ExceptionHandlingStrategy exceptionStrategy, ISdkTimer cacheSdkTimer = null)
    {
        var dataFetcher = new TestDataFetcher();
        _fetcherMock = new Mock<IDataFetcher>();

        _fetcherMock.Setup(args => args.GetDataAsync(_enMatchStatusUri)).ReturnsAsync(GetMatchStatusStream(_matchStatusDescriptionEn));

        _fetcherMock.Setup(args => args.GetDataAsync(_deMatchStatusUri)).ReturnsAsync(GetMatchStatusStream(_matchStatusDescriptionDe));

        _fetcherMock.Setup(args => args.GetDataAsync(_huMatchStatusUri)).ReturnsAsync(GetMatchStatusStream(_matchStatusDescriptionHu));

        _fetcherMock.Setup(args => args.GetDataAsync(_nlMatchStatusUri)).ReturnsAsync(GetMatchStatusStream(_matchStatusDescriptionNl));

        const string uriFormat = "http://localhost/match_status_descriptions_{0}.xml";
        var nameCacheSdkTimer = cacheSdkTimer ?? SdkTimer.Create(UofSdkBootstrap.TimerForLocalizedNamedValueCache, TimeSpan.FromMilliseconds(10), TimeSpan.Zero);
        return new LocalizedNamedValueCache("MatchStatus",
                                            exceptionStrategy,
                                            new NamedValueDataProvider(UofSdkBootstrap.DataProviderForNamedValueCacheMatchStatus, uriFormat, _fetcherMock.Object, "match_status"),
                                            nameCacheSdkTimer,
                                                [new CultureInfo("en"), new CultureInfo("de"), new CultureInfo("hu")]);
    }

    [Fact]
    public void ConstructingWithNullCacheNameThrows()
    {
        var sdkTimer = SdkTimer.Create(UofSdkBootstrap.TimerForLocalizedNamedValueCache, TimeSpan.FromMilliseconds(10), TimeSpan.Zero);
        var dataProvider = new NamedValueDataProvider(UofSdkBootstrap.DataProviderForNamedValueCacheMatchStatus, "any", new TestDataFetcher(), "match_status");

        var exception = Should.Throw<ArgumentNullException>(() =>
                                                                new LocalizedNamedValueCache(null, ExceptionHandlingStrategy.Catch, dataProvider, sdkTimer, new List<CultureInfo> { new("en") }));

        exception.ParamName.ShouldBe("cacheName");
    }

    [Fact]
    public void ConstructingWithEmptyCacheNameThrows()
    {
        var sdkTimer = new TestTimer(false);
        var dataProvider = new NamedValueDataProvider(UofSdkBootstrap.DataProviderForNamedValueCacheMatchStatus, "any", new TestDataFetcher(), "match_status");

        var exception = Should.Throw<ArgumentNullException>(() =>
                                                                new LocalizedNamedValueCache(string.Empty, ExceptionHandlingStrategy.Catch, dataProvider, sdkTimer, new List<CultureInfo> { new("en") }));

        exception.ParamName.ShouldBe("cacheName");
    }

    [Fact]
    public void ConstructingWithNullDataProviderThrows()
    {
        var sdkTimer = new TestTimer(false);

        var exception = Should.Throw<ArgumentNullException>(() =>
                                                                new LocalizedNamedValueCache("AnyCacheName", ExceptionHandlingStrategy.Catch, null, sdkTimer, new List<CultureInfo> { new("en") }));

        exception.ParamName.ShouldBe("dataProvider");
    }

    [Fact]
    public void ConstructingWithNullSdkTimerThrows()
    {
        var dataProvider = new NamedValueDataProvider(UofSdkBootstrap.DataProviderForNamedValueCacheMatchStatus, "any", new TestDataFetcher(), "match_status");

        var exception = Should.Throw<ArgumentNullException>(() =>
                                                                new LocalizedNamedValueCache("AnyCacheName", ExceptionHandlingStrategy.Catch, dataProvider, null, new List<CultureInfo> { new("en") }));

        exception.ParamName.ShouldBe("sdkTimer");
    }

    [Fact]
    public void ConstructingWithEmptyCulturesThrows()
    {
        var sdkTimer = new TestTimer(false);
        var dataProvider = new NamedValueDataProvider(UofSdkBootstrap.DataProviderForNamedValueCacheMatchStatus, "any", new TestDataFetcher(), "match_status");

        var exception = Should.Throw<ArgumentNullException>(() =>
                                                                new LocalizedNamedValueCache("AnyCacheName", ExceptionHandlingStrategy.Catch, dataProvider, sdkTimer, new List<CultureInfo>()));

        exception.ParamName.ShouldBe("cultures");
    }

    [Fact]
    public void ConstructingWithNullCulturesThrows()
    {
        var sdkTimer = new TestTimer(false);
        var dataProvider = new NamedValueDataProvider(UofSdkBootstrap.DataProviderForNamedValueCacheMatchStatus, "any", new TestDataFetcher(), "match_status");

        var exception = Should.Throw<ArgumentNullException>(() =>
                                                                new LocalizedNamedValueCache("AnyCacheName", ExceptionHandlingStrategy.Catch, dataProvider, sdkTimer, null));

        exception.ParamName.ShouldBe("cultures");
    }

    [Fact]
    [SuppressMessage("ReSharper", "RedundantAssignment")]
    [SuppressMessage("Major Code Smell", "S1854:Unused assignments should be removed", Justification = "Allowed in this test")]
    public async Task DataIsFetchedOnlyOncePerLocale()
    {
        var cache = Setup(ExceptionHandlingStrategy.Throw);

        var namedValue = await cache.GetAsync(0);
        namedValue = await cache.GetAsync(0, [new CultureInfo("en")]);
        namedValue = await cache.GetAsync(0, [new CultureInfo("de")]);
        namedValue = await cache.GetAsync(0, [new CultureInfo("hu")]);

        namedValue.ShouldNotBeNull();

        _fetcherMock.Verify(x => x.GetDataAsync(_enMatchStatusUri), Times.Once);
        _fetcherMock.Verify(x => x.GetDataAsync(_deMatchStatusUri), Times.Once);
        _fetcherMock.Verify(x => x.GetDataAsync(_huMatchStatusUri), Times.Once);
        _fetcherMock.Verify(x => x.GetDataAsync(_nlMatchStatusUri), Times.Never);

        namedValue = await cache.GetAsync(0, [new CultureInfo("nl")]);
        namedValue = await cache.GetAsync(0, TestData.Cultures4);

        namedValue.ShouldNotBeNull();

        _fetcherMock.Verify(x => x.GetDataAsync(_enMatchStatusUri), Times.Once);
        _fetcherMock.Verify(x => x.GetDataAsync(_deMatchStatusUri), Times.Once);
        _fetcherMock.Verify(x => x.GetDataAsync(_huMatchStatusUri), Times.Once);
        _fetcherMock.Verify(x => x.GetDataAsync(_nlMatchStatusUri), Times.Once);
    }

    [Fact]
    public void InitialDataFetchDoesNotBlockConstructor()
    {
        Setup(ExceptionHandlingStrategy.Catch, SdkTimer.Create(UofSdkBootstrap.TimerForLocalizedNamedValueCache, TimeSpan.FromSeconds(10), TimeSpan.Zero));
        _fetcherMock.Verify(x => x.GetDataAsync(_enMatchStatusUri), Times.Never);
        _fetcherMock.Verify(x => x.GetDataAsync(_deMatchStatusUri), Times.Never);
        _fetcherMock.Verify(x => x.GetDataAsync(_huMatchStatusUri), Times.Never);
        _fetcherMock.Verify(x => x.GetDataAsync(_nlMatchStatusUri), Times.Never);
    }

    [Fact]
    public void InitialDataFetchStartedByConstructor()
    {
        Setup(ExceptionHandlingStrategy.Catch, SdkTimer.Create(UofSdkBootstrap.TimerForLocalizedNamedValueCache, TimeSpan.FromMilliseconds(10), TimeSpan.Zero));
        var finished = TestExecutionHelper.WaitToComplete(() =>
                                                          {
                                                              _fetcherMock.Verify(x => x.GetDataAsync(_enMatchStatusUri), Times.Once);
                                                              _fetcherMock.Verify(x => x.GetDataAsync(_deMatchStatusUri), Times.Once);
                                                              _fetcherMock.Verify(x => x.GetDataAsync(_huMatchStatusUri), Times.Once);
                                                              _fetcherMock.Verify(x => x.GetDataAsync(_nlMatchStatusUri), Times.Never);
                                                          },
                                                          15000);

        finished.ShouldBeTrue();
    }

    [Fact]
    public async Task CorrectValuesAreLoaded()
    {
        var cache = Setup(ExceptionHandlingStrategy.Throw);
        var doc = XDocument.Load(GetMatchStatusStream(_matchStatusDescriptionEn));

        doc.ShouldNotBeNull();
        doc.Element("match_status_descriptions").ShouldNotBeNull();

        foreach (var xElement in doc.Element("match_status_descriptions")!.Elements("match_status"))
        {
            xElement.Attribute("id").ShouldNotBeNull();
            var id = int.Parse(xElement.Attribute("id")!.Value);
            var namedValue = await cache.GetAsync(id);

            namedValue.ShouldNotBeNull();
            namedValue.Id.ShouldBe(id);

            namedValue.Descriptions.Should().ContainKey(new CultureInfo("en"));
            namedValue.Descriptions.Should().ContainKey(new CultureInfo("de"));
            namedValue.Descriptions.Should().ContainKey(new CultureInfo("hu"));
            namedValue.Descriptions.Should().NotContainKey(new CultureInfo("nl"));

            namedValue.GetDescription(new CultureInfo("en")).ShouldNotBe(new CultureInfo("de").Name);
            namedValue.GetDescription(new CultureInfo("en")).ShouldNotBe(new CultureInfo("hu").Name);
            namedValue.GetDescription(new CultureInfo("de")).ShouldNotBe(new CultureInfo("hu").Name);
        }
    }

    [Fact]
    public async Task ThrowingExceptionStrategyIsRespected()
    {
        var cache = Setup(ExceptionHandlingStrategy.Throw);

        await Should.ThrowAsync<ArgumentOutOfRangeException>(() => cache.GetAsync(1000));
    }

    [Fact]
    public async Task CatchingExceptionStrategyIsRespected()
    {
        var cache = Setup(ExceptionHandlingStrategy.Catch);
        var value = await cache.GetAsync(1000);

        value.Id.ShouldBe(1000);
        value.Descriptions.ShouldNotBeNull();
        value.Descriptions.Any().ShouldBeFalse();
    }

    private static MemoryStream GetMatchStatusStream(match_status_descriptions matchStatusDescriptionEn)
    {
        var serializer = new XmlSerializer(typeof(match_status_descriptions));

        var memoryStream = new MemoryStream();
        var writer = new StreamWriter(memoryStream, new UTF8Encoding(false), leaveOpen: true);
        serializer.Serialize(writer, matchStatusDescriptionEn);
        writer.Flush();
        memoryStream.Position = 0;

        return memoryStream;
    }
}
