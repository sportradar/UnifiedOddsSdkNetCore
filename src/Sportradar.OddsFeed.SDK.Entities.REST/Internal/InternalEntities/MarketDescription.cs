/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Linq;
using Sportradar.OddsFeed.SDK.Common.Internal;
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

        [Obsolete("Use OutcomeType")]
        public string IncludesOutcomesOfType =>
            OutcomeType == null
                ? null
                : OutcomeType == SdkInfo.FreeTextVariantValue
                    ? SdkInfo.OutcometextVariantValue
                    : "sr:" + OutcomeType;

        public string OutcomeType { get; }

        public IEnumerable<IOutcomeDescription> Outcomes { get; internal set; }

        public IEnumerable<ISpecifier> Specifiers { get; }

        public IEnumerable<IMarketMappingData> Mappings { get; internal set; }

        public IEnumerable<IMarketAttribute> Attributes { get; }

        public IEnumerable<string> Groups { get; }

        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public MarketDescriptionCacheItem MarketDescriptionCI { get; }

        internal MarketDescription(MarketDescriptionCacheItem cacheItem, IEnumerable<CultureInfo> cultures)
        {
            Contract.Requires(cacheItem != null);
            Contract.Requires(cultures != null && cultures.Any());

            var cultureList = cultures as IList<CultureInfo> ?? cultures.ToList();

            MarketDescriptionCI = cacheItem;

            Id = cacheItem.Id;
            _names = new ReadOnlyDictionary<CultureInfo, string>(cultureList.ToDictionary(c => c, cacheItem.GetName));
            _descriptions = new ReadOnlyDictionary<CultureInfo, string>(cultureList.Where(cacheItem.HasTranslationsFor).ToDictionary(c => c, cacheItem.GetDescription));
            Outcomes = cacheItem.Outcomes == null
                ? null
                : new ReadOnlyCollection<IOutcomeDescription>(cacheItem.Outcomes.Select(o => (IOutcomeDescription) new OutcomeDescription(o, cultureList)).ToList());
            Specifiers = cacheItem.Specifiers == null
                ? null
                : new ReadOnlyCollection<ISpecifier>(cacheItem.Specifiers.Select(s => (ISpecifier) new Specifier(s)).ToList());
            Mappings = cacheItem.Mappings == null
                ? null
                : new ReadOnlyCollection<IMarketMappingData>(cacheItem.Mappings.Select(m => (IMarketMappingData) new MarketMapping(m)).ToList());
            Attributes = cacheItem.Attributes == null
                ? null
                : new ReadOnlyCollection<IMarketAttribute>(cacheItem.Attributes.Select(a => (IMarketAttribute) new MarketAttribute(a)).ToList());

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

        public void SetFetchInfo(string source, DateTime lastDataReceived)
        {
            MarketDescriptionCI.SetFetchInfo(source, lastDataReceived);
        }

        public string GetName(CultureInfo culture)
        {
            string name;
            return _names.TryGetValue(culture, out name)
                ? name
                : null;
        }

        public string GetDescription(CultureInfo culture)
        {
            string description;
            return _descriptions.TryGetValue(culture, out description)
                ? description
                : null;
        }
    }
}
