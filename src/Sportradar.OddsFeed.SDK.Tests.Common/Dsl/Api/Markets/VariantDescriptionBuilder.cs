// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using System.Collections.Generic;
using System.Linq;
using Sportradar.OddsFeed.SDK.Common.Extensions;
using Sportradar.OddsFeed.SDK.Messages.Rest;

namespace Sportradar.OddsFeed.SDK.Tests.Common.Dsl.Api.Markets;

public class VariantDescriptionBuilder
{
    private readonly desc_variant _marketDescription = new desc_variant();
    private string _nameSuffix = string.Empty;

    public static VariantDescriptionBuilder Create()
    {
        return new VariantDescriptionBuilder();
    }

    public static VariantDescriptionBuilder Create(string id)
    {
        return new VariantDescriptionBuilder().WithId(id);
    }

    public VariantDescriptionBuilder WithId(string id)
    {
        _marketDescription.id = id;
        return this;
    }

    public VariantDescriptionBuilder WithSuffix(string suffix)
    {
        _nameSuffix = suffix;
        return this;
    }

    public VariantDescriptionBuilder AddOutcome(desc_variant_outcomesOutcome outcome)
    {
        _marketDescription.outcomes =
            _marketDescription.outcomes.IsNullOrEmpty()
                ? new[] { outcome }
                : _marketDescription.outcomes.Append(outcome).ToArray();
        return this;
    }

    public VariantDescriptionBuilder AddOutcome(Action<OutcomeDescriptionBuilder> outcomeBuilder)
    {
        var builder = OutcomeDescriptionBuilder.Create();
        outcomeBuilder(builder);
        return AddOutcome(builder.BuildVariant());
    }

    public VariantDescriptionBuilder WithOutcomes(ICollection<desc_variant_outcomesOutcome> outcomes)
    {
        _marketDescription.outcomes = outcomes.ToArray();
        return this;
    }

    public VariantDescriptionBuilder AddMapping(Action<MappingBuilder> mappingBuilder)
    {
        var builder = MappingBuilder.Create();
        mappingBuilder(builder);
        _marketDescription.mappings =
            _marketDescription.mappings.IsNullOrEmpty()
                ? new[] { builder.BuildVariant() }
                : _marketDescription.mappings.Append(builder.BuildVariant()).ToArray();
        return this;
    }

    public desc_variant Build()
    {
        if (!_nameSuffix.IsNullOrEmpty())
        {
            _marketDescription.AddSuffix(_nameSuffix);
        }
        return _marketDescription;
    }
}
