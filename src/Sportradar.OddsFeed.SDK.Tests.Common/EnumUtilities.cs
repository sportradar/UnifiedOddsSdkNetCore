// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using System.Linq;
using Sportradar.OddsFeed.SDK.Common.Extensions;

namespace Sportradar.OddsFeed.SDK.Tests.Common;

internal static class EnumUtilities
{
    public static T GetRandomCacheItemTypeExcludingSpecified<T>(params T[] excludeItems) where T : struct, IConvertible
    {
        if (!typeof(T).IsEnum)
        {
            throw new ArgumentException("T must be an enumerated type");
        }

        var availableValues = Enum.GetValues(typeof(T)).Cast<T>().ToList();
        if (!excludeItems.IsNullOrEmpty())
        {
            _ = availableValues.RemoveAll(excludeItems.Contains);
        }

        return availableValues[Random.Shared.Next(0, availableValues.Count)];
    }
}
