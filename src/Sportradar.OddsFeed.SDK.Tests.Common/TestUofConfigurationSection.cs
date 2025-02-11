// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using Sportradar.OddsFeed.SDK.Api.Internal.Config;

namespace Sportradar.OddsFeed.SDK.Tests.Common;

internal class TestUofConfigurationSection : UofConfigurationSection
{
    public void SetSectionValue(string sectionName, object value)
    {
        base[sectionName] = value;
    }
}
