/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System.Diagnostics.CodeAnalysis;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching.CI;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.DTO;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.EntitiesImpl;
using Sportradar.OddsFeed.SDK.Messages.Internal;
using SR = Sportradar.OddsFeed.SDK.Test.Shared.StaticRandom;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Test
{
    [TestClass]
    public class SeasonCoverageTest
    {

        [TestMethod]
        [SuppressMessage("ReSharper", "RedundantAssignment")]
        public void OptionalValuesCanBeNull()
        {

            var record = RestMessageBuilder.BuildCoverageRecord(null, null, null, int.MinValue, 1, "sr:season:1");
            var item = new SeasonCoverage(new SeasonCoverageCI(new SeasonCoverageDTO(record)));

            record = RestMessageBuilder.BuildCoverageRecord(string.Empty, string.Empty, null, int.MaxValue, 1, "sr:season:1");
            item = new SeasonCoverage(new SeasonCoverageCI(new SeasonCoverageDTO(record)));

            record = RestMessageBuilder.BuildCoverageRecord("test", "test", 0, 0, 1, "sr:season:1");
            item = new SeasonCoverage(new SeasonCoverageCI(new SeasonCoverageDTO(record)));

            record = RestMessageBuilder.BuildCoverageRecord("test", "test", 100, 100, 1, SR.Urn("season").ToString());
            item = new SeasonCoverage(new SeasonCoverageCI(new SeasonCoverageDTO(record)));

            record = RestMessageBuilder.BuildCoverageRecord("test", "test", null, 100, 1, SR.Urn("season").ToString());
            item = new SeasonCoverage(new SeasonCoverageCI(new SeasonCoverageDTO(record)));
            Assert.IsNotNull(item);
            Assert.AreEqual("test", item.MaxCoverageLevel);
            Assert.AreEqual("test", item.MinCoverageLevel);
            Assert.IsNull(item.MaxCovered);
            Assert.AreEqual(100, item.Played);
        }
    }
}
