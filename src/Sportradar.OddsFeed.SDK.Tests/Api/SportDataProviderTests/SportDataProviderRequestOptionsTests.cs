// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Moq;
using Shouldly;
using Sportradar.OddsFeed.SDK.Api.Config;
using Sportradar.OddsFeed.SDK.Api.Internal;
using Sportradar.OddsFeed.SDK.Api.Internal.Caching;
using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Common.Enums;
using Sportradar.OddsFeed.SDK.Entities.Rest;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Caching.Events;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.EntitiesImpl;
using Sportradar.OddsFeed.SDK.Tests.Common.Builders;
using Sportradar.OddsFeed.SDK.Tests.Common.Builders.Extensions;
using Sportradar.OddsFeed.SDK.Tests.Common.Dsl;
using Sportradar.OddsFeed.SDK.Tests.Common.Extensions;
using Xunit;

namespace Sportradar.OddsFeed.SDK.Tests.API.SportDataProviderTests;

public class SportDataProviderRequestOptionsTests
{
    private static readonly Urn AnySportUrn = "sr:sport:1".ToUrn();
    private static readonly Urn SimpleTournament86 = Urn.Parse("sr:simple_tournament:86");
    private static readonly Urn Tournament853 = Urn.Parse("sr:tournament:853");
    private static readonly Urn SimpleTournamentWithFewEvents = Urn.Parse("sr:simple_tournament:5");
    private static readonly Urn TournamentWithFewEvents = Urn.Parse("sr:tournament:42");
    private static readonly CultureInfo[] TwoLanguages = [new("en"), new("es")];
    private static readonly RequestOptions NonTimeCriticalRequestOptions = new(ExecutionPath.NonTimeCritical);

    private readonly IUofConfiguration _uofConfiguration;

    public SportDataProviderRequestOptionsTests()
    {
        _uofConfiguration = UofConfigurations.SingleLanguage;
    }

    public static IEnumerable<object[]> ConfiguredLanguages =>
        new List<object[]>
            {
                new object[] { new[] { new CultureInfo("en") } },
                new object[] { new[] { new CultureInfo("en"), new CultureInfo("it") } },
                new object[] { new[] { new CultureInfo("es"), new CultureInfo("pt"), new CultureInfo("fr") } }
            };

    [Fact]
    public void GetSportEventWhenInvokedForSimpleTournament86AndSportEventCacheItemIsMissingDoesNotFetchingSummary()
    {
        var sportEntityFactory = new Mock<ISportEntityFactory>();
        var sportEventCacheMock = new Mock<ISportEventCache>();

        var basicTournament = new BasicTournament(SimpleTournament86,
                                                  AnySportUrn,
                                                  sportEntityFactory.Object,
                                                  sportEventCacheMock.Object,
                                                  new Mock<ISportDataCache>().Object,
                                                  TwoLanguages,
                                                  ExceptionHandlingStrategy.Throw);
        sportEntityFactory
           .Setup(factory => factory.BuildSportEvent<ISportEvent>(SimpleTournament86, It.IsAny<Urn>(), It.IsAny<IReadOnlyCollection<CultureInfo>>(), ExceptionHandlingStrategy.Throw))
           .Returns(basicTournament);

        sportEventCacheMock.Setup(c => c.GetEventCacheItem(It.Is<Urn>(urn => urn.ToString() == SimpleTournament86.ToString())))
                           .Returns((ISportEventCacheItem)null);

        var sportDataProvider = new SportDataProviderBuilder()
                               .WithAllMockedDependencies()
                               .WithExceptionStrategy(ExceptionHandlingStrategy.Throw)
                               .WithSportEntityFactory(sportEntityFactory.Object)
                               .WithSportEventCache(sportEventCacheMock.Object)
                               .Build();

        Should.NotThrow(() => sportDataProvider.GetSportEvent(SimpleTournament86));
    }

