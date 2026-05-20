// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using Sportradar.OddsFeed.SDK.Api.Managers;
using Sportradar.OddsFeed.SDK.Entities.Rest.CustomBet;

namespace Sportradar.OddsFeed.SDK.Tests.Entities.Rest.CustomBet;

internal class FakeCalculateRequestBuilder : ICalculateRequestBuilder
{
    public ICalculateRequestBuilder AndSelection(ISelection selection)
    {
        return this;
    }

    public ICalculateRequestBuilder AndAnyOfSelections(params ISelection[] selections)
    {
        return this;
    }
}
