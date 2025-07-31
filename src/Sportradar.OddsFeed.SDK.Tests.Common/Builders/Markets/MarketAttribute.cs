// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using Sportradar.OddsFeed.SDK.Entities.Rest.Market;
using Sportradar.OddsFeed.SDK.Messages.Rest;

namespace Sportradar.OddsFeed.SDK.Tests.Common.Builders.Markets;

internal class MarketAttribute : IMarketAttribute
{
    public MarketAttribute(string name, string description)
    {
        Name = name;
        Description = description;
    }

    public MarketAttribute(attributesAttribute attribute)
        : this(attribute.name, attribute.description)
    {
    }

    public string Name
    {
        get;
    }

    public string Description
    {
        get;
    }
}
