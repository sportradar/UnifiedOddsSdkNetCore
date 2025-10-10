// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System.Linq;
using System.Net.Http;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Sportradar.OddsFeed.SDK.Api.Internal.ApiAccess;
using Sportradar.OddsFeed.SDK.Api.Internal.Authentication;
using Sportradar.OddsFeed.SDK.Api.Internal.Config;
using Sportradar.OddsFeed.SDK.Api.Internal.Replay;
using Sportradar.OddsFeed.SDK.Common.Internal;
using Sportradar.OddsFeed.SDK.Common.Internal.Extensions;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Dto;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Dto.CustomBet;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Dto.Lottery;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Mapping;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Mapping.Lottery;
using Sportradar.OddsFeed.SDK.Messages.Rest;
using Sportradar.OddsFeed.SDK.Tests.Common;
using Xunit;
using ISdkHttpClientFastFailing = Sportradar.OddsFeed.SDK.Api.Internal.ApiAccess.ISdkHttpClientFastFailing;

namespace Sportradar.OddsFeed.SDK.Tests.Api.Config;

public class UofSdkBootstrapApiTests : UofSdkBootstrapBase
{
    [Fact]
    public void HttpClientFactoryIsRegistered()
    {
        var httpClientFactory = ServiceScope1.ServiceProvider.GetRequiredService<IHttpClientFactory>();
        Assert.NotNull(httpClientFactory);
    }

    [Fact]
    public void HttpClientFactoryIsSingleton()
    {
        CheckSingletonType<IHttpClientFactory>();
    }

    [Fact]
    public void HttpClientIsRegistered()
    {
        var service1 = ServiceScope1.ServiceProvider.GetRequiredService<IHttpClientFactory>().CreateClient(UofSdkBootstrap.HttpClientNameForNormal);
        var service1A = ServiceScope1.ServiceProvider.GetRequiredService<IHttpClientFactory>().CreateClient(UofSdkBootstrap.HttpClientNameForNormal);
        var service2 = ServiceScope2.ServiceProvider.GetRequiredService<IHttpClientFactory>().CreateClient(UofSdkBootstrap.HttpClientNameForNormal);
        Assert.NotNull(service1);
        Assert.NotNull(service1A);
        Assert.NotNull(service2);
        Assert.NotSame(service1, service1A);
        Assert.NotSame(service1, service2);
    }

    [Fact]
    public void HttpClientRecoveryIsRegistered()
    {
        var service1 = ServiceScope1.ServiceProvider.GetRequiredService<IHttpClientFactory>().CreateClient(UofSdkBootstrap.HttpClientNameForRecovery);
        var service1A = ServiceScope1.ServiceProvider.GetRequiredService<IHttpClientFactory>().CreateClient(UofSdkBootstrap.HttpClientNameForRecovery);
        var service2 = ServiceScope2.ServiceProvider.GetRequiredService<IHttpClientFactory>().CreateClient(UofSdkBootstrap.HttpClientNameForRecovery);
        Assert.NotNull(service1);
        Assert.NotNull(service1A);
        Assert.NotNull(service2);
        Assert.NotSame(service1, service1A);
        Assert.NotSame(service1, service2);
    }

    [Fact]
    public void HttpClientFastFailingIsRegistered()
    {
        var service1 = ServiceScope1.ServiceProvider.GetRequiredService<IHttpClientFactory>().CreateClient(UofSdkBootstrap.HttpClientNameForFastFailing);
        var service1A = ServiceScope1.ServiceProvider.GetRequiredService<IHttpClientFactory>().CreateClient(UofSdkBootstrap.HttpClientNameForFastFailing);
        var service2 = ServiceScope2.ServiceProvider.GetRequiredService<IHttpClientFactory>().CreateClient(UofSdkBootstrap.HttpClientNameForFastFailing);
        Assert.NotNull(service1);
        Assert.NotNull(service1A);
        Assert.NotNull(service2);
        Assert.NotSame(service1, service1A);
        Assert.NotSame(service1, service2);
    }

    [Fact]
    public void HttpClientAuthenticationIsRegistered()
    {
        var service1 = ServiceScope1.ServiceProvider.GetRequiredService<IHttpClientFactory>().CreateClient(UofSdkBootstrap.HttpClientNameForAuthentication);
        var service1A = ServiceScope1.ServiceProvider.GetRequiredService<IHttpClientFactory>().CreateClient(UofSdkBootstrap.HttpClientNameForAuthentication);
        var service2 = ServiceScope2.ServiceProvider.GetRequiredService<IHttpClientFactory>().CreateClient(UofSdkBootstrap.HttpClientNameForAuthentication);
        Assert.NotNull(service1);
        Assert.NotNull(service1A);
        Assert.NotNull(service2);
        Assert.NotSame(service1, service1A);
        Assert.NotSame(service1, service2);
    }

    [Fact]
    public void HttpClientsHaveDifferentTimeouts()
    {
        var httpClientNormal = ServiceScope1.ServiceProvider.GetRequiredService<IHttpClientFactory>().CreateClient(UofSdkBootstrap.HttpClientNameForNormal);
        var httpClientRecovery = ServiceScope1.ServiceProvider.GetRequiredService<IHttpClientFactory>().CreateClient(UofSdkBootstrap.HttpClientNameForRecovery);
        var httpClientFastFailing = ServiceScope1.ServiceProvider.GetRequiredService<IHttpClientFactory>().CreateClient(UofSdkBootstrap.HttpClientNameForFastFailing);
        var httpClientAuthentication = ServiceScope1.ServiceProvider.GetRequiredService<IHttpClientFactory>().CreateClient(UofSdkBootstrap.HttpClientNameForAuthentication);
        Assert.NotNull(httpClientNormal);
        Assert.NotNull(httpClientRecovery);
        Assert.NotNull(httpClientFastFailing);
        Assert.NotNull(httpClientAuthentication);
        Assert.NotSame(httpClientNormal, httpClientRecovery);
        Assert.NotSame(httpClientNormal, httpClientFastFailing);
        Assert.NotSame(httpClientRecovery, httpClientFastFailing);
        Assert.NotSame(httpClientAuthentication, httpClientFastFailing);

        Assert.Equal(UofConfig.Api.HttpClientTimeout, httpClientNormal.Timeout);
        Assert.Equal(UofConfig.Api.HttpClientRecoveryTimeout, httpClientRecovery.Timeout);
        Assert.Equal(UofConfig.Api.HttpClientFastFailingTimeout, httpClientFastFailing.Timeout);
        Assert.Equal(UofConfig.Api.HttpClientFastFailingTimeout, httpClientAuthentication.Timeout);
    }

