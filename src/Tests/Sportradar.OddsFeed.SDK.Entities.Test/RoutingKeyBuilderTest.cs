/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sportradar.OddsFeed.SDK.Entities.Internal;

namespace Sportradar.OddsFeed.SDK.Entities.Test
{
    [TestClass]
    public class RoutingKeyBuilderTest
    {
        [TestMethod]
        [ExpectedException(typeof(Exception), AllowDerivedTypes = true)]
        public void NoMessageInterestTest()
        {
            var keys = FeedRoutingKeyBuilder.GenerateKeys(new List<MessageInterest>());
            Assert.IsNotNull(keys);
            Assert.IsTrue(keys.Any());
        }

        [TestMethod]
        public void SingleMessageInterestTest()
        {
            var interests = new List<MessageInterest> { MessageInterest.AllMessages };
            var keys = FeedRoutingKeyBuilder.GenerateKeys(interests);
            Assert.IsNotNull(keys);
            Assert.IsTrue(keys.Any());

            interests = new List<MessageInterest> { MessageInterest.PrematchMessagesOnly };
            keys = FeedRoutingKeyBuilder.GenerateKeys(interests);
            Assert.IsNotNull(keys);
            Assert.IsTrue(keys.Any());

            interests = new List<MessageInterest> { MessageInterest.LiveMessagesOnly };
            keys = FeedRoutingKeyBuilder.GenerateKeys(interests);
            Assert.IsNotNull(keys);
            Assert.IsTrue(keys.Any());

            interests = new List<MessageInterest> { MessageInterest.HighPriorityMessages };
            keys = FeedRoutingKeyBuilder.GenerateKeys(interests);
            Assert.IsNotNull(keys);
            Assert.IsTrue(keys.Any());

            interests = new List<MessageInterest> { MessageInterest.LowPriorityMessages };
            keys = FeedRoutingKeyBuilder.GenerateKeys(interests);
            Assert.IsNotNull(keys);
            Assert.IsTrue(keys.Any());

            interests = new List<MessageInterest> { MessageInterest.VirtualSportMessages };
            keys = FeedRoutingKeyBuilder.GenerateKeys(interests);
            Assert.IsNotNull(keys);
            Assert.IsTrue(keys.Any());
        }

        [TestMethod]
        public void MessageInterestCombinations()
        {
            var interests = new List<MessageInterest> { MessageInterest.PrematchMessagesOnly, MessageInterest.LiveMessagesOnly };
            var keys = FeedRoutingKeyBuilder.GenerateKeys(interests);
            Assert.IsNotNull(keys);
            Assert.IsTrue(keys.Any());

            interests = new List<MessageInterest> { MessageInterest.HighPriorityMessages, MessageInterest.LowPriorityMessages };
            keys = FeedRoutingKeyBuilder.GenerateKeys(interests);
            Assert.IsNotNull(keys);
            Assert.IsTrue(keys.Any());

            interests = new List<MessageInterest> { MessageInterest.PrematchMessagesOnly, MessageInterest.LiveMessagesOnly, MessageInterest.VirtualSportMessages };
            keys = FeedRoutingKeyBuilder.GenerateKeys(interests);
            Assert.IsNotNull(keys);
            Assert.IsTrue(keys.Any());

            interests = new List<MessageInterest> { MessageInterest.PrematchMessagesOnly, MessageInterest.VirtualSportMessages };
            keys = FeedRoutingKeyBuilder.GenerateKeys(interests);
            Assert.IsNotNull(keys);
            Assert.IsTrue(keys.Any());

            interests = new List<MessageInterest> { MessageInterest.LiveMessagesOnly, MessageInterest.VirtualSportMessages };
            keys = FeedRoutingKeyBuilder.GenerateKeys(interests);
            Assert.IsNotNull(keys);
            Assert.IsTrue(keys.Any());
        }
    }
}
