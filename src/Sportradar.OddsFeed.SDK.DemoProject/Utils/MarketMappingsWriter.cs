/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Common.Logging;
using Sportradar.OddsFeed.SDK.Entities;
using Sportradar.OddsFeed.SDK.Entities.REST.MarketMapping;

namespace Sportradar.OddsFeed.SDK.DemoProject.Utils
{
    /// <summary>
    /// Writes Market info (Id, Specifiers, Name and all Outcomes)
    /// </summary>
    internal class MarketMappingsWriter
    {
        private readonly ILog _log;
        private readonly CultureInfo _culture;

        /// <summary>
        /// A <see cref="TaskProcessor"/> used for processing of asynchronous requests
        /// </summary>
        private readonly TaskProcessor _taskProcessor;

        public MarketMappingsWriter(ILog log, TaskProcessor taskProcessor, CultureInfo culture)
        {
            _log = log;
            _taskProcessor = taskProcessor;
            _culture = culture;
        }

        public void WriteMarketNamesForEvent(IEnumerable<IMarket> markets)
        {
            if (markets == null)
            {
                _log.Debug("No Markets for this SportEvent.");
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
                _log.Debug("No Markets for this SportEvent.");
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
                _log.Debug("No Markets for this SportEvent.");
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
                _log.Debug("No Markets for this SportEvent.");
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
                _log.Debug($"No market mapping for marketId:{market.Id}.");
                return;
            }
            foreach (var orgId in orgIds)
            {
                if (orgId is LcooMarketMapping)
                {
                    var lcooId = orgId as LcooMarketMapping;
                    _log.Debug($"Market {market.Id} mapping TypeId:{lcooId.TypeId}, Sov:'{lcooId.Sov}'");
                }
                else if (orgId is LoMarketMapping)
                {
                    var loId = orgId as LoMarketMapping;
                    _log.Debug($"Market {market.Id} mapping TypeId:{loId.TypeId}, SubTypeId:{loId.SubTypeId}, Sov:'{loId.Sov}'");
                }
                else
                {
                    _log.Warn($"Unknown market mapping type for market:{market.Id}. Market mapping TypeId:{orgId.TypeId}, Sov:'{orgId.Sov}'");
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
                _log.Debug($"\tOutcome for market={marketId} and outcomeId={outcome.Id} has no outcome mapping!");
                return;
            }

            var outcomeMappings = mappedOutcomes.ToList();
            if (!outcomeMappings.Any())
            {
                _log.Debug($"\tOutcome for market={marketId} and outcomeId={outcome.Id} has no outcome mapping!");
                return;
            }
            foreach (var mappedOutcome in outcomeMappings)
            {
                _log.Debug($"\tOutcome for market={marketId} and outcomeId={outcome.Id} is mapped to [Id:{mappedOutcome.Id}, Name:'{mappedOutcome.GetName(_culture)}'].");
            }
        }

        private static string GetSpecifiers(IReadOnlyDictionary<string, string> specifiers)
        {
            if (specifiers == null || !specifiers.Any())
            {
                return string.Empty;
            }
            var tmp = specifiers.Aggregate(string.Empty, (current, pair) => current + $"{pair.Key}={pair.Value}|");
            return tmp.Remove(tmp.Length - 1);
        }

        //public static string GetMarketMappingData(IMarketMappingData data, CultureInfo culture)
        //{
        //    if (data == null)
        //    {
        //        return null;
        //    }

        //    var producerIds = data.ProducerIds?.Aggregate(string.Empty, (current, i) => current + $"{i},");
        //    var outcomeMappingData =
        //        data.OutcomeMappings?.Aggregate(string.Empty, (current, mappingData) => current + $"{GetOutcomeMappingData(mappingData, culture)}|");
        //    return $"MarketId:{data.MarketId}, MarketTypeId:{data.MarketTypeId}, MarketSubTypeId:{data.MarketSubTypeId}, SovTemplate:{data.SovTemplate}, ValidFor:{data.ValidFor}, ProducerIds:[{producerIds}], OutcomeMappingData:[{outcomeMappingData}]";
        //}

        //public static string GetOutcomeMappingData(IOutcomeMappingData data, CultureInfo culture)
        //{
        //    if (data == null)
        //    {
        //        return null;
        //    }
        //    return $"MarketId:{data.MarketId}, OutcomeId:{data.OutcomeId}, ProducerOutcomeId:{data.ProducerOutcomeId}, ProducerOutcomeName[{culture.TwoLetterISOLanguageName}]:{data.GetProducerOutcomeName(culture)}";
        //}
    }
}
