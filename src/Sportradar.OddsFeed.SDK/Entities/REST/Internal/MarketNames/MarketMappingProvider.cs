
// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dawn;
using Microsoft.Extensions.Logging;
using Sportradar.OddsFeed.SDK.Api;
using Sportradar.OddsFeed.SDK.Api.Internal.Caching;
using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Common.Enums;
using Sportradar.OddsFeed.SDK.Common.Exceptions;
using Sportradar.OddsFeed.SDK.Common.Internal;
using Sportradar.OddsFeed.SDK.Common.Internal.Telemetry;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.MarketNameGeneration;
using Sportradar.OddsFeed.SDK.Entities.Rest.Market;
using Sportradar.OddsFeed.SDK.Entities.Rest.MarketMapping;

namespace Sportradar.OddsFeed.SDK.Entities.Rest.Internal.MarketNames
{
    /// <summary>
    /// Provides mapping ids of markets and outcomes
    /// </summary>
    /// <seealso cref="IMarketMappingProvider" />
    internal class MarketMappingProvider : IMarketMappingProvider
    {
        /// <summary>
        /// A <see cref="ILogger"/> instance used for execution logging
        /// </summary>
        private static readonly ILogger ExecutionLog = SdkLoggerFactory.GetLogger(typeof(MarketMappingProvider));

        /// <summary>
        /// A <see cref="ILogger"/> instance used for execution logging
        /// </summary>
        private static readonly ILogger CacheLog = SdkLoggerFactory.GetLoggerForCache(typeof(MarketMappingProvider));

        /// <summary>
        /// A <see cref="IMarketCacheProvider"/> instance used to retrieve market descriptors
        /// </summary>
        private readonly IMarketCacheProvider _marketCacheProvider;

        /// <summary>
        /// The event status cache
        /// </summary>
        private readonly ISportEventStatusCache _eventStatusCache;

        /// <summary>
        /// A <see cref="ICompetition"/> instance representing associated sport event
        /// </summary>
        private readonly ISportEvent _sportEvent;

        /// <summary>
        /// A market identifier of the market associated with the constructed instance
        /// </summary>
        private readonly int _marketId;

        private readonly int _producerId;

        private readonly Urn _sportId;

        /// <summary>
        /// A <see cref="IReadOnlyDictionary{TKey,TValue}"/> representing specifiers of the associated market
        /// </summary>
        private readonly IReadOnlyDictionary<string, string> _specifiers;

        private string SpecifiersString
        {
            get
            {
                return _specifiers == null ? "null" : string.Join(SdkInfo.SpecifiersDelimiter, _specifiers.Select(k => $"{k.Key}={k.Value}"));
            }
        }

        /// <summary>
        /// A <see cref="ExceptionHandlingStrategy"/> describing the mode in which the SDK is running
        /// </summary>
        private readonly ExceptionHandlingStrategy _exceptionStrategy;

