// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using FluentAssertions;
using Sportradar.OddsFeed.SDK.Entities.Enums;
using Sportradar.OddsFeed.SDK.Entities.Rest.Enums;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal;
using Xunit;

namespace Sportradar.OddsFeed.SDK.Tests.Entities;

public class MessageMapperBaseTests
{
    [Fact]
    public void VoidFactorsAreConverted()
    {
        var voidFactor = MessageMapperHelper.GetVoidFactor(true, 0);
        Assert.Equal(VoidFactor.Zero, voidFactor);

        voidFactor = MessageMapperHelper.GetVoidFactor(true, 0.5);
        Assert.Equal(VoidFactor.Half, voidFactor);

        voidFactor = MessageMapperHelper.GetVoidFactor(true, 1);
        Assert.Equal(VoidFactor.One, voidFactor);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(0.3)]
    [InlineData(0.5)]
    [InlineData(0.7)]
    [InlineData(1)]
    public void VoidFactorsNotSpecifiedAllAreConvertedToZero(double factor)
    {
        var voidFactor = MessageMapperHelper.GetVoidFactor(false, factor);
        Assert.Equal(VoidFactor.Zero, voidFactor);
    }

    [Fact]
    public void IncorrectVoidFactorsThrow()
    {
        Action action = () => MessageMapperHelper.GetVoidFactor(true, 0.7);
        action.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void IncorrectVoidFactorsDoNotThrowWhenNotSpecified()
    {
        var voidFactor = MessageMapperHelper.GetVoidFactor(false, 0.7);
        Assert.Equal(VoidFactor.Zero, voidFactor);
    }

    [Fact]
    public void CorrectEnumValuesForMarketStatusAreResolved()
    {
        var active = MessageMapperHelper.GetEnumValue<MarketStatus>(1);
        Assert.Equal(MarketStatus.Active, active);

        var suspended = MessageMapperHelper.GetEnumValue<MarketStatus>(-1);
        Assert.Equal(MarketStatus.Suspended, suspended);

        var deactivated = MessageMapperHelper.GetEnumValue<MarketStatus>(0);
        Assert.Equal(MarketStatus.Inactive, deactivated);
    }

    [Fact]
    public void DefaultValueIsReturned()
    {
        var active = MessageMapperHelper.GetEnumValue(false, 99, MarketStatus.Active);
        Assert.Equal(MarketStatus.Active, active);
    }

    [Fact]
    public void IncorrectMarketStatusesThrow()
    {
        Action action = () => MessageMapperHelper.GetEnumValue<MarketStatus>(2);
        action.Should().Throw<ArgumentException>();
    }
}
