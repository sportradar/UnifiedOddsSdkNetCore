using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Moq;
using Sportradar.OddsFeed.SDK.Api.Internal.Caching;
using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Entities.Rest;
using Sportradar.OddsFeed.SDK.Entities.Rest.Enums;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Caching.Events;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Dto;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.EntitiesImpl;
using Sportradar.OddsFeed.SDK.Messages.Feed;
using Sportradar.OddsFeed.SDK.Messages.Rest;
using Sportradar.OddsFeed.SDK.Tests.Common;
using Xunit;

namespace Sportradar.OddsFeed.SDK.Tests.Entities.Rest;
public class MatchStatusTests
{
    private readonly Mock<ILocalizedNamedValueCache> _matchStatusesCacheMock;

    public MatchStatusTests()
    {
        var matchStatuses = new Dictionary<CultureInfo, string> { { TestData.Culture, "1st period" } };
        _matchStatusesCacheMock = new Mock<ILocalizedNamedValueCache>();
        _matchStatusesCacheMock.Setup(x => x.CacheName).Returns("AnyCacheName");
        _matchStatusesCacheMock.Setup(x => x.GetAsync(1, It.IsAny<IEnumerable<CultureInfo>>()))
            .Returns(Task.FromResult((ILocalizedNamedValue)new LocalizedNamedValue(1, matchStatuses, TestData.Culture)));
        _matchStatusesCacheMock.Setup(x => x.IsValueDefined(1)).Returns(true);
        _matchStatusesCacheMock.Setup(x => x.IsValueDefined(It.IsNotIn(1))).Returns(false);
    }

    [Fact]
    public void ConstructorWithoutCacheItemThrows()
    {
        Assert.Throws<ArgumentNullException>(() => new MatchStatus(null, _matchStatusesCacheMock.Object));
    }

    [Fact]
    public void ConstructorWithoutMatchStatusCacheThrows()
    {
        var sportEventStatus = new sportEventStatus
        {
            home_score = 1.6m,
            home_scoreSpecified = true
        };
        var sportEventStatusDto = new SportEventStatusDto(sportEventStatus, new Dictionary<HomeAway, Urn>());
        var sportEventStatusCi = new SportEventStatusCacheItem(sportEventStatusDto, null);

        Assert.Throws<ArgumentNullException>(() => new MatchStatus(sportEventStatusCi, null));
    }

    [Fact]
    public void ConstructorSetsHomeScoreFromFeedData()
    {
        var sportEventStatus = new sportEventStatus
        {
            home_score = 1.6m,
            home_scoreSpecified = true
        };
        var sportEventStatusDto = new SportEventStatusDto(sportEventStatus, new Dictionary<HomeAway, Urn>());
        var sportEventStatusCi = new SportEventStatusCacheItem(sportEventStatusDto, null);
        var matchStatus = new MatchStatus(sportEventStatusCi, _matchStatusesCacheMock.Object);

        Assert.Equal(1.6m, matchStatus.HomeScore);
    }

    [Fact]
    public void ConstructorSetsAwayScoreFromFeedData()
    {
        var sportEventStatus = new sportEventStatus
        {
            away_score = 1.6m,
            away_scoreSpecified = true
        };
        var sportEventStatusDto = new SportEventStatusDto(sportEventStatus, new Dictionary<HomeAway, Urn>());
        var sportEventStatusCi = new SportEventStatusCacheItem(sportEventStatusDto, null);
        var matchStatus = new MatchStatus(sportEventStatusCi, _matchStatusesCacheMock.Object);

        Assert.Equal(1.6m, matchStatus.AwayScore);
    }

    [Fact]
    public void ConstructorSetsHomePenaltyScoreFromFeedData()
    {
        var sportEventStatus = new sportEventStatus
        {
            home_penalty_score = 2,
            home_penalty_scoreSpecified = true
        };
        var sportEventStatusDto = new SportEventStatusDto(sportEventStatus, new Dictionary<HomeAway, Urn>());
        var sportEventStatusCi = new SportEventStatusCacheItem(sportEventStatusDto, null);
        var matchStatus = new MatchStatus(sportEventStatusCi, _matchStatusesCacheMock.Object);

        Assert.Equal(2, matchStatus.HomePenaltyScore);
    }