    [Fact]
    public void SdkHttpClientInterfaceIsTransient()
    {
        CheckTransientType<ISdkHttpClient>();
    }

    [Fact]
    public void SdkHttpClientsHasDifferentInstances()
    {
        var httpClientNormal = ServiceScope1.ServiceProvider.GetRequiredService<ISdkHttpClient>();
        var httpClientRecovery = ServiceScope1.ServiceProvider.GetRequiredService<ISdkHttpClientRecovery>();
        var httpClientFastFailing = ServiceScope2.ServiceProvider.GetRequiredService<ISdkHttpClientFastFailing>();
        Assert.NotNull(httpClientNormal);
        Assert.NotNull(httpClientRecovery);
        Assert.NotNull(httpClientFastFailing);
        Assert.NotSame(httpClientNormal, httpClientRecovery);
        Assert.NotSame(httpClientNormal, httpClientFastFailing);
    }

    [Fact]
    public void SdkHttpClientNormalHasDefaultRequestHeadersConfigured()
    {
        SdkHttpClientHasDefaultRequestHeadersConfigured<ISdkHttpClient>();
    }

    [Fact]
    public void SdkHttpClientRecoveryHasDefaultRequestHeadersConfigured()
    {
        SdkHttpClientHasDefaultRequestHeadersConfigured<ISdkHttpClientRecovery>();
    }

    [Fact]
    public void SdkHttpClientFastFailingHasDefaultRequestHeadersConfigured()
    {
        SdkHttpClientHasDefaultRequestHeadersConfigured<ISdkHttpClientFastFailing>();
    }

    private void SdkHttpClientHasDefaultRequestHeadersConfigured<T>() where T : class
    {
        var sdkHttpClient = (ISdkHttpClient)ServiceScope1.ServiceProvider.GetRequiredService<T>();
        Assert.NotNull(sdkHttpClient);
        Assert.IsType<T>(sdkHttpClient, false);
        Assert.NotNull(sdkHttpClient.DefaultRequestHeaders);
        Assert.NotEmpty(sdkHttpClient.DefaultRequestHeaders);
        Assert.NotNull(sdkHttpClient.DefaultRequestHeaders.UserAgent);
        Assert.True(sdkHttpClient.DefaultRequestHeaders.TryGetValues(UofSdkBootstrap.HttpClientDefaultRequestHeaderForUserAgent, out var userAgent));
        var userAgents = userAgent.ToList();
        Assert.Equal(2, userAgents.Count);
        Assert.False(string.IsNullOrEmpty(userAgents.First()));
        Assert.StartsWith("UfSdk-", userAgents.First());
    }

    [Fact]
    public void SdkHttpClientNormalHasTimeoutConfigured()
    {
        var sdkHttpClient = ServiceScope1.ServiceProvider.GetRequiredService<ISdkHttpClient>();
        Assert.Equal(UofConfig.Api.HttpClientTimeout, sdkHttpClient.Timeout);
    }

    [Fact]
    public void SdkHttpClientRecoveryHasTimeoutConfigured()
    {
        var sdkHttpClient = ServiceScope1.ServiceProvider.GetRequiredService<ISdkHttpClientRecovery>();
        Assert.Equal(UofConfig.Api.HttpClientRecoveryTimeout, sdkHttpClient.Timeout);
    }

    [Fact]
    public void SdkHttpClientFastFailingHasTimeoutConfigured()
    {
        var sdkHttpClient = ServiceScope1.ServiceProvider.GetRequiredService<ISdkHttpClientFastFailing>();
        Assert.Equal(UofConfig.Api.HttpClientFastFailingTimeout, sdkHttpClient.Timeout);
    }

    [Fact]
    public void DataFetcherIsSingleton()
    {
        CheckSingletonType<IDataFetcher>();
    }

    [Fact]
    public void DataPosterIsSingleton()
    {
        CheckSingletonType<IDataPoster>();
    }

    [Fact]
    public void DataRestfulIsSingleton()
    {
        CheckSingletonType<IDataRestful>();

        var service1 = ServiceScope1.ServiceProvider.GetRequiredService<IDataRestful>();
        Assert.NotNull(service1);
        Assert.IsType<HttpDataRestful>(service1, false);
    }

    [Fact]
    public void LogHttpDataFetcherIsSingleton()
    {
        CheckSingletonType<ILogHttpDataFetcher>();

        var service1 = ServiceScope1.ServiceProvider.GetRequiredService<ILogHttpDataFetcher>();
        Assert.NotNull(service1);
        Assert.IsType<LogHttpDataFetcher>(service1, false);
        var logHttpDataFetcher = (LogHttpDataFetcher)service1;
        Assert.NotNull(logHttpDataFetcher);
        Assert.NotNull(logHttpDataFetcher.SdkHttpClient);
        Assert.Equal(UofConfig.Api.HttpClientTimeout, logHttpDataFetcher.SdkHttpClient.Timeout);
    }

    [Fact]
    public void LogHttpDataFetcherRecoveryIsSingleton()
    {
        CheckSingletonType<ILogHttpDataFetcherRecovery>();

        var service1 = ServiceScope1.ServiceProvider.GetRequiredService<ILogHttpDataFetcherRecovery>();
        Assert.NotNull(service1);
        Assert.IsType<LogHttpDataFetcherRecovery>(service1, false);
        var logHttpDataFetcherRecovery = (LogHttpDataFetcherRecovery)service1;
        Assert.NotNull(logHttpDataFetcherRecovery);
        Assert.NotNull(logHttpDataFetcherRecovery.SdkHttpClient);
        Assert.Equal(UofConfig.Api.HttpClientRecoveryTimeout, logHttpDataFetcherRecovery.SdkHttpClient.Timeout);
    }

    [Fact]
    public void LogHttpDataFetcherFastFailingIsSingleton()
    {
        CheckSingletonType<ILogHttpDataFetcherFastFailing>();

        var service1 = ServiceScope1.ServiceProvider.GetRequiredService<ILogHttpDataFetcherFastFailing>();
        Assert.NotNull(service1);
        Assert.IsType<LogHttpDataFetcherFastFailing>(service1, false);
        var logHttpDataFetcherFastFailing = (LogHttpDataFetcherFastFailing)service1;
        Assert.NotNull(logHttpDataFetcherFastFailing);
        Assert.NotNull(logHttpDataFetcherFastFailing.SdkHttpClient);
        Assert.Equal(UofConfig.Api.HttpClientFastFailingTimeout, logHttpDataFetcherFastFailing.SdkHttpClient.Timeout);
    }

