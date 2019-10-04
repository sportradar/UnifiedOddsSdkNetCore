/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Sportradar.OddsFeed.SDK.Messages.Test
{
    [TestClass]
    public class URNTests
    {
        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void MissingPrefixIsNotAllowed()
        {
            var urn = "match:1234";
            URN.Parse(urn);
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void MissingTypeIsNotAllowed()
        {
            var urn = "sr:1234";
            URN.Parse(urn);
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void NumberInTypeIsNotAllowed()
        {
            var urn = "sr:match1:12333";
            URN.Parse(urn);
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void MinusInTypeIsNotAllowed()
        {
            var urn = "sr:ma-tch:1232";
            URN.Parse(urn);
        }

        [TestMethod]
        public void UnsupportedTypeIsNotAllowed()
        {
            var urn = URN.Parse("sr:event_tournament:1232");
            Assert.AreEqual(ResourceTypeGroup.UNKNOWN, urn.TypeGroup);
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void MissingIdIsNotAllowed()
        {
            var urn = "sr:match";
            URN.Parse(urn);
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void LetterInIdIsNotAllowed()
        {
            var urn = "sr:match:12a34";
            URN.Parse(urn);
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void UnderscoreInIdIsNotAllowed()
        {
            var urn = "sr:match:123_4";
            URN.Parse(urn);
        }

        [TestMethod]
        public void SportEventResourceIsSupported()
        {
            var urnString = "sr:sport_event:1234";
            var urn = URN.Parse(urnString);
            Assert.IsNotNull(urn, "urn should not be null");
            Assert.AreEqual(ResourceTypeGroup.MATCH, urn.TypeGroup, "Value of TypeGroup is not correct");
        }

        [TestMethod]
        public void VfPrefixIsAllowed()
        {
            var urnString = "vf:match:1234";
            var urn = URN.Parse(urnString);
            Assert.IsNotNull(urn, "urn should not be null");
        }

        [TestMethod]
        public void RaceEventResourceIsSupported()
        {
            var urnString = "sr:race_event:1234";
            var urn = URN.Parse(urnString);
            Assert.IsNotNull(urn, "urn should not be null");
            Assert.AreEqual(ResourceTypeGroup.STAGE, urn.TypeGroup, "Value of TypeGroup is not correct");
        }

        [TestMethod]
        public void SeasonResourceIsSupported()
        {
            var urnString = "sr:season:1234";
            var urn = URN.Parse(urnString);
            Assert.IsNotNull(urn, "urn should not be null");
            Assert.AreEqual(ResourceTypeGroup.SEASON, urn.TypeGroup, "Value of TypeGroup is not correct");
        }

        [TestMethod]
        public void TournamentResourceIsSupported()
        {
            var urnString = "sr:tournament:1234";
            var urn = URN.Parse(urnString);
            Assert.IsNotNull(urn, "urn should not be null");
            Assert.AreEqual(ResourceTypeGroup.TOURNAMENT, urn.TypeGroup, "Value of TypeGroup is not correct");
        }

        [TestMethod]
        public void SimpleTournamentResourceIsSupported()
        {
            var urnString = "sr:simple_tournament:1234";
            var urn = URN.Parse(urnString);
            Assert.IsNotNull(urn, "urn should not be null");
            Assert.AreEqual(ResourceTypeGroup.BASIC_TOURNAMENT, urn.TypeGroup, "Value of TypeGroup is not correct");
        }

        [TestMethod]
        public void RaceTournamentResourceIsSupported()
        {
            var urnString = "sr:race_tournament:1234";
            var urn = URN.Parse(urnString);
            Assert.IsNotNull(urn, "urn should not be null");
            Assert.AreEqual(ResourceTypeGroup.STAGE, urn.TypeGroup, "Value of TypeGroup is not correct");
        }

        [TestMethod]
        public void ParsedURNHasCorrectValues()
        {
            var urnString = "sr:sport_event:1234";
            var urn = URN.Parse(urnString);

            Assert.IsNotNull(urn, "urn cannot be a null reference");
            Assert.AreEqual("sr", urn.Prefix, "Value of the prefix is not correct");
            Assert.AreEqual("sport_event", urn.Type, "Value of type is not correct");
            Assert.AreEqual(ResourceTypeGroup.MATCH, urn.TypeGroup, "Value of the typeGroup ils not correct");
            Assert.AreEqual(1234, urn.Id, "Value of the Id is not correct");
        }
    }
}
