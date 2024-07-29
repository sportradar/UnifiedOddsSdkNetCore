// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;

namespace Sportradar.OddsFeed.SDK.Tests.Common;

public static class UrlUtils
{
    public static string ExtractDomainName(string url)
    {
        if (string.IsNullOrEmpty(url))
        {
            throw new ArgumentException("URL cannot be null or empty.", nameof(url));
        }

        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri) || (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
        {
            throw new ArgumentException("Invalid or unsupported URL format.", nameof(url));
        }

        return $"{uri.Host}:{uri.Port}";
    }

    public static string SanitizeHtml(string originalHtml)
    {
        return originalHtml.Replace("\n", string.Empty)
            .Replace("\r", string.Empty)
            .Replace("\t", string.Empty);
    }
}
