// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using Sportradar.OddsFeed.SDK.Entities.Rest.Market;
using Sportradar.OddsFeed.SDK.Messages.Rest;

namespace Sportradar.OddsFeed.SDK.Tests.Common.Builders.Markets;

internal class Specifier : ISpecifier
{
    public string Type
    {
        get;
    }

    public string Name
    {
        get;
    }

    public Specifier(desc_specifiersSpecifier specifier)
        : this(specifier.name, specifier.type)
    {
    }

    public Specifier(string name, string type)
    {
        Name = name;
        Type = type;
    }
}
