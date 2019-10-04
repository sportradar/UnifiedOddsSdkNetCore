/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Moq;
using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Common.Internal;
using Sportradar.OddsFeed.SDK.Entities.Internal;
using Sportradar.OddsFeed.SDK.Entities.REST;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching.Events;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.MarketNames;
using Sportradar.OddsFeed.SDK.Messages;
using Sportradar.OddsFeed.SDK.Messages.Feed;
using Sportradar.OddsFeed.SDK.Test.Shared;

namespace Sportradar.OddsFeed.SDK.Entities.Test
{
    public abstract class MapEntityTestBase
    {
        private static readonly IDeserializer<FeedMessage> Deserializer = new Deserializer<FeedMessage>();

        internal static readonly IFeedMessageMapper Mapper;

        static MapEntityTestBase()
        {
            var nameProviderFactoryMock = new Mock<INameProviderFactory>();
            var nameProviderMock = new Mock<INameProvider>();
            nameProviderFactoryMock.Setup(m => m.BuildNameProvider(It.IsAny<ICompetition>(), It.IsAny<int>(), It.IsAny<IReadOnlyDictionary<string, string>>())).Returns(nameProviderMock.Object);

            var voidReasonCache = new NamedValueCache(new NamedValueDataProvider(TestData.RestXmlPath + @"\void_reasons.xml", new TestDataFetcher(), "void_reason"), ExceptionHandlingStrategy.THROW);

            var namedValuesProviderMock = new Mock<INamedValuesProvider>();
            namedValuesProviderMock.Setup(x => x.VoidReasons).Returns(voidReasonCache);

            Mapper = new FeedMessageMapper(
                new TestSportEventFactory(),
                nameProviderFactoryMock.Object,
                new Mock<IMarketMappingProviderFactory>().Object,
                namedValuesProviderMock.Object,
                ExceptionHandlingStrategy.THROW,
                TestProducerManager.Create(),
                new Mock<IMarketCacheProvider>().Object,
                voidReasonCache);
        }

        protected T Load<T>(string fileName, URN sportId, IEnumerable<CultureInfo> cultures)
            where T : FeedMessage
        {
            var stream = FileHelper.OpenFile(TestData.FeedXmlPath, fileName);
            var record = Deserializer.Deserialize<T>(stream);
            record.SportId = sportId;
            return record;
        }

        protected static bool CompareSpecifiers(IReadOnlyDictionary<string, string> marketSpecifiers, string recordSpecifiers)
        {
            if (string.IsNullOrEmpty(recordSpecifiers))
            {
                return marketSpecifiers == null;
            }

            var specifierParts = recordSpecifiers.Split(new[] { SdkInfo.SpecifiersDelimiter }, StringSplitOptions.RemoveEmptyEntries).ToList();
            specifierParts = specifierParts.OrderBy(p => p).ToList();

            var orderedRecordSpecifiers = string.Join(SdkInfo.SpecifiersDelimiter, specifierParts);
            var orderedSpecifiers = string.Join(SdkInfo.SpecifiersDelimiter, marketSpecifiers.OrderBy(kv => kv.Key).Select(kv => kv.Key + "=" + kv.Value));

            return orderedSpecifiers == orderedRecordSpecifiers;
        }

        protected static T FindMarket<T>(IEnumerable<T> markets, int id, string specifiersString) where T : IMarket
        {
            return markets == null
                ? default(T)
                : markets.FirstOrDefault(m => m.Id == id && CompareSpecifiers(m.Specifiers, specifiersString));
        }

        protected static T FindOutcome<T>(IEnumerable<T> outcomes, string id) where T : IOutcome
        {
            return outcomes == null
                ? default(T)
                : outcomes.FirstOrDefault(o => o.Id == id);
        }

        protected void TestMessageProperties(AssertHelper assertHelper, IMessageV1 message, long timestamp, int productId)
        {
            assertHelper.AreEqual(() => message.Timestamps.Created, timestamp);
            assertHelper.AreEqual(() => message.Producer, TestProducerManager.Create().Get(productId));
        }

        protected void TestEventMessageProperties<T>(AssertHelper assertHelper, IEventMessage<T> message, long timestamp, int productId, string eventId, long? requestId) where T : class, ISportEvent
        {
            TestMessageProperties(assertHelper, message, timestamp, productId);
            assertHelper.IsNotNull(() => message.Event);
            assertHelper.AreEqual(() => message.Event.Id.ToString(), eventId);
            assertHelper.AreEqual(() => message.RequestId, requestId);
        }
    }
}