    [Theory]
    [InlineData(ExceptionHandlingStrategy.Throw)]
    [InlineData(ExceptionHandlingStrategy.Catch)]
    public void GetSportEventReturnsBasicTournamentPrefetchedForSingleConfiguredLanguageWhenSportEventIsSimpleTournament86(ExceptionHandlingStrategy exceptionHandlingStrategy)
    {
        var enLanguage = new CultureInfo("en");
        var configuredLanguages = new[] { enLanguage };

        var sportEntityFactory = new Mock<ISportEntityFactory>();
        var sportEventCacheMock = new Mock<ISportEventCache>();

        var basicTournament = new BasicTournament(SimpleTournament86,
                                                            AnySportUrn,
                                                            sportEntityFactory.Object,
                                                            sportEventCacheMock.Object,
                                                            new Mock<ISportDataCache>().Object,
                                                            configuredLanguages,
                                                            exceptionHandlingStrategy);
        sportEntityFactory
           .Setup(factory => factory.BuildSportEvent<ISportEvent>(SimpleTournament86, It.IsAny<Urn>(), It.IsAny<IReadOnlyCollection<CultureInfo>>(), exceptionHandlingStrategy))
           .Returns(basicTournament);

        var sportEventCacheItemMock = new Mock<ISportEventCacheItem>();
        sportEventCacheItemMock.Setup(ci => ci.FetchMissingSummary(It.Is<IReadOnlyCollection<CultureInfo>>(langs => langs.AreEquivalentTo(configuredLanguages)),
                                                                   NonTimeCriticalRequestOptions,
                                                                   false))
                               .Returns(Task.CompletedTask);

        sportEventCacheMock.Setup(c => c.GetEventCacheItem(SimpleTournament86))
                           .Returns(sportEventCacheItemMock.Object);

        var sportDataProvider = new SportDataProviderBuilder()
                               .WithAllMockedDependencies()
                               .WithExceptionStrategy(exceptionHandlingStrategy)
                               .WithDefaultCultures(configuredLanguages)
                               .WithSportEntityFactory(sportEntityFactory.Object)
                               .WithSportEventCache(sportEventCacheMock.Object)
                               .Build();

        var sportEvent = sportDataProvider.GetSportEvent(SimpleTournament86, AnySportUrn, enLanguage);

        sportEvent.ShouldBe(basicTournament);
        sportEventCacheItemMock.Verify(p => p.FetchMissingSummary(It.Is<IReadOnlyCollection<CultureInfo>>(languages => languages.Count == 1 && languages.First() == enLanguage), It.IsAny<RequestOptions>(), false),
                                       Times.Once);
    }

    [Theory]
    [MemberData(nameof(ConfiguredLanguages))]
    public void GetSportEventReturnsBasicTournamentPrefetchedForAllConfiguredLanguagesWhenSportEventIsSimpleTournament86(CultureInfo[] configuredLanguages)
    {
        var sportEntityFactory = new Mock<ISportEntityFactory>();
        var sportEventCacheMock = new Mock<ISportEventCache>();

        var basicTournament = new BasicTournament(SimpleTournament86,
                                                            AnySportUrn,
                                                            sportEntityFactory.Object,
                                                            sportEventCacheMock.Object,
                                                            new Mock<ISportDataCache>().Object,
                                                            configuredLanguages,
                                                            ExceptionHandlingStrategy.Throw);
        sportEntityFactory
           .Setup(factory => factory.BuildSportEvent<ISportEvent>(SimpleTournament86, It.IsAny<Urn>(), It.IsAny<IReadOnlyCollection<CultureInfo>>(), ExceptionHandlingStrategy.Throw))
           .Returns(basicTournament);

        var sportEventCacheItemMock = new Mock<ISportEventCacheItem>();
        sportEventCacheItemMock.Setup(ci => ci.FetchMissingSummary(It.Is<IReadOnlyCollection<CultureInfo>>(langs => langs.AreEquivalentTo(configuredLanguages)),
                                                                   NonTimeCriticalRequestOptions,
                                                                   false))
                               .Returns(Task.CompletedTask);

        sportEventCacheMock.Setup(c => c.GetEventCacheItem(SimpleTournament86))
                           .Returns(sportEventCacheItemMock.Object);

        var sportDataProvider = new SportDataProviderBuilder()
                               .WithAllMockedDependencies()
                               .WithExceptionStrategy(ExceptionHandlingStrategy.Throw)
                               .WithDefaultCultures(configuredLanguages)
                               .WithSportEntityFactory(sportEntityFactory.Object)
                               .WithSportEventCache(sportEventCacheMock.Object)
                               .Build();

        var sportEvent = sportDataProvider.GetSportEvent(SimpleTournament86, AnySportUrn);

        sportEvent.ShouldBe(basicTournament);
        sportEventCacheItemMock.Verify(p => p.FetchMissingSummary(It.Is<IReadOnlyCollection<CultureInfo>>(languages => languages.AreEquivalentTo(configuredLanguages)), It.IsAny<RequestOptions>(), false),
                                       Times.Once);
    }