    [Fact]
    public void ConstructorSetsAwayPenaltyScoreFromFeedData()
    {
        var sportEventStatus = new sportEventStatus
        {
            away_penalty_score = 2,
            away_penalty_scoreSpecified = true
        };
        var sportEventStatusDto = new SportEventStatusDto(sportEventStatus, new Dictionary<HomeAway, Urn>());
        var sportEventStatusCi = new SportEventStatusCacheItem(sportEventStatusDto, null);
        var matchStatus = new MatchStatus(sportEventStatusCi, _matchStatusesCacheMock.Object);

        Assert.Equal(2, matchStatus.AwayPenaltyScore);
    }

    [Fact]
    public void ConstructorSetsHomeScoreFromRestData()
    {
        var sportEventStatus = new sportEventStatus
        {
            home_score = 1.6m,
            home_scoreSpecified = true
        };
        var sportEventStatusDto = new SportEventStatusDto(sportEventStatus, new Dictionary<HomeAway, Urn>());
        var sportEventStatusCi = new SportEventStatusCacheItem(null, sportEventStatusDto);
        var matchStatus = new MatchStatus(sportEventStatusCi, _matchStatusesCacheMock.Object);

        Assert.Equal(1.6m, matchStatus.HomeScore);
    }

    [Fact]
    public void ConstructorSetsAwayScoreFromRestData()
    {
        var sportEventStatus = new sportEventStatus
        {
            away_score = 1.6m,
            away_scoreSpecified = true
        };
        var sportEventStatusDto = new SportEventStatusDto(sportEventStatus, new Dictionary<HomeAway, Urn>());
        var sportEventStatusCi = new SportEventStatusCacheItem(null, sportEventStatusDto);
        var matchStatus = new MatchStatus(sportEventStatusCi, _matchStatusesCacheMock.Object);

        Assert.Equal(1.6m, matchStatus.AwayScore);
    }

    [Fact]
    public void ConstructorSetsHomePenaltyScoreFromRestData()
    {
        var sportEventStatus = new sportEventStatus
        {
            home_penalty_score = 2,
            home_penalty_scoreSpecified = true
        };
        var sportEventStatusDto = new SportEventStatusDto(sportEventStatus, new Dictionary<HomeAway, Urn>());
        var sportEventStatusCi = new SportEventStatusCacheItem(null, sportEventStatusDto);
        var matchStatus = new MatchStatus(sportEventStatusCi, _matchStatusesCacheMock.Object);

        Assert.Equal(2, matchStatus.HomePenaltyScore);
    }

    [Fact]
    public void ConstructorSetsAwayPenaltyScoreFromRestData()
    {
        var sportEventStatus = new sportEventStatus
        {
            away_penalty_score = 2,
            away_penalty_scoreSpecified = true
        };
        var sportEventStatusDto = new SportEventStatusDto(sportEventStatus, new Dictionary<HomeAway, Urn>());
        var sportEventStatusCi = new SportEventStatusCacheItem(null, sportEventStatusDto);
        var matchStatus = new MatchStatus(sportEventStatusCi, _matchStatusesCacheMock.Object);

        Assert.Equal(2, matchStatus.AwayPenaltyScore);
    }

    [Fact]
    public void ConstructorSetsDecidedByFedFromRestData()
    {
        var sportEventStatus = new restSportEventStatus
        {
            decided_by_fed = true,
            decided_by_fedSpecified = true
        };
        var sportEventStatusDto = new SportEventStatusDto(sportEventStatus, new matchStatistics(), new Dictionary<HomeAway, Urn>());
        var sportEventStatusCi = new SportEventStatusCacheItem(null, sportEventStatusDto);
        var matchStatus = new MatchStatus(sportEventStatusCi, _matchStatusesCacheMock.Object);

        Assert.True(matchStatus.DecidedByFed);
    }

    [Fact]
    public void ConstructorSetsEmptyEventClockFromRestData()
    {
        var sportEventStatus = new sportEventStatus();
        var sportEventStatusDto = new SportEventStatusDto(sportEventStatus, new Dictionary<HomeAway, Urn>());
        var sportEventStatusCi = new SportEventStatusCacheItem(null, sportEventStatusDto);
        var matchStatus = new MatchStatus(sportEventStatusCi, _matchStatusesCacheMock.Object);

        Assert.Null(matchStatus.EventClock);
    }

    [Fact]
    public void ConstructorSetsEmptyPeriodScoresFromRestData()
    {
        var sportEventStatus = new sportEventStatus();
        var sportEventStatusDto = new SportEventStatusDto(sportEventStatus, new Dictionary<HomeAway, Urn>());
        var sportEventStatusCi = new SportEventStatusCacheItem(null, sportEventStatusDto);
        var matchStatus = new MatchStatus(sportEventStatusCi, _matchStatusesCacheMock.Object);

        Assert.Null(matchStatus.PeriodScores);
    }

