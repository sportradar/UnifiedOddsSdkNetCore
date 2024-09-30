// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Dawn;
using Microsoft.Extensions.Logging;
using Sportradar.OddsFeed.SDK.Api.Internal.Caching;
using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Common.Enums;
using Sportradar.OddsFeed.SDK.Common.Exceptions;
using Sportradar.OddsFeed.SDK.Common.Extensions;
using Sportradar.OddsFeed.SDK.Common.Internal;
using Sportradar.OddsFeed.SDK.Entities.Internal.EntitiesImpl;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.EntitiesImpl;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.MarketNames;
using Sportradar.OddsFeed.SDK.Entities.Rest.Market;

namespace Sportradar.OddsFeed.SDK.Entities.Rest.Internal.MarketNameGeneration
{
    /// <summary>
    /// Provides names of markets and outcomes
    /// </summary>
    /// <seealso cref="INameProvider" />
    internal class NameProvider : INameProvider
    {
        /// <summary>
        /// A <see cref="ILogger"/> instance used for execution logging
        /// </summary>
        private readonly ILogger _executionLog;

        /// <summary>
        /// A <see cref="IMarketCacheProvider"/> instance used to retrieve market descriptors
        /// </summary>
        private readonly IMarketCacheProvider _marketCacheProvider;

        /// <summary>
        /// A <see cref="IProfileCache"/> instance used to retrieve player and competitor profiles
        /// </summary>
        private readonly IProfileCache _profileCache;

        /// <summary>
        /// A <see cref="INameExpressionFactory"/> instance used to built <see cref="INameExpression"/> instances
        /// </summary>
        private readonly INameExpressionFactory _expressionFactory;

        /// <summary>
        /// A <see cref="ICompetition"/> instance representing associated sport event
        /// </summary>
        private readonly ISportEvent _sportEvent;

        /// <summary>
        /// A market identifier of the market associated with the constructed instance
        /// </summary>
        private readonly int _marketId;

        /// <summary>
        /// A <see cref="IReadOnlyDictionary{String,String}"/> representing specifiers of the associated market
        /// </summary>
        private readonly IReadOnlyDictionary<string, string> _specifiers;

        /// <summary>
        /// A <see cref="ExceptionHandlingStrategy"/> describing the mode in which the SDK is running
        /// </summary>
        private readonly ExceptionHandlingStrategy _exceptionStrategy;

        /// <summary>
        /// Indicates if the competitors was already fetched
        /// </summary>
        private readonly ConcurrentBag<CultureInfo> _competitorProfilesLoaded;

        /// <summary>
        /// Initializes a new instance of the <see cref="NameProvider"/> class
        /// </summary>
        /// <param name="marketCacheProvider">A <see cref="IMarketCacheProvider"/> instance used to retrieve market descriptors</param>
        /// <param name="profileCache">A <see cref="IProfileCache"/> instance used to retrieve player and competitor profiles</param>
        /// <param name="expressionFactory">A <see cref="INameExpressionFactory"/> instance used to built <see cref="INameExpression"/> instances</param>
        /// <param name="sportEvent">A <see cref="ISportEvent"/> instance representing associated sport @event</param>
        /// <param name="marketId">A market identifier of the market associated with the constructed instance</param>
        /// <param name="specifiers">A <see cref="IReadOnlyDictionary{String, String}"/> representing specifiers of the associated market</param>
        /// <param name="exceptionStrategy">A <see cref="ExceptionHandlingStrategy"/> describing the mode in which the SDK is running</param>
        /// <param name="logger">A logger used to log to execution log</param>
        [SuppressMessage("Major Code Smell", "S107:Methods should not have too many parameters", Justification = "Allowed here")]
        internal NameProvider(IMarketCacheProvider marketCacheProvider,
                              IProfileCache profileCache,
                              INameExpressionFactory expressionFactory,
                              ISportEvent sportEvent,
                              int marketId,
                              IReadOnlyDictionary<string, string> specifiers,
                              ExceptionHandlingStrategy exceptionStrategy,
                              ILogger logger)
        {
            Guard.Argument(marketCacheProvider, nameof(marketCacheProvider)).NotNull();
            Guard.Argument(profileCache, nameof(profileCache)).NotNull();
            Guard.Argument(expressionFactory, nameof(expressionFactory)).NotNull();
            Guard.Argument(sportEvent, nameof(sportEvent)).NotNull();

            _marketCacheProvider = marketCacheProvider;
            _profileCache = profileCache;
            _expressionFactory = expressionFactory;
            _sportEvent = sportEvent;
            _marketId = marketId;
            _specifiers = specifiers;
            _exceptionStrategy = exceptionStrategy;
            _competitorProfilesLoaded = new ConcurrentBag<CultureInfo>();
            _executionLog = logger;
        }

