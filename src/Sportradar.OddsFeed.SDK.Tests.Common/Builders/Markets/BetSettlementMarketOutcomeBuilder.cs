// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using Sportradar.OddsFeed.SDK.Messages.Feed;

namespace Sportradar.OddsFeed.SDK.Tests.Common.Builders.Markets;

public sealed class BetSettlementMarketOutcomeBuilder
{
    private readonly betSettlementMarketOutcome _outcome = new();

    public static BetSettlementMarketOutcomeBuilder Create()
    {
        return new BetSettlementMarketOutcomeBuilder();
    }

    public BetSettlementMarketOutcomeBuilder WithId(int id)
    {
        _outcome.id = id.ToString();
        return this;
    }

    public BetSettlementMarketOutcomeBuilder WithId(string id)
    {
        _outcome.id = id;
        return this;
    }

    public BetSettlementMarketOutcomeBuilder WithResult(int result)
    {
        _outcome.result = result;
        return this;
    }

    public BetSettlementMarketOutcomeBuilder WithVoidFactor(double voidFactor)
    {
        _outcome.void_factor = voidFactor;
        _outcome.void_factorSpecified = true;
        return this;
    }

    public BetSettlementMarketOutcomeBuilder WithDeadHeatFactor(double deadHeatFactor)
    {
        _outcome.dead_heat_factor = deadHeatFactor;
        _outcome.dead_heat_factorSpecified = true;
        return this;
    }

    public BetSettlementMarketOutcomeBuilder WithEachWayResult(string eachWayResult)
    {
        _outcome.each_way_result = eachWayResult;
        _outcome.each_way_resultSpecified = true;
        return this;
    }

    public BetSettlementMarketOutcomeBuilder WithEachWayFactor(double eachWayFactor)
    {
        _outcome.each_way_factor = eachWayFactor;
        _outcome.each_way_factorSpecified = true;
        return this;
    }

    public BetSettlementMarketOutcomeBuilder WithDeadHeatFactorPlace(double deadHeatFactorPlace)
    {
        _outcome.dead_heat_factor_place = deadHeatFactorPlace;
        _outcome.dead_heat_factor_placeSpecified = true;
        return this;
    }

    public betSettlementMarketOutcome Build()
    {
        return _outcome;
    }
}