    [Fact]
    public void DeserializerForScheduleEndpointIsSingleton()
    {
        CheckSingletonType<IDeserializer<scheduleEndpoint>>();

        var service1 = ServiceScope1.ServiceProvider.GetRequiredService<IDeserializer<scheduleEndpoint>>();
        Assert.NotNull(service1);
        Assert.IsType<Deserializer<scheduleEndpoint>>(service1, false);
    }

    [Fact]
    public void SingleTypeMapperFactoryForScheduleEndpointIsSingleton()
    {
        CheckSingletonType<ISingleTypeMapperFactory<scheduleEndpoint, EntityList<SportEventSummaryDto>>>();

        var service1 = ServiceScope1.ServiceProvider.GetRequiredService<ISingleTypeMapperFactory<scheduleEndpoint, EntityList<SportEventSummaryDto>>>();
        Assert.NotNull(service1);
        Assert.IsType<SportEventsScheduleMapperFactory>(service1, false);
    }

    [Fact]
    public void DeserializerForRestMessageIsSingleton()
    {
        CheckSingletonType<IDeserializer<RestMessage>>();
    }

    [Fact]
    public void SingleTypeMapperFactoryForSportEventSummaryMapperFactoryIsSingleton()
    {
        CheckSingletonType<ISingleTypeMapperFactory<RestMessage, SportEventSummaryDto>>();

        var service1 = ServiceScope1.ServiceProvider.GetRequiredService<ISingleTypeMapperFactory<RestMessage, SportEventSummaryDto>>();
        Assert.NotNull(service1);
        Assert.IsType<SportEventSummaryMapperFactory>(service1, false);
    }

    [Fact]
    public void DataProviderForSummaryEndpointIsSingleton()
    {
        CheckSingletonType<IDataProvider<SportEventSummaryDto>>();

        var service1 = ServiceScope1.ServiceProvider.GetRequiredService<IDataProvider<SportEventSummaryDto>>();
        Assert.NotNull(service1);
        Assert.IsType<DataProvider<RestMessage, SportEventSummaryDto>>(service1, false);
        Assert.IsType<ILogHttpDataFetcherFastFailing>(service1.DataFetcher, false);
    }

    [Fact]
    public void DeserializerForFixtureEndpointIsSingleton()
    {
        CheckSingletonType<IDeserializer<fixturesEndpoint>>();
    }

    [Fact]
    public void SingleTypeMapperFactoryForFixtureMapperFactoryIsSingleton()
    {
        CheckSingletonType<ISingleTypeMapperFactory<fixturesEndpoint, FixtureDto>>();

        var service1 = ServiceScope1.ServiceProvider.GetRequiredService<ISingleTypeMapperFactory<fixturesEndpoint, FixtureDto>>();
        Assert.NotNull(service1);
        Assert.IsType<FixtureMapperFactory>(service1, false);
    }

    [Fact]
    public void DataProviderForFixtureEndpointIsSingleton()
    {
        CheckSingletonType<IDataProvider<FixtureDto>>();

        var service1 = ServiceScope1.ServiceProvider.GetRequiredService<IDataProvider<FixtureDto>>();
        Assert.NotNull(service1);
        Assert.IsType<DataProvider<fixturesEndpoint, FixtureDto>>(service1, false);
    }

    [Fact]
    public void DataProviderForFixtureChangeFixtureEndpointIsSingleton()
    {
        CheckSingletonType<IDataProviderNamed<FixtureDto>>();

        var service1 = ServiceScope1.ServiceProvider.GetRequiredService<IDataProviderNamed<FixtureDto>>();
        Assert.NotNull(service1);
        Assert.IsType<DataProviderNamed<fixturesEndpoint, FixtureDto>>(service1, false);
        Assert.Equal(UofSdkBootstrap.DataProviderForFixtureChangeFixtureEndpoint, service1.DataProviderName);
    }

    [Fact]
    public void DeserializerForFixtureEndpointForTournamentInfoIsSingleton()
    {
        CheckSingletonType<IDeserializer<tournamentInfoEndpoint>>();
    }

    [Fact]
    public void SingleTypeMapperFactoryForFixtureMapperFactoryForTournamentInfoIsSingleton()
    {
        CheckSingletonType<ISingleTypeMapperFactory<tournamentInfoEndpoint, TournamentInfoDto>>();

        var service1 = ServiceScope1.ServiceProvider.GetRequiredService<ISingleTypeMapperFactory<tournamentInfoEndpoint, TournamentInfoDto>>();
        Assert.NotNull(service1);
        Assert.IsType<TournamentInfoMapperFactory>(service1, false);
    }

    [Fact]
    public void DataProviderForFixtureEndpointForTournamentInfoIsSingleton()
    {
        CheckSingletonType<IDataProvider<TournamentInfoDto>>();

        var service1 = ServiceScope1.ServiceProvider.GetRequiredService<IDataProvider<TournamentInfoDto>>();
        Assert.NotNull(service1);
        Assert.IsType<DataProvider<tournamentInfoEndpoint, TournamentInfoDto>>(service1, false);
    }

    [Fact]
    public void DataProviderForFixtureChangeFixtureEndpointForTournamentInfoIsSingleton()
    {
        CheckSingletonType<IDataProviderNamed<TournamentInfoDto>>();

        var service1 = ServiceScope1.ServiceProvider.GetRequiredService<IDataProviderNamed<TournamentInfoDto>>();
        Assert.NotNull(service1);
        Assert.IsType<DataProviderNamed<tournamentInfoEndpoint, TournamentInfoDto>>(service1, false);
        Assert.Equal(UofSdkBootstrap.DataProviderForFixtureChangeFixtureEndpointForTournamentInfo, service1.DataProviderName);
    }

    [Fact]
    public void DeserializerForAllTournamentEndpointIsSingleton()
    {
        CheckSingletonType<IDeserializer<tournamentsEndpoint>>();
    }

    [Fact]
    public void SingleTypeMapperFactoryForTournamentsMapperFactoryIsSingleton()
    {
        CheckSingletonType<ISingleTypeMapperFactory<tournamentsEndpoint, EntityList<SportDto>>>();

        var service1 = ServiceScope1.ServiceProvider.GetRequiredService<ISingleTypeMapperFactory<tournamentsEndpoint, EntityList<SportDto>>>();
        Assert.NotNull(service1);
        Assert.IsType<TournamentsMapperFactory>(service1, false);
    }

    [Fact]
    public void DataProviderForAllTournamentEndpointIsSingleton()
    {
        CheckSingletonDataProviderNamed<EntityList<SportDto>>(UofSdkBootstrap.DataProviderForAllTournamentsForAllSports);

        var service1 = ServiceScope1.ServiceProvider.GetDataProviderNamed<EntityList<SportDto>>(UofSdkBootstrap.DataProviderForAllTournamentsForAllSports);
        Assert.NotNull(service1);
        Assert.IsType<DataProviderNamed<tournamentsEndpoint, EntityList<SportDto>>>(service1, false);
        Assert.Equal(UofSdkBootstrap.DataProviderForAllTournamentsForAllSports, service1.DataProviderName);
    }

