/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sportradar.OddsFeed.SDK.Entities.REST.Enums;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal;

namespace Sportradar.OddsFeed.SDK.Entities.Test
{
    [TestClass]
    public class MessageMapperBaseTest
    {
        [TestMethod]
        public void VoidFactorsAreConverted()
        {
            var voidFactor = MessageMapperHelper.GetVoidFactor(true, 0);
            Assert.AreEqual(voidFactor, VoidFactor.Zero, "The value of voidFactor must be VoidFactor.ZERO");

            voidFactor = MessageMapperHelper.GetVoidFactor(true, 0.5);
            Assert.AreEqual(voidFactor, VoidFactor.Half, "The value of voidFactor must be VoidFactor.Half");

            voidFactor = MessageMapperHelper.GetVoidFactor(true, 1);
            Assert.AreEqual(voidFactor, VoidFactor.One, "The value of voidFactor must be VoidFactor.ONE");

            voidFactor = MessageMapperHelper.GetVoidFactor(false, 0);
            Assert.AreEqual(voidFactor, VoidFactor.Zero, "The value of voidFactor must be VoidFactor.ZERO");

            voidFactor = MessageMapperHelper.GetVoidFactor(false, 0.5);
            Assert.AreEqual(voidFactor, VoidFactor.Zero, "The value of voidFactor must be VoidFactor.ZERO");

            voidFactor = MessageMapperHelper.GetVoidFactor(false, 1);
            Assert.AreEqual(voidFactor, VoidFactor.Zero, "The value of voidFactor must be VoidFactor.ZERO");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void IncorrectVoidFactorsThrow()
        {
            MessageMapperHelper.GetVoidFactor(true, 0.7);
        }

        [TestMethod]
        public void IncorrectVoidFactorsDoNotThrowWhenNotSpecified()
        {
            var voidFactor = MessageMapperHelper.GetVoidFactor(false, 0.7);
            Assert.AreEqual(voidFactor, VoidFactor.Zero, "The value of voidFactor must be VoidFactor.ZERO");
        }

        [TestMethod]
        public void CorrectEnumValuesAreResolved()
        {
            var active = MessageMapperHelper.GetEnumValue<MarketStatus>(1);
            Assert.AreEqual(active, MarketStatus.ACTIVE, "Value of active must be MarketStatus.ACTIVE");

            var suspended = MessageMapperHelper.GetEnumValue<MarketStatus>(-1);
            Assert.AreEqual(suspended, MarketStatus.SUSPENDED, "Value of active must be MarketStatus.SUSPENDED");

            var deactivated = MessageMapperHelper.GetEnumValue<MarketStatus>(0);
            Assert.AreEqual(deactivated, MarketStatus.INACTIVE, "Value of active must be MarketStatus.DEACTIVATED");
        }

        [TestMethod]
        public void DefaultValueIsReturned()
        {
            var active = MessageMapperHelper.GetEnumValue<MarketStatus>(false, 99, MarketStatus.ACTIVE);
            Assert.AreEqual(active, MarketStatus.ACTIVE, "Value of active must be MarketStatus.ACTIVE");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void IncorrectMarketStatusesThrow()
        {
            MessageMapperHelper.GetEnumValue<MarketStatus>(2);
        }
    }
}
