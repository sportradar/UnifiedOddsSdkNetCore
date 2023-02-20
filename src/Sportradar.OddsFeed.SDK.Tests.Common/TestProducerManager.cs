/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/

using Sportradar.OddsFeed.SDK.API;
using Sportradar.OddsFeed.SDK.API.Internal;
using Sportradar.OddsFeed.SDK.Messages;

namespace Sportradar.OddsFeed.SDK.Tests.Common
{
    internal class TestProducerManager : ProducerManager
    {
        public TestProducerManager(IProducersProvider producersProvider, IOddsFeedConfiguration config)
            : base(producersProvider, config)
        {
        }

        public static IProducerManager Create()
        {
            return new TestProducerManager(new TestProducersProvider(), TestConfigurationInternal.GetConfig());
        }
    }
}
