/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/

using Sportradar.OddsFeed.SDK.Entities.REST.Internal;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.DTO;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.Mapping;
using Sportradar.OddsFeed.SDK.Messages.REST;
using Sportradar.OddsFeed.SDK.Tests.Common;
using Xunit;

namespace Sportradar.OddsFeed.SDK.Tests.Entities.REST
{
    public class TournamentDetailsMapperTests
    {
        private const string InputXml = "tournament_info.xml";

        [Fact]
        public void TestInstanceIsNotNull()
        {
            var deserializer = new Deserializer<tournamentInfoEndpoint>();
            var dataFetcher = new TestDataFetcher();
            var mapperFactory = new TournamentInfoMapperFactory();

            var dataProvider = new DataProvider<tournamentInfoEndpoint, TournamentInfoDTO>(
                TestData.RestXmlPath + InputXml,
                dataFetcher,
                deserializer,
                mapperFactory);

            var entity = dataProvider.GetDataAsync("", "en").GetAwaiter().GetResult();

            Assert.NotNull(entity);
        }
    }
}