    [Fact]
    public void DeserializerForAllSportsEndpointIsSingleton()
    {
        CheckSingletonType<IDeserializer<sportsEndpoint>>();
    }

    [Fact]
    public void SingleTypeMapperFactoryForSportsMapperFactoryIsSingleton()
    {
        CheckSingletonType<ISingleTypeMapperFactory<sportsEndpoint, EntityList<SportDto>>>();

        var service1 = ServiceScope1.ServiceProvider.GetRequiredService<ISingleTypeMapperFactory<sportsEndpoint, EntityList<SportDto>>>();
        Assert.NotNull(service1);
        Assert.IsType<SportsMapperFactory>(service1, false);
    }

    [Fact]
    public void DataProviderForAllSportsEndpointIsSingleton()
    {
        CheckSingletonDataProviderNamed<EntityList<SportDto>>(UofSdkBootstrap.DataProviderForAllSports);

        var service1 = ServiceScope1.ServiceProvider.GetDataProviderNamed<EntityList<SportDto>>(UofSdkBootstrap.DataProviderForAllSports);
        Assert.NotNull(service1);
        Assert.IsType<DataProviderNamed<sportsEndpoint, EntityList<SportDto>>>(service1, false);
        Assert.Equal(UofSdkBootstrap.DataProviderForAllSports, service1.DataProviderName);
    }

    [Fact]
    public void DataProviderForDateScheduleIsSingleton()
    {
        CheckSingletonDataProviderNamed<EntityList<SportEventSummaryDto>>(UofSdkBootstrap.DataProviderForDateSchedule);

        var service1 = ServiceScope1.ServiceProvider.GetDataProviderNamed<EntityList<SportEventSummaryDto>>(UofSdkBootstrap.DataProviderForDateSchedule);
        Assert.NotNull(service1);
        Assert.IsType<DateScheduleProvider>(service1, false);
        Assert.Equal(UofSdkBootstrap.DataProviderForDateSchedule, service1.DataProviderName);
    }

    [Fact]
    public void DeserializerForTournamentScheduleInfoIsSingleton()
    {
        CheckSingletonType<IDeserializer<tournamentSchedule>>();
    }

    [Fact]
    public void SingleTypeMapperFactoryForTournamentScheduleIsSingleton()
    {
        CheckSingletonType<ISingleTypeMapperFactory<scheduleEndpoint, EntityList<SportEventSummaryDto>>>();

        var service1 = ServiceScope1.ServiceProvider.GetRequiredService<ISingleTypeMapperFactory<tournamentSchedule, EntityList<SportEventSummaryDto>>>();
        Assert.NotNull(service1);
        Assert.IsType<TournamentScheduleMapperFactory>(service1, false);
    }

    [Fact]
    public void DataProviderForTournamentScheduleIsSingleton()
    {
        CheckSingletonDataProviderNamed<EntityList<SportEventSummaryDto>>(UofSdkBootstrap.DataProviderForTournamentSchedule);

        var service1 = ServiceScope1.ServiceProvider.GetDataProviderNamed<EntityList<SportEventSummaryDto>>(UofSdkBootstrap.DataProviderForTournamentSchedule);
        Assert.NotNull(service1);
        Assert.IsType<DataProviderNamed<tournamentSchedule, EntityList<SportEventSummaryDto>>>(service1, false);
        Assert.Equal(UofSdkBootstrap.DataProviderForTournamentSchedule, service1.DataProviderName);
    }

    [Fact]
    public void DeserializerForStageTournamentScheduleInfoIsSingleton()
    {
        CheckSingletonType<IDeserializer<raceScheduleEndpoint>>();
    }

    [Fact]
    public void SingleTypeMapperFactoryForStageTournamentScheduleIsSingleton()
    {
        CheckSingletonType<ISingleTypeMapperFactory<raceScheduleEndpoint, EntityList<SportEventSummaryDto>>>();

        var service1 = ServiceScope1.ServiceProvider.GetRequiredService<ISingleTypeMapperFactory<raceScheduleEndpoint, EntityList<SportEventSummaryDto>>>();
        Assert.NotNull(service1);
        Assert.IsType<TournamentRaceScheduleMapperFactory>(service1, false);
    }

    [Fact]
    public void DataProviderForStageTournamentScheduleIsSingleton()
    {
        CheckSingletonDataProviderNamed<EntityList<SportEventSummaryDto>>(UofSdkBootstrap.DataProviderForStageTournamentSchedule);

        var service1 = ServiceScope1.ServiceProvider.GetDataProviderNamed<EntityList<SportEventSummaryDto>>(UofSdkBootstrap.DataProviderForStageTournamentSchedule);
        Assert.NotNull(service1);
        Assert.IsType<DataProviderNamed<raceScheduleEndpoint, EntityList<SportEventSummaryDto>>>(service1, false);
        Assert.Equal(UofSdkBootstrap.DataProviderForStageTournamentSchedule, service1.DataProviderName);
    }

    [Fact]
    public void DeserializerForTournamentSeasonsIsSingleton()
    {
        CheckSingletonType<IDeserializer<tournamentSeasons>>();
    }

    [Fact]
    public void SingleTypeMapperFactoryForTournamentSeasonsIsSingleton()
    {
        CheckSingletonType<ISingleTypeMapperFactory<tournamentSeasons, TournamentSeasonsDto>>();

        var service1 = ServiceScope1.ServiceProvider.GetRequiredService<ISingleTypeMapperFactory<tournamentSeasons, TournamentSeasonsDto>>();
        Assert.NotNull(service1);
        Assert.IsType<TournamentSeasonsMapperFactory>(service1, false);
    }

    [Fact]
    public void DataProviderForTournamentSeasonsIsSingleton()
    {
        CheckSingletonType<IDataProvider<TournamentSeasonsDto>>();

        var service1 = ServiceScope1.ServiceProvider.GetRequiredService<IDataProvider<TournamentSeasonsDto>>();
        Assert.NotNull(service1);
        Assert.IsType<DataProvider<tournamentSeasons, TournamentSeasonsDto>>(service1, false);
    }

    [Fact]
    public void DeserializerForPlayerProfileIsSingleton()
    {
        CheckSingletonType<IDeserializer<playerProfileEndpoint>>();
    }

