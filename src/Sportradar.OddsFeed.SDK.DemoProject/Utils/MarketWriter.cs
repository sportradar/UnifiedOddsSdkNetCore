/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Common.Logging;
using Sportradar.OddsFeed.SDK.Entities;

namespace Sportradar.OddsFeed.SDK.DemoProject.Utils
{
    /// <summary>
    /// Writes Market info (Id, Specifiers, Name and all Outcomes)
    /// </summary>
    internal class MarketWriter
    {
        private readonly ILog _log;
        private readonly CultureInfo _culture;

        /// <summary>
        /// A <see cref="TaskProcessor"/> used for processing of asynchronous requests
        /// </summary>
        private readonly TaskProcessor _taskProcessor;

        public MarketWriter(ILog log, TaskProcessor taskProcessor, CultureInfo culture)
        {
            _log = log;
            _culture = culture;
            _taskProcessor = taskProcessor;
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
                WriteMarket(market, _culture);
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
                WriteMarket(market, _culture);
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
                WriteMarket(market, _culture);
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
                WriteMarket(market, _culture);
            }
        }

        private void WriteMarket(IMarket market, CultureInfo culture)
        {
            var marketName = _taskProcessor.GetTaskResult(market.GetNameAsync(culture));
            _log.Debug($"MarketId:{market.Id}, Specifiers:'{WriteSpecifiers(market.Specifiers)}', Name[{culture.TwoLetterISOLanguageName}]:'{marketName}'");
        }

        private void WriteMarket(IMarketWithOdds market, CultureInfo culture)
        {
            if (market == null)
            {
                _log.Debug("Market is null.");
                return;
            }
            var marketName = _taskProcessor.GetTaskResult(market.GetNameAsync(culture));
            _log.Debug($"MarketId:{market.Id}, Specifiers:'{WriteSpecifiers(market.Specifiers)}', Name[{culture.TwoLetterISOLanguageName}]:'{marketName}', Status:{market.Status}, IsFavorite:{market.IsFavorite}");
            if (market.OutcomeOdds == null)
            {
                return;
            }

            foreach (var outcome in market.OutcomeOdds)
            {
                _log.Debug(WriteOutcome(outcome, culture));
            }
        }

        private void WriteMarket(IMarketCancel market, CultureInfo culture)
        {
            var marketName = _taskProcessor.GetTaskResult(market.GetNameAsync(culture));
            var voidReason = market.VoidReason == null
                ? "null"
                : $"[{market.VoidReason.Id}-{market.VoidReason.Description}]";
            _log.Debug($"MarketId:{market.Id}, Specifiers:'{WriteSpecifiers(market.Specifiers)}', AdditionalInfo:'{WriteAdditionalInfo(market.AdditionalInfo)}', Name[{culture.TwoLetterISOLanguageName}]:'{marketName}', VoidReason:{voidReason}, MarketDefinition:{market.MarketDefinition?.GetNameTemplate(culture)}");
        }

        private void WriteMarket(IMarketWithSettlement market, CultureInfo culture)
        {
            WriteMarket((IMarketCancel) market, culture);
            if (market.OutcomeSettlements != null)
            {
                foreach (var outcome in market.OutcomeSettlements)
                {
                    _log.Debug(WriteOutcome(outcome, culture));
                }
            }
        }

        private static string WriteSpecifiers(IReadOnlyDictionary<string, string> specifiers)
        {
            if (specifiers == null || !specifiers.Any())
            {
                return string.Empty;
            }
            var tmp = specifiers.Aggregate(string.Empty, (current, pair) => current + $"{pair.Key}={pair.Value}|");
            return tmp.Remove(tmp.Length - 1);
        }

        private static string WriteAdditionalInfo(IReadOnlyDictionary<string, string> additionalInfos)
        {
            if (additionalInfos == null || !additionalInfos.Any())
            {
                return string.Empty;
            }
            var tmp = additionalInfos.Aggregate(string.Empty, (current, pair) => current + $"{pair.Key}={pair.Value}|");
            return tmp.Remove(tmp.Length - 1);
        }

