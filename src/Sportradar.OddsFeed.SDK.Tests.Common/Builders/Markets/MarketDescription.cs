// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System.Collections.Generic;
using System.Globalization;
using Sportradar.OddsFeed.SDK.Entities.Rest.Market;
using Sportradar.OddsFeed.SDK.Entities.Rest.MarketMapping;

namespace Sportradar.OddsFeed.SDK.Tests.Common.Builders.Markets;

internal class MarketDescription : IMarketDescription
{
    public long Id { get; }
    public IEnumerable<IOutcomeDescription> Outcomes { get; }
    public IEnumerable<ISpecifier> Specifiers { get; }
    public IEnumerable<IMarketMappingData> Mappings { get; }
    public IEnumerable<IMarketAttribute> Attributes { get; }
    public string OutcomeType { get; }
    public IEnumerable<string> Groups { get; }
    private readonly Dictionary<CultureInfo, string> _names;
    private readonly Dictionary<CultureInfo, string> _descriptions;

    public MarketDescription(long id,
                             ICollection<IOutcomeDescription> outcomes,
                             ICollection<ISpecifier> specifiers,
                             ICollection<IMarketMappingData> mappings,
                             ICollection<IMarketAttribute> attributes,
                             string outcomeType,
                             ICollection<string> groups,
                             Dictionary<CultureInfo, string> names,
                             Dictionary<CultureInfo, string> descriptions)
    {
        Id = id;
        Outcomes = outcomes;
        Specifiers = specifiers;
        Mappings = mappings;
        Attributes = attributes;
        OutcomeType = outcomeType;
        Groups = groups;
        _names = names;
        _descriptions = descriptions;
    }

    public string GetName(CultureInfo culture)
    {
        return _names.GetValueOrDefault(culture);
    }

    public string GetDescription(CultureInfo culture)
    {
        return _descriptions.GetValueOrDefault(culture);
    }
}
