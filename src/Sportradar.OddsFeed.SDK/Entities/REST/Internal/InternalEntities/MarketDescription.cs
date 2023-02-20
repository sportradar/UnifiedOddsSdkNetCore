/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using Dawn;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.MarketNames;
using Sportradar.OddsFeed.SDK.Entities.REST.Market;
using Sportradar.OddsFeed.SDK.Entities.REST.MarketMapping;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Internal.InternalEntities
{
    internal class MarketDescription : IMarketDescription
    {
        private readonly IDictionary<CultureInfo, string> _names;

        private readonly IDictionary<CultureInfo, string> _descriptions;

        public long Id { get; }

        public string OutcomeType { get; }

        public IEnumerable<IOutcomeDescription> Outcomes { get; internal set; }

        public IEnumerable<ISpecifier> Specifiers { get; internal set; }

        public IEnumerable<IMarketMappingData> Mappings { get; internal set; }

        public IEnumerable<IMarketAttribute> Attributes { get; }

        public IEnumerable<string> Groups { get; }

        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public MarketDescriptionCacheItem MarketDescriptionCI { get; }

        internal MarketDescription(MarketDescriptionCacheItem cacheItem, IReadOnlyCollection<CultureInfo> cultures)
        {
            Guard.Argument(cacheItem, nameof(cacheItem)).NotNull();
            Guard.Argument(cultures, nameof(cultures)).NotNull().NotEmpty();

            MarketDescriptionCI = cacheItem;

            Id = cacheItem.Id;
            _names = new ReadOnlyDictionary<CultureInfo, string>(cultures.ToDictionary(c => c, cacheItem.GetName));
            _descriptions = new ReadOnlyDictionary<CultureInfo, string>(cultures.Where(cacheItem.HasTranslationsFor).ToDictionary(c => c, cacheItem.GetDescription));
            Outcomes = cacheItem.Outcomes == null
                ? null
                : new ReadOnlyCollection<IOutcomeDescription>(cacheItem.Outcomes.Select(o => (IOutcomeDescription)new OutcomeDescription(o, cultures)).ToList());
            Specifiers = cacheItem.Specifiers == null
                ? null
                : new ReadOnlyCollection<ISpecifier>(cacheItem.Specifiers.Select(s => (ISpecifier)new Specifier(s)).ToList());
            Mappings = cacheItem.Mappings == null
                ? null
                : new ReadOnlyCollection<IMarketMappingData>(cacheItem.Mappings.Select(m => (IMarketMappingData)new MarketMapping(m)).ToList());
            Attributes = cacheItem.Attributes == null
                ? null
                : new ReadOnlyCollection<IMarketAttribute>(cacheItem.Attributes.Select(a => (IMarketAttribute)new MarketAttribute(a)).ToList());

            OutcomeType = cacheItem.OutcomeType;

            Groups = cacheItem.Groups == null ? null : new ReadOnlyCollection<string>(cacheItem.Groups.ToList());
        }

        public void SetOutcomes(IReadOnlyCollection<IOutcomeDescription> outcomes)
        {
            Outcomes = outcomes;
        }

        public void SetMappings(IReadOnlyCollection<IMarketMappingData> mappings)
        {
            Mappings = mappings;
        }

        public void SetSpecifiers(IReadOnlyCollection<ISpecifier> specifiers)
        {
            Specifiers = specifiers;
        }

        public void SetFetchInfo(string source, DateTime lastDataReceived)
        {
            MarketDescriptionCI.SetFetchInfo(source, lastDataReceived);
        }

        public string GetName(CultureInfo culture)
        {
            return _names.TryGetValue(culture, out var name)
                ? name
                : null;
        }

        public string GetDescription(CultureInfo culture)
        {
            return _descriptions.TryGetValue(culture, out var description)
                ? description
                : null;
        }
    }
}