    [Fact]
    public void GetSportEventReturnsBasicTournamentWithoutPrefetchingWhenSportEventIsSimpleTournamentOtherThan86()
    {
        var enLanguage = new CultureInfo("en");
        var configuredLanguages = new[] { enLanguage };
        var sportEntityFactory = new Mock<ISportEntityFactory>();
        var sportEventCacheMock = new Mock<ISportEventCache>();

        var basicTournament = new BasicTournament(SimpleTournamentWithFewEvents,
                                                            AnySportUrn,
                                                            sportEntityFactory.Object,
                                                            sportEventCacheMock.Object,
                                                            new Mock<ISportDataCache>().Object,
                                                            configuredLanguages,
                                                            ExceptionHandlingStrategy.Throw);
        sportEntityFactory
           .Setup(factory => factory.BuildSportEvent<ISportEvent>(It.IsAny<Urn>(), It.IsAny<Urn>(), It.IsAny<IReadOnlyCollection<CultureInfo>>(), It.IsAny<ExceptionHandlingStrategy>()))
           .Returns(basicTournament);

        var sportEventCacheItemMock = new Mock<ISportEventCacheItem>();
        sportEventCacheItemMock.Setup(ci => ci.FetchMissingSummary(It.Is<IReadOnlyCollection<CultureInfo>>(langs => langs.AreEquivalentTo(configuredLanguages)),
                                                                   NonTimeCriticalRequestOptions,
                                                                   false))
                               .Returns(Task.CompletedTask);

        sportEventCacheMock.Setup(c => c.GetEventCacheItem(SimpleTournamentWithFewEvents))
                           .Returns(sportEventCacheItemMock.Object);

        var sportDataProvider = new SportDataProviderBuilder()
                               .WithAllMockedDependencies()
                               .WithDefaultCultures(configuredLanguages)
                               .WithSportEntityFactory(sportEntityFactory.Object)
                               .WithSportEventCache(sportEventCacheMock.Object)
                               .Build();

        var sportEvent = sportDataProvider.GetSportEvent(TournamentWithFewEvents);

        sportEvent.ShouldBe(basicTournament);
        sportEventCacheItemMock.Verify(p => p.FetchMissingSummary(It.IsAny<IReadOnlyCollection<CultureInfo>>(), It.IsAny<RequestOptions>(), false),
                                       Times.Never);
    }

    [Theory]
    [InlineData(ExceptionHandlingStrategy.Throw)]
    [InlineData(ExceptionHandlingStrategy.Catch)]
    public void GetTournamentReturnsBasicTournamentPrefetchedForSingleConfiguredLanguageWhenSportEventIsSimpleTournament86(ExceptionHandlingStrategy exceptionHandlingStrategy)
    {
        var enLanguage = new CultureInfo("en");
        var configuredLanguages = new[] { enLanguage };

        var sportEntityFactory = new Mock<ISportEntityFactory>();
        var sportEventCacheMock = new Mock<ISportEventCache>();

        var basicTournament = new BasicTournament(SimpleTournament86,
                                                            AnySportUrn,
                                                            sportEntityFactory.Object,
                                                            sportEventCacheMock.Object,
                                                            new Mock<ISportDataCache>().Object,
                                                            configuredLanguages,
                                                            exceptionHandlingStrategy);
        sportEntityFactory
           .Setup(factory => factory.BuildSportEvent<ISportEvent>(SimpleTournament86, It.IsAny<Urn>(), It.IsAny<IReadOnlyCollection<CultureInfo>>(), exceptionHandlingStrategy))
           .Returns(basicTournament);

        var sportEventCacheItemMock = new Mock<ISportEventCacheItem>();
        sportEventCacheItemMock.Setup(ci => ci.FetchMissingSummary(It.Is<IReadOnlyCollection<CultureInfo>>(langs => langs.AreEquivalentTo(configuredLanguages)),
                                                                   NonTimeCriticalRequestOptions,
                                                                   false))
                               .Returns(Task.CompletedTask);

        sportEventCacheMock.Setup(c => c.GetEventCacheItem(SimpleTournament86))
                           .Returns(sportEventCacheItemMock.Object);

        var sportDataProvider = new SportDataProviderBuilder()
                               .WithAllMockedDependencies()
                               .WithExceptionStrategy(exceptionHandlingStrategy)
                               .WithDefaultCultures(configuredLanguages)
                               .WithSportEntityFactory(sportEntityFactory.Object)
                               .WithSportEventCache(sportEventCacheMock.Object)
                               .Build();

        var sportEvent = sportDataProvider.GetTournament(SimpleTournament86, enLanguage);

        sportEvent.ShouldBe(basicTournament);
        sportEventCacheItemMock.Verify(p => p.FetchMissingSummary(It.Is<IReadOnlyCollection<CultureInfo>>(languages => languages.Count == 1 && languages.First() == enLanguage), It.IsAny<RequestOptions>(), false),
                                       Times.Once);
    }

