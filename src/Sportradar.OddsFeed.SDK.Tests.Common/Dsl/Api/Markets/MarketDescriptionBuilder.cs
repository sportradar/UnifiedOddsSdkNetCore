// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using System.Collections.Generic;
using System.Linq;
using Sportradar.OddsFeed.SDK.Common.Extensions;
using Sportradar.OddsFeed.SDK.Messages.Rest;

namespace Sportradar.OddsFeed.SDK.Tests.Common.Dsl.Api.Markets;

public class MarketDescriptionBuilder
{
    private readonly desc_market _marketDescription = new desc_market();
    private string _nameSuffix = string.Empty;

    public static MarketDescriptionBuilder Create()
    {
        return new MarketDescriptionBuilder();
    }

    public static MarketDescriptionBuilder Create(int id, string name)
    {
        return new MarketDescriptionBuilder().WithId(id).WithName(name);
    }

    public MarketDescriptionBuilder WithId(int id)
    {
        _marketDescription.id = id;
        return this;
    }

    public MarketDescriptionBuilder WithName(string name)
    {
        _marketDescription.name = name;
        return this;
    }

    public MarketDescriptionBuilder WithSuffix(string suffix)
    {
        _nameSuffix = suffix;
        return this;
    }

    public MarketDescriptionBuilder AddOutcome(desc_outcomesOutcome outcome)
    {
        _marketDescription.outcomes = _marketDescription.outcomes.IsNullOrEmpty()
            ? new[] { outcome }
            : _marketDescription.outcomes.Append(outcome).ToArray();
        return this;
    }

    public MarketDescriptionBuilder AddOutcome(Action<OutcomeDescriptionBuilder> outcomeBuilder)
    {
        var builder = OutcomeDescriptionBuilder.Create();
        outcomeBuilder(builder);
        return AddOutcome(builder.BuildInvariant());
    }

    public MarketDescriptionBuilder WithOutcomes(ICollection<desc_outcomesOutcome> outcomes)
    {
        _marketDescription.outcomes = outcomes.ToArray();
        return this;
    }

    public MarketDescriptionBuilder AddSpecifier(string name, string type, string description = null)
    {
        _marketDescription.specifiers = _marketDescription.specifiers.IsNullOrEmpty()
            ? new[] { new desc_specifiersSpecifier { name = name, type = type, description = description } }
            : _marketDescription.specifiers.Append(new desc_specifiersSpecifier { name = name, type = type, description = description }).ToArray();
        return this;
    }

    public MarketDescriptionBuilder WithGroups(string groups)
    {
        _marketDescription.groups = groups;
        return this;
    }

    public MarketDescriptionBuilder WithVariant(string variant)
    {
        _marketDescription.variant = variant;
        return this;
    }

    public MarketDescriptionBuilder WithIncludesOutcomesOfType(string includesOutcomesOfType)
    {
        _marketDescription.includes_outcomes_of_type = includesOutcomesOfType;
        return this;
    }

    public MarketDescriptionBuilder WithOutcomeType(string outcomesType)
    {
        _marketDescription.outcome_type = outcomesType;
        return this;
    }

    public MarketDescriptionBuilder AddAttribute(string name, string description = null)
    {
        _marketDescription.attributes = _marketDescription.attributes.IsNullOrEmpty()
            ? new[] { new attributesAttribute { name = name, description = description } }
            : _marketDescription.attributes.Append(new attributesAttribute { name = name, description = description }).ToArray();
        return this;
    }

    public MarketDescriptionBuilder AddMapping(Action<MappingBuilder> mappingBuilder)
    {
        var builder = MappingBuilder.Create();
        mappingBuilder(builder);
        _marketDescription.mappings = _marketDescription.mappings.IsNullOrEmpty()
            ? new[] { builder.BuildInvariant() }
            : _marketDescription.mappings.Append(builder.BuildInvariant()).ToArray();
        return this;
    }

    public desc_market Build()
    {
        if (!_nameSuffix.IsNullOrEmpty())
        {
            _marketDescription.AddSuffix(_nameSuffix);
        }
        return _marketDescription;
    }
}