        /// <summary>
        /// Initializes a new instance of the <see cref="NameProvider"/> class
        /// </summary>
        /// <param name="marketCacheProvider">A <see cref="IMarketCacheProvider"/> instance used to retrieve market descriptors</param>
        /// <param name="eventStatusCache">A <see cref="ISportEventStatusCache"/> instance used to retrieve event status data</param>
        /// <param name="sportEvent">A <see cref="ISportEvent"/> instance representing associated sport @event</param>
        /// <param name="marketId">A market identifier of the market associated with the constructed instance</param>
        /// <param name="specifiers">A <see cref="IReadOnlyDictionary{String, String}"/> representing specifiers of the associated market</param>
        /// <param name="producerId">The id of the <see cref="IProducer"/> used to get market/outcome mappings</param>
        /// <param name="sportId">A sportId used to get market/outcome mappings</param>
        /// <param name="exceptionStrategy">A <see cref="ExceptionHandlingStrategy"/> describing the mode in which the SDK is running</param>
        [SuppressMessage("Major Code Smell", "S107:Methods should not have too many parameters", Justification = "Allowed")]
        internal MarketMappingProvider(IMarketCacheProvider marketCacheProvider,
                                       ISportEventStatusCache eventStatusCache,
                                       ISportEvent sportEvent,
                                       int marketId,
                                       IReadOnlyDictionary<string, string> specifiers,
                                       int producerId,
                                       Urn sportId,
                                       ExceptionHandlingStrategy exceptionStrategy)
        {
            Guard.Argument(marketCacheProvider, nameof(marketCacheProvider)).NotNull();
            Guard.Argument(sportEvent, nameof(sportEvent)).NotNull();
            Guard.Argument(eventStatusCache, nameof(eventStatusCache)).NotNull();
            Guard.Argument(producerId, nameof(producerId)).Positive();

            _marketCacheProvider = marketCacheProvider;
            _eventStatusCache = eventStatusCache;
            _sportEvent = sportEvent;
            _marketId = marketId;
            _specifiers = specifiers;
            _exceptionStrategy = exceptionStrategy;
            _producerId = producerId;
            _sportId = sportId;
        }

        /// <summary>
        /// Asynchronously gets a <see cref="IMarketDescription"/> instance describing the specified market in the specified language
        /// </summary>
        /// <returns>A <see cref="Task{IMarketDescriptor}"/> representing the asynchronous operation</returns>
        /// <exception cref="CacheItemNotFoundException">The requested key was not found in the cache and could not be loaded</exception>
        private async Task<IMarketDescription> GetMarketDescriptorAsync(IReadOnlyCollection<CultureInfo> cultures)
        {
            try
            {
                //fetch the market description from the invariant market cache to get the mapping information
                var marketDescriptor = await _marketCacheProvider.GetMarketDescriptionAsync(_marketId, _specifiers, cultures, true).ConfigureAwait(false);
                if (marketDescriptor == null)
                {
                    ExecutionLog.LogWarning("An error occurred getting marketDescription for marketId={MarketId}", _marketId);
                }
                return marketDescriptor;
            }
            catch (CacheItemNotFoundException ex)
            {
                HandleMappingErrorCondition("Failed to retrieve market name descriptor", "MarketId", _marketId.ToString(), string.Empty, ex);
                return null;
            }
        }

        private IEnumerable<IMarketMappingData> GetMarketMapping(IMarketDescription marketDescription)
        {
            if (marketDescription == null)
            {
                return new List<IMarketMappingData>();
            }

            if (marketDescription.Mappings == null || !marketDescription.Mappings.Any())
            {
                CacheLog.LogDebug("MarketDescription for marketId={MarketId} has no mappings", _marketId);
                return new List<IMarketMappingData>();
            }

            var mappings = marketDescription.Mappings.Where(m => m.CanMap(_producerId, _sportId, _specifiers)).ToList();

            if (!mappings.Any())
            {
                CacheLog.LogDebug("Market with id:{MarketId}, producer:{ProducerId}, sportId:{SportId} has no mappings", _marketId, _producerId, _sportId);
                return mappings;
            }

            if (mappings.Count > 1)
            {
                foreach (var mapping in mappings)
                {
                    if (mapping.MarketId.Equals(marketDescription.Id.ToString()))
                    {
                        return new[] { mapping };
                    }
                }

                CacheLog.LogInformation("MarketId:{MarketId}, producer:{ProducerId}, sportId:{SportId}, specifiers={MarketSpecifiers} has multiple mappings [{MappingsCount}]", _marketId, _producerId, _sportId.Id, SdkInfo.SpecifiersKeysToString(marketDescription.Specifiers), mappings.Count);
                var i = 0;
                foreach (var mapping in mappings)
                {
                    CacheLog.LogDebug("MarketId:{MarketId}, producer:{ProducerId}, sportId:{SportId} mapping[{MappingId}]: {Mapping}", _marketId, _producerId, _sportId.Id, i, mapping);
                    i++;
                }
            }

            return mappings;
        }