    [Theory]
    [MemberData(nameof(ConfiguredLanguages))]
    public void GetTournamentReturnsBasicTournamentPrefetchedForAllConfiguredLanguagesWhenSportEventIsSimpleTournament86(CultureInfo[] configuredLanguages)
    {
        var sportEntityFactory = new Mock<ISportEntityFactory>();
        var sportEventCacheMock = new Mock<ISportEventCache>();

        var basicTournament = new BasicTournament(SimpleTournament86,
                                                            AnySportUrn,
                                                            sportEntityFactory.Object,
                                                            sportEventCacheMock.Object,
                                                            new Mock<ISportDataCache>().Object,
                                                            configuredLanguages,
                                                            ExceptionHandlingStrategy.Throw);
        sportEntityFactory
           .Setup(factory => factory.BuildSportEvent<ISportEvent>(SimpleTournament86, It.IsAny<Urn>(), It.IsAny<IReadOnlyCollection<CultureInfo>>(), ExceptionHandlingStrategy.Throw))
           .Returns(basicTournament);

        var sportEventCacheItemMock = new Mock<ISportEventCacheItem>();
        sportEventCacheItemMock.Setup(ci => ci.FetchMissingSummary(It.Is<IReadOnlyCollection<CultureInfo>>(langs => langs.AreEquivalentTo(configuredLanguages)),
                                                                   NonTimeCriticalRequestOptions,
                                                                   false))
                               .Returns(Task.CompletedTask);

        sportEventCacheMock.Setup(c => c.GetEventCacheItem(SimpleTournament86))
                           .Returns(sportEventCacheItemMock.Object);

        var sportDataProvider = new SportDataProviderBuilder()
                               .WithAllMockedDependencies()
                               .WithExceptionStrategy(ExceptionHandlingStrategy.Throw)
                               .WithDefaultCultures(configuredLanguages)
                               .WithSportEntityFactory(sportEntityFactory.Object)
                               .WithSportEventCache(sportEventCacheMock.Object)
                               .Build();

        var tournament = sportDataProvider.GetTournament(SimpleTournament86);

        tournament.ShouldBe(basicTournament);
        sportEventCacheItemMock.Verify(p => p.FetchMissingSummary(It.Is<IReadOnlyCollection<CultureInfo>>(languages => languages.AreEquivalentTo(configuredLanguages)), It.IsAny<RequestOptions>(), false),
                                       Times.Once);
    }

