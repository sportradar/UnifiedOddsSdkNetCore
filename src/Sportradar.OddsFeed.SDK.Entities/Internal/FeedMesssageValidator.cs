/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Common.Logging;
using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Common.Exceptions;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching.Events;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.MarketNames;
using Sportradar.OddsFeed.SDK.Entities.REST.Market;
using Sportradar.OddsFeed.SDK.Messages;
using Sportradar.OddsFeed.SDK.Messages.Feed;

namespace Sportradar.OddsFeed.SDK.Entities.Internal
{
    /// <summary>
    /// A class used for validation of <see cref="FeedMessage"/> instances
    /// </summary>
    internal class FeedMessageValidator : IFeedMessageValidator
    {
        /// <summary>
        /// The <see cref="ILog"/> instance used for execution logging
        /// </summary>
        private static readonly ILog ExecutionLog = SdkLoggerFactory.GetLoggerForExecution(typeof(FeedMessageValidator));

        /// <summary>
        /// A <see cref="IReadOnlyCollection{T}"/> containing default culture
        /// </summary>
        private readonly IReadOnlyList<CultureInfo> _defaultCulture;

        /// <summary>
        /// A <see cref="IMarketCacheProvider"/> providing market descriptions
        /// </summary>
        private readonly IMarketCacheProvider _marketCacheProvider;

        /// <summary>
        /// A <see cref="INamedValuesProvider"/> used to provide descriptions for named values
        /// </summary>
        private readonly INamedValuesProvider _namedValuesProvider;

        /// <summary>
        /// The producer manager
        /// </summary>
        private readonly IProducerManager _producerManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="IFeedMessageValidator"/> class
        /// </summary>
        /// <param name="marketCacheProvider">The <see cref="IMarketCacheProvider"/> used to get <see cref="IMarketDescription"/></param>
        /// <param name="defaultCulture">The default culture used to retrieve <see cref="IMarketDescription"/></param>
        /// <param name="namedValuesProvider">A <see cref="INamedValuesProvider"/> used to provide descriptions for named values</param>
        /// <param name="producerManager">A <see cref="IProducerManager"/> used to check available producers</param>
        public FeedMessageValidator(IMarketCacheProvider marketCacheProvider, CultureInfo defaultCulture, INamedValuesProvider namedValuesProvider, IProducerManager producerManager)
        {
            Contract.Requires(marketCacheProvider != null);
            Contract.Requires(defaultCulture != null);
            Contract.Requires(namedValuesProvider != null);
            Contract.Requires(producerManager != null);

            _marketCacheProvider = marketCacheProvider;
            _defaultCulture = new List<CultureInfo> { defaultCulture };
            _namedValuesProvider = namedValuesProvider;
            _producerManager = producerManager;
        }

        /// <summary>
        /// Defines object invariants as required by code contracts
        /// </summary>
        [ContractInvariantMethod]
        private void ObjectInvariant()
        {
            Contract.Invariant(_marketCacheProvider != null);
            Contract.Invariant(_defaultCulture != null);
            Contract.Invariant(_namedValuesProvider != null);
            Contract.Invariant(_producerManager != null);
        }

        /// <summary>
        /// Logs the correctly formated warning to execution log
        /// </summary>
        /// <typeparam name="T">The type of the incorrect value</typeparam>
        /// <param name="message">The <see cref="FeedMessage"/> containing the incorrect value</param>
        /// <param name="propertyName">The name of the property set to incorrect value</param>
        /// <param name="propertyValue">The incorrect value</param>
        private static void LogWarning<T>(FeedMessage message, string propertyName, T propertyValue)
        {
            Contract.Requires(message != null);
            Contract.Requires(!string.IsNullOrEmpty(propertyName));

            ExecutionLog.WarnFormat("Validation warning: message={{{0}}}, property value {1}={2} is not expected", message, propertyName, propertyValue);
        }

        /// <summary>
        /// Logs the correctly formated failure to execution log
        /// </summary>
        /// <typeparam name="T">The type of the incorrect value</typeparam>
        /// <param name="message">The <see cref="FeedMessage"/> containing the incorrect value</param>
        /// <param name="propertyName">The name of the property set to incorrect value</param>
        /// <param name="propertyValue">The incorrect value</param>
        private static void LogFailure<T>(FeedMessage message, string propertyName, T propertyValue)
        {
            Contract.Requires(message != null);
            Contract.Requires(!string.IsNullOrEmpty(propertyName));

            ExecutionLog.ErrorFormat("Validation failure: message={{{0}}}, property value {1}={2} is not supported", message, propertyName, propertyValue);
        }

