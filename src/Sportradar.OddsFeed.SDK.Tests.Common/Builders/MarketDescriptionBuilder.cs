// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using System.Collections.Generic;
using System.Globalization;
using Sportradar.OddsFeed.SDK.Common.Internal;
using Sportradar.OddsFeed.SDK.Entities.Rest.Market;
using Sportradar.OddsFeed.SDK.Entities.Rest.MarketMapping;
using Sportradar.OddsFeed.SDK.Messages.Rest;
using Sportradar.OddsFeed.SDK.Tests.Common.Builders.Markets;

namespace Sportradar.OddsFeed.SDK.Tests.Common.Builders;

public class MarketDescriptionBuilder
{
    private long _id;
    private ICollection<IOutcomeDescription> _outcomes = new List<IOutcomeDescription>();
    private ICollection<ISpecifier> _specifiers = new List<ISpecifier>();
    private ICollection<IMarketMappingData> _mappings = new List<IMarketMappingData>();
    private ICollection<IMarketAttribute> _attributes = new List<IMarketAttribute>();
    private string _outcomeType;
    private ICollection<string> _groups = new List<string>();
    private readonly Dictionary<CultureInfo, string> _names = new Dictionary<CultureInfo, string>();
    private readonly Dictionary<CultureInfo, string> _descriptions = new Dictionary<CultureInfo, string>();

    public MarketDescriptionBuilder WithId(long id)
    {
        _id = id;
        return this;
    }

    public MarketDescriptionBuilder WithOutcomes(ICollection<IOutcomeDescription> outcomes)
    {
        _outcomes = outcomes;
        return this;
    }

    public MarketDescriptionBuilder WithSpecifiers(ICollection<ISpecifier> specifiers)
    {
        _specifiers = specifiers;
        return this;
    }

    public MarketDescriptionBuilder WithMappings(ICollection<IMarketMappingData> mappings)
    {
        _mappings = mappings;
        return this;
    }

    public MarketDescriptionBuilder WithAttributes(ICollection<IMarketAttribute> attributes)
    {
        _attributes = attributes;
        return this;
    }

    public MarketDescriptionBuilder WithOutcomeType(string outcomeType)
    {
        _outcomeType = outcomeType;
        return this;
    }

    public MarketDescriptionBuilder WithGroups(ICollection<string> groups)
    {
        _groups = groups;
        return this;
    }

    public MarketDescriptionBuilder WithName(string name, CultureInfo language)
    {
        _names[language] = name;
        return this;
    }

    public MarketDescriptionBuilder WithDescription(string description, CultureInfo language)
    {
        _descriptions[language] = description;
        return this;
    }

    public IMarketDescription Build()
    {
        return new MarketDescription(_id, _outcomes, _specifiers, _mappings, _attributes, _outcomeType, _groups, _names, _descriptions);
    }

    public IMarketDescription BuildWith(desc_market apiMarketDescription, CultureInfo culture)
    {
        var outcomes = new List<IOutcomeDescription>();
        foreach (var outcome in apiMarketDescription.outcomes)
        {
            outcomes.Add(new OutcomeDescription(outcome, culture));
        }

        var specifiers = new List<ISpecifier>();
        foreach (var specifier in apiMarketDescription.specifiers)
        {
            specifiers.Add(new Specifier(specifier));
        }

        var mappings = new List<IMarketMappingData>();
        foreach (var mapping in apiMarketDescription.mappings)
        {
            mappings.Add(new MarketMapping(apiMarketDescription.id.ToString(), mapping, culture));
        }

        var attributes = new List<IMarketAttribute>();
        foreach (var attribute in apiMarketDescription.attributes)
        {
            attributes.Add(new MarketAttribute(attribute));
        }

        var groups = apiMarketDescription.groups?.Split([SdkInfo.MarketGroupsDelimiter], StringSplitOptions.RemoveEmptyEntries);

        return new MarketDescription(apiMarketDescription.id,
                                     outcomes,
                                     specifiers,
                                     mappings,
                                     attributes,
                                     apiMarketDescription.outcome_type,
                                     groups,
                                     _names,
                                     _descriptions);
    }
}
