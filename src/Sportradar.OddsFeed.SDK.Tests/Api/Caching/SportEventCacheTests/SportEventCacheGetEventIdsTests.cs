// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using Moq;
using Sportradar.OddsFeed.SDK.Api.Internal.ApiAccess;
using Sportradar.OddsFeed.SDK.Api.Internal.Caching;
using Sportradar.OddsFeed.SDK.Api.Internal.Config;
using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Dto;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Mapping;
using Sportradar.OddsFeed.SDK.Messages.Rest;
using Sportradar.OddsFeed.SDK.Tests.Common;
using Sportradar.OddsFeed.SDK.Tests.Common.Builders;
using Sportradar.OddsFeed.SDK.Tests.Common.Builders.Extensions;
using Xunit;
using Xunit.Abstractions;

namespace Sportradar.OddsFeed.SDK.Tests.API.Caching.SportEventCacheTests;

public class SportEventCacheGetEventIdsTests : SportEventCacheSetup
{
    private const string ScheduleEndpoint = "/v1/sports/1/tournaments/0/schedule.xml";
    private readonly Urn _anyTournamentId = UrnCreate.TournamentId(123);
    private readonly Mock<IDataFetcher> _sportEventsForTournamentFetcherMock;

    public SportEventCacheGetEventIdsTests(ITestOutputHelper outputHelper)
        : base(outputHelper)
    {
        _sportEventsForTournamentFetcherMock = new Mock<IDataFetcher>();

        var cacheManager = new CacheManager();
        var dataRouterManager = StubDataRouterManagerForGetSportEventsForTournament(cacheManager, _sportEventsForTournamentFetcherMock.Object);

        SportEventCache = CreateSportEventCache(LoggerFactory, dataRouterManager, cacheManager, TestConfig, SportEventCacheStore);
    }

    [Fact]
    public async Task GetEventIdsForTournament()
    {
        var endpointResponse = new tournamentSchedule
        {
            tournament = [],
            sport_events = []
        };
        SetupSportEventsForTournamentFetcher(endpointResponse);

        await SportEventCache.GetEventIdsAsync(_anyTournamentId, (IEnumerable<CultureInfo>)null);

        var endpointUriEnd = "/1/tournaments/0/schedule.xml";
        _sportEventsForTournamentFetcherMock.Verify(v => v.GetDataAsync(It.Is<Uri>(uri => uri.ToString().EndsWith(endpointUriEnd))), Times.Exactly(TestConfig.Languages.Count));
    }

    private DataRouterManager StubDataRouterManagerForGetSportEventsForTournament(CacheManager cacheManager, IDataFetcher sportEventsForTournamentFetcher)
    {
        var sportEventsForTournamentProvider = new DataProviderNamed<tournamentSchedule, EntityList<SportEventSummaryDto>>(UofSdkBootstrap.DataProviderForTournamentSchedule,
                                                                                                                           $"{TestConfig.Api.BaseUrl}/v1/sports/{1}/tournaments/{0}/schedule.xml",
                                                                                                                           sportEventsForTournamentFetcher,
                                                                                                                           new Deserializer<tournamentSchedule>(),
                                                                                                                           new TournamentScheduleMapperFactory());

        return new DataRouterManagerBuilder()
              .AddMockedDependencies()
              .WithCacheManager(cacheManager)
              .WithSportEventsForTournamentProvider(sportEventsForTournamentProvider)
              .Build();
    }

    private void SetupSportEventsForTournamentFetcher(tournamentSchedule endpointResponse)
    {
        var endpointUri = new Uri(TestConfig.Api.BaseUrl + ScheduleEndpoint);
        _sportEventsForTournamentFetcherMock.Setup(fetcher => fetcher.GetDataAsync(endpointUri))
                                            .ReturnsAsync(() => DeserializerHelper.SerializeToMemoryStream(endpointResponse));
    }
}