        private string GetSovValue(string sovTemplate)
        {
            if (string.IsNullOrEmpty(sovTemplate) || !sovTemplate.Contains("{"))
            {
                return sovTemplate;
            }

            var sovValue = sovTemplate;
            var c = sovTemplate.Count(s => s == '{');

            while (sovValue.Contains("{") && c > 0)
            {
                c--;
                sovValue = SwitchSpecifierPlaceHolder(sovValue);
            }

            return sovValue;
        }

        private string SwitchSpecifierPlaceHolder(string currentTemplate)
        {
            var i = currentTemplate.IndexOf("{", StringComparison.InvariantCultureIgnoreCase);
            var j = currentTemplate.IndexOf("}", i, StringComparison.InvariantCultureIgnoreCase);

            var sovKey = currentTemplate.Substring(i + 1, j - i - 1);

            var sovValue = sovKey.Contains("$") ? HandleSpecialSovKeys(sovKey) : _specifiers.FirstOrDefault(s => s.Key == sovKey).Value;

            if (string.IsNullOrEmpty(sovValue))
            {
                HandleMappingErrorCondition($"Market with id:{_marketId}, producer:{_producerId}, sportId:{_sportId} and SOV:{currentTemplate} has no appropriate specifier.", "SovTemplate", currentTemplate, "MarketMapping", null);
            }
            else
            {
                currentTemplate = currentTemplate.Replace("{" + sovKey + "}", sovValue);
            }

            return currentTemplate;
        }

        private string HandleSpecialSovKeys(string sovKey)
        {
            var result = string.Empty;
            if (sovKey == "$score")
            {
                var eventStatus = _eventStatusCache.GetSportEventStatusAsync(_sportEvent.Id).GetAwaiter().GetResult();
                if (eventStatus != null)
                {
                    result = $"{eventStatus.HomeScore ?? 0}:{eventStatus.AwayScore ?? 0}";
                }
            }
            return result;
        }

        private void HandleMappingErrorCondition(string message, string propertyName, string propertyValue, string targetTypeName, Exception innerException)
        {
            Guard.Argument(message, nameof(message)).NotNull().NotEmpty();

            var sb = new StringBuilder("An error occurred while generating the mappedId for item=[");
            sb.Append(" MarketId=").Append(_marketId)
              .Append(", Specifiers=[").Append(SpecifiersString).Append("]");

            if (!string.IsNullOrEmpty(propertyName))
            {
                sb.Append(", PropertyName[").Append(propertyName);
                sb.Append("]=").Append(propertyValue);
            }

            if (!string.IsNullOrEmpty(targetTypeName))
            {
                sb.Append(", TargetType=[").Append(targetTypeName);
            }

            sb.Append("]. AdditionalMessage=").Append(message);

            // ReSharper disable once TemplateIsNotCompileTimeConstantProblem
            ExecutionLog.LogError(innerException, sb.ToString());
            if (_exceptionStrategy == ExceptionHandlingStrategy.Throw)
            {
                throw new MappingException(message, propertyName, propertyValue, targetTypeName, innerException);
            }
        }

