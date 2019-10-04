/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Linq;
using Common.Logging;
using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Common.Internal;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.DTO;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Internal.MarketNames
{
    internal class VariantDescriptionCacheItem
    {
        private static readonly ILog Log = SdkLoggerFactory.GetLogger(typeof(VariantDescriptionCacheItem));

        internal readonly IList<CultureInfo> FetchedLanguages;

        internal string Id { get; }

        internal IEnumerable<MarketMappingCacheItem> Mappings { get; }

        internal IEnumerable<MarketOutcomeCacheItem> Outcomes { get; }

        internal DateTime LastDataReceived { get; private set; }

        internal string SourceCache { get; }

        protected VariantDescriptionCacheItem(
            string id,
            IEnumerable<MarketOutcomeCacheItem> outcomes,
            IEnumerable<MarketMappingCacheItem> mappings,
            CultureInfo culture,
            string source)
        {

            Contract.Requires(culture != null);

            Id = id;
            Outcomes = outcomes;
            Mappings = mappings;
            FetchedLanguages = new List<CultureInfo>(new[] {culture});

            SourceCache = source;
            LastDataReceived = DateTime.Now;
        }

        /// <summary>
        /// Constructs and returns a <see cref="VariantDescriptionCacheItem"/> from the provided DTO
        /// </summary>
        /// <param name="dto">The <see cref="VariantDescriptionDTO"/> containing variant description data</param>
        /// <param name="factory">The <see cref="IMappingValidatorFactory"/> instance used to build market mapping validators</param>
        /// <param name="culture">A <see cref="CultureInfo"/> specifying the language of the provided DTO</param>
        /// <param name="source">The source cache where <see cref="MarketDescriptionCacheItem"/> is built</param>
        /// <returns>The constructed <see cref="VariantDescriptionCacheItem"/></returns>
        /// <exception cref="InvalidOperationException">The cache item could not be build from the provided DTO</exception>
        public static VariantDescriptionCacheItem Build(VariantDescriptionDTO dto, IMappingValidatorFactory factory, CultureInfo culture, string source)
        {
            Contract.Requires(dto != null);
            Contract.Requires(factory != null);
            Contract.Requires(culture != null);

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

        internal bool CanBeFetched()
        {
            return (DateTime.Now - LastDataReceived).TotalSeconds > SdkInfo.MarketDescriptionMinFetchInterval;
        }

        internal void Merge(VariantDescriptionDTO dto, CultureInfo culture)
        {
            Contract.Requires(dto != null);
            Contract.Requires(culture != null);

            if (dto.Outcomes != null)
            {
                foreach (var outcomeDto in dto.Outcomes)
                {
                    var existingOutcome = Outcomes?.FirstOrDefault(o => o.Id == outcomeDto.Id);
                    if (existingOutcome != null)
                    {
                        existingOutcome.Merge(outcomeDto, culture);
                    }
                    else
                    {
                        Log.Warn($"Could not merge outcome[Id={outcomeDto.Id}] on variantDescription[Id={dto.Id}] because the specified outcome does not exist on stored variant description");
                    }
                }
            }

            if (dto.Mappings != null)
            {
                foreach (var mappingDto in dto.Mappings)
                {
                    var existingMapping = Mappings?.FirstOrDefault(m => m.MarketTypeId == mappingDto.MarketTypeId && m.MarketSubTypeId == mappingDto.MarketSubTypeId);
                    if (existingMapping != null)
                    {
                        existingMapping.Merge(mappingDto, culture);
                    }
                    else
                    {
                        Log.Warn($"Could not merge mapping[MarketId={mappingDto.MarketTypeId}:{mappingDto.MarketSubTypeId}] on variantDescription[Id={dto.Id}] because the specified mapping does not exist on stored variant description");
                    }
                }
            }

            FetchedLanguages.Add(culture);

            LastDataReceived = DateTime.Now;
        }
    }
}
