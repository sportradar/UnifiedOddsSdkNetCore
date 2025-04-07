// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

namespace Sportradar.OddsFeed.SDK.Tests.Api;

public interface IRequestHeaderInspector
{
    void VerifyRequestHeader(string requestHeaderValue);
}