    [Fact]
    public void SingleTypeMapperFactoryForPlayerProfileIsSingleton()
    {
        CheckSingletonType<ISingleTypeMapperFactory<playerProfileEndpoint, PlayerProfileDto>>();

        var service1 = ServiceScope1.ServiceProvider.GetRequiredService<ISingleTypeMapperFactory<playerProfileEndpoint, PlayerProfileDto>>();
        Assert.NotNull(service1);
        Assert.IsType<PlayerProfileMapperFactory>(service1, false);
    }

    [Fact]
    public void DataProviderForPlayerProfileIsSingleton()
    {
        CheckSingletonType<IDataProvider<PlayerProfileDto>>();

        var service1 = ServiceScope1.ServiceProvider.GetRequiredService<IDataProvider<PlayerProfileDto>>();
        Assert.NotNull(service1);
        Assert.IsType<DataProvider<playerProfileEndpoint, PlayerProfileDto>>(service1, false);
        Assert.IsType<ILogHttpDataFetcherFastFailing>(service1.DataFetcher, false);
    }

    [Fact]
    public void DeserializerForCompetitorProfileIsSingleton()
    {
        CheckSingletonType<IDeserializer<competitorProfileEndpoint>>();
    }

    [Fact]
    public void SingleTypeMapperFactoryForCompetitorProfileIsSingleton()
    {
        CheckSingletonType<ISingleTypeMapperFactory<competitorProfileEndpoint, CompetitorProfileDto>>();

        var service1 = ServiceScope1.ServiceProvider.GetRequiredService<ISingleTypeMapperFactory<competitorProfileEndpoint, CompetitorProfileDto>>();
        Assert.NotNull(service1);
        Assert.IsType<CompetitorProfileMapperFactory>(service1, false);
    }

    [Fact]
    public void DataProviderForCompetitorProfileIsSingleton()
    {
        CheckSingletonType<IDataProvider<CompetitorProfileDto>>();

        var service1 = ServiceScope1.ServiceProvider.GetRequiredService<IDataProvider<CompetitorProfileDto>>();
        Assert.NotNull(service1);
        Assert.IsType<DataProvider<competitorProfileEndpoint, CompetitorProfileDto>>(service1, false);
        Assert.IsType<ILogHttpDataFetcherFastFailing>(service1.DataFetcher, false);
    }

    [Fact]
    public void DeserializerForSimpleTeamProfileIsSingleton()
    {
        CheckSingletonType<IDeserializer<simpleTeamProfileEndpoint>>();
    }

    [Fact]
    public void SingleTypeMapperFactoryForSimpleTeamProfileIsSingleton()
    {
        CheckSingletonType<ISingleTypeMapperFactory<simpleTeamProfileEndpoint, SimpleTeamProfileDto>>();

        var service1 = ServiceScope1.ServiceProvider.GetRequiredService<ISingleTypeMapperFactory<simpleTeamProfileEndpoint, SimpleTeamProfileDto>>();
        Assert.NotNull(service1);
        Assert.IsType<SimpleTeamProfileMapperFactory>(service1, false);
    }

    [Fact]
    public void DataProviderForSimpleTeamProfileIsSingleton()
    {
        CheckSingletonType<IDataProvider<SimpleTeamProfileDto>>();

        var service1 = ServiceScope1.ServiceProvider.GetRequiredService<IDataProvider<SimpleTeamProfileDto>>();
        Assert.NotNull(service1);
        Assert.IsType<DataProvider<simpleTeamProfileEndpoint, SimpleTeamProfileDto>>(service1, false);
        Assert.IsType<ILogHttpDataFetcherFastFailing>(service1.DataFetcher, false);
    }

    [Fact]
    public void DeserializerForMatchTimelineIsSingleton()
    {
        CheckSingletonType<IDeserializer<matchTimelineEndpoint>>();
    }

    [Fact]
    public void SingleTypeMapperFactoryForMatchTimelineIsSingleton()
    {
        CheckSingletonType<ISingleTypeMapperFactory<matchTimelineEndpoint, MatchTimelineDto>>();

        var service1 = ServiceScope1.ServiceProvider.GetRequiredService<ISingleTypeMapperFactory<matchTimelineEndpoint, MatchTimelineDto>>();
        Assert.NotNull(service1);
        Assert.IsType<MatchTimelineMapperFactory>(service1, false);
    }

    [Fact]
    public void DataProviderForMatchTimelineIsSingleton()
    {
        CheckSingletonType<IDataProvider<MatchTimelineDto>>();

        var service1 = ServiceScope1.ServiceProvider.GetRequiredService<IDataProvider<MatchTimelineDto>>();
        Assert.NotNull(service1);
        Assert.IsType<DataProvider<matchTimelineEndpoint, MatchTimelineDto>>(service1, false);
    }

    [Fact]
    public void DeserializerForSportCategoriesIsSingleton()
    {
        CheckSingletonType<IDeserializer<sportCategoriesEndpoint>>();
    }

    [Fact]
    public void SingleTypeMapperFactoryForSportCategoriesIsSingleton()
    {
        CheckSingletonType<ISingleTypeMapperFactory<sportCategoriesEndpoint, SportCategoriesDto>>();

        var service1 = ServiceScope1.ServiceProvider.GetRequiredService<ISingleTypeMapperFactory<sportCategoriesEndpoint, SportCategoriesDto>>();
        Assert.NotNull(service1);
        Assert.IsType<SportCategoriesMapperFactory>(service1, false);
    }

    [Fact]
    public void DataProviderForSportCategoriesIsSingleton()
    {
        CheckSingletonType<IDataProvider<SportCategoriesDto>>();

        var service1 = ServiceScope1.ServiceProvider.GetRequiredService<IDataProvider<SportCategoriesDto>>();
        Assert.NotNull(service1);
        Assert.IsType<DataProvider<sportCategoriesEndpoint, SportCategoriesDto>>(service1, false);
    }

    [Fact]
    public void DeserializerForFixtureChangesIsSingleton()
    {
        CheckSingletonType<IDeserializer<fixtureChangesEndpoint>>();
    }

    [Fact]
    public void SingleTypeMapperFactoryForFixtureChangesIsSingleton()
    {
        CheckSingletonType<ISingleTypeMapperFactory<fixtureChangesEndpoint, EntityList<FixtureChangeDto>>>();

        var service1 = ServiceScope1.ServiceProvider.GetRequiredService<ISingleTypeMapperFactory<fixtureChangesEndpoint, EntityList<FixtureChangeDto>>>();
        Assert.NotNull(service1);
        Assert.IsType<FixtureChangesMapperFactory>(service1, false);
    }

