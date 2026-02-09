// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

namespace Sportradar.OddsFeed.SDK.Tests.Common.Helpers;

internal static class CommonIAmRequestBodyHelper
{
    internal static string ExtractClientAssertionFrom(string commonIAmBody)
    {
        var parsed = System.Web.HttpUtility.ParseQueryString(commonIAmBody);
        return parsed["client_assertion"];
    }
}