        /// <summary>
        /// Asynchronously gets the name of the specified market in the requested language
        /// </summary>
        /// <param name="culture">The language of the returned name</param>
        /// <returns>A <see cref="Task{String}"/> representing the asynchronous operation</returns>
        public async Task<string> GetMarketNameAsync(CultureInfo culture)
        {
            IMarketDescription marketDescriptor;
            try
            {
                marketDescriptor = await GetMarketDescriptorAsync(culture).ConfigureAwait(false);
            }
            catch (CacheItemNotFoundException ex)
            {
                HandleErrorCondition("Failed to retrieve market name descriptor", null, null, culture, ex);
                return null;
            }
            if (marketDescriptor == null)
            {
                HandleErrorCondition("Missing market descriptor", null, null, culture, null);
                return null;
            }
            var nameDescription = marketDescriptor.GetName(culture);
            if (nameDescription == null)
            {
                HandleErrorCondition("Retrieved market descriptor does not contain name descriptor in the specified language", null, null, culture, null);
                return null;
            }

            string nameDescriptorFormat;
            IList<INameExpression> expressions;
            try
            {
                expressions = GetNameExpressions(nameDescription, out nameDescriptorFormat);
            }
            catch (Exception ex)
            {
                if (ex is ArgumentException || ex is FormatException)
                {
                    HandleErrorCondition("The name description parsing failed", null, nameDescription, culture, ex);
                    return null;
                }
                throw;
            }

            if (expressions == null)
            {
                return nameDescription;
            }
            List<Task<string>> tasks;
            try
            {
                tasks = expressions.Select(e => e.BuildNameAsync(culture)).ToList();
                await Task.WhenAll(tasks).ConfigureAwait(false);
            }
            catch (NameExpressionException ex)
            {
                HandleErrorCondition("Error occurred while evaluating the name expression", null, nameDescription, culture, ex);
                return null;
            }
            return string.Format(nameDescriptorFormat, tasks.Select(t => t.GetAwaiter().GetResult()).Cast<object>().ToArray());
        }