    [Fact]
    public void DataProviderForFixtureChangesIsSingleton()
    {
        CheckSingletonType<IDataProvider<EntityList<FixtureChangeDto>>>();

        var service1 = ServiceScope1.ServiceProvider.GetRequiredService<IDataProvider<EntityList<FixtureChangeDto>>>();
        Assert.NotNull(service1);
        Assert.IsType<DataProvider<fixtureChangesEndpoint, EntityList<FixtureChangeDto>>>(service1, false);
    }

    [Fact]
    public void DeserializerForInvariantMarketListIsSingleton()
    {
        CheckSingletonType<IDeserializer<market_descriptions>>();
    }

    [Fact]
    public void SingleTypeMapperFactoryForInvariantMarketListIsSingleton()
    {
        CheckSingletonType<ISingleTypeMapperFactory<market_descriptions, EntityList<MarketDescriptionDto>>>();

        var service1 = ServiceScope1.ServiceProvider.GetRequiredService<ISingleTypeMapperFactory<market_descriptions, EntityList<MarketDescriptionDto>>>();
        Assert.NotNull(service1);
        Assert.IsType<MarketDescriptionsMapperFactory>(service1, false);
    }

    [Fact]
    public void DataProviderForInvariantMarketListIsSingleton()
    {
        CheckSingletonDataProviderNamed<EntityList<MarketDescriptionDto>>(UofSdkBootstrap.DataProviderForInvariantMarketList);

        var service1 = ServiceScope1.ServiceProvider.GetDataProviderNamed<EntityList<MarketDescriptionDto>>(UofSdkBootstrap.DataProviderForInvariantMarketList);
        Assert.NotNull(service1);
        Assert.IsType<DataProviderNamed<market_descriptions, EntityList<MarketDescriptionDto>>>(service1, false);
        Assert.Equal(UofSdkBootstrap.DataProviderForInvariantMarketList, service1.DataProviderName);
    }

    [Fact]
    public void DeserializerForVariantMarketListIsSingleton()
    {
        CheckSingletonType<IDeserializer<variant_descriptions>>();
    }

    [Fact]
    public void SingleTypeMapperFactoryForVariantMarketListIsSingleton()
    {
        CheckSingletonType<ISingleTypeMapperFactory<variant_descriptions, EntityList<VariantDescriptionDto>>>();

        var service1 = ServiceScope1.ServiceProvider.GetRequiredService<ISingleTypeMapperFactory<variant_descriptions, EntityList<VariantDescriptionDto>>>();
        Assert.NotNull(service1);
        Assert.IsType<VariantDescriptionsMapperFactory>(service1, false);
    }

    [Fact]
    public void DataProviderForVariantMarketListIsSingleton()
    {
        CheckSingletonDataProviderNamed<EntityList<VariantDescriptionDto>>(UofSdkBootstrap.DataProviderForVariantMarketList);

        var service1 = ServiceScope1.ServiceProvider.GetDataProviderNamed<EntityList<VariantDescriptionDto>>(UofSdkBootstrap.DataProviderForVariantMarketList);
        Assert.NotNull(service1);
        Assert.IsType<DataProviderNamed<variant_descriptions, EntityList<VariantDescriptionDto>>>(service1, false);
        Assert.Equal(UofSdkBootstrap.DataProviderForVariantMarketList, service1.DataProviderName);
    }

    [Fact]
    public void DeserializerForVariantMarketIsSingleton()
    {
        CheckSingletonType<IDeserializer<market_descriptions>>();
    }

    [Fact]
    public void SingleTypeMapperFactoryForVariantMarketIsSingleton()
    {
        CheckSingletonType<ISingleTypeMapperFactory<market_descriptions, MarketDescriptionDto>>();

        var service1 = ServiceScope1.ServiceProvider.GetRequiredService<ISingleTypeMapperFactory<market_descriptions, MarketDescriptionDto>>();
        Assert.NotNull(service1);
        Assert.IsType<MarketDescriptionMapperFactory>(service1, false);
    }

    [Fact]
    public void DataProviderForVariantMarketIsSingleton()
    {
        CheckSingletonDataProviderNamed<MarketDescriptionDto>(UofSdkBootstrap.DataProviderForVariantMarket);

        var service1 = ServiceScope1.ServiceProvider.GetDataProviderNamed<MarketDescriptionDto>(UofSdkBootstrap.DataProviderForVariantMarket);
        Assert.NotNull(service1);
        Assert.IsType<DataProviderNamed<market_descriptions, MarketDescriptionDto>>(service1, false);
        Assert.Equal(UofSdkBootstrap.DataProviderForVariantMarket, service1.DataProviderName);
        Assert.IsType<ILogHttpDataFetcherFastFailing>(service1.DataFetcher, false);
    }

    [Fact]
    public void DeserializerForLotteryDrawSummaryIsSingleton()
    {
        CheckSingletonType<IDeserializer<draw_summary>>();
    }

    [Fact]
    public void SingleTypeMapperFactoryForLotteryDrawSummaryIsSingleton()
    {
        CheckSingletonType<ISingleTypeMapperFactory<draw_summary, DrawDto>>();

        var service1 = ServiceScope1.ServiceProvider.GetRequiredService<ISingleTypeMapperFactory<draw_summary, DrawDto>>();
        Assert.NotNull(service1);
        Assert.IsType<DrawSummaryMapperFactory>(service1, false);
    }

    [Fact]
    public void DataProviderForLotteryDrawSummaryIsSingleton()
    {
        CheckSingletonDataProviderNamed<DrawDto>(UofSdkBootstrap.DataProviderForLotteryDrawSummary);

        var service1 = ServiceScope1.ServiceProvider.GetDataProviderNamed<DrawDto>(UofSdkBootstrap.DataProviderForLotteryDrawSummary);
        Assert.NotNull(service1);
        Assert.IsType<DataProviderNamed<draw_summary, DrawDto>>(service1, false);
        Assert.Equal(UofSdkBootstrap.DataProviderForLotteryDrawSummary, service1.DataProviderName);
    }

    [Fact]
    public void DeserializerForLotteryDrawFixtureIsSingleton()
    {
        CheckSingletonType<IDeserializer<draw_fixtures>>();
    }

    [Fact]
    public void SingleTypeMapperFactoryForLotteryDrawFixtureIsSingleton()
    {
        CheckSingletonType<ISingleTypeMapperFactory<draw_fixtures, DrawDto>>();

        var service1 = ServiceScope1.ServiceProvider.GetRequiredService<ISingleTypeMapperFactory<draw_fixtures, DrawDto>>();
        Assert.NotNull(service1);
        Assert.IsType<DrawFixtureMapperFactory>(service1, false);
    }

