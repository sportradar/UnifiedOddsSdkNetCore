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
    internal class MarketDescriptionCacheItem
    {
        private static readonly ILog ExecutionLog = SdkLoggerFactory.GetLogger(typeof(MarketDescriptionCacheItem));

        private readonly IDictionary<CultureInfo, string> _names;

        private readonly IDictionary<CultureInfo, string> _descriptions;

        internal readonly IList<CultureInfo> FetchedLanguages;

        internal string Variant { get; private set; }

        internal long Id { get; }

        internal string OutcomeType { get; private set; }

        internal IEnumerable<MarketMappingCacheItem> Mappings { get; }

        internal IEnumerable<MarketOutcomeCacheItem> Outcomes { get; }

        internal IEnumerable<MarketSpecifierCacheItem> Specifiers { get; }

        internal IEnumerable<MarketAttributeCacheItem> Attributes { get; }

        internal IEnumerable<string> Groups { get; private set; }

        internal DateTime LastDataReceived { get; private set; }

        internal string SourceCache { get; private set; }

        /// <summary>
        /// A <see cref="object" /> instance used for thread synchronization
        /// </summary>
        private readonly object _lock = new object();

        protected MarketDescriptionCacheItem(
            long id,
            IDictionary<CultureInfo, string> names,
            IDictionary<CultureInfo, string> descriptions,
            string variant,
            string outcomeType,
            IEnumerable<MarketOutcomeCacheItem> outcomes,
            IEnumerable<MarketMappingCacheItem> mappings,
            IEnumerable<MarketSpecifierCacheItem> specifiers,
            IEnumerable<MarketAttributeCacheItem> attributes,
            IEnumerable<string> groups,
            CultureInfo culture,
            string source)
        {

            Contract.Requires(culture != null);
            Contract.Requires(names != null);
            Contract.Requires(descriptions != null);

            Id = id;
            _names = names;
            _descriptions = descriptions;
            FetchedLanguages = new List<CultureInfo>(new[] { culture });
            Outcomes = outcomes;
            Mappings = mappings;
            Specifiers = specifiers;
            Attributes = attributes;
            Variant = variant;
            OutcomeType = outcomeType;
            Groups = groups;
            SourceCache = source;
            LastDataReceived = DateTime.Now;
        }

        /// <summary>
        /// Constructs and returns a <see cref="MarketDescriptionCacheItem"/> from the provided DTO
        /// </summary>
        /// <param name="dto">The <see cref="MarketDescriptionDTO"/> containing market description data.</param>
        /// <param name="factory">The <see cref="IMappingValidatorFactory"/> instance used to build market mapping validators .</param>
        /// <param name="culture">A <see cref="CultureInfo"/> specifying the language of the provided DTO.</param>
        /// <param name="source">The source cache where <see cref="MarketDescriptionCacheItem"/> is built</param>
        /// <returns>The constructed <see cref="MarketDescriptionCacheItem"/>.</returns>
        /// <exception cref="InvalidOperationException">The cache item could not be build from the provided DTO</exception>
        public static MarketDescriptionCacheItem Build(MarketDescriptionDTO dto, IMappingValidatorFactory factory, CultureInfo culture, string source)
        {
            Contract.Requires(dto != null);
            Contract.Requires(factory != null);
            Contract.Requires(culture != null);

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

            var groups = dto.Groups == null ? null : new ReadOnlyCollection<string>(dto.Groups.ToList());

            return new MarketDescriptionCacheItem(dto.Id, names, descriptions, dto.Variant, dto.OutcomeType, outcomes, mappings, specifiers, attributes, groups, culture, source);
        }

        internal string GetName(CultureInfo culture)
        {
            Contract.Requires(culture != null);

            string name;
            return _names.TryGetValue(culture, out name) ? name : null;
        }

        internal string GetDescription(CultureInfo culture)
        {
            Contract.Requires(culture != null);

            string description;
            if (_descriptions.TryGetValue(culture, out description))
            {
                return description;
            }
            return null;
        }

        internal bool HasTranslationsFor(CultureInfo culture)
        {
            return FetchedLanguages.Contains(culture);
        }

        internal bool CanBeFetched()
        {
            return (DateTime.Now - LastDataReceived).TotalSeconds > SdkInfo.MarketDescriptionMinFetchInterval;
        }

        public void SetFetchInfo(string source, DateTime lastDataReceived)
        {
            if (!string.IsNullOrEmpty(source))
            {
                SourceCache = source;
            }
            LastDataReceived = lastDataReceived;
        }

        internal void Merge(MarketDescriptionDTO dto, CultureInfo culture)
        {
            Contract.Requires(dto != null);
            Contract.Requires(culture != null);

            lock (_lock)
            {
                _names[culture] = dto.Name;
                if (!string.IsNullOrEmpty(dto.Description))
                {
                    _descriptions[culture] = dto.Description;
                }
                Variant = dto.Variant;

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
                            ExecutionLog.Warn($"Could not merge outcome[Id={outcomeDto.Id}] for lang={culture.TwoLetterISOLanguageName} on marketDescription[Id={Id}] because the specified outcome does not exist on stored market description.");
                            ComparePrint(dto, culture);
                        }
                    }
                }

                if (dto.Mappings != null)
                {
                    foreach (var mappingDto in dto.Mappings)
                    {
                        var existingMapping = Mappings?.FirstOrDefault(m => MarketMappingsMatch(m, mappingDto));
                        if (existingMapping != null)
                        {
                            existingMapping.Merge(mappingDto, culture);
                        }
                        else
                        {
                            ExecutionLog.Warn($"Could not merge mapping[MarketId={mappingDto.MarketTypeId}:{mappingDto.MarketSubTypeId}] for lang={culture.TwoLetterISOLanguageName} on marketDescription[Id={dto.Id}]because the specified mapping does not exist on stored market description.");
                            ComparePrint(dto, culture);
                        }
                    }
                }

                OutcomeType = dto.OutcomeType;

                Groups = dto.Groups == null ? null : new ReadOnlyCollection<string>(dto.Groups.ToList());

                FetchedLanguages.Add(culture);

                LastDataReceived = DateTime.Now;
            }
        }

        private bool MarketMappingsMatch(MarketMappingCacheItem ci, MarketMappingDTO dto)
        {
            var isMatch = ci.MarketTypeId == dto.MarketTypeId && ci.MarketSubTypeId == dto.MarketSubTypeId;

            if (isMatch && ci.SportId != null)
            {
                isMatch = ci.SportId.Equals(dto.SportId);
            }

            if (isMatch && ci.ProducerIds != null)
            {
                isMatch = ci.ProducerIds.All(a => dto.ProducerIds.Contains(a));
            }

            if (isMatch && !string.IsNullOrEmpty(ci.ValidFor))
            {
                isMatch = ci.ValidFor.Equals(dto.ValidFor, StringComparison.InvariantCultureIgnoreCase);
            }

            return isMatch;
        }

        private void ComparePrint(MarketDescriptionDTO dto, CultureInfo culture)
        {
            var names = _names.Aggregate(string.Empty, (current, name) => current + $", {name.Key.TwoLetterISOLanguageName}-{name.Value}").Substring(2);
            var desc = _descriptions.Aggregate(string.Empty, (current, d) => current + $", {d.Key.TwoLetterISOLanguageName}-{d.Value}");
            var specs = Specifiers == null ? null : string.Join(", ", Specifiers.Select(s => s.Name));
            var outcomes = Outcomes == null ? null : string.Join(",", Outcomes.Select(s => s.Id));
            var maps = Mappings == null ? null : string.Join(",", Mappings.Select(s => s.MarketTypeId));
            ExecutionLog.Debug($"Original Id={Id}, Names=[{names}], Descriptions=[{desc}], Variant=[{Variant}], Specifiers=[{specs}], Outocomes=[{outcomes}], Mappings=[{maps}].");

            var specsNew = dto.Specifiers == null ? null : string.Join(", ", dto.Specifiers.Select(s => s.Name));
            var outcomesNew = dto.Outcomes == null ? null : string.Join(",", dto.Outcomes.Select(s => s.Id));
            var mapsNew = dto.Mappings == null ? null : string.Join(",", dto.Mappings.Select(s => s.MarketTypeId));
            ExecutionLog.Debug($"New Id={dto.Id}, Name=[{culture.TwoLetterISOLanguageName}-{dto.Name}], Descriptions=[{dto.Description}], Variant=[{dto.Variant}], Specifiers=[{specsNew}], Outocomes=[{outcomesNew}], Mappings=[{mapsNew}].");
        }
    }
}
