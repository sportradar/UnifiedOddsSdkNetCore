// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using Sportradar.OddsFeed.SDK.Api.Managers;

namespace Sportradar.OddsFeed.SDK.Tests.Common.TestUnits.EntityUnit;

public class SportEventUnit(ISportDataProvider sportDataProvider)
{
    public ISportDataProvider SportDataProvider { get; } = sportDataProvider;
}