    [Fact]
    public void GetTournamentReturnsBasicTournamentWithoutPrefetchingWhenSportEventIsSimpleTournamentOtherThan86()
    {
        var enLanguage = new CultureInfo("en");
        var configuredLanguages = new[] { enLanguage };
        var sportEntityFactory = new Mock<ISportEntityFactory>();
        var sportEventCacheMock = new Mock<ISportEventCache>();

        var basicTournament = new BasicTournament(SimpleTournamentWithFewEvents,
                                                            AnySportUrn,
                                                            sportEntityFactory.Object,
                                                            sportEventCacheMock.Object,
                                                            new Mock<ISportDataCache>().Object,
                                                            configuredLanguages,
                                                            ExceptionHandlingStrategy.Throw);
        sportEntityFactory
           .Setup(factory => factory.BuildSportEvent<ISportEvent>(SimpleTournamentWithFewEvents, It.IsAny<Urn>(), It.IsAny<IReadOnlyCollection<CultureInfo>>(), ExceptionHandlingStrategy.Throw))
           .Returns(basicTournament);

        var sportEventCacheItemMock = new Mock<ISportEventCacheItem>();
        sportEventCacheItemMock.Setup(ci => ci.FetchMissingSummary(It.Is<IReadOnlyCollection<CultureInfo>>(langs => langs.AreEquivalentTo(configuredLanguages)),
                                                                   NonTimeCriticalRequestOptions,
                                                                   false))
                               .Returns(Task.CompletedTask);

        sportEventCacheMock.Setup(c => c.GetEventCacheItem(SimpleTournamentWithFewEvents))
                           .Returns(sportEventCacheItemMock.Object);

        var sportDataProvider = new SportDataProviderBuilder()
                               .WithAllMockedDependencies()
                               .WithExceptionStrategy(ExceptionHandlingStrategy.Throw)
                               .WithDefaultCultures(configuredLanguages)
                               .WithSportEntityFactory(sportEntityFactory.Object)
                               .WithSportEventCache(sportEventCacheMock.Object)
                               .Build();

        var tournament = sportDataProvider.GetTournament(SimpleTournamentWithFewEvents);

        tournament.ShouldBe(basicTournament);
        sportEventCacheItemMock.Verify(p => p.FetchMissingSummary(It.IsAny<IReadOnlyCollection<CultureInfo>>(), It.IsAny<RequestOptions>(), false),
                                       Times.Never);
    }

    [Fact]
    public void GetSportEventWhenInvokedForTournament853AndSportEventCacheItemIsMissingDoesNotFetchingSummary()
    {
        var sportEntityFactory = new Mock<ISportEntityFactory>();
        var sportEventCacheMock = new Mock<ISportEventCache>();

        var basicTournament = new Tournament(Tournament853,
                                                  AnySportUrn,
                                                  sportEntityFactory.Object,
                                                  sportEventCacheMock.Object,
                                                  new Mock<ISportDataCache>().Object,
                                                  TwoLanguages,
                                                  ExceptionHandlingStrategy.Throw);
        sportEntityFactory
           .Setup(factory => factory.BuildSportEvent<ISportEvent>(Tournament853, It.IsAny<Urn>(), It.IsAny<IReadOnlyCollection<CultureInfo>>(), ExceptionHandlingStrategy.Throw))
           .Returns(basicTournament);

        sportEventCacheMock.Setup(c => c.GetEventCacheItem(It.Is<Urn>(urn => urn == Tournament853)))
                           .Returns((ISportEventCacheItem)null);

        var sportDataProvider = new SportDataProviderBuilder()
                               .WithAllMockedDependencies()
                               .WithExceptionStrategy(ExceptionHandlingStrategy.Throw)
                               .WithSportEntityFactory(sportEntityFactory.Object)
                               .WithSportEventCache(sportEventCacheMock.Object)
                               .Build();

        Should.NotThrow(() => sportDataProvider.GetSportEvent(Tournament853));
    }

    [Theory]
    [InlineData(ExceptionHandlingStrategy.Throw)]
    [InlineData(ExceptionHandlingStrategy.Catch)]
    public void GetSportEventReturnsTournamentPrefetchedForSingleConfiguredLanguageWhenSportEventIsTournament853(ExceptionHandlingStrategy exceptionHandlingStrategy)
    {
        var enLanguage = new CultureInfo("en");
        var configuredLanguages = new[] { enLanguage };

        var sportEntityFactory = new Mock<ISportEntityFactory>();
        var sportEventCacheMock = new Mock<ISportEventCache>();

        var tournament = new Tournament(Tournament853,
                                                  AnySportUrn,
                                                  sportEntityFactory.Object,
                                                  sportEventCacheMock.Object,
                                                  new Mock<ISportDataCache>().Object,
                                                  configuredLanguages,
                                                  exceptionHandlingStrategy);
        sportEntityFactory
           .Setup(factory => factory.BuildSportEvent<ISportEvent>(Tournament853, It.IsAny<Urn>(), It.IsAny<IReadOnlyCollection<CultureInfo>>(), exceptionHandlingStrategy))
           .Returns(tournament);

        var sportEventCacheItemMock = new Mock<ISportEventCacheItem>();
        sportEventCacheItemMock.Setup(ci => ci.FetchMissingSummary(It.Is<IReadOnlyCollection<CultureInfo>>(langs => langs.AreEquivalentTo(configuredLanguages)),
                                                                   NonTimeCriticalRequestOptions,
                                                                   false))
                               .Returns(Task.CompletedTask);

        sportEventCacheMock.Setup(c => c.GetEventCacheItem(Tournament853))
                           .Returns(sportEventCacheItemMock.Object);

        var sportDataProvider = new SportDataProviderBuilder()
                               .WithAllMockedDependencies()
                               .WithExceptionStrategy(exceptionHandlingStrategy)
                               .WithDefaultCultures(configuredLanguages)
                               .WithSportEntityFactory(sportEntityFactory.Object)
                               .WithSportEventCache(sportEventCacheMock.Object)
                               .Build();

        var sportEvent = sportDataProvider.GetSportEvent(Tournament853, AnySportUrn, enLanguage);

        sportEvent.ShouldBe(tournament);
        sportEventCacheItemMock.Verify(p => p.FetchMissingSummary(It.Is<IReadOnlyCollection<CultureInfo>>(languages => languages.Count == 1 && languages.First() == enLanguage), It.IsAny<RequestOptions>(), false),
                                       Times.Once);
    }

