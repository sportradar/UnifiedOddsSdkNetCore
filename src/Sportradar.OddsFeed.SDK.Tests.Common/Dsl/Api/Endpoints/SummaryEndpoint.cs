// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using Sportradar.OddsFeed.SDK.Messages.Rest;
using Sportradar.OddsFeed.SDK.Tests.Common.Dsl.Api.SportEventBlock.Builders;

namespace Sportradar.OddsFeed.SDK.Tests.Common.Dsl.Api.Endpoints;

public class SummaryEndpoint
{
    private sportEvent _sportEvent = new sportEvent();
    private stageSportEventStatus _stageSportEventStatus;
    private restSportEventStatus _restSportEventStatus;
    private sportEventConditions _sportEventConditions;
    private coverageInfo _coverageInfo;
    private matchStatistics _matchStatistics;
    private venue _venue;

    public SummaryEndpoint WithSportEvent(Func<SportEventBuilder, SportEventBuilder> builderFunc)
    {
        _sportEvent = builderFunc(new SportEventBuilder()).Build();
        return this;
    }

    public SummaryEndpoint WithRestSportEventStatus(restSportEventStatus restSportEventStatus)
    {
        _restSportEventStatus = restSportEventStatus;
        return this;
    }

    public SummaryEndpoint WithStageSportEventStatus(stageSportEventStatus stageSportEventStatus)
    {
        _stageSportEventStatus = stageSportEventStatus;
        return this;
    }

    public SummaryEndpoint WithSportEventConditions(Func<SportEventConditionsBuilder, SportEventConditionsBuilder> builderFunc)
    {
        _sportEventConditions = builderFunc(new SportEventConditionsBuilder()).Build();
        return this;
    }

    public SummaryEndpoint WithCoverageInfo(Func<CoverageInfoBuilder, CoverageInfoBuilder> builderFunc)
    {
        _coverageInfo = builderFunc(new CoverageInfoBuilder()).Build();
        return this;
    }

    public SummaryEndpoint WithMatchStatistics(Func<MatchStatisticsBuilder, MatchStatisticsBuilder> builderFunc)
    {
        _matchStatistics = builderFunc(new MatchStatisticsBuilder()).Build();
        return this;
    }

    public SummaryEndpoint WithVenue(Func<VenueBuilder, VenueBuilder> builderFunc)
    {
        _venue = builderFunc(new VenueBuilder()).Build();
        return this;
    }

    public SummaryEndpoint MoveVenueFromSportEvent()
    {
        _venue = _sportEvent.venue;
        _sportEvent.venue = null;
        return this;
    }

    public matchSummaryEndpoint BuildMatchSummary()
    {
        return new matchSummaryEndpoint
        {
            generated_at = DateTime.Now,
            generated_atSpecified = true,
            sport_event = _sportEvent,
            sport_event_status = _restSportEventStatus,
            sport_event_conditions = _sportEventConditions,
            coverage_info = _coverageInfo,
            statistics = _matchStatistics,
            venue = _venue
        };
    }

    public stageSummaryEndpoint BuildStageSummary()
    {
        return new stageSummaryEndpoint
        {
            generated_at = DateTime.Now,
            generated_atSpecified = true,
            sport_event = _sportEvent,
            sport_event_status = _stageSportEventStatus
        };
    }
}