    [Fact]
    public void DataProviderForLotteryDrawFixtureIsSingleton()
    {
        CheckSingletonDataProviderNamed<DrawDto>(UofSdkBootstrap.DataProviderForLotteryDrawFixture);

        var service1 = ServiceScope1.ServiceProvider.GetDataProviderNamed<DrawDto>(UofSdkBootstrap.DataProviderForLotteryDrawFixture);
        Assert.NotNull(service1);
        Assert.IsType<DataProviderNamed<draw_fixtures, DrawDto>>(service1, false);
        Assert.Equal(UofSdkBootstrap.DataProviderForLotteryDrawFixture, service1.DataProviderName);
    }

    [Fact]
    public void DeserializerForLotteryScheduleIsSingleton()
    {
        CheckSingletonType<IDeserializer<lottery_schedule>>();
    }

    [Fact]
    public void SingleTypeMapperFactoryForLotteryScheduleIsSingleton()
    {
        CheckSingletonType<ISingleTypeMapperFactory<lottery_schedule, LotteryDto>>();

        var service1 = ServiceScope1.ServiceProvider.GetRequiredService<ISingleTypeMapperFactory<lottery_schedule, LotteryDto>>();
        Assert.NotNull(service1);
        Assert.IsType<LotteryScheduleMapperFactory>(service1, false);
    }

    [Fact]
    public void DataProviderForLotteryScheduleIsSingleton()
    {
        CheckSingletonType<IDataProvider<LotteryDto>>();

        var service1 = ServiceScope1.ServiceProvider.GetRequiredService<IDataProvider<LotteryDto>>();
        Assert.NotNull(service1);
        Assert.IsType<DataProvider<lottery_schedule, LotteryDto>>(service1, false);
    }

    [Fact]
    public void DeserializerForLotteryListIsSingleton()
    {
        CheckSingletonType<IDeserializer<lotteries>>();
    }

    [Fact]
    public void SingleTypeMapperFactoryForLotteryListIsSingleton()
    {
        CheckSingletonType<ISingleTypeMapperFactory<lotteries, EntityList<LotteryDto>>>();

        var service1 = ServiceScope1.ServiceProvider.GetRequiredService<ISingleTypeMapperFactory<lotteries, EntityList<LotteryDto>>>();
        Assert.NotNull(service1);
        Assert.IsType<LotteriesMapperFactory>(service1, false);
    }

    [Fact]
    public void DataProviderForLotteryListIsSingleton()
    {
        CheckSingletonType<IDataProvider<EntityList<LotteryDto>>>();

        var service1 = ServiceScope1.ServiceProvider.GetRequiredService<IDataProvider<EntityList<LotteryDto>>>();
        Assert.NotNull(service1);
        Assert.IsType<DataProvider<lotteries, EntityList<LotteryDto>>>(service1, false);
    }

    [Fact]
    public void DataProviderForSportEventListIsSingleton()
    {
        CheckSingletonDataProviderNamed<EntityList<SportEventSummaryDto>>(UofSdkBootstrap.DataProviderForSportEventList);

        var service1 = ServiceScope1.ServiceProvider.GetDataProviderNamed<EntityList<SportEventSummaryDto>>(UofSdkBootstrap.DataProviderForSportEventList);
        Assert.NotNull(service1);
        Assert.IsType<DataProviderNamed<scheduleEndpoint, EntityList<SportEventSummaryDto>>>(service1, false);
        Assert.Equal(UofSdkBootstrap.DataProviderForSportEventList, service1.DataProviderName);
    }

    [Fact]
    public void DeserializerForSportAvailableTournamentListIsSingleton()
    {
        CheckSingletonType<IDeserializer<sportTournamentsEndpoint>>();
    }

    [Fact]
    public void SingleTypeMapperFactoryForSportAvailableTournamentListIsSingleton()
    {
        CheckSingletonType<ISingleTypeMapperFactory<sportTournamentsEndpoint, EntityList<TournamentInfoDto>>>();

        var service1 = ServiceScope1.ServiceProvider.GetRequiredService<ISingleTypeMapperFactory<sportTournamentsEndpoint, EntityList<TournamentInfoDto>>>();
        Assert.NotNull(service1);
        Assert.IsType<ListSportAvailableTournamentMapperFactory>(service1, false);
    }

    [Fact]
    public void DataProviderForSportAvailableTournamentListIsSingleton()
    {
        CheckSingletonType<IDataProvider<EntityList<TournamentInfoDto>>>();

        var service1 = ServiceScope1.ServiceProvider.GetRequiredService<IDataProvider<EntityList<TournamentInfoDto>>>();
        Assert.NotNull(service1);
        Assert.IsType<DataProvider<sportTournamentsEndpoint, EntityList<TournamentInfoDto>>>(service1, false);
    }

    [Fact]
    public void DeserializerForStagePeriodSummaryIsSingleton()
    {
        CheckSingletonType<IDeserializer<stagePeriodEndpoint>>();
    }

    [Fact]
    public void SingleTypeMapperFactoryForStagePeriodSummaryIsSingleton()
    {
        CheckSingletonType<ISingleTypeMapperFactory<stagePeriodEndpoint, PeriodSummaryDto>>();

        var service1 = ServiceScope1.ServiceProvider.GetRequiredService<ISingleTypeMapperFactory<stagePeriodEndpoint, PeriodSummaryDto>>();
        Assert.NotNull(service1);
        Assert.IsType<PeriodSummaryMapperFactory>(service1, false);
    }

    [Fact]
    public void DataProviderForStagePeriodSummaryIsSingleton()
    {
        CheckSingletonType<IDataProvider<EntityList<TournamentInfoDto>>>();

        var service1 = ServiceScope1.ServiceProvider.GetRequiredService<IDataProvider<PeriodSummaryDto>>();
        Assert.NotNull(service1);
        Assert.IsType<DataProvider<stagePeriodEndpoint, PeriodSummaryDto>>(service1, false);
    }

    [Fact]
    public void DeserializerForCustomBetAvailableSelectionsIsSingleton()
    {
        CheckSingletonType<IDeserializer<AvailableSelectionsType>>();
    }

    [Fact]
    public void SingleTypeMapperFactoryForCustomBetAvailableSelectionsIsSingleton()
    {
        CheckSingletonType<ISingleTypeMapperFactory<AvailableSelectionsType, AvailableSelectionsDto>>();

        var service1 = ServiceScope1.ServiceProvider.GetRequiredService<ISingleTypeMapperFactory<AvailableSelectionsType, AvailableSelectionsDto>>();
        Assert.NotNull(service1);
        Assert.IsType<AvailableSelectionsMapperFactory>(service1, false);
    }