    [Theory]
    [MemberData(nameof(ConfiguredLanguages))]
    public void GetSportEventReturnsTournamentPrefetchedForAllConfiguredLanguagesWhenSportEventIsTournament853(CultureInfo[] configuredLanguages)
    {
        var sportEntityFactory = new Mock<ISportEntityFactory>();
        var sportEventCacheMock = new Mock<ISportEventCache>();

        var tournament = new Tournament(Tournament853,
                                                  AnySportUrn,
                                                  sportEntityFactory.Object,
                                                  sportEventCacheMock.Object,
                                                  new Mock<ISportDataCache>().Object,
                                                  configuredLanguages,
                                                  ExceptionHandlingStrategy.Throw);
        sportEntityFactory
           .Setup(factory => factory.BuildSportEvent<ISportEvent>(Tournament853, It.IsAny<Urn>(), It.IsAny<IReadOnlyCollection<CultureInfo>>(), ExceptionHandlingStrategy.Throw))
           .Returns(tournament);

        var sportEventCacheItemMock = new Mock<ISportEventCacheItem>();
        sportEventCacheItemMock.Setup(ci => ci.FetchMissingSummary(It.Is<IReadOnlyCollection<CultureInfo>>(langs => langs.AreEquivalentTo(configuredLanguages)),
                                                                   NonTimeCriticalRequestOptions,
                                                                   false))
                               .Returns(Task.CompletedTask);

        sportEventCacheMock.Setup(c => c.GetEventCacheItem(Tournament853))
                           .Returns(sportEventCacheItemMock.Object);

        var sportDataProvider = new SportDataProviderBuilder()
                               .WithAllMockedDependencies()
                               .WithExceptionStrategy(ExceptionHandlingStrategy.Throw)
                               .WithDefaultCultures(configuredLanguages)
                               .WithSportEntityFactory(sportEntityFactory.Object)
                               .WithSportEventCache(sportEventCacheMock.Object)
                               .Build();

        var sportEvent = sportDataProvider.GetSportEvent(Tournament853, AnySportUrn);

        sportEvent.ShouldBe(tournament);
        sportEventCacheItemMock.Verify(p => p.FetchMissingSummary(It.Is<IReadOnlyCollection<CultureInfo>>(languages => languages.AreEquivalentTo(configuredLanguages)), It.IsAny<RequestOptions>(), false),
                                       Times.Once);
    }