        /// <summary>
        /// Asynchronously gets the name of the specified outcome in the requested language
        /// </summary>
        /// <param name="outcomeId">The outcome identifier</param>
        /// <param name="culture">The language of the returned name</param>
        /// <returns>A <see cref="Task{String}"/> representing the asynchronous operation</returns>
        public async Task<string> GetOutcomeNameAsync(string outcomeId, CultureInfo culture)
        {
            if (outcomeId.StartsWith(SdkInfo.PlayerProfileMarketPrefix, StringComparison.InvariantCultureIgnoreCase)
             || outcomeId.StartsWith(SdkInfo.CompetitorProfileMarketPrefix, StringComparison.InvariantCultureIgnoreCase))
            {
                try
                {
                    return await GetOutcomeNameFromProfileAsync(outcomeId, culture).ConfigureAwait(false);
                }
                catch (NameExpressionException ex)
                {
                    HandleErrorCondition("Failed to generate outcome name from profile", outcomeId, null, culture, ex);
                    return null;
                }
            }

            var marketDescriptor = await GetMarketDescriptionForOutcomeAsync(outcomeId, culture, true).ConfigureAwait(false);

            var outcome = marketDescriptor?.Outcomes.FirstOrDefault(o => o.Id == outcomeId);
            if (outcome == null)
            {
                return null;
            }

            var nameDescription = outcome.GetName(culture);
            if (string.IsNullOrEmpty(nameDescription))
            {
                HandleErrorCondition("Retrieved market descriptor does not contain name descriptor for associated outcome in the specified language", outcomeId, null, culture, null);
                return null;
            }

            if (marketDescriptor.Attributes?.FirstOrDefault(a => a.Name == SdkInfo.FlexScoreMarketAttributeName) != null)
            {
                try
                {
                    return FlexMarketHelper.GetName(nameDescription, _specifiers);
                }
                catch (NameExpressionException ex)
                {
                    HandleErrorCondition("The generation of name for flex score market outcome failed", outcomeId, nameDescription, culture, ex);
                }
            }

            string nameDescriptionFormat;
            IList<INameExpression> expressions;
            try
            {
                expressions = GetNameExpressions(nameDescription, out nameDescriptionFormat);
            }
            catch (Exception ex)
            {
                if (ex is ArgumentException || ex is FormatException)
                {
                    HandleErrorCondition("The name description parsing failed", outcomeId, nameDescription, culture, ex);
                    return null;
                }
                throw;
            }

            if (expressions == null || !expressions.Any())
            {
                return nameDescription;
            }

            IEnumerable<Task<string>> tasks;
            try
            {
                tasks = expressions.Select(e => e.BuildNameAsync(culture)).ToList();
                await Task.WhenAll(tasks).ConfigureAwait(false);
            }
            catch (NameExpressionException ex)
            {
                HandleErrorCondition("Error occurred while evaluating the name expression", outcomeId, nameDescription, culture, ex);
                return null;
            }
            return string.Format(nameDescriptionFormat, tasks.Select(t => (object)t.GetAwaiter().GetResult()).ToArray());
        }

        /// <summary>
        /// Gets the outcome name from profile
        /// </summary>
        /// <param name="outcomeId">The outcome identifier</param>
        /// <param name="culture">The language of the returned name</param>
        /// <returns>A <see cref="Task{String}"/> representing the async operation</returns>
        /// <exception cref="NameExpressionException">The name of the specified outcome could not be generated</exception>
        private async Task<string> GetOutcomeNameFromProfileAsync(string outcomeId, CultureInfo culture)
        {
            var idParts = outcomeId.Split(new[] { SdkInfo.NameProviderCompositeIdSeparator }, StringSplitOptions.RemoveEmptyEntries);
            var names = new List<string>(idParts.Length);

            foreach (var idPart in idParts)
            {
                Urn profileId;
                try
                {
                    profileId = Urn.Parse(idPart);
                }
                catch (FormatException ex)
                {
                    throw new NameExpressionException($"OutcomeId={idPart} is not a valid urn", ex);
                }

                try
                {
                    if (idPart.StartsWith(SdkInfo.PlayerProfileMarketPrefix, StringComparison.InvariantCultureIgnoreCase))
                    {
                        var cachedPlayerName = await _profileCache.GetPlayerNameAsync(profileId, culture, false).ConfigureAwait(false);
                        if (!string.IsNullOrEmpty(cachedPlayerName))
                        {
                            names.Add(cachedPlayerName);
                            continue;
                        }

                        // first try to fetch all the competitors for the sportEvent, so all player profiles are preloaded
                        if (!_competitorProfilesLoaded.Contains(culture))
                        {
                            if (_sportEvent is Competition competition)
                            {
                                await LoadPlayerDataFromSportEvent(competition, culture).ConfigureAwait(false);
                            }
                            else if (_sportEvent is ITournament tournament)
                            {
                                await LoadCompetitorDataFromSportEvent(tournament, culture, true).ConfigureAwait(false);
                            }
                            else if (_sportEvent is ISeason season)
                            {
                                await LoadCompetitorDataFromSportEvent(season, culture, true).ConfigureAwait(false);
                            }
                        }

                        var playerName = await _profileCache.GetPlayerNameAsync(profileId, culture, true).ConfigureAwait(false);
                        names.Add(playerName);
                        continue;
                    }
                    if (idPart.StartsWith(SdkInfo.CompetitorProfileMarketPrefix, StringComparison.InvariantCultureIgnoreCase))
                    {
                        var cachedCompetitorName = await _profileCache.GetCompetitorNameAsync(profileId, culture, false).ConfigureAwait(false);
                        if (!string.IsNullOrEmpty(cachedCompetitorName))
                        {
                            names.Add(cachedCompetitorName);
                            continue;
                        }

                        // first try to fetch all the competitors for the sportEvent, so all competitor names are preloaded
                        if (!_competitorProfilesLoaded.Contains(culture))
                        {
                            if (_sportEvent is Competition competition)
                            {
                                await LoadCompetitorDataFromSportEvent(competition, culture).ConfigureAwait(false);
                            }
                            else if (_sportEvent is IBasicTournament basicTournament)
                            {
                                await LoadCompetitorDataFromSportEvent(basicTournament, culture, false).ConfigureAwait(false);
                            }
                            else if (_sportEvent is ITournament tournament)
                            {
                                await LoadCompetitorDataFromSportEvent(tournament, culture, false).ConfigureAwait(false);
                            }
                            else if (_sportEvent is ISeason season)
                            {
                                await LoadCompetitorDataFromSportEvent(season, culture, false).ConfigureAwait(false);
                            }
                        }

                        var competitorName = await _profileCache.GetCompetitorNameAsync(profileId, culture, true).ConfigureAwait(false);
                        names.Add(competitorName);
                        continue;
                    }
                }
                catch (CacheItemNotFoundException ex)
                {
                    throw new NameExpressionException("Error occurred while evaluating name expression", ex);
                }

                throw new ArgumentException($"OutcomeId={idPart} must start with '{SdkInfo.PlayerProfileMarketPrefix}' or '{SdkInfo.CompetitorProfileMarketPrefix}'");
            }
            return string.Join(SdkInfo.NameProviderCompositeIdSeparator, names);
        }

