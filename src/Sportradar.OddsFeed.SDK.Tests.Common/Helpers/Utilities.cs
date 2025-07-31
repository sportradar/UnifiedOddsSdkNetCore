// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System.Collections.Generic;
using System.Collections.ObjectModel;
using Sportradar.OddsFeed.SDK.Common.Internal;

namespace Sportradar.OddsFeed.SDK.Tests.Common.Helpers;

public static class Utilities
{
    /// <summary>
    /// List of specifiers as string
    /// </summary>
    /// <param name="specifiers">The specifiers.</param>
    /// <returns>Dictionary of specifiers</returns>
    public static IReadOnlyDictionary<string, string> SpecifiersStringToReadOnlyDictionary(string specifiers)
    {
        return new ReadOnlyDictionary<string, string>(SdkInfo.SpecifiersStringToDictionary(specifiers));
    }
}
