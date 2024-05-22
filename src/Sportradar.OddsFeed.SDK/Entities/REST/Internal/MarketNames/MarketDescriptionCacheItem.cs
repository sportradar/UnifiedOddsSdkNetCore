// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using Dawn;
using Sportradar.OddsFeed.SDK.Common.Extensions;
using Sportradar.OddsFeed.SDK.Common.Internal.Extensions;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Dto;

namespace Sportradar.OddsFeed.SDK.Entities.Rest.Internal.MarketNames
{
    internal class MarketDescriptionCacheItem
    {
        internal long Id { get; }

        internal readonly IDictionary<CultureInfo, string> Names;

        internal readonly IDictionary<CultureInfo, string> Descriptions;

        internal string Variant { get; private set; }

        internal string OutcomeType { get; private set; }

        internal ICollection<MarketMappingCacheItem> Mappings { get; }

        internal ICollection<MarketOutcomeCacheItem> Outcomes { get; }

        internal ICollection<MarketSpecifierCacheItem> Specifiers { get; }

        internal ICollection<MarketAttributeCacheItem> Attributes { get; }

        internal ICollection<string> Groups { get; private set; }

        internal string SourceCache { get; private set; }

        private readonly object _lock = new object();

        [SuppressMessage("Major Code Smell", "S107:Methods should not have too many parameters", Justification = "Allowed here")]
        [SuppressMessage("ReSharper", "TooManyDependencies", Justification = "Allowed here")]
        private MarketDescriptionCacheItem(long id,
                                           IDictionary<CultureInfo, string> names,
                                           IDictionary<CultureInfo, string> descriptions,
                                           string variant,
                                           string outcomeType,
                                           ICollection<MarketOutcomeCacheItem> outcomes,
                                           ICollection<MarketMappingCacheItem> mappings,
                                           ICollection<MarketSpecifierCacheItem> specifiers,
                                           ICollection<MarketAttributeCacheItem> attributes,
                                           ICollection<string> groups,
                                           CultureInfo culture,
                                           string source)
        {
            Guard.Argument(culture, nameof(culture)).NotNull();
            Guard.Argument(names, nameof(names)).NotNull();
            Guard.Argument(descriptions, nameof(descriptions)).NotNull();

            Id = id;
            Names = names;
            Descriptions = descriptions;
            Outcomes = outcomes;
            Mappings = mappings;
            Specifiers = specifiers;
            Attributes = attributes;
            Variant = variant;
            OutcomeType = outcomeType;
            Groups = groups;
            SourceCache = source;
        }

        /// <summary>
        /// Constructs and returns a <see cref="MarketDescriptionCacheItem"/> from the provided Dto
        /// </summary>
        /// <param name="dto">The <see cref="MarketDescriptionDto"/> containing market description data.</param>
        /// <param name="factory">The <see cref="IMappingValidatorFactory"/> instance used to build market mapping validators .</param>
        /// <param name="culture">A <see cref="CultureInfo"/> specifying the language of the provided Dto.</param>
        /// <param name="source">The source cache where <see cref="MarketDescriptionCacheItem"/> is built</param>
        /// <returns>The constructed <see cref="MarketDescriptionCacheItem"/>.</returns>
        /// <exception cref="InvalidOperationException">The cache item could not be build from the provided Dto</exception>
        [SuppressMessage("ReSharper", "TooManyChainedReferences", Justification = "Are allowed")]
        [SuppressMessage("ReSharper", "TooManyArguments", Justification = "Allowed here")]
        [SuppressMessage("ReSharper", "TooManyDeclarations", Justification = "Allowed here")]
        public static MarketDescriptionCacheItem Build(MarketDescriptionDto dto, IMappingValidatorFactory factory, CultureInfo culture, string source)
        {
            Guard.Argument(dto, nameof(dto)).NotNull();
            Guard.Argument(factory, nameof(factory)).NotNull();
            Guard.Argument(culture, nameof(culture)).NotNull();

            var names = new Dictionary<CultureInfo, string> { { culture, dto.Name } };
            var descriptions = string.IsNullOrEmpty(dto.Description)
                ? new Dictionary<CultureInfo, string>()
                : new Dictionary<CultureInfo, string> { { culture, dto.Description } };

            var outcomes = dto.Outcomes == null
                ? null
                : new ReadOnlyCollection<MarketOutcomeCacheItem>(dto.Outcomes.Select(o => new MarketOutcomeCacheItem(o, culture)).ToList());

            var mappings = dto.Mappings == null
                ? null
                : new ReadOnlyCollection<MarketMappingCacheItem>(dto.Mappings.Select(m => MarketMappingCacheItem.Build(m, factory, culture)).ToList());

            var specifiers = dto.Specifiers == null
                ? null
                : new ReadOnlyCollection<MarketSpecifierCacheItem>(dto.Specifiers.Select(s => new MarketSpecifierCacheItem(s)).ToList());

            var attributes = dto.Attributes == null
                ? null
                : new ReadOnlyCollection<MarketAttributeCacheItem>(dto.Attributes.Select(a => new MarketAttributeCacheItem(a)).ToList());

            var groups = dto.Groups == null
                ? null
                : new ReadOnlyCollection<string>(dto.Groups.ToList());

            return new MarketDescriptionCacheItem(dto.Id, names, descriptions, dto.Variant, dto.OutcomeType, outcomes, mappings, specifiers, attributes, groups, culture, source);
        }