        private async Task LoadPlayerDataFromSportEvent(Competition competition, CultureInfo culture)
        {
            try
            {
                var competitors = await competition.GetCompetitorIdsAsync().ConfigureAwait(false);
                var competitorsList = competitors.ToList();
                if (competitorsList.Any())
                {
                    var tasks = competitorsList.Select(s => _profileCache.GetCompetitorProfileAsync(s, new[] { culture }, true));
                    await Task.WhenAll(tasks).ConfigureAwait(false);
                }

                if (!_competitorProfilesLoaded.Contains(culture))
                {
                    _competitorProfilesLoaded.Add(culture);
                }
            }
            catch (Exception ex)
            {
                LogErrorDuringFetching(ex);
            }
        }

        private async Task LoadCompetitorDataFromSportEvent(Competition competition, CultureInfo culture)
        {
            try
            {
                var competitors = await competition.GetCompetitorIdsAsync(culture).ConfigureAwait(false);
                var competitorsList = competitors.ToList();
                if (competitorsList.Any())
                {
                    var tasks = competitorsList.Select(s => _profileCache.GetCompetitorNamesAsync(s, new[] { culture }, true));
                    await Task.WhenAll(tasks).ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                LogErrorDuringFetching(ex);
            }
        }

        private async Task LoadCompetitorDataFromSportEvent(IBasicTournament basicTournament, CultureInfo culture, bool fetchCompetitorNames)
        {
            try
            {
                var competitors = await basicTournament.GetCompetitorsAsync().ConfigureAwait(false);
                var listCompetitors = competitors.ToList();
                if (fetchCompetitorNames && !listCompetitors.IsNullOrEmpty())
                {
                    var tasks = listCompetitors.Select(s => _profileCache.GetCompetitorNameAsync(s.Id, culture, true));
                    await Task.WhenAll(tasks).ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                LogErrorDuringFetching(ex);
            }
        }

        private async Task LoadCompetitorDataFromSportEvent(ITournament tournament, CultureInfo culture, bool fetchCompetitorNames)
        {
            try
            {
                if (tournament is Tournament tour)
                {
                    var competitorIds = tour.GetCompetitorIdsAsync(culture).GetAwaiter().GetResult();
                    if (fetchCompetitorNames && !competitorIds.IsNullOrEmpty())
                    {
                        var tasks = competitorIds.Select(s => _profileCache.GetCompetitorNameAsync(s, culture, true));
                        await Task.WhenAll(tasks).ConfigureAwait(false);
                    }
                }
            }
            catch (Exception ex)
            {
                LogErrorDuringFetching(ex);
            }
        }

        private async Task LoadCompetitorDataFromSportEvent(ISeason season, CultureInfo culture, bool fetchCompetitorNames)
        {
            try
            {
                var competitors = await season.GetCompetitorsAsync().ConfigureAwait(false);
                var listCompetitors = competitors.ToList();
                if (fetchCompetitorNames && !listCompetitors.IsNullOrEmpty())
                {
                    var tasks = listCompetitors.Select(s => _profileCache.GetCompetitorNameAsync(s.Id, culture, true));
                    await Task.WhenAll(tasks).ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                LogErrorDuringFetching(ex);
            }
        }

        private void LogErrorDuringFetching(Exception ex)
        {
            _executionLog.LogDebug(ex, "Error fetching all competitor profiles for {SportEventId}", _sportEvent.Id);
        }

        /// <summary>
        /// Asynchronously gets a <see cref="IMarketDescription"/> instance describing the specified market in the specified language
        /// </summary>
        /// <param name="culture">The <see cref="CultureInfo"/> specifying the language of the returned descriptor</param>
        /// <returns>A <see cref="Task{IMarketDescriptor}"/> representing the asynchronous operation</returns>
        /// <exception cref="CacheItemNotFoundException">The requested key was not found in the cache and could not be loaded</exception>
        private async Task<IMarketDescription> GetMarketDescriptorAsync(CultureInfo culture)
        {
            Guard.Argument(culture, nameof(culture)).NotNull();

            return await _marketCacheProvider.GetMarketDescriptionAsync(_marketId, _specifiers, new[] { culture }, true).ConfigureAwait(false);
        }

        /// <summary>
        /// Handles the error condition occurred during name generation either by logging or throwing an exception - depending on the <see cref="_exceptionStrategy"/>
        /// </summary>
        /// <param name="message">The message describing the condition</param>
        /// <param name="outcomeId">The id of the outcome or a null reference if name was being generated for a market</param>
        /// <param name="nameDescriptor">The retrieved nameDescriptor or a null reference if name descriptor could not be retrieved</param>
        /// <param name="culture">The <see cref="CultureInfo"/> specifying the language associated with the generated name</param>
        /// <param name="innerException">The exception which caused the error condition or a null reference</param>
        /// <exception cref="NameGenerationException">If so specified by the <see cref="_exceptionStrategy"/> field</exception>
        private void HandleErrorCondition(string message, string outcomeId, string nameDescriptor, CultureInfo culture, Exception innerException)
        {
            Guard.Argument(message, nameof(message)).NotNull().NotEmpty();
            Guard.Argument(culture, nameof(culture)).NotNull();

            var specifiersString = _specifiers == null ? "null" : string.Join(SdkInfo.SpecifiersDelimiter, _specifiers.Select(k => $"{k.Key}={k.Value}"));

            _executionLog.LogError(innerException,
                                   "An error occurred while generating the name for item=[MarketId={MarketId}, Specifiers=[{SpecifierStr}], OutcomeId={OutcomeId}], Lang={Language}, Retrieved nameDescriptor=[{NameDescriptor}], AdditionalMessage={Message}",
                                   _marketId,
                                   specifiersString,
                                   outcomeId,
                                   culture.TwoLetterISOLanguageName,
                                   nameDescriptor,
                                   message);

            if (_exceptionStrategy == ExceptionHandlingStrategy.Throw)
            {
                throw new NameGenerationException(message, _marketId, _specifiers, outcomeId, nameDescriptor, culture, innerException);
            }
        }

        /// <summary>
        /// Gets a <see cref="IList{INameExpression}"/> constructed from the provided descriptor
        /// </summary>
        /// <param name="nameDescriptor">The name descriptor</param>
        /// <param name="nameDescriptorFormat">When the call completes, the <c>nameDescriptor</c> replaced with string format placeholders</param>
        /// <returns>a <see cref="IList{INameExpression}"/> constructed from the provided descriptor</returns>
        /// <exception cref="FormatException">Provided <c>nameDescriptor</c> couldn't be parsed due to incorrect format</exception>
        /// <exception cref="ArgumentException">One of the operators specified in the <c>nameDescriptor</c> is not supported</exception>
        protected IList<INameExpression> GetNameExpressions(string nameDescriptor, out string nameDescriptorFormat)
        {
            Guard.Argument(nameDescriptor, nameof(nameDescriptor)).NotNull().NotEmpty();

            var expressionStrings = NameExpressionHelper.ParseDescriptor(nameDescriptor, out nameDescriptorFormat);
            if (expressionStrings.IsNullOrEmpty())
            {
                return new List<INameExpression>();
            }

            var expressions = new List<INameExpression>(expressionStrings.Count);
            foreach (var expression in expressionStrings)
            {
                // @operator can be null
                // operand cannot be null
                NameExpressionHelper.ParseExpression(expression, out var @operator, out var operand);
                expressions.Add(_expressionFactory.BuildExpression(_sportEvent, _specifiers, @operator, operand));
            }
            return expressions;
        }

        private async Task<IMarketDescription> GetMarketDescriptionForOutcomeAsync(string outcomeId, CultureInfo culture, bool firstTime)
        {
            IMarketDescription marketDescriptor;
            try
            {
                marketDescriptor = await GetMarketDescriptorAsync(culture).ConfigureAwait(false);
            }
            catch (CacheItemNotFoundException ex)
            {
                HandleErrorCondition("Failed to retrieve market name descriptor", outcomeId, null, culture, ex);
                return null;
            }

            if (marketDescriptor == null)
            {
                HandleErrorCondition("Failed to retrieve market descriptor", outcomeId, null, culture, null);
                return null;
            }

            if (marketDescriptor.Outcomes == null)
            {
                if (firstTime)
                {
                    HandleErrorCondition("Retrieved market descriptor has no outcomes", outcomeId, null, culture, null);
                    //if (((MarketDescription)marketDescriptor).MarketDescriptionCacheItem.CanBeFetched())
                    {
                        HandleErrorCondition("Reloading market description", outcomeId, null, culture, null);
                        await _marketCacheProvider.ReloadMarketDescriptionAsync((int)marketDescriptor.Id, _specifiers).ConfigureAwait(false);
                        return await GetMarketDescriptionForOutcomeAsync(outcomeId, culture, false).ConfigureAwait(false);
                    }
                }
                HandleErrorCondition("Retrieved market descriptor does not contain outcomes", outcomeId, null, culture, null);
                return null;
            }

            var outcome = marketDescriptor.Outcomes.FirstOrDefault(o => o.Id == outcomeId);
            if (outcome == null)
            {
                if (firstTime)
                {
                    HandleErrorCondition("Retrieved market descriptor is missing outcome", outcomeId, null, culture, null);
                    //if (((MarketDescription)marketDescriptor).MarketDescriptionCacheItem.CanBeFetched())
                    {
                        HandleErrorCondition("Reloading market description", outcomeId, null, culture, null);
                        await _marketCacheProvider.ReloadMarketDescriptionAsync((int)marketDescriptor.Id, _specifiers).ConfigureAwait(false);
                        return await GetMarketDescriptionForOutcomeAsync(outcomeId, culture, false).ConfigureAwait(false);
                    }
                }
                HandleErrorCondition("Retrieved market descriptor does not contain outcome", outcomeId, null, culture, null);
                return null;
            }

            return marketDescriptor;
        }
    }
}
