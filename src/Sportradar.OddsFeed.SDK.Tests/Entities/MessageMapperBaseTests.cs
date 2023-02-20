/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/

using System;
using FluentAssertions;
using Sportradar.OddsFeed.SDK.Entities;
using Sportradar.OddsFeed.SDK.Entities.REST.Enums;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal;
using Xunit;

namespace Sportradar.OddsFeed.SDK.Tests.Entities
{
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

            voidFactor = MessageMapperHelper.GetVoidFactor(false, 0);
            Assert.Equal(VoidFactor.Zero, voidFactor);

            voidFactor = MessageMapperHelper.GetVoidFactor(false, 0.5);
            Assert.Equal(VoidFactor.Zero, voidFactor);

            voidFactor = MessageMapperHelper.GetVoidFactor(false, 1);
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
        public void CorrectEnumValuesAreResolved()
        {
            var active = MessageMapperHelper.GetEnumValue<MarketStatus>(1);
            Assert.Equal(MarketStatus.ACTIVE, active);

            var suspended = MessageMapperHelper.GetEnumValue<MarketStatus>(-1);
            Assert.Equal(MarketStatus.SUSPENDED, suspended);

            var deactivated = MessageMapperHelper.GetEnumValue<MarketStatus>(0);
            Assert.Equal(MarketStatus.INACTIVE, deactivated);
        }

        [Fact]
        public void DefaultValueIsReturned()
        {
            var active = MessageMapperHelper.GetEnumValue(false, 99, MarketStatus.ACTIVE);
            Assert.Equal(MarketStatus.ACTIVE, active);
        }

        [Fact]
        public void IncorrectMarketStatusesThrow()
        {
            Action action = () => MessageMapperHelper.GetEnumValue<MarketStatus>(2);
            action.Should().Throw<ArgumentException>();
        }
    }
}