        private string WriteMarketDefinition(IMarketDefinition marketDefinition, CultureInfo culture)
        {
            if (marketDefinition == null)
            {
                return null;
            }

            var attributes = marketDefinition.GetAttributes() == null
                                 ? null
                                 : marketDefinition.GetAttributes().Aggregate(string.Empty, (current, pair) => current + $"{pair.Key}={pair.Value}|");

            return $"OutcomeType: {marketDefinition.GetOutcomeType()}, NameTemplate: {marketDefinition.GetNameTemplate(culture)}, Attributes:[{attributes}], Groups:[{marketDefinition.GetGroups()?.Aggregate(string.Empty, (current, s1) => current + "," + s1)}]";
        }

        private string WriteOutcomeDefinition(IOutcomeDefinition outcomeDefinition, CultureInfo culture)
        {
            return $"NameTemplate[{culture.TwoLetterISOLanguageName}]:{outcomeDefinition?.GetNameTemplate(culture)}";
        }

        private string WriteOutcome(IOutcome outcome, CultureInfo culture)
        {
            if (outcome == null)
            {
                return "no outcome";
            }

            var outcomeName = _taskProcessor.GetTaskResult(outcome.GetNameAsync(culture));

            var playerOutcome = outcome as IPlayerOutcomeOdds;
            if (playerOutcome != null)
            {
                var competitor = _taskProcessor.GetTaskResult(playerOutcome.GetCompetitorAsync());
                return $"\tOutcomeForPlayer:{outcome.Id}, Name[{culture.TwoLetterISOLanguageName}]:'{outcomeName}', Active:{playerOutcome.Active?.ToString().ToLower()}, Odds:{playerOutcome.GetOdds(OddsDisplayType.Decimal)}, OddsUs:{playerOutcome.GetOdds(OddsDisplayType.American)}, Probabilities:{playerOutcome.Probabilities}, HomeOrAwayTeam:{playerOutcome.HomeOrAwayTeam}, Competitor:{competitor?.Id}, OutcomeDefinition:[{WriteOutcomeDefinition(((IOutcomeV1)playerOutcome).OutcomeDefinition, culture)}]";
            }
            var outcomeOdds = outcome as IOutcomeOddsV1;
            if (outcomeOdds != null)
            {
                return $"\tOutcomeWithOdds:{outcome.Id}, Name[{culture.TwoLetterISOLanguageName}]:'{outcomeName}', Active:{outcomeOdds.Active?.ToString().ToLower()}, Odds:{outcomeOdds.GetOdds(OddsDisplayType.Decimal)}, OddsUs:{outcomeOdds.GetOdds(OddsDisplayType.American)}, Probabilities:{outcomeOdds.Probabilities}, OutcomeDefinition:[{WriteOutcomeDefinition(((IOutcomeV1)outcomeOdds).OutcomeDefinition, culture)}]";
            }

            var outcomeProbabilities = outcome as IOutcomeProbabilities;
            if (outcomeProbabilities != null)
            {
                return $"\tOutcomeWithProbabilities:{outcome.Id}, Name[{culture.TwoLetterISOLanguageName}]:'{outcomeName}', Active:{outcomeProbabilities.Active?.ToString().ToLower()}, Probabilities:{outcomeProbabilities.Probabilities}, OutcomeDefinition:[{WriteOutcomeDefinition(((IOutcomeV1)outcomeProbabilities).OutcomeDefinition, culture)}]";
            }

            var outcomeSettlement = outcome as IOutcomeSettlement;
            if (outcomeSettlement != null)
            {
                return $"\tOutcomeForSettlement:{outcome.Id}, Name[{culture.TwoLetterISOLanguageName}]:'{outcomeName}', Result:{outcomeSettlement.Result.ToString().ToLower()}, VoidFactor: {outcomeSettlement.VoidFactor}, DeadHeatFactor:{outcomeSettlement.DeadHeatFactor}, OutcomeDefinition:[{WriteOutcomeDefinition(((IOutcomeV1)outcomeSettlement).OutcomeDefinition, culture)}]";
            }

            var outcomeV1 = outcome as IOutcomeV1;
            if (outcomeV1 != null)
            {
                return $"\tOutcomeId:{outcomeV1.Id}, Name[{culture.TwoLetterISOLanguageName}]:'{outcomeName}', OutcomeDefinition:[{WriteOutcomeDefinition(((IOutcomeV1)outcomeV1).OutcomeDefinition, culture)}]";
            }

            return $"\tOutcomeId:{outcome.Id}, Name[{culture.TwoLetterISOLanguageName}]:'{outcomeName}'";
        }
    }
}
