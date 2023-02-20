/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/

using System.Linq;
using System.Threading.Tasks;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.DTO;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.Mapping;
using Sportradar.OddsFeed.SDK.Messages.REST;
using Sportradar.OddsFeed.SDK.Tests.Common;
using Xunit;

namespace Sportradar.OddsFeed.SDK.Tests.Entities.REST.Markets
{
    public class MarketDescriptionListMapperTests
    {
        private const string InputXml = "invariant_market_descriptions_en.xml";

        [Fact]
        public async Task TestInstanceIsNotNull()
        {
            var deserializer = new Deserializer<market_descriptions>();
            var dataFetcher = new TestDataFetcher();
            var mapperFactory = new MarketDescriptionsMapperFactory();

            var dataProvider = new DataProvider<market_descriptions, EntityList<MarketDescriptionDTO>>(
                TestData.RestXmlPath + InputXml,
                dataFetcher,
                deserializer,
                mapperFactory);

            var entity = await dataProvider.GetDataAsync("", "en");

            Assert.NotNull(entity);
            Assert.NotNull(entity.Items);
            Assert.Equal(TestData.InvariantListCacheCount, entity.Items.Count());
        }
    }
}
