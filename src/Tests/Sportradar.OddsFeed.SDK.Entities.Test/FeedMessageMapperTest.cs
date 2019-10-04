/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Common.Internal;
using Sportradar.OddsFeed.SDK.Entities.Internal;
using Sportradar.OddsFeed.SDK.Entities.REST;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching.Events;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.EntitiesImpl;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.MarketNames;
using Sportradar.OddsFeed.SDK.Messages;
using Sportradar.OddsFeed.SDK.Messages.Feed;
using Sportradar.OddsFeed.SDK.Messages.REST;
using Sportradar.OddsFeed.SDK.Test.Shared;

namespace Sportradar.OddsFeed.SDK.Entities.Test
{
    [TestClass]
    public class FeedMessageMapperTest
    {
        private IFeedMessageMapper _mapper;

        private IFeedMessageValidator _validator;

        private IDeserializer<FeedMessage> _deserializer;

        [TestInitialize]
        public void Init()
        {
            var nameProviderFactoryMock = new Mock<INameProviderFactory>();
            var nameProviderMock = new Mock<INameProvider>();
            var mappingProviderFactoryMock = new Mock<IMarketMappingProviderFactory>();
            nameProviderFactoryMock.Setup(m => m.BuildNameProvider(It.IsAny<ISportEvent>(), It.IsAny<int>(), It.IsAny<IReadOnlyDictionary<string, string>>())).Returns(nameProviderMock.Object);

            var namedValuesCacheMock = new Mock<INamedValueCache>();
            namedValuesCacheMock.Setup(x => x.IsValueDefined(It.IsAny<int>())).Returns(true);
            namedValuesCacheMock.Setup(x => x.GetNamedValue(It.IsAny<int>())).Returns((int id) => new NamedValue(id, "somevalue"));

            var namedValuesProviderMock = new Mock<INamedValuesProvider>();
            namedValuesProviderMock.Setup(x => x.VoidReasons).Returns(namedValuesCacheMock.Object);
            namedValuesProviderMock.Setup(x => x.BetStopReasons).Returns(namedValuesCacheMock.Object);
            namedValuesProviderMock.Setup(x => x.BettingStatuses).Returns(namedValuesCacheMock.Object);

            _mapper = new FeedMessageMapper(new TestSportEventFactory(),
                                            nameProviderFactoryMock.Object,
                                            mappingProviderFactoryMock.Object,
                                            namedValuesProviderMock.Object,
                                            ExceptionHandlingStrategy.THROW,
                                            TestProducerManager.Create(),
                                            new Mock<IMarketCacheProvider>().Object,
                                            namedValuesCacheMock.Object);
            _deserializer = new Deserializer<FeedMessage>();
            _validator = new TestFeedMessageValidator();
        }

        [TestMethod]
        public void AliveIsMapped()
        {
            var stream = FileHelper.OpenFile(TestData.FeedXmlPath, "alive.xml");
            var message = _deserializer.Deserialize<alive>(stream);
            TestData.FillMessageTimestamp(message);
            _validator.Validate(message);
            var entity = _mapper.MapAlive(message);
            Assert.IsNotNull(entity, "entity should not be a null reference");
        }

        [TestMethod]
        public void BetSettlementIsMapped()
        {
            var stream = FileHelper.OpenFile(TestData.FeedXmlPath,"bet_settlement.xml");
            var message = _deserializer.Deserialize<bet_settlement>(stream);
            TestData.FillMessageTimestamp(message);
            message.SportId = URN.Parse("sr:sport:1000");
            _validator.Validate(message);
            var entity = _mapper.MapBetSettlement<ICompetition>(message, new []{TestData.Culture}, null);
            Assert.IsNotNull(entity, "entity should not be a null reference");
        }

        [TestMethod]
        public void BetStopIsMapped()
        {
            var stream = FileHelper.OpenFile(TestData.FeedXmlPath, "bet_stop.xml");
            var message = _deserializer.Deserialize<bet_stop>(stream);
            TestData.FillMessageTimestamp(message);
            message.SportId = URN.Parse("sr:sport:1000");
            _validator.Validate(message);
            var entity = _mapper.MapBetStop<ICompetition>(message, new [] {TestData.Culture}, null);
            Assert.IsNotNull(entity, "entity should not be a null reference");
        }

