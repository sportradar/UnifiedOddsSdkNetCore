/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Common.Internal;
using Sportradar.OddsFeed.SDK.Entities.Internal;
using Sportradar.OddsFeed.SDK.Entities.REST;
using Sportradar.OddsFeed.SDK.Entities.REST.Enums;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching.Events;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.MarketNames;
using Sportradar.OddsFeed.SDK.Messages;
using Sportradar.OddsFeed.SDK.Messages.Feed;
using Sportradar.OddsFeed.SDK.Test.Shared;

namespace Sportradar.OddsFeed.SDK.Entities.Test
{
    [TestClass]
    public class MapBetSettlementTest
    {
        private static IFeedMessageMapper _mapper;

        private static readonly IDeserializer<FeedMessage> Deserializer = new Deserializer<FeedMessage>();

        private bet_settlement _record;

        private IBetSettlement<ICompetition> _entity;

        [ClassInitialize]
        public static void Init(TestContext context)
        {
            var nameProviderFactoryMock = new Mock<INameProviderFactory>();
            var nameProviderMock = new Mock<INameProvider>();
            nameProviderFactoryMock.Setup(m => m.BuildNameProvider(It.IsAny<ICompetition>(), It.IsAny<int>(), It.IsAny<IReadOnlyDictionary<string, string>>())).Returns(nameProviderMock.Object);

            var voidReasonCache = new NamedValueCache(new NamedValueDataProvider(TestData.RestXmlPath + @"\void_reasons.xml", new TestDataFetcher(), "void_reason"), ExceptionHandlingStrategy.THROW);

            var namedValuesProviderMock = new Mock<INamedValuesProvider>();
            namedValuesProviderMock.Setup(x => x.VoidReasons).Returns(voidReasonCache);

            _mapper = new FeedMessageMapper(
                new TestSportEntityFactory(),
                nameProviderFactoryMock.Object,
                new Mock<IMarketMappingProviderFactory>().Object,
                namedValuesProviderMock.Object,
                ExceptionHandlingStrategy.THROW,
                TestProducerManager.Create(),
                new Mock<IMarketCacheProvider>().Object,
                voidReasonCache);
        }

        [TestMethod]
        public void BetSettlementIsMapped()
        {
            if (_entity == null)
            {
                LoadEntityFromFile();
            }

            Assert.IsNotNull(_entity, "entity should not be a null reference");
        }

        [TestMethod]
        public void TestBetSettlementMapping()
        {
            LoadEntityFromFile(true);

            var assertHelper = new AssertHelper(_entity);
            TestEntityValues(_entity, _record, assertHelper);
        }

        private void LoadEntityFromFile(bool load = false)
        {
            if (_entity == null || load)
            {
                var stream = FileHelper.OpenFile(TestData.FeedXmlPath, "bet_settlement.xml");
                _record = Deserializer.Deserialize<bet_settlement>(stream);
                TestData.FillMessageTimestamp(_record);
                _record.SportId = URN.Parse("sr:sport:1000");
                _entity = _mapper.MapBetSettlement<ICompetition>(_record, new[] {TestData.Culture}, null);
            }
        }

        private void TestEntityValues(IBetSettlement<ICompetition> entity, bet_settlement record, AssertHelper assertHelper)
        {
            assertHelper.AreEqual(() => entity.Event.Id.ToString(), record.event_id);
            var rProduct = TestProducerManager.Create().Get(record.product);
            assertHelper.AreEqual(() => entity.Producer, rProduct);
            assertHelper.AreEqual(() => entity.RequestId, record.request_idSpecified
                                                                ? (long?)record.request_id
                                                                : null);
            assertHelper.AreEqual(() => entity.Timestamps.Created, record.timestamp);

            TestMarketOutcomes(entity.Markets.ToList(), record.outcomes, assertHelper);
        }

        private void TestMarketOutcomes(IReadOnlyList<IMarketWithSettlement> markets, betSettlementMarket[] bets, AssertHelper assertHelper)
        {
            foreach(var b in bets)
            {
                var m = FindMarketWithSettlementByIdAndSpecifier(markets, b.id, b.specifiers);

                assertHelper.AreEqual(() => m.Id, b.id);
                var marketSpecifiersMatch = CompareMarketSpecifiers(m.Specifiers, b.specifiers);
                var marketOutcomeSettlementMatch = CompareMarketOutcomeSettlement(m.OutcomeSettlements.ToList(), b.Items, assertHelper);
                assertHelper.AreEqual(() => m.Specifiers, marketSpecifiersMatch
                                                            ? m.Specifiers
                                                            : null);
                assertHelper.AreEqual(() => m.OutcomeSettlements, marketOutcomeSettlementMatch
                                                            ? m.OutcomeSettlements
                                                            : null);
            }
        }

        private static IMarketWithSettlement FindMarketWithSettlementByIdAndSpecifier(IReadOnlyList<IMarketWithSettlement> markets, long recordId, string recordSpecifiers)
        {
            return markets.FirstOrDefault(m => m.Id == recordId && CompareMarketSpecifiers(m.Specifiers, recordSpecifiers));
        }

        private static bool CompareMarketSpecifiers(IReadOnlyDictionary<string, string> marketSpecifiers, string recordSpecifiers)
        {
            if(marketSpecifiers.Count == 0 && string.IsNullOrWhiteSpace(recordSpecifiers))
            {
                return true;
            }

            var result = true;

            var recordsSplit = recordSpecifiers.Split('|');

            foreach (var s in recordsSplit)
            {
                var recSplit = s.Split('=');

                string marketValue;

                result = marketSpecifiers.TryGetValue(recSplit[0], out marketValue);

                if(!result || marketValue != recSplit[1])
                {
                    result = false;
                    break;
                }
            }

            return result;
        }

        private static bool CompareMarketOutcomeSettlement(IReadOnlyList<IOutcomeSettlement> marketOutcomeSettlement, betSettlementMarketOutcome[] betOutcomes, AssertHelper assertHelper)
        {
            var mos = marketOutcomeSettlement.ToList();
            if (mos.Count == 0 && betOutcomes.Length == 0)
            {
                return true;
            }

            foreach (var b in betOutcomes)
            {
                foreach(var m in mos)
                {
                    if(b.id == m.Id)
                    {
                        assertHelper.AreEqual(() => m.Id, b.id);
                        assertHelper.AreEqual(() => m.DeadHeatFactor, b.dead_heat_factorSpecified
                                                                ? (double?)b.dead_heat_factor
                                                                : null);
                        assertHelper.AreEqual(() => m.Result, b.result == 1);

                        var voidFactor = (VoidFactor?)MessageMapperHelper.GetVoidFactor(b.void_factorSpecified, b.void_factor);
                        assertHelper.AreEqual(() => m.VoidFactor, voidFactor);

                        break;
                    }
                }
            }

            return true;
        }
    }
}