        /// <summary>
        /// Asynchronously gets the market mapping Id of the specified market
        /// </summary>
        /// <param name="cultures">The list of <see cref="CultureInfo"/> to fetch <see cref="IMarketMapping"/></param>
        /// <returns>A <see cref="Task{IMarketMappingId}"/> representing the asynchronous operation</returns>
        public async Task<IEnumerable<IMarketMapping>> GetMappedMarketIdAsync(IReadOnlyCollection<CultureInfo> cultures)
        {
            if (_producerId == 5)
            {
                return new List<IMarketMapping>();
            }

            var marketDescription = await GetMarketDescriptorAsync(cultures).ConfigureAwait(false);

            var marketMappings = GetMarketMapping(marketDescription);
            var marketMappingDatas = marketMappings.ToList();
            if (!marketMappingDatas.Any())
            {
                return new List<IMarketMapping>();
            }

            var mappings = new List<IMarketMapping>();
            foreach (var mappingData in marketMappingDatas)
            {
                var sov = mappingData.SovTemplate;
                if (!string.IsNullOrEmpty(mappingData.SovTemplate) && !mappingData.SovTemplate.Equals("{id}", StringComparison.InvariantCultureIgnoreCase))
                {
                    sov = GetSovValue(mappingData.SovTemplate);
                }
                if (mappingData.MarketSubTypeId != null || _producerId == 1) //_producer.Equals(_producerManager.Get("LO")))
                {
                    mappings.Add(new LoMarketMapping(mappingData.MarketTypeId, mappingData.MarketSubTypeId ?? -1, sov));
                }
                else
                {
                    mappings.Add(new LcooMarketMapping(mappingData.MarketTypeId, sov));
                }
            }

            return mappings;
        }

        /// <summary>
        /// Asynchronously gets the mapping Id of the specified outcome
        /// </summary>
        /// <param name="outcomeId">The outcome identifier used to get mapped outcomeId</param>
        /// <param name="cultures">The list of <see cref="CultureInfo"/> to fetch <see cref="IOutcomeMapping"/></param>
        /// <returns>A <see cref="Task{IOutcomeMappingId}"/> representing the asynchronous operation</returns>
        public async Task<IEnumerable<IOutcomeMapping>> GetMappedOutcomeIdAsync(string outcomeId, IReadOnlyCollection<CultureInfo> cultures)
        {
            if (_producerId == 5)
            {
                return null;
            }

            var marketDescription = await GetMarketDescriptorAsync(cultures).ConfigureAwait(false);
            var marketMappings = GetMarketMapping(marketDescription);
            var marketMappingDatas = marketMappings.ToList();
            if (!marketMappingDatas.Any())
            {
                return null;
            }

            var allOutcomes = new List<IOutcomeMappingData>();
            foreach (var marketMapping in marketMappingDatas)
            {
                if (marketMapping?.OutcomeMappings != null)
                {
                    allOutcomes.AddRange(marketMapping.OutcomeMappings);
                }
            }

            if (!allOutcomes.Any())
            {
                return null;
            }

            var mappedOutcomes = allOutcomes.Where(o => Equals(o.OutcomeId.ToString(), outcomeId)).ToList();
            if (!mappedOutcomes.Any())
            {
                return null;
            }

            var resultMappedOutcomes = new List<IOutcomeMapping>();

            foreach (var mappedOutcome in mappedOutcomes)
            {
                var producerOutcomeNames = new Dictionary<CultureInfo, string>();
                foreach (var cultureInfo in cultures)
                {
                    var producerOutcomeName = mappedOutcome.GetProducerOutcomeName(cultureInfo);

                    if (!string.IsNullOrEmpty(producerOutcomeName)
                        && _specifiers != null
                        && _specifiers.ContainsKey("score")
                        && marketDescription.Attributes?.FirstOrDefault(a => a.Name == SdkInfo.FlexScoreMarketAttributeName) != null)
                    {
                        try
                        {
                            producerOutcomeName = FlexMarketHelper.GetName(producerOutcomeName, _specifiers);
                        }
                        catch (NameExpressionException ex)
                        {
                            ExecutionLog.LogError(ex, "The generation of name for flex score mapped outcome {OutcomeId} failed", outcomeId);
                        }
                    }

                    producerOutcomeNames[cultureInfo] = producerOutcomeName;
                }

                if (!string.IsNullOrEmpty(mappedOutcome.OutcomeId))
                {
                    resultMappedOutcomes.Add(new OutcomeMapping(mappedOutcome.ProducerOutcomeId, producerOutcomeNames, mappedOutcome.MarketId));
                }
            }

            return !resultMappedOutcomes.Any()
                ? null
                : resultMappedOutcomes;
        }
    }
}