    [Fact]
    public void ConstructorSetsEventClockFromRestData()
    {
        var clockType = new clockType
        {
            match_time = "some-match-time",
            remaining_time = "some-remaining-time",
            stoppage_time = "some-stoppage-time",
            stopped = true,
            stoppedSpecified = true
        };
        var sportEventStatus = new sportEventStatus
        {
            clock = clockType
        };

        var sportEventStatusDto = new SportEventStatusDto(sportEventStatus, new Dictionary<HomeAway, Urn>());
        var sportEventStatusCi = new SportEventStatusCacheItem(null, sportEventStatusDto);
        var matchStatus = new MatchStatus(sportEventStatusCi, _matchStatusesCacheMock.Object);

        Assert.NotNull(matchStatus.EventClock);
        Assert.Equal(clockType.match_time, matchStatus.EventClock.EventTime);
        Assert.Equal(clockType.remaining_time, matchStatus.EventClock.RemainingDate);
        Assert.Equal(clockType.stoppage_time, matchStatus.EventClock.StoppageTime);
        Assert.True(matchStatus.EventClock.Stopped);
    }

    [Fact]
    public void ConstructorSetsPeriodScoresFromRestData()
    {
        var periodScores = new periodScoreType
        {
            home_score = 1.3m,
            away_score = 2.3m,
            match_status_code = 1,
            number = 33
        };
        var sportEventStatus = new sportEventStatus()
        {
            period_scores = new[] { periodScores }
        };
        var sportEventStatusDto = new SportEventStatusDto(sportEventStatus, new Dictionary<HomeAway, Urn>());
        var sportEventStatusCi = new SportEventStatusCacheItem(null, sportEventStatusDto);
        var matchStatus = new MatchStatus(sportEventStatusCi, _matchStatusesCacheMock.Object);

        Assert.NotNull(matchStatus.PeriodScores);
        Assert.Single(matchStatus.PeriodScores);
        Assert.Equal(periodScores.home_score, matchStatus.PeriodScores.First().HomeScore);
        Assert.Equal(periodScores.away_score, matchStatus.PeriodScores.First().AwayScore);
        Assert.Equal(periodScores.match_status_code, matchStatus.PeriodScores.First().GetMatchStatusAsync(TestData.Culture).GetAwaiter().GetResult().Id);
        Assert.Equal(periodScores.number, matchStatus.PeriodScores.First().Number);
    }

    [Fact]
    public async Task GetMatchStatusForKnownId()
    {
        var sportEventStatus = new sportEventStatus
        {
            match_status = 1
        };
        var sportEventStatusDto = new SportEventStatusDto(sportEventStatus, new Dictionary<HomeAway, Urn>());
        var sportEventStatusCi = new SportEventStatusCacheItem(sportEventStatusDto, null);
        var matchStatus = new MatchStatus(sportEventStatusCi, _matchStatusesCacheMock.Object);

        var matchStatusId = await matchStatus.GetMatchStatusAsync(TestData.Culture);

        Assert.Equal(sportEventStatus.match_status, matchStatusId.Id);
    }

    [Fact]
    public async Task GetMatchStatusForUnknownId()
    {
        var sportEventStatus = new sportEventStatus
        {
            match_status = 1111
        };
        var sportEventStatusDto = new SportEventStatusDto(sportEventStatus, new Dictionary<HomeAway, Urn>());
        var sportEventStatusCi = new SportEventStatusCacheItem(sportEventStatusDto, null);
        var matchStatus = new MatchStatus(sportEventStatusCi, _matchStatusesCacheMock.Object);

        var matchStatusId = await matchStatus.GetMatchStatusAsync(TestData.Culture);

        Assert.Null(matchStatusId);
    }

    [Fact]
    public async Task GetMatchStatusForNegativeId()
    {
        var sportEventStatus = new sportEventStatus
        {
            match_status = -1
        };
        var sportEventStatusDto = new SportEventStatusDto(sportEventStatus, new Dictionary<HomeAway, Urn>());
        var sportEventStatusCi = new SportEventStatusCacheItem(sportEventStatusDto, null);
        var matchStatus = new MatchStatus(sportEventStatusCi, _matchStatusesCacheMock.Object);

        var matchStatusId = await matchStatus.GetMatchStatusAsync(TestData.Culture);

        Assert.Null(matchStatusId);
    }
}
