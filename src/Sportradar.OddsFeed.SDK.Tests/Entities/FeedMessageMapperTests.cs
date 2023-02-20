/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/

using System.Collections.Generic;
using System.Linq;
using Moq;
using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Common.Internal;
using Sportradar.OddsFeed.SDK.Entities;
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
using Sportradar.OddsFeed.SDK.Tests.Common;
using Xunit;
using Xunit.Abstractions;

namespace Sportradar.OddsFeed.SDK.Tests.Entities
{
    public class FeedMessageMapperTests
    {
        private readonly IFeedMessageMapper _mapper;
        private readonly IFeedMessageValidator _validator;
        private readonly IDeserializer<FeedMessage> _deserializer;

        public FeedMessageMapperTests(ITestOutputHelper outputHelper)
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

            _mapper = new FeedMessageMapper(new TestSportEntityFactory(outputHelper),
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

        [Fact]
        public void AliveIsMapped()
        {
            var stream = FileHelper.OpenFile(TestData.FeedXmlPath, "alive.xml");
            var message = _deserializer.Deserialize<alive>(stream);
            TestData.FillMessageTimestamp(message);
            _validator.Validate(message);
            var entity = _mapper.MapAlive(message);
            Assert.NotNull(entity);
        }

        [Fact]
        public void BetSettlementIsMapped()
        {
            var stream = FileHelper.OpenFile(TestData.FeedXmlPath, "bet_settlement.xml");
            var message = _deserializer.Deserialize<bet_settlement>(stream);
            TestData.FillMessageTimestamp(message);
            message.SportId = URN.Parse("sr:sport:1000");
            _validator.Validate(message);
            var entity = _mapper.MapBetSettlement<ICompetition>(message, new[] { TestData.Culture }, null);
            Assert.NotNull(entity);
        }

        [Fact]
        public void BetStopIsMapped()
        {
            var stream = FileHelper.OpenFile(TestData.FeedXmlPath, "bet_stop.xml");
            var message = _deserializer.Deserialize<bet_stop>(stream);
            TestData.FillMessageTimestamp(message);
            message.SportId = URN.Parse("sr:sport:1000");
            _validator.Validate(message);
            var entity = _mapper.MapBetStop<ICompetition>(message, new[] { TestData.Culture }, null);
            Assert.NotNull(entity);
        }

        [Fact]
        public void FixtureChangeIsMapped()
        {
            var stream = FileHelper.OpenFile(TestData.FeedXmlPath, "fixture_change.xml");
            var message = _deserializer.Deserialize<fixture_change>(stream);
            TestData.FillMessageTimestamp(message);
            message.SportId = URN.Parse("sr:sport:1000");
            _validator.Validate(message);
            var entity = _mapper.MapFixtureChange<ICompetition>(message, new[] { TestData.Culture }, null);
            Assert.NotNull(entity);
        }

        [Fact]
        public void OddsChangeIsMapped()
        {
            var stream = FileHelper.OpenFile(TestData.FeedXmlPath, "odds_change.xml");
            var message = _deserializer.Deserialize<odds_change>(stream);
            TestData.FillMessageTimestamp(message);
            message.SportId = URN.Parse("sr:sport:1000");
            _validator.Validate(message);
            var entity = _mapper.MapOddsChange<ICompetition>(message, new[] { TestData.Culture }, null);
            Assert.NotNull(entity);
        }

        [Fact]
        public void ProbabilityIsMapped()
        {
            var stream = FileHelper.OpenFile(TestData.FeedXmlPath, "probabilities.xml");
            var message = _deserializer.Deserialize<cashout>(stream);
            TestData.FillMessageTimestamp(message);
            message.SportId = URN.Parse("sr:sport:1000");
            var entity = _mapper.MapCashOutProbabilities<ICompetition>(message, new[] { TestData.Culture }, null);
            Assert.NotNull(entity);
        }

        [Fact]
        public void OddsOnProbabilityCanBeOmitted()
        {
            var stream = FileHelper.OpenFile(TestData.FeedXmlPath, "probabilities.xml");
            var message = _deserializer.Deserialize<cashout>(stream);
            TestData.FillMessageTimestamp(message);
            message.SportId = URN.Parse("sr:sport:1000");
            message.odds = null;
            var entity = _mapper.MapCashOutProbabilities<ICompetition>(message, new[] { TestData.Culture }, null);
            Assert.NotNull(entity);
            Assert.Null(entity.Markets);
            Assert.Null(entity.BetStopReason);
            Assert.Null(entity.BettingStatus);
        }

        [Fact]
        public void OptionalFieldsOnProbabilitiesCanBeOmitted()
        {
            var stream = FileHelper.OpenFile(TestData.FeedXmlPath, "probabilities.xml");
            var message = _deserializer.Deserialize<cashout>(stream);
            TestData.FillMessageTimestamp(message);
            message.SportId = URN.Parse("sr:sport:1000");
            message.odds.betstop_reasonSpecified = false;
            message.odds.betting_statusSpecified = false;
            var entity = _mapper.MapCashOutProbabilities<ICompetition>(message, new[] { TestData.Culture }, null);
            Assert.NotNull(entity);
            Assert.NotNull(entity.Markets);
            Assert.Null(entity.BetStopReason);
            Assert.Null(entity.BettingStatus);
        }

        [Fact]
        public void OptionalFieldsOnProbabilitiesAreMapped()
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
            Assert.NotNull(entity);
            Assert.NotNull(entity.Markets);
            Assert.NotNull(entity.BetStopReason);
            Assert.Equal(2, entity.BetStopReason.Id);
            Assert.NotNull(entity.BettingStatus);
            Assert.Equal(3, entity.BettingStatus.Id);
        }

        [Fact]
        public void ProbabilityWithMarketWithCashoutStatusIsMapped()
        {
            var stream = FileHelper.OpenFile(TestData.FeedXmlPath, "probabilities.xml");
            var message = _deserializer.Deserialize<cashout>(stream);
            TestData.FillMessageTimestamp(message);
            message.SportId = URN.Parse("sr:sport:1000");
            var entity = _mapper.MapCashOutProbabilities<ICompetition>(message, new[] { TestData.Culture }, null);
            Assert.NotNull(entity);
            Assert.NotNull(entity.Markets);
            Assert.Equal(1, entity.Markets.Count(w => w.CashoutStatus != null));
            var marketWithCashoutStatus = entity.Markets.First(w => w.CashoutStatus != null);
            Assert.Equal(CashoutStatus.AVAILABLE, marketWithCashoutStatus.CashoutStatus);
        }

        [Fact]
        public void OddsChangeWithMarketWithCashoutStatusIsMapped()
        {
            var stream = FileHelper.OpenFile(TestData.FeedXmlPath, "odds_change.xml");
            var message = _deserializer.Deserialize<odds_change>(stream);
            TestData.FillMessageTimestamp(message);
            message.SportId = URN.Parse("sr:sport:1000");
            _validator.Validate(message);
            var entity = _mapper.MapOddsChange<ICompetition>(message, new[] { TestData.Culture }, null);
            Assert.NotNull(entity);
            Assert.NotNull(entity.Markets);
            Assert.Equal(1, entity.Markets.Count(w => w.CashoutStatus != null));
        }
    }
}