    [Fact]
    public void DataProviderForCustomBetAvailableSelectionsIsSingleton()
    {
        CheckSingletonType<IDataProvider<AvailableSelectionsDto>>();

        var service1 = ServiceScope1.ServiceProvider.GetRequiredService<IDataProvider<AvailableSelectionsDto>>();
        Assert.NotNull(service1);
        Assert.IsType<DataProvider<AvailableSelectionsType, AvailableSelectionsDto>>(service1, false);
    }

    [Fact]
    public void DeserializerForCustomBetCalculationIsSingleton()
    {
        CheckSingletonType<IDeserializer<CalculationResponseType>>();
    }

    [Fact]
    public void SingleTypeMapperFactoryForCustomBetCalculationIsSingleton()
    {
        CheckSingletonType<ISingleTypeMapperFactory<CalculationResponseType, CalculationDto>>();

        var service1 = ServiceScope1.ServiceProvider.GetRequiredService<ISingleTypeMapperFactory<CalculationResponseType, CalculationDto>>();
        Assert.NotNull(service1);
        Assert.IsType<CalculationMapperFactory>(service1, false);
    }

    [Fact]
    public void DataProviderForCustomBetCalculationIsSingleton()
    {
        CheckSingletonType<ICalculateProbabilityProvider>();

        var service1 = ServiceScope1.ServiceProvider.GetRequiredService<ICalculateProbabilityProvider>();
        Assert.NotNull(service1);
        Assert.IsType<CalculateProbabilityProvider>(service1, false);
    }

    [Fact]
    public void DeserializerForCustomBetCalculationFilteredIsSingleton()
    {
        CheckSingletonType<IDeserializer<FilteredCalculationResponseType>>();
    }

    [Fact]
    public void SingleTypeMapperFactoryForCustomBetCalculationFilteredIsSingleton()
    {
        CheckSingletonType<ISingleTypeMapperFactory<FilteredCalculationResponseType, FilteredCalculationDto>>();

        var service1 = ServiceScope1.ServiceProvider.GetRequiredService<ISingleTypeMapperFactory<FilteredCalculationResponseType, FilteredCalculationDto>>();
        Assert.NotNull(service1);
        Assert.IsType<CalculationFilteredMapperFactory>(service1, false);
    }

    [Fact]
    public void DataProviderForCustomBetCalculationFilteredIsSingleton()
    {
        CheckSingletonType<ICalculateProbabilityFilteredProvider>();

        var service1 = ServiceScope1.ServiceProvider.GetRequiredService<ICalculateProbabilityFilteredProvider>();
        Assert.NotNull(service1);
        Assert.IsType<CalculateProbabilityFilteredProvider>(service1, false);
    }

    [Fact]
    public void DataRouterManagerIsSingleton()
    {
        CheckSingletonType<IDataRouterManager>();
    }

    [Fact]
    public void DataProviderForNamedCacheVoidReasonIsSingleton()
    {
        CheckSingletonDataProviderNamed<EntityList<NamedValueDto>>(UofSdkBootstrap.DataProviderForNamedValueCacheVoidReason);

        var service1 = ServiceScope1.ServiceProvider.GetDataProviderNamed<EntityList<NamedValueDto>>(UofSdkBootstrap.DataProviderForNamedValueCacheVoidReason);
        Assert.NotNull(service1);
        Assert.IsType<NamedValueDataProvider>(service1, false);
        Assert.Equal(UofSdkBootstrap.DataProviderForNamedValueCacheVoidReason, service1.DataProviderName);
    }

    [Fact]
    public void DataProviderForNamedCacheBetStopReasonIsSingleton()
    {
        CheckSingletonDataProviderNamed<EntityList<NamedValueDto>>(UofSdkBootstrap.DataProviderForNamedValueCacheBetStopReason);

        var service1 = ServiceScope1.ServiceProvider.GetDataProviderNamed<EntityList<NamedValueDto>>(UofSdkBootstrap.DataProviderForNamedValueCacheBetStopReason);
        Assert.NotNull(service1);
        Assert.IsType<NamedValueDataProvider>(service1, false);
        Assert.Equal(UofSdkBootstrap.DataProviderForNamedValueCacheBetStopReason, service1.DataProviderName);
    }

    [Fact]
    public void DataProviderForNamedCacheBettingStatusIsSingleton()
    {
        CheckSingletonDataProviderNamed<EntityList<NamedValueDto>>(UofSdkBootstrap.DataProviderForNamedValueCacheBettingStatus);

        var service1 = ServiceScope1.ServiceProvider.GetDataProviderNamed<EntityList<NamedValueDto>>(UofSdkBootstrap.DataProviderForNamedValueCacheBettingStatus);
        Assert.NotNull(service1);
        Assert.IsType<NamedValueDataProvider>(service1, false);
        Assert.Equal(UofSdkBootstrap.DataProviderForNamedValueCacheBettingStatus, service1.DataProviderName);
    }

    [Fact]
    public void DataProviderForNamedCacheMatchStatusIsSingleton()
    {
        CheckSingletonDataProviderNamed<EntityList<NamedValueDto>>(UofSdkBootstrap.DataProviderForNamedValueCacheMatchStatus);

        var service1 = ServiceScope1.ServiceProvider.GetDataProviderNamed<EntityList<NamedValueDto>>(UofSdkBootstrap.DataProviderForNamedValueCacheMatchStatus);
        Assert.NotNull(service1);
        Assert.IsType<NamedValueDataProvider>(service1, false);
        Assert.Equal(UofSdkBootstrap.DataProviderForNamedValueCacheMatchStatus, service1.DataProviderName);
    }

    [Fact]
    public void HttpClientsForAuthenticationWhenAuthenticationConfigurationIsMissingThenNoBaseAddress()
    {
        var httpClientForAuthentication = ServiceScope1.ServiceProvider.GetRequiredService<IHttpClientFactory>().CreateClient(UofSdkBootstrap.HttpClientNameForAuthentication);

        httpClientForAuthentication.BaseAddress.ShouldBeNull();
    }

    [Fact]
    public void HttpClientsForAuthenticationWhenAuthenticationConfigurationIsPresentThenValidBaseAddress()
    {
        var configWithAuth = TestConfiguration.GetConfigWithCiam();

        var serviceCollection = new ServiceCollection();
        serviceCollection.AddUofSdkServices(configWithAuth);

        var serviceProvider = serviceCollection.BuildServiceProvider(true);
        var httpClientForAuthentication = serviceProvider.GetRequiredService<IHttpClientFactory>().CreateClient(UofSdkBootstrap.HttpClientNameForAuthentication);

        httpClientForAuthentication.BaseAddress.ShouldNotBeNull();
        var address = httpClientForAuthentication.BaseAddress.ToString();

        address.ShouldStartWith("http");
        address.ShouldBe(configWithAuth.Authentication.GetAudienceForLocalToken());
    }
}

