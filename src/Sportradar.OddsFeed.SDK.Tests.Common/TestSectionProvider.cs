// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using Sportradar.OddsFeed.SDK.Api.Internal.Config;

namespace Sportradar.OddsFeed.SDK.Tests.Common;

internal class TestSectionProvider : IUofConfigurationSectionProvider
{
    private readonly IUofConfigurationSection _section;

    public TestSectionProvider(IUofConfigurationSection section)
    {
        _section = section;
    }

    public IUofConfigurationSection GetSection()
    {
        return _section;
    }
}
