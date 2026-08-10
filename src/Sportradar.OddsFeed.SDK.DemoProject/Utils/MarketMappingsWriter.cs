// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Sportradar.OddsFeed.SDK.Entities;
using Sportradar.OddsFeed.SDK.Entities.Rest.MarketMapping;

namespace Sportradar.OddsFeed.SDK.DemoProject.Utils;

/// <summary>
/// Writes Market info (Id, Specifiers, Name, and all Outcomes)
/// </summary>
internal class MarketMappingsWriter
{
    private readonly ILogger _log;
    private readonly CultureInfo _culture;

    /// <summary>
    /// A <see cref="TaskProcessor"/> used for processing of asynchronous requests
    /// </summary>
    private readonly TaskProcessor _taskProcessor;

    public MarketMappingsWriter(TaskProcessor taskProcessor, CultureInfo culture, ILogger log)
    {
        _taskProcessor = taskProcessor;
        _culture = culture;
        _log = log ?? new NullLogger<MarketMappingsWriter>();
    }

    public void WriteMarketNamesForEvent(IEnumerable<IMarket> markets)
    {
        if (markets == null)
        {
            _log.LogDebug("No Markets for this SportEvent");
            return;
        }
        foreach (var market in markets)
        {
            WriteMarket(market);
        }
    }

    public void WriteMarketNamesForEvent(IEnumerable<IMarketCancel> markets)
    {
        if (markets == null)
        {
            _log.LogDebug("No Markets for this SportEvent");
            return;
        }
        foreach (var market in markets)
        {
            WriteMarket(market);
        }
    }

    public void WriteMarketNamesForEvent(IEnumerable<IMarketWithOdds> markets)
    {
        if (markets == null)
        {
            _log.LogDebug("No Markets for this SportEvent");
            return;
        }
        foreach (var market in markets)
        {
            WriteMarket(market);
        }
    }

    public void WriteMarketNamesForEvent(IEnumerable<IMarketWithSettlement> markets)
    {
        if (markets == null)
        {
            _log.LogDebug("No Markets for this SportEvent");
            return;
        }
        foreach (var market in markets)
        {
            WriteMarket(market);
        }
    }

    private void WriteMarketMappings(IMarket market)
    {
        var orgIds = _taskProcessor.GetTaskResult(market.GetMappedMarketIdsAsync());

        if (orgIds == null)
        {
            _log.LogDebug("No market mapping for marketId:{MarketId}", market.Id);
            return;
        }
        foreach (var orgId in orgIds)
        {
            if (orgId is LcooMarketMapping)
            {
                var lcooId = orgId as LcooMarketMapping;
                _log.LogDebug("Market {MarketId} mapping TypeId:{TypeId}, Sov:'{Sov}'", market.Id, lcooId.TypeId, lcooId.Sov);
            }
            else if (orgId is LoMarketMapping)
            {
                var loId = orgId as LoMarketMapping;
                _log.LogDebug("Market {MarketId} mapping TypeId:{TypeId}, SubTypeId:{SubTypeId}, Sov:'{Sov}'", market.Id, loId.TypeId, loId.SubTypeId, loId.Sov);
            }
            else
            {
                _log.LogWarning("Unknown market mapping type for market:{MarketId}. Market mapping TypeId:{TypeId}, Sov:'{Sov}'", market.Id, orgId.TypeId, orgId.Sov);
            }
        }
    }

    private void WriteMarket(IMarket market)
    {
        WriteMarketMappings(market);
    }

    private void WriteMarket(IMarketWithOdds market)
    {
        if (market == null)
        {
            return;
        }
        WriteMarket((IMarket)market);
        if (market.OutcomeOdds != null)
        {
            foreach (var outcome in market.OutcomeOdds)
            {
                WriteMarketOutcomeMappings(market.Id, outcome);
            }
        }
    }

    private void WriteMarket(IMarketWithSettlement market)
    {
        if (market == null)
        {
            return;
        }
        WriteMarket((IMarket)market);
        if (market.OutcomeSettlements != null)
        {
            foreach (var outcome in market.OutcomeSettlements)
            {
                WriteMarketOutcomeMappings(market.Id, outcome);
            }
        }
    }

    private void WriteMarketOutcomeMappings(int marketId, IOutcome outcome)
    {
        var mappedOutcomes = _taskProcessor.GetTaskResult(outcome.GetMappedOutcomeIdsAsync());
        if (mappedOutcomes == null)
        {
            _log.LogDebug("\tOutcome for market={MarketId} and outcomeId={OutcomeId} has no outcome mapping", marketId, outcome.Id);
            return;
        }

        var outcomeMappings = mappedOutcomes.ToList();
        if (!outcomeMappings.Any())
        {
            _log.LogDebug("\tOutcome for market={MarketId} and outcomeId={OutcomeId} has no outcome mapping", marketId, outcome.Id);
            return;
        }
        foreach (var mappedOutcome in outcomeMappings)
        {
            _log.LogDebug("\tOutcome for market={MarketId} and outcomeId={OutcomeId} is mapped to [Id:{MappedOutcomeId}, Name:'{MappedOutcomeName}']", marketId, outcome.Id, mappedOutcome.Id, mappedOutcome.GetName(_culture));
        }
    }
}