        [TestMethod]
        public void FixtureChangeIsMapped()
        {
            var stream = FileHelper.OpenFile(TestData.FeedXmlPath, "fixture_change.xml");
            var message = _deserializer.Deserialize<fixture_change>(stream);
            TestData.FillMessageTimestamp(message);
            message.SportId = URN.Parse("sr:sport:1000");
            _validator.Validate(message);
            var entity = _mapper.MapFixtureChange<ICompetition>(message, new [] {TestData.Culture}, null);
            Assert.IsNotNull(entity, "entity should not be a null reference");
        }

        [TestMethod]
        public void OddsChangeIsMapped()
        {
            var stream = FileHelper.OpenFile(TestData.FeedXmlPath, "odds_change.xml");
            var message = _deserializer.Deserialize<odds_change>(stream);
            TestData.FillMessageTimestamp(message);
            message.SportId = URN.Parse("sr:sport:1000");
            _validator.Validate(message);
            var entity = _mapper.MapOddsChange<ICompetition>(message, new []{TestData.Culture}, null);
            Assert.IsNotNull(entity, "entity should not be a null reference");
        }

        [TestMethod]
        public void probability_is_mapped()
        {
            var stream = FileHelper.OpenFile(TestData.FeedXmlPath, "probabilities.xml");
            var message = _deserializer.Deserialize<cashout>(stream);
            TestData.FillMessageTimestamp(message);
            message.SportId = URN.Parse("sr:sport:1000");
            var entity = _mapper.MapCashOutProbabilities<ICompetition>(message, new [] {TestData.Culture}, null);
            Assert.IsNotNull(entity, "entity should not be a null reference");
        }

        [TestMethod]
        public void odds_on_probability_can_be_omitted()
        {
            var stream = FileHelper.OpenFile(TestData.FeedXmlPath, "probabilities.xml");
            var message = _deserializer.Deserialize<cashout>(stream);
            TestData.FillMessageTimestamp(message);
            message.SportId = URN.Parse("sr:sport:1000");
            message.odds = null;
            var entity = _mapper.MapCashOutProbabilities<ICompetition>(message, new[] {TestData.Culture}, null);
            Assert.IsNotNull(entity, "entity should not be a null reference");
            Assert.IsNull(entity.Markets, "markets should be a null reference");
            Assert.IsNull(entity.BetStopReason, "BetStopReason should a null reference");
            Assert.IsNull(entity.BettingStatus, "BettingStatus should a null reference");
        }

        [TestMethod]
        public void optional_fields_on_probabilities_can_be_omitted()
        {
            var stream = FileHelper.OpenFile(TestData.FeedXmlPath, "probabilities.xml");
            var message = _deserializer.Deserialize<cashout>(stream);
            TestData.FillMessageTimestamp(message);
            message.SportId = URN.Parse("sr:sport:1000");
            message.odds.betstop_reasonSpecified = false;
            message.odds.betting_statusSpecified = false;
            var entity = _mapper.MapCashOutProbabilities<ICompetition>(message, new[] { TestData.Culture }, null);
            Assert.IsNotNull(entity, "entity should not be a null reference");
            Assert.IsNotNull(entity.Markets, "markets should be a null reference");
            Assert.IsNull(entity.BetStopReason, "BetStopReason should a null reference");
            Assert.IsNull(entity.BettingStatus, "BettingStatus should a null reference");
        }

        [TestMethod]
        public void optional_fields_on_probabilities_are_mapped()
        {
            var stream = FileHelper.OpenFile(TestData.FeedXmlPath, "probabilities.xml");
            var message = _deserializer.Deserialize<cashout>(stream);
            TestData.FillMessageTimestamp(message);
            message.SportId = URN.Parse("sr:sport:1000");
            message.odds.betstop_reasonSpecified = true;
            message.odds.betstop_reason = 2;
            message.odds.betting_statusSpecified = true;
            message.odds.betting_status = 3;
            var entity = _mapper.MapCashOutProbabilities<ICompetition>(message, new[] { TestData.Culture }, null);
            Assert.IsNotNull(entity, "entity should not be a null reference");
            Assert.IsNotNull(entity.Markets, "markets should be a null reference");
            Assert.IsNotNull(entity.BetStopReason, "BetStopReason should not be a null reference");
            Assert.AreEqual(2, entity.BetStopReason.Id);
            Assert.IsNotNull(entity.BettingStatus, "BettingStatus should not be a null reference");
            Assert.AreEqual(3, entity.BettingStatus.Id);
        }
    }
}
