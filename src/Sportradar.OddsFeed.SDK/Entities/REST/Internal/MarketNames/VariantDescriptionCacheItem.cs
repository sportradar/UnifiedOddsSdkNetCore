// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using Dawn;
using Sportradar.OddsFeed.SDK.Common.Internal.Extensions;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Dto;

namespace Sportradar.OddsFeed.SDK.Entities.Rest.Internal.MarketNames
{
    internal class VariantDescriptionCacheItem
    {
        internal readonly IList<CultureInfo> FetchedLanguages;

        internal string Id { get; }

        internal ICollection<MarketMappingCacheItem> Mappings { get; }

        internal ICollection<MarketOutcomeCacheItem> Outcomes { get; }

        internal DateTime LastDataReceived { get; private set; }

        internal string SourceCache { get; }

        // ReSharper disable once TooManyDependencies
        private VariantDescriptionCacheItem(
            string id,
            ICollection<MarketOutcomeCacheItem> outcomes,
            ICollection<MarketMappingCacheItem> mappings,
            CultureInfo culture,
            string source)
        {
            Guard.Argument(culture, nameof(culture)).NotNull();

            Id = id;
            Outcomes = outcomes;
            Mappings = mappings;
            FetchedLanguages = new List<CultureInfo>(new[] { culture });

            SourceCache = source;
            LastDataReceived = DateTime.Now;
        }

        /// <summary>
        /// Constructs and returns a <see cref="VariantDescriptionCacheItem"/> from the provided Dto
        /// </summary>
        /// <param name="dto">The <see cref="VariantDescriptionDto"/> containing variant description data</param>
        /// <param name="factory">The <see cref="IMappingValidatorFactory"/> instance used to build market mapping validators</param>
        /// <param name="culture">A <see cref="CultureInfo"/> specifying the language of the provided Dto</param>
        /// <param name="source">The source cache where <see cref="MarketDescriptionCacheItem"/> is built</param>
        /// <returns>The constructed <see cref="VariantDescriptionCacheItem"/></returns>
        /// <exception cref="InvalidOperationException">The cache item could not be build from the provided Dto</exception>
        [SuppressMessage("ReSharper", "TooManyChainedReferences")]
        public static VariantDescriptionCacheItem Build(VariantDescriptionDto dto, IMappingValidatorFactory factory, CultureInfo culture, string source)
        {
            Guard.Argument(dto, nameof(dto)).NotNull();
            Guard.Argument(factory, nameof(factory)).NotNull();
            Guard.Argument(culture, nameof(culture)).NotNull();

            var outcomes = dto.Outcomes == null
                ? null
                : new ReadOnlyCollection<MarketOutcomeCacheItem>(dto.Outcomes.Select(o => new MarketOutcomeCacheItem(o, culture)).ToList());

            var mappings = dto.Mappings == null
                ? null
                : new ReadOnlyCollection<MarketMappingCacheItem>(dto.Mappings.Select(m => MarketMappingCacheItem.Build(m, factory, culture)).ToList());

            return new VariantDescriptionCacheItem(dto.Id, outcomes, mappings, culture, source);
        }

        internal bool HasTranslationsFor(CultureInfo culture)
        {
            return FetchedLanguages.Contains(culture);
        }

        internal MarketMergeResult Merge(VariantDescriptionDto dto, CultureInfo culture)
        {
            Guard.Argument(dto, nameof(dto)).NotNull();
            Guard.Argument(culture, nameof(culture)).NotNull();

            var mergeResult = new MarketMergeResult();

            MergeOutcomes(dto, culture, mergeResult);
            MergeMappings(dto, culture, mergeResult);

            FetchedLanguages.Add(culture);

            LastDataReceived = DateTime.Now;

            return mergeResult;
        }

        private void MergeOutcomes(VariantDescriptionDto dto, CultureInfo culture, MarketMergeResult mergeResult)
        {
            if (dto.Outcomes == null)
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

        private void MergeMappings(VariantDescriptionDto dto, CultureInfo culture, MarketMergeResult mergeResult)
        {
            if (dto.Mappings == null)
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
    }
}
