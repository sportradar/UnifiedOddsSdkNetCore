// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Sportradar.OddsFeed.SDK.Api.Config;
using Sportradar.OddsFeed.SDK.Entities.Internal;
using Xunit;

namespace Sportradar.OddsFeed.SDK.Tests.Entities;

public class RoutingKeyBuilderTests
{
    [Fact]
    public void NoMessageInterest()
    {
        Action action = () => FeedRoutingKeyBuilder.GenerateKeys(new List<MessageInterest>());
        action.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void SingleMessageInterest()
    {
        var interests = new List<MessageInterest> { MessageInterest.AllMessages };
        var keys = FeedRoutingKeyBuilder.GenerateKeys(interests);
        Assert.NotNull(keys);
        Assert.True(keys.Any());

        interests = new List<MessageInterest> { MessageInterest.PrematchMessagesOnly };
        keys = FeedRoutingKeyBuilder.GenerateKeys(interests);
        Assert.NotNull(keys);
        Assert.True(keys.Any());

        interests = new List<MessageInterest> { MessageInterest.LiveMessagesOnly };
        keys = FeedRoutingKeyBuilder.GenerateKeys(interests);
        Assert.NotNull(keys);
        Assert.True(keys.Any());

        interests = new List<MessageInterest> { MessageInterest.HighPriorityMessages };
        keys = FeedRoutingKeyBuilder.GenerateKeys(interests);
        Assert.NotNull(keys);
        Assert.True(keys.Any());

        interests = new List<MessageInterest> { MessageInterest.LowPriorityMessages };
        keys = FeedRoutingKeyBuilder.GenerateKeys(interests);
        Assert.NotNull(keys);
        Assert.True(keys.Any());

        interests = new List<MessageInterest> { MessageInterest.VirtualSportMessages };
        keys = FeedRoutingKeyBuilder.GenerateKeys(interests);
        Assert.NotNull(keys);
        Assert.True(keys.Any());
    }

    [Fact]
    public void MessageInterestCombinations()
    {
        var interests = new List<MessageInterest> { MessageInterest.PrematchMessagesOnly, MessageInterest.LiveMessagesOnly };
        var keys = FeedRoutingKeyBuilder.GenerateKeys(interests);
        Assert.NotNull(keys);
        Assert.True(keys.Any());

        interests = new List<MessageInterest> { MessageInterest.HighPriorityMessages, MessageInterest.LowPriorityMessages };
        keys = FeedRoutingKeyBuilder.GenerateKeys(interests);
        Assert.NotNull(keys);
        Assert.True(keys.Any());

        interests = new List<MessageInterest> { MessageInterest.PrematchMessagesOnly, MessageInterest.LiveMessagesOnly, MessageInterest.VirtualSportMessages };
        keys = FeedRoutingKeyBuilder.GenerateKeys(interests);
        Assert.NotNull(keys);
        Assert.True(keys.Any());

        interests = new List<MessageInterest> { MessageInterest.PrematchMessagesOnly, MessageInterest.VirtualSportMessages };
        keys = FeedRoutingKeyBuilder.GenerateKeys(interests);
        Assert.NotNull(keys);
        Assert.True(keys.Any());

        interests = new List<MessageInterest> { MessageInterest.LiveMessagesOnly, MessageInterest.VirtualSportMessages };
        keys = FeedRoutingKeyBuilder.GenerateKeys(interests);
        Assert.NotNull(keys);
        Assert.True(keys.Any());
    }
}
