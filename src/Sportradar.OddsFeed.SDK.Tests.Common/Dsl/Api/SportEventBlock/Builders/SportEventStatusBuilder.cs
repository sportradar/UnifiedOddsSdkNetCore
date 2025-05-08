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

    public class RestSportEventStatusBuilder
    {
        private readonly restSportEventStatus _restSportEventStatus = new restSportEventStatus();

        // it can have decimal values
        public RestSportEventStatusBuilder WithScore(string homeScore, string awayScore)
        {
            _restSportEventStatus.home_score = homeScore;
            _restSportEventStatus.away_score = awayScore;
            return this;
        }

        // it can have decimal values
        public RestSportEventStatusBuilder WithAggregateScore(string homeScore, string awayScore)
        {
            _restSportEventStatus.aggregate_home_score = homeScore;
            _restSportEventStatus.aggregate_away_score = awayScore;
            return this;
        }

        public RestSportEventStatusBuilder WithAggregateWinner(Urn competitorId)
        {
            _restSportEventStatus.aggregate_winner_id = competitorId.ToString();
            return this;
        }

        public RestSportEventStatusBuilder WithWinner(Urn competitorId)
        {
            _restSportEventStatus.winner_id = competitorId.ToString();
            return this;
        }

        public RestSportEventStatusBuilder WithStatusCode(int statusCode)
        {
            _restSportEventStatus.status_code = statusCode;
            _restSportEventStatus.status_codeSpecified = true;
            return this;
        }

        public RestSportEventStatusBuilder WithStatus(string status)
        {
            _restSportEventStatus.status = status;
            return this;
        }

        public RestSportEventStatusBuilder WithMatchStatusCode(int statusCode)
        {
            _restSportEventStatus.match_status_code = statusCode;
            _restSportEventStatus.match_status_codeSpecified = true;
            return this;
        }

        public RestSportEventStatusBuilder WithMatchStatus(string status)
        {
            _restSportEventStatus.match_status = status;
            return this;
        }

        public RestSportEventStatusBuilder AddPeriodScore(int number, string homeScore, string awayScore, int matchStatusCode, string type)
        {
            _restSportEventStatus.period_scores ??= [];
            var list = _restSportEventStatus.period_scores.ToList();
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
            _restSportEventStatus.period_scores = list.ToArray();
            return this;
        }

        public RestSportEventStatusBuilder AddResult(string homeScore, string awayScore, int matchStatusCode)
        {
            _restSportEventStatus.results ??= [];
            var list = _restSportEventStatus.results.ToList();
            var result = new resultScore
            {
                home_score = homeScore,
                away_score = awayScore,
                match_status_code = matchStatusCode
            };
            list.Add(result);
            _restSportEventStatus.results = list.ToArray();
            return this;
        }

        public restSportEventStatus Build()
        {
            return _restSportEventStatus;
        }
    }
}