        /// <summary>
        /// Validates and parses the provided market specifiers
        /// </summary>
        /// <param name="message">The <see cref="FeedMessage"/> instance containing the market whose specifiers are being validated</param>
        /// <param name="market">A <see cref="FeedMarket"/> instance containing the specifiers to validate</param>
        /// <param name="marketIndex">The index of the <code>market</code> in the <code>message</code></param>
        private static bool ValidateSpecifiers(FeedMessage message, FeedMarket market, int marketIndex)
        {
            Contract.Requires(message != null);
            Contract.Requires(market != null);
            Contract.Requires(marketIndex >= 0);

            if (string.IsNullOrEmpty(market.SpecifierString))
            {
                return true;
            }

            try
            {
                market.Specifiers = FeedMapperHelper.GetSpecifiers(market.SpecifierString);
                return true;
            }
            catch (Exception ex)
            {
                if (ex is FormatException || ex is ArgumentException)
                {
                    market.ValidationFailed = true;
                    LogWarning(message, $"markets[{marketIndex}].specifiers", market.SpecifierString);
                    return false;
                }
                throw;
            }
        }

        /// <summary>
        /// Checks if the market descriptor exists for this marketId and specifiers
        /// </summary>
        /// <param name="producerId">The id of the producer which produced the message</param>
        /// <param name="marketId">A market identifier, identifying the market to be checked</param>
        /// <param name="specifiers">A <see cref="IReadOnlyDictionary{String, String}" /> representing specifiers of the associated market</param>
        private async Task<bool> CheckSpecifiersAsync(int producerId, int marketId, IReadOnlyDictionary<string, string> specifiers)
        {
            IMarketDescription marketDescriptor;
            try
            {
                marketDescriptor = await _marketCacheProvider.GetMarketDescriptionAsync(marketId, specifiers, _defaultCulture, false).ConfigureAwait(false);
            }
            catch (CacheItemNotFoundException ex)
            {
                ExecutionLog.Info($"Check failed. Failed to retrieve market name descriptor market[id={marketId}].", ex);
                return false;
            }
            if (marketDescriptor == null)
            {
                ExecutionLog.Info($"Check failed. Failed to retrieve market name descriptor for market[id={marketId}].");
                return false;
            }

            var nameDescription = marketDescriptor.GetName(_defaultCulture.First());
            if (nameDescription == null)
            {
                ExecutionLog.Info($"Check failed. Retrieved market[id={marketId}] descriptor does not contain name descriptor in the specified language");
                return false;
            }

            if (marketDescriptor.Id != marketId)
            {
                ExecutionLog.Info($"Check failed. Retrieved market descriptor has different marketId. ({marketDescriptor.Id}!={marketId})");
                return false;
            }

            if (marketDescriptor.Specifiers != null && marketDescriptor.Specifiers.Any() && specifiers != null)
            {
                if (marketDescriptor.Specifiers.Count() != specifiers.Count)
                {
                    var requiredSpecifiers = string.Join(",", marketDescriptor.Specifiers.Select(d => d.Name));
                    var actualSpecifiers = string.Join(",", specifiers.Select(k => k.Key));
                    ExecutionLog.Info($"Specifiers check failed. Producer={producerId}, market[id={marketId}], Required:{requiredSpecifiers}, Actual:{actualSpecifiers}");
                    return false;
                }

                foreach (var specifier in marketDescriptor.Specifiers)
                {
                    var keyValuePair = specifiers.FirstOrDefault(f => f.Key == specifier.Name);
                    if (string.IsNullOrEmpty(keyValuePair.Value))
                    {
                        var requiredSpecifiers = string.Join(",", marketDescriptor.Specifiers.Select(d => d.Name));
                        var actualSpecifiers = string.Join(",", specifiers.Select(k => k.Key));
                        ExecutionLog.Info($"Specifiers check for market[id={marketId}] failed. Required:{requiredSpecifiers}, Actual:{actualSpecifiers}");
                        return false;
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// Validates the basic properties of the provided <see cref="FeedMessage"/>
        /// </summary>
        /// <param name="message">The message to be validated</param>
        /// <returns>True if the validation was successful, otherwise false</returns>
        private static bool ValidateMessage(FeedMessage message)
        {
            Contract.Requires(message != null);

            var result = true;

            if (message.IsEventRelated)
            {
                if (message.SportId == null)
                {
                    LogFailure(message, "SportId", message.SportId);
                    return false;
                }
                URN eventUrn;
                if (URN.TryParse(message.EventId, out eventUrn))
                {
                    message.EventURN = eventUrn;
                }
                else
                {
                    LogFailure(message, "event_id", message.EventId);
                    result = false;
                }
            }

            return result;
        }

        // ReSharper disable once UnusedMethodReturnValue.Local
        private ValidationResult ValidateMessageProducer(FeedMessage message)
        {
            if (!_producerManager.Exists(message.ProducerId))
            {
                LogFailure(message, "producer", message.ProducerId);
                return ValidationResult.PROBLEMS_DETECTED;
            }
            var producer = _producerManager.Get(message.ProducerId);
            if (!producer.IsAvailable || producer.IsDisabled)
            {
                LogFailure(message, "producer", message.ProducerId);
                return ValidationResult.PROBLEMS_DETECTED;
            }
            return ValidationResult.SUCCESS;
        }

        /// <summary>
        /// Validates the provided <see cref="FeedMessage"/> and it's collection of <see cref="market"/> instances
        /// </summary>
        /// <param name="message">The message to be validated</param>
        /// <param name="markets">The message markets</param>
        /// <returns>A <see cref="ValidationResult"/> representing the result of the validation</returns>
        private ValidationResult ValidateMessageWithMarkets(FeedMessage message, market[] markets)
        {
            Contract.Requires(message != null);

            ValidateMessageProducer(message);

            if (!ValidateMessage(message) || markets == null)
            {
                return ValidationResult.FAILURE;
            }

            if (!markets.Any())
            {
                return ValidationResult.SUCCESS;
            }

            var result = ValidationResult.SUCCESS;
            for (var i = 0; i < markets.Length; i++)
            {
                if (!ValidateSpecifiers(message, markets[i], i))
                {
                    result = ValidationResult.PROBLEMS_DETECTED;
                }

                if (!CheckSpecifiersAsync(message.ProducerId, markets[i].id, markets[i].Specifiers).Result)
                {
                    result = ValidationResult.PROBLEMS_DETECTED;
                }
            }
            return result;
        }

        /// <summary>
        /// Validates the provided <see cref="snapshot_complete" /> message
        /// </summary>
        /// <param name="message">The <see cref="snapshot_complete" /> message to validate</param>
        /// <returns>The <see cref="ValidationResult" /> specifying the result of validation</returns>
        protected ValidationResult ValidateSnapshotCompleted(snapshot_complete message)
        {
            Contract.Requires(message != null);

            ValidateMessageProducer(message);

            return ValidateMessage(message)
                ? ValidationResult.SUCCESS
                : ValidationResult.FAILURE;
        }

        /// <summary>
        /// Validates the provided <see cref="alive" /> message
        /// </summary>
        /// <param name="message">The <see cref="alive" /> message to validate</param>
        /// <returns>The <see cref="ValidationResult" /> specifying the result of validation</returns>
        protected ValidationResult ValidateAlive(alive message)
        {
            Contract.Requires(message != null);

            ValidateMessageProducer(message);

            if (!ValidateMessage(message))
            {
                return ValidationResult.FAILURE;
            }

            if (message.subscribed >= 0)
            {
                return ValidationResult.SUCCESS;
            }
            LogWarning(message, "subscribed", message.subscribed);
            return ValidationResult.PROBLEMS_DETECTED;
        }

        /// <summary>
        /// Validates the provided <see cref="fixture_change" /> message
        /// </summary>
        /// <param name="message">The <see cref="fixture_change" /> message to validate</param>
        /// <returns>The <see cref="ValidationResult" /> specifying the result of validation</returns>
        protected ValidationResult ValidateFixtureChange(fixture_change message)
        {
            Contract.Requires(message != null);

            ValidateMessageProducer(message);

            if (!ValidateMessage(message))
            {
                return ValidationResult.FAILURE;
            }

            if (message.change_typeSpecified && !MessageMapperHelper.IsEnumMember<FixtureChangeType>(message.change_type))
            {
                LogWarning(message, "change_type", message.change_type);
                return ValidationResult.PROBLEMS_DETECTED;
            }
            return ValidationResult.SUCCESS;
        }

        /// <summary>
        /// Validates the provided <see cref="bet_stop" /> message
        /// </summary>
        /// <param name="message">The <see cref="bet_stop" /> message to validate</param>
        /// <returns>The <see cref="ValidationResult" /> specifying the result of validation</returns>
        protected ValidationResult ValidateBetStop(bet_stop message)
        {
            Contract.Requires(message != null);

            ValidateMessageProducer(message);

            if (!ValidateMessage(message))
            {
                return ValidationResult.FAILURE;
            }

            if (string.IsNullOrEmpty(message.groups))
            {
                LogFailure(message, "groups", message.groups);
                return ValidationResult.FAILURE;
            }
            return ValidationResult.SUCCESS;
        }

        /// <summary>
        /// Validates the provided <see cref="bet_settlement" /> message
        /// </summary>
        /// <param name="message">The <see cref="bet_settlement" /> message to validate</param>
        /// <returns>The <see cref="ValidationResult" /> specifying the result of validation</returns>
        protected ValidationResult ValidateBetSettlement(bet_settlement message)
        {
            Contract.Requires(message != null);

            ValidateMessageProducer(message);

            if (!ValidateMessage(message))
            {
                return ValidationResult.FAILURE;
            }

            if (message.outcomes == null || !message.outcomes.Any())
            {
                return ValidationResult.SUCCESS;
            }

            var result = ValidationResult.SUCCESS;
            for (var marketIndex = 0; marketIndex < message.outcomes.Length; marketIndex++)
            {
                var market = message.outcomes[marketIndex];
                if (!ValidateSpecifiers(message, market, marketIndex))
                {
                    result = ValidationResult.PROBLEMS_DETECTED;
                }

                if (!CheckSpecifiersAsync(message.ProducerId, market.id, market.Specifiers).Result)
                {
                    result = ValidationResult.PROBLEMS_DETECTED;
                }
            }
            return result;
        }

        /// <summary>
        /// Validates the provided <see cref="bet_cancel"/> message
        /// </summary>
        /// <param name="message">The <see cref="bet_cancel" /> message to validate</param>
        /// <returns>The <see cref="ValidationResult" /> specifying the result of validation</returns>
        protected ValidationResult ValidateBetCancel(bet_cancel message)
        {
            ValidateMessageProducer(message);

            var result = ValidateMessageWithMarkets(message, message.market);

            URN supersededBy;
            if (!string.IsNullOrEmpty(message.superceded_by) && !URN.TryParse(message.superceded_by, out supersededBy))
            {
                //set the value to null so it will not be processed by the message mapper
                message.superceded_by = null;
                LogWarning(message, "superseded_by", message.superceded_by);
                if (result == ValidationResult.SUCCESS)
                {
                    result = ValidationResult.PROBLEMS_DETECTED;
                }
            }
            return result;
        }

        /// <summary>
        /// Validates the provided <see cref="odds_change" /> message
        /// </summary>
        /// <param name="message">The <see cref="odds_change" /> message to validate</param>
        /// <returns>The <see cref="ValidationResult" /> specifying the result of validation</returns>
        protected ValidationResult ValidateOddsChange(odds_change message)
        {
            Contract.Requires(message != null);

            ValidateMessageProducer(message);

            if (!ValidateMessage(message))
            {
                return ValidationResult.FAILURE;
            }

            var result = ValidationResult.SUCCESS;
            if (message.odds_change_reasonSpecified && !MessageMapperHelper.IsEnumMember<OddsChangeReason>(message.odds_change_reason))
            {
                LogWarning(message, "odds_change_reason", message.odds_change_reason);
                result = ValidationResult.PROBLEMS_DETECTED;
            }

            if (message.odds == null)
            {
                return result;
            }

            if (message.odds.betstop_reasonSpecified && !_namedValuesProvider.BetStopReasons.IsValueDefined(message.odds.betstop_reason))
            {
                LogWarning(message, "betstop_reason", message.odds.betstop_reason);
                result = ValidationResult.PROBLEMS_DETECTED;
            }
            if (message.odds.betting_statusSpecified && !_namedValuesProvider.BettingStatuses.IsValueDefined(message.odds.betting_status))
            {
                LogWarning(message, "betting_status", message.odds.betting_status);
                result = ValidationResult.PROBLEMS_DETECTED;
            }

            if (message.odds.market == null || !message.odds.market.Any())
            {
                return result;
            }

            for (var marketIndex = 0; marketIndex < message.odds.market.Length; marketIndex++)
            {
                var market = message.odds.market[marketIndex];
                if (market.statusSpecified && !MessageMapperHelper.IsEnumMember<MarketStatus>(market.status))
                {
                    LogFailure(message, $"market[{marketIndex}].market_status", market.status);
                    return ValidationResult.FAILURE;
                }

                //if (market.favouriteSpecified && market.favourite != 1)
                //{
                //    LogWarning(message, $"market[{marketIndex}].favourite", market.favourite);
                //    result = ValidationResult.PROBLEMS_DETECTED;
                //}

                if (!ValidateSpecifiers(message, market, marketIndex))
                {
                    result = ValidationResult.PROBLEMS_DETECTED;
                }

                if (!CheckSpecifiersAsync(message.ProducerId, market.id, market.Specifiers).Result)
                {
                    result = ValidationResult.PROBLEMS_DETECTED;
                }

                if (market.outcome == null || !market.outcome.Any())
                {
                    continue;
                }

                for (var outcomeIndex = 0; outcomeIndex < market.outcome.Length; outcomeIndex++)
                {
                    var outcome = market.outcome[outcomeIndex];
                    result = ValidateOddsChangeOutcomes(message, market, outcome, result);
                }
            }
            return result;
        }

        private static ValidationResult ValidateOddsChangeOutcomes(FeedMessage message, oddsChangeMarket market, oddsChangeMarketOutcome outcome, ValidationResult currentResult)
        {
            if (outcome.activeSpecified && (outcome.active < 0 || outcome.active > 1))
            {
                LogWarning(message, $"markets[{market.id}].outcomes[{outcome.id}].active", outcome.active);
                currentResult = ValidationResult.PROBLEMS_DETECTED;
            }

            if (outcome.teamSpecified)
            {
                if (message.EventURN.TypeGroup != ResourceTypeGroup.MATCH)
                {
                    LogWarning(message, $"Player outcome=[marketId={market.id}, outcomeId={outcome.id}] cannot be mapped to IPlayerOutcomeOdds because associated event[id={message.EventURN}] is not an match", outcome.team);
                    currentResult = ValidationResult.PROBLEMS_DETECTED;
                }

                if (outcome.team < 1 || outcome.team > 2)
                {
                    LogWarning(message, $"Player outcome=[marketId={market.id}, outcomeId={outcome.id}] cannot be mapped to IPlayerOutcomeOdds because team attribute value {outcome.team} is out of range", outcome.team);
                    currentResult = ValidationResult.PROBLEMS_DETECTED;
                }
            }
            return currentResult;
        }

        /// <summary>
        /// Validates the specified <see cref="FeedMessage" /> instance
        /// </summary>
        /// <param name="message">A <see cref="FeedMessage" /> instance to be validated</param>
        /// <returns>A <see cref="ValidationResult" /> specifying the validation result</returns>
        /// <exception cref="System.ArgumentException"></exception>
        public ValidationResult Validate(FeedMessage message)
        {
            var oddsChange = message as odds_change;
            if (oddsChange != null)
            {
                return ValidateOddsChange(oddsChange);
            }

            var betStop = message as bet_stop;
            if (betStop != null)
            {
                return ValidateBetStop(betStop);
            }

            var settlement = message as bet_settlement;
            if (settlement != null)
            {
                return ValidateBetSettlement(settlement);
            }

            var betCancel = message as bet_cancel;
            if (betCancel != null)
            {
                return ValidateBetCancel(betCancel);
            }

            var snapshotCompleted = message as snapshot_complete;
            if (snapshotCompleted != null)
            {
                return ValidateSnapshotCompleted(snapshotCompleted);
            }

            var alive = message as alive;
            if (alive != null)
            {
                return ValidateAlive(alive);
            }

            var fixtureChange = message as fixture_change;
            if (fixtureChange != null)
            {
                return ValidateFixtureChange(fixtureChange);
            }

            var rollbackSettlement = message as rollback_bet_settlement;
            if (rollbackSettlement != null)
            {
                return ValidateMessageWithMarkets(rollbackSettlement, rollbackSettlement.market);
            }

            var rollbackCancel = message as rollback_bet_cancel;
            if (rollbackCancel != null)
            {
                return ValidateMessageWithMarkets(rollbackCancel, rollbackCancel.market);
            }

            throw new ArgumentException($"Validation of {message.GetType().FullName} messages is not supported.");
        }
    }
}
