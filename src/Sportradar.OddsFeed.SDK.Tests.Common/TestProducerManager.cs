// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using Sportradar.OddsFeed.SDK.Api.Config;
using Sportradar.OddsFeed.SDK.Api.Internal.Managers;
using Sportradar.OddsFeed.SDK.Api.Managers;

namespace Sportradar.OddsFeed.SDK.Tests.Common;

internal class TestProducerManager(IUofConfiguration config) : ProducerManager(config)
{
    public static IProducerManager Create()
    {
        return new TestProducerManager(TestConfiguration.GetConfig());
    }

    public static IProducerManager Create(IUofConfiguration config)
    {
        return new TestProducerManager(config);
    }
}
