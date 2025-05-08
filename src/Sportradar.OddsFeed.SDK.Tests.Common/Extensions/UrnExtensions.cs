// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using Sportradar.OddsFeed.SDK.Common;

namespace Sportradar.OddsFeed.SDK.Tests.Common.Extensions;

public static class UrnExtensions
{
    public static Urn ToUrn(this string urn)
    {
        return Urn.Parse(urn);
    }
}