    [Fact]
    public void GetSportEventReturnsTournamentWithoutPrefetchingWhenSportEventIsTournamentOtherThan853()
    {
        var enLanguage = new CultureInfo("en");
        var configuredLanguages = new[] { enLanguage };
        var sportEntityFactory = new Mock<ISportEntityFactory>();
        var sportEventCacheMock = new Mock<ISportEventCache>();

        var tournament = new Tournament(SimpleTournamentWithFewEvents,
                                                            AnySportUrn,
                                                            sportEntityFactory.Object,
                                                            sportEventCacheMock.Object,
                                                            new Mock<ISportDataCache>().Object,
                                                            configuredLanguages,
                                                            ExceptionHandlingStrategy.Throw);
        sportEntityFactory
           .Setup(factory => factory.BuildSportEvent<ISportEvent>(It.IsAny<Urn>(), It.IsAny<Urn>(), It.IsAny<IReadOnlyCollection<CultureInfo>>(), It.IsAny<ExceptionHandlingStrategy>()))
           .Returns(tournament);

        var sportEventCacheItemMock = new Mock<ISportEventCacheItem>();
        sportEventCacheItemMock.Setup(ci => ci.FetchMissingSummary(It.Is<IReadOnlyCollection<CultureInfo>>(langs => langs.AreEquivalentTo(configuredLanguages)),
                                                                   NonTimeCriticalRequestOptions,
                                                                   false))
                               .Returns(Task.CompletedTask);

        sportEventCacheMock.Setup(c => c.GetEventCacheItem(SimpleTournamentWithFewEvents))
                           .Returns(sportEventCacheItemMock.Object);

        var sportDataProvider = new SportDataProviderBuilder()
                               .WithAllMockedDependencies()
                               .WithDefaultCultures(configuredLanguages)
                               .WithSportEntityFactory(sportEntityFactory.Object)
                               .WithSportEventCache(sportEventCacheMock.Object)
                               .Build();

        var sportEvent = sportDataProvider.GetSportEvent(TournamentWithFewEvents);

        sportEvent.ShouldBe(tournament);
        sportEventCacheItemMock.Verify(p => p.FetchMissingSummary(It.IsAny<IReadOnlyCollection<CultureInfo>>(), It.IsAny<RequestOptions>(), false),
                                       Times.Never);
    }

    [Theory]
    [InlineData(ExceptionHandlingStrategy.Throw)]
    [InlineData(ExceptionHandlingStrategy.Catch)]
    public void GetTournamentReturnsTournamentPrefetchedForSingleConfiguredLanguageWhenSportEventIsTournament853(ExceptionHandlingStrategy exceptionHandlingStrategy)
    {
        var enLanguage = new CultureInfo("en");
        var configuredLanguages = new[] { enLanguage };

        var sportEntityFactory = new Mock<ISportEntityFactory>();
        var sportEventCacheMock = new Mock<ISportEventCache>();

        var tournament = new BasicTournament(Tournament853,
                                                  AnySportUrn,
                                                  sportEntityFactory.Object,
                                                  sportEventCacheMock.Object,
                                                  new Mock<ISportDataCache>().Object,
                                                  configuredLanguages,
                                                  exceptionHandlingStrategy);
        sportEntityFactory
           .Setup(factory => factory.BuildSportEvent<ISportEvent>(Tournament853, It.IsAny<Urn>(), It.IsAny<IReadOnlyCollection<CultureInfo>>(), exceptionHandlingStrategy))
           .Returns(tournament);

        var sportEventCacheItemMock = new Mock<ISportEventCacheItem>();
        sportEventCacheItemMock.Setup(ci => ci.FetchMissingSummary(It.Is<IReadOnlyCollection<CultureInfo>>(langs => langs.AreEquivalentTo(configuredLanguages)),
                                                                   NonTimeCriticalRequestOptions,
                                                                   false))
                               .Returns(Task.CompletedTask);

        sportEventCacheMock.Setup(c => c.GetEventCacheItem(Tournament853))
                           .Returns(sportEventCacheItemMock.Object);

        var sportDataProvider = new SportDataProviderBuilder()
                               .WithAllMockedDependencies()
                               .WithExceptionStrategy(exceptionHandlingStrategy)
                               .WithDefaultCultures(configuredLanguages)
                               .WithSportEntityFactory(sportEntityFactory.Object)
                               .WithSportEventCache(sportEventCacheMock.Object)
                               .Build();

        var sportEvent = sportDataProvider.GetTournament(Tournament853, enLanguage);

        sportEvent.ShouldBe(tournament);
        sportEventCacheItemMock.Verify(p => p.FetchMissingSummary(It.Is<IReadOnlyCollection<CultureInfo>>(languages => languages.Count == 1 && languages.First() == enLanguage), It.IsAny<RequestOptions>(), false),
                                       Times.Once);
    }

