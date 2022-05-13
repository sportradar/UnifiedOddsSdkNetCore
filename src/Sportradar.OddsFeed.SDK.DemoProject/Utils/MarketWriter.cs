/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Sportradar.OddsFeed.SDK.Entities;

namespace Sportradar.OddsFeed.SDK.DemoProject.Utils
{
    /// <summary>
    /// Writes Market info (Id, Specifiers, Name and all Outcomes)
    /// </summary>
    internal class MarketWriter
    {
        private readonly ILogger _log;
        private readonly CultureInfo _culture;

        /// <summary>
        /// A <see cref="TaskProcessor"/> used for processing of asynchronous requests
        /// </summary>
        private readonly TaskProcessor _taskProcessor;

        public MarketWriter(TaskProcessor taskProcessor, CultureInfo culture, ILogger log)
        {
            _culture = culture;
            _taskProcessor = taskProcessor;
            _log = log ?? new NullLogger<MarketWriter>();
        }

        public void WriteMarketNamesForEvent(IEnumerable<IMarket> markets)
        {
            if (markets == null)
            {
                _log.LogInformation("No Markets for this SportEvent.");
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
                _log.LogInformation("No Markets for this SportEvent.");
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
                _log.LogInformation("No Markets for this SportEvent.");
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
                _log.LogInformation("No Markets for this SportEvent.");
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
            _log.LogInformation($"MarketId:{market.Id}, Specifiers:'{WriteSpecifiers(market.Specifiers)}', Name[{culture.TwoLetterISOLanguageName}]:'{marketName}'");
        }

        private void WriteMarket(IMarketWithOdds market, CultureInfo culture)
        {
            if (market == null)
            {
                _log.LogInformation("Market is null.");
                return;
            }
            var marketName = _taskProcessor.GetTaskResult(market.GetNameAsync(culture));
            _log.LogInformation($"MarketId:{market.Id}, Specifiers:'{WriteSpecifiers(market.Specifiers)}', Name[{culture.TwoLetterISOLanguageName}]:'{marketName}', Status:{market.Status}, IsFavorite:{market.IsFavorite}");
            if (market.OutcomeOdds == null)
            {
                return;
            }

            foreach (var outcome in market.OutcomeOdds)
            {
                _log.LogInformation(WriteOutcome(outcome, culture));
            }
        }

        private void WriteMarket(IMarketCancel market, CultureInfo culture)
        {
            var marketName = _taskProcessor.GetTaskResult(market.GetNameAsync(culture));
            var voidReason = market.VoidReason == null
                ? "null"
                : $"[{market.VoidReason.Id}-{market.VoidReason.Description}]";
            _log.LogInformation($"MarketId:{market.Id}, Specifiers:'{WriteSpecifiers(market.Specifiers)}', AdditionalInfo:'{WriteAdditionalInfo(market.AdditionalInfo)}', Name[{culture.TwoLetterISOLanguageName}]:'{marketName}', VoidReason:{voidReason}, MarketDefinition:{market.MarketDefinition?.GetNameTemplate(culture)}");
        }

        private void WriteMarket(IMarketWithSettlement market, CultureInfo culture)
        {
            WriteMarket((IMarketCancel) market, culture);
            if (market.OutcomeSettlements != null)
            {
                foreach (var outcome in market.OutcomeSettlements)
                {
                    _log.LogInformation(WriteOutcome(outcome, culture));
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
            try
            {
                return $"NameTemplate[{culture.TwoLetterISOLanguageName}]:{outcomeDefinition?.GetNameTemplate(culture)}";
            }
            catch (Exception e)
            {
                _log.LogWarning(e, e.Message);
            }

            return null;
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
                return $"\tOutcomeForPlayer:{outcome.Id}, Name[{culture.TwoLetterISOLanguageName}]:'{outcomeName}', Active:{playerOutcome.Active?.ToString().ToLower()}, Odds:{playerOutcome.GetOdds(OddsDisplayType.Decimal)}, OddsUs:{playerOutcome.GetOdds(OddsDisplayType.American)}, Probabilities:{playerOutcome.Probabilities}, AdditionalProbabilities[PO]={GetAdditionalProbabilities(playerOutcome.AdditionalProbabilities)}, HomeOrAwayTeam:{playerOutcome.HomeOrAwayTeam}, Competitor:{competitor?.Id}, OutcomeDefinition:[{WriteOutcomeDefinition(playerOutcome.OutcomeDefinition, culture)}]";
            }
            var outcomeOdds = outcome as IOutcomeOdds;
            if (outcomeOdds != null)
            {
                return $"\tOutcomeWithOdds:{outcome.Id}, Name[{culture.TwoLetterISOLanguageName}]:'{outcomeName}', Active:{outcomeOdds.Active?.ToString().ToLower()}, Odds:{outcomeOdds.GetOdds(OddsDisplayType.Decimal)}, OddsUs:{outcomeOdds.GetOdds(OddsDisplayType.American)}, Probabilities:{outcomeOdds.Probabilities}, AdditionalProbabilities[OO]={GetAdditionalProbabilities(outcomeOdds.AdditionalProbabilities)}, OutcomeDefinition:[{WriteOutcomeDefinition(outcomeOdds.OutcomeDefinition, culture)}]";
            }

            var outcomeProbabilities = outcome as IOutcomeProbabilities;
            if (outcomeProbabilities != null)
            {
                return $"\tOutcomeWithProbabilities:{outcome.Id}, Name[{culture.TwoLetterISOLanguageName}]:'{outcomeName}', Active:{outcomeProbabilities.Active?.ToString().ToLower()}, Probabilities:{outcomeProbabilities.Probabilities}, AdditionalProbabilities[OP]={GetAdditionalProbabilities(null)}, OutcomeDefinition:[{WriteOutcomeDefinition(outcomeProbabilities.OutcomeDefinition, culture)}]";
            }

            var outcomeSettlement = outcome as IOutcomeSettlement;
            if (outcomeSettlement != null)
            {
                return $"\tOutcomeForSettlement:{outcome.Id}, Name[{culture.TwoLetterISOLanguageName}]:'{outcomeName}', Result:{outcomeSettlement.OutcomeResult}, VoidFactor: {outcomeSettlement.VoidFactor}, DeadHeatFactor:{outcomeSettlement.DeadHeatFactor}, OutcomeDefinition:[{WriteOutcomeDefinition(outcomeSettlement.OutcomeDefinition, culture)}]";
            }

            return $"\tOutcomeId:{outcome.Id}, Name[{culture.TwoLetterISOLanguageName}]:'{outcomeName}', OutcomeDefinition:[{WriteOutcomeDefinition(outcome.OutcomeDefinition, culture)}]";
        }

        private string GetAdditionalProbabilities(IAdditionalProbabilities probabilities)
        {
            if (probabilities == null)
            {
                return string.Empty;
            }

            return $"Win={probabilities.Win}, Lose={probabilities.Lose}, HalfWin={probabilities.HalfWin}, HalfLose={probabilities.HalfLose}, Refund={probabilities.Refund}";
        }
    }
}
