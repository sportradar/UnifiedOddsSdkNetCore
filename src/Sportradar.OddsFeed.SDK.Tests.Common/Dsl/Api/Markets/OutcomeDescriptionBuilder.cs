// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using Sportradar.OddsFeed.SDK.Messages.Rest;

namespace Sportradar.OddsFeed.SDK.Tests.Common.Dsl.Api.Markets;

public class OutcomeDescriptionBuilder
{
    private string _id;
    private string _name;
    private string _description;

    public static OutcomeDescriptionBuilder Create()
    {
        return new OutcomeDescriptionBuilder();
    }

    public OutcomeDescriptionBuilder WithId(string id)
    {
        _id = id;
        return this;
    }

    public OutcomeDescriptionBuilder WithName(string name)
    {
        _name = name;
        return this;
    }

    public OutcomeDescriptionBuilder WithDescription(string description)
    {
        _description = description;
        return this;
    }

    public desc_outcomesOutcome BuildInvariant()
    {
        return new desc_outcomesOutcome
        {
            id = _id,
            name = _name,
            description = _description
        };
    }

    public desc_variant_outcomesOutcome BuildVariant()
    {
        return new desc_variant_outcomesOutcome
        {
            id = _id,
            name = _name
        };
    }
}