    [Theory]
    [MemberData(nameof(ConfiguredLanguages))]
    public void GetTournamentReturnsTournamentPrefetchedForAllConfiguredLanguagesWhenSportEventIsTournament853(CultureInfo[] configuredLanguages)
    {
        var sportEntityFactory = new Mock<ISportEntityFactory>();
        var sportEventCacheMock = new Mock<ISportEventCache>();

        var tournament = new BasicTournament(Tournament853,
                                                  AnySportUrn,
                                                  sportEntityFactory.Object,
                                                  sportEventCacheMock.Object,
                                                  new Mock<ISportDataCache>().Object,
                                                  configuredLanguages,
                                                  ExceptionHandlingStrategy.Throw);
        sportEntityFactory
           .Setup(factory => factory.BuildSportEvent<ISportEvent>(Tournament853, It.IsAny<Urn>(), It.IsAny<IReadOnlyCollection<CultureInfo>>(), ExceptionHandlingStrategy.Throw))
           .Returns(tournament);

        var sportEventCacheItemMock = new Mock<ISportEventCacheItem>();
        sportEventCacheItemMock.Setup(ci => ci.FetchMissingSummary(It.Is<IReadOnlyCollection<CultureInfo>>(langs => langs.AreEquivalentTo(configuredLanguages)),
                                                                   NonTimeCriticalRequestOptions,
                                                                   false))
                               .Returns(Task.CompletedTask);

        sportEventCacheMock.Setup(c => c.GetEventCacheItem(Tournament853))
                           .Returns(sportEventCacheItemMock.Object);

        var sportDataProvider = new SportDataProviderBuilder()
                               .WithAllMockedDependencies()
                               .WithExceptionStrategy(ExceptionHandlingStrategy.Throw)
                               .WithDefaultCultures(configuredLanguages)
                               .WithSportEntityFactory(sportEntityFactory.Object)
                               .WithSportEventCache(sportEventCacheMock.Object)
                               .Build();

        var sportEvent = sportDataProvider.GetTournament(Tournament853);

        sportEvent.ShouldBe(tournament);
        sportEventCacheItemMock.Verify(p => p.FetchMissingSummary(It.Is<IReadOnlyCollection<CultureInfo>>(languages => languages.AreEquivalentTo(configuredLanguages)), It.IsAny<RequestOptions>(), false),
                                       Times.Once);
    }

    [Fact]
    public void GetTournamentReturnsTournamentWithoutPrefetchingWhenSportEventIsTournamentOtherThan853()
    {
        var enLanguage = new CultureInfo("en");
        var configuredLanguages = new[] { enLanguage };
        var sportEntityFactory = new Mock<ISportEntityFactory>();
        var sportEventCacheMock = new Mock<ISportEventCache>();

        var tournament = new BasicTournament(TournamentWithFewEvents,
                                                  AnySportUrn,
                                                  sportEntityFactory.Object,
                                                  sportEventCacheMock.Object,
                                                  new Mock<ISportDataCache>().Object,
                                                  configuredLanguages,
                                                  ExceptionHandlingStrategy.Throw);
        sportEntityFactory
           .Setup(factory => factory.BuildSportEvent<ISportEvent>(TournamentWithFewEvents, It.IsAny<Urn>(), It.IsAny<IReadOnlyCollection<CultureInfo>>(), ExceptionHandlingStrategy.Throw))
           .Returns(tournament);

        var sportEventCacheItemMock = new Mock<ISportEventCacheItem>();
        sportEventCacheItemMock.Setup(ci => ci.FetchMissingSummary(It.Is<IReadOnlyCollection<CultureInfo>>(langs => langs.AreEquivalentTo(configuredLanguages)),
                                                                   NonTimeCriticalRequestOptions,
                                                                   false))
                               .Returns(Task.CompletedTask);

        sportEventCacheMock.Setup(c => c.GetEventCacheItem(TournamentWithFewEvents))
                           .Returns(sportEventCacheItemMock.Object);

        var sportDataProvider = new SportDataProviderBuilder()
                               .WithAllMockedDependencies()
                               .WithExceptionStrategy(ExceptionHandlingStrategy.Throw)
                               .WithDefaultCultures(configuredLanguages)
                               .WithSportEntityFactory(sportEntityFactory.Object)
                               .WithSportEventCache(sportEventCacheMock.Object)
                               .Build();

        var sportEvent = sportDataProvider.GetTournament(TournamentWithFewEvents);

        sportEvent.ShouldBe(tournament);
        sportEventCacheItemMock.Verify(p => p.FetchMissingSummary(It.IsAny<IReadOnlyCollection<CultureInfo>>(), It.IsAny<RequestOptions>(), false),
                                       Times.Never);
    }
}
