/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/

using System.Diagnostics.CodeAnalysis;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching.CI;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.DTO;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.EntitiesImpl;
using Sportradar.OddsFeed.SDK.Messages.Internal;
using Xunit;
using SR = Sportradar.OddsFeed.SDK.Tests.Common.StaticRandom;

namespace Sportradar.OddsFeed.SDK.Tests.Entities.REST
{
    public class SeasonCoverageTests
    {
        [Fact]
        [SuppressMessage("ReSharper", "RedundantAssignment")]
        public void OptionalValuesCanBeNull()
        {
            var record = RestMessageBuilder.BuildCoverageRecord(null, null, null, int.MinValue, 1, "sr:season:1");
            _ = new SeasonCoverage(new SeasonCoverageCI(new SeasonCoverageDTO(record)));

            record = RestMessageBuilder.BuildCoverageRecord(string.Empty, string.Empty, null, int.MaxValue, 1, "sr:season:1");
            _ = new SeasonCoverage(new SeasonCoverageCI(new SeasonCoverageDTO(record)));

            record = RestMessageBuilder.BuildCoverageRecord("test", "test", 0, 0, 1, "sr:season:1");
            _ = new SeasonCoverage(new SeasonCoverageCI(new SeasonCoverageDTO(record)));

            record = RestMessageBuilder.BuildCoverageRecord("test", "test", 100, 100, 1, SR.Urn("season").ToString());
            _ = new SeasonCoverage(new SeasonCoverageCI(new SeasonCoverageDTO(record)));

            record = RestMessageBuilder.BuildCoverageRecord("test", "test", null, 100, 1, SR.Urn("season").ToString());
            var item = new SeasonCoverage(new SeasonCoverageCI(new SeasonCoverageDTO(record)));

            Assert.NotNull(item);
            Assert.Equal("test", item.MaxCoverageLevel);
            Assert.Equal("test", item.MinCoverageLevel);
            Assert.Null(item.MaxCovered);
            Assert.Equal(100, item.Played);
        }
    }
}
