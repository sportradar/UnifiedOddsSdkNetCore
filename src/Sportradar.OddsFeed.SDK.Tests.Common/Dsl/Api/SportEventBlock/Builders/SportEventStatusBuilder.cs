// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System.Linq;
using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Messages.Rest;

namespace Sportradar.OddsFeed.SDK.Tests.Common.Dsl.Api.SportEventBlock.Builders;

public static class SportEventStatusBuilder
{
    public static RestSportEventStatusBuilder Rest()
    {
        return new RestSportEventStatusBuilder();
    }

    public static StageSportEventStatusBuilder Stage()
    {
        return new StageSportEventStatusBuilder();
    }

    public class RestSportEventStatusBuilder
    {
        private readonly restSportEventStatus _sportEventStatus = new restSportEventStatus();

        // it can have decimal values
        public RestSportEventStatusBuilder WithScore(string homeScore, string awayScore)
        {
            _sportEventStatus.home_score = homeScore;
            _sportEventStatus.away_score = awayScore;
            return this;
        }

        // it can have decimal values
        public RestSportEventStatusBuilder WithAggregateScore(string homeScore, string awayScore)
        {
            _sportEventStatus.aggregate_home_score = homeScore;
            _sportEventStatus.aggregate_away_score = awayScore;
            return this;
        }

        public RestSportEventStatusBuilder WithAggregateWinner(Urn competitorId)
        {
            _sportEventStatus.aggregate_winner_id = competitorId.ToString();
            return this;
        }

        public RestSportEventStatusBuilder WithWinner(Urn competitorId)
        {
            _sportEventStatus.winner_id = competitorId.ToString();
            return this;
        }

        public RestSportEventStatusBuilder WithStatusCode(int statusCode)
        {
            _sportEventStatus.status_code = statusCode;
            _sportEventStatus.status_codeSpecified = true;
            return this;
        }

        public RestSportEventStatusBuilder WithStatus(string status)
        {
            _sportEventStatus.status = status;
            return this;
        }

        public RestSportEventStatusBuilder WithMatchStatusCode(int statusCode)
        {
            _sportEventStatus.match_status_code = statusCode;
            _sportEventStatus.match_status_codeSpecified = true;
            return this;
        }

        public RestSportEventStatusBuilder WithMatchStatus(string status)
        {
            _sportEventStatus.match_status = status;
            return this;
        }

        public RestSportEventStatusBuilder AddPeriodScore(int number, string homeScore, string awayScore, int matchStatusCode, string type)
        {
            _sportEventStatus.period_scores ??= [];
            var list = _sportEventStatus.period_scores.ToList();
            var periodScore = new periodScore
            {
                number = number,
                numberSpecified = true,
                home_score = homeScore,
                away_score = awayScore,
                match_status_code = matchStatusCode,
                type = type
            };
            list.Add(periodScore);
            _sportEventStatus.period_scores = list.ToArray();
            return this;
        }

        public RestSportEventStatusBuilder AddResult(string homeScore, string awayScore, int matchStatusCode)
        {
            _sportEventStatus.results ??= [];
            var list = _sportEventStatus.results.ToList();
            var result = new resultScore
            {
                home_score = homeScore,
                away_score = awayScore,
                match_status_code = matchStatusCode
            };
            list.Add(result);
            _sportEventStatus.results = list.ToArray();
            return this;
        }

        public restSportEventStatus Build()
        {
            return _sportEventStatus;
        }
    }

    public class StageSportEventStatusBuilder
    {
        private readonly stageSportEventStatus _sportEventStatus = new stageSportEventStatus();

        public StageSportEventStatusBuilder WithWinner(Urn competitorId)
        {
            _sportEventStatus.winner_id = competitorId.ToString();
            return this;
        }

        public StageSportEventStatusBuilder WithStatus(string status)
        {
            _sportEventStatus.status = status;
            return this;
        }

        public StageSportEventStatusBuilder AddResult(Urn competitorId, int? position = null)
        {
            _sportEventStatus.results ??= new stageResult();
            _sportEventStatus.results.competitor ??= [];
            var list = _sportEventStatus.results.competitor.ToList();
            var result = new stageResultCompetitor
            {
                id = competitorId.ToString(),
                position = position ?? 0,
                positionSpecified = position.HasValue
            };
            list.Add(result);
            _sportEventStatus.results.competitor = list.ToArray();
            return this;
        }

        public StageSportEventStatusBuilder AddResult(StageResultBuilder stageResultBuilder)
        {
            _sportEventStatus.results = stageResultBuilder.Build();
            return this;
        }

        public stageSportEventStatus Build()
        {
            return _sportEventStatus;
        }
    }
}

