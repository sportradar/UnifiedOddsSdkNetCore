using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Entities.REST;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.EntitiesImpl;

namespace Sportradar.OddsFeed.SDK.Tests.Common
{
    internal class TestSportEventStatusMapper : SportEventStatusMapperBase
    {
        public static ISportEventStatus GetTestEventStatus()
        {
            return new SportEventStatus(new TestSportEventStatusMapper().CreateNotStarted(), TestLocalizedNamedValueCache.CreateMatchStatusCache(TestData.Cultures3, ExceptionHandlingStrategy.THROW));
        }
    }
}