        internal string GetName(CultureInfo culture)
        {
            Guard.Argument(culture, nameof(culture)).NotNull();

            return Names.TryGetValue(culture, out var name) ? name : null;
        }

        internal string GetDescription(CultureInfo culture)
        {
            Guard.Argument(culture, nameof(culture)).NotNull();

            return Descriptions.TryGetValue(culture, out var description) ? description : null;
        }

        internal bool HasTranslationsFor(CultureInfo culture)
        {
            return culture == null
                ? throw new ArgumentNullException(nameof(culture))
                : Names.ContainsKey(culture);
        }

        public void SetFetchInfo(string source)
        {
            if (!string.IsNullOrEmpty(source))
            {
                SourceCache = source;
            }
        }

        internal MarketMergeResult Merge(MarketDescriptionDto dto, CultureInfo culture)
        {
            Guard.Argument(dto, nameof(dto)).NotNull();

            lock (_lock)
            {
                var mergeResult = new MarketMergeResult();
                Names[culture] = dto.Name;
                if (!string.IsNullOrEmpty(dto.Description))
                {
                    Descriptions[culture] = dto.Description;
                }
                MergeOutcomes(dto, culture, mergeResult);
                MergeMappings(dto, culture, mergeResult);
                Variant = dto.Variant;
                OutcomeType = dto.OutcomeType;
                Groups = dto.Groups == null ? null : new ReadOnlyCollection<string>(dto.Groups.ToList());

                return mergeResult;
            }
        }

        private void MergeOutcomes(MarketDescriptionDto dto, CultureInfo culture, MarketMergeResult mergeResult)
        {
            if (dto.Outcomes.IsNullOrEmpty())
            {
                return;
            }

            foreach (var outcomeDto in dto.Outcomes)
            {
                var existingOutcome = Outcomes?.FirstOrDefault(o => o.Id == outcomeDto.Id);
                if (existingOutcome != null)
                {
                    existingOutcome.Merge(outcomeDto, culture);
                }
                else
                {
                    mergeResult.AddOutcomeProblem(outcomeDto.Id);
                }
            }
        }

        private void MergeMappings(MarketDescriptionDto dto, CultureInfo culture, MarketMergeResult mergeResult)
        {
            if (dto.Mappings.IsNullOrEmpty())
            {
                return;
            }

            foreach (var mappingDto in dto.Mappings)
            {
                var existingMapping = Mappings?.FirstOrDefault(m => m.MarketMappingsMatch(mappingDto));
                if (existingMapping != null)
                {
                    existingMapping.Merge(mappingDto, culture);
                }
                else
                {
                    mergeResult.AddMappingProblem(mappingDto.GenerateMarketMappingId());
                }
            }
        }

        public ICollection<CultureInfo> GetFaultyLanguages()
        {
            ICollection<CultureInfo> faultyLanguages = new List<CultureInfo>();

            foreach (var language in Names.Keys)
            {
                if (Names.TryGetValue(language, out var marketName) && marketName.IsNullOrEmpty())
                {
                    faultyLanguages.AddUnique(language);
                    continue;
                }

                CheckOutcomesNameIsPresent(language, faultyLanguages);
            }

            return faultyLanguages;
        }

        private void CheckOutcomesNameIsPresent(CultureInfo checkLanguage, ICollection<CultureInfo> faultyLanguages)
        {
            if (Outcomes.IsNullOrEmpty())
            {
                return;
            }
            foreach (var outcomeNames in Outcomes.Select(s => s.Names))
            {
                if (!outcomeNames.ContainsKey(checkLanguage))
                {
                    faultyLanguages.AddUnique(checkLanguage);
                    break;
                }

                if (outcomeNames.TryGetValue(checkLanguage, out var outcomeName) && outcomeName.IsNullOrEmpty())
                {
                    faultyLanguages.AddUnique(checkLanguage);
                    break;
                }
            }
        }
    }
}
