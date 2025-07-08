// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Sportradar.OddsFeed.SDK.Tests.Common.Extensions;

public static class CultureInfoExtensions
{
    public static bool AreEquivalentTo(this IReadOnlyCollection<CultureInfo> actual, CultureInfo[] expected)
    {
        if (actual == null || expected == null)
        {
            return false;
        }

        if (actual.Count != expected.Length)
        {
            return false;
        }

        var areEquivalentTo = !actual.Except(expected, CultureInfoComparer.Instance).Any();
        return areEquivalentTo;
    }

    private class CultureInfoComparer : IEqualityComparer<CultureInfo>
    {
        public static readonly CultureInfoComparer Instance = new();

        public bool Equals(CultureInfo x, CultureInfo y)
        {
            return string.Equals(x?.Name, y?.Name, StringComparison.OrdinalIgnoreCase);
        }

        public int GetHashCode(CultureInfo obj)
        {
            return obj.Name.ToLowerInvariant().GetHashCode();
        }
    }
}
