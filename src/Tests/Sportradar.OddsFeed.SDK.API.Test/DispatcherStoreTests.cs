/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Sportradar.OddsFeed.SDK.API.Internal;
using Sportradar.OddsFeed.SDK.Entities.Internal;
using Sportradar.OddsFeed.SDK.Entities.REST;
using Sportradar.OddsFeed.SDK.Messages;
using Sportradar.OddsFeed.SDK.Test.Shared;

namespace Sportradar.OddsFeed.SDK.API.Test
{
    [TestClass]
    public class DispatcherStoreTests
    {
        private IDispatcherStore _store;

        [TestInitialize]
        public void Init()
        {
            _store = new DispatcherStore(new TestEntityTypeMapper());
        }

        [TestMethod]
        public void ClosedDispatcherIsNotRetrieved()
        {
            var messageMapperMock = new Mock<IFeedMessageMapper>();
            var dispatcher = new SpecificEntityDispatcher<ITestSoccerMatch>(messageMapperMock.Object, TestData.Cultures);

            _store.Add(dispatcher);
            var retrievedDispatcher = _store.Get(URN.Parse("sr:match:112"), URN.Parse("sr:sport:1"));
            Assert.IsNull(retrievedDispatcher, $"{nameof(retrievedDispatcher)} must be null");
        }

        [TestMethod]
        public void SpecificDispatcherIsRetrieved()
        {
            var messageMapperMock = new Mock<IFeedMessageMapper>();
            var dispatcher = new SpecificEntityDispatcher<ITestSoccerMatch>(messageMapperMock.Object, TestData.Cultures);

            _store.Add(dispatcher);
            dispatcher.Open();
            var retrievedDispatcher = _store.Get(URN.Parse("sr:match:112"), URN.Parse("sr:sport:1"));
            Assert.AreEqual(dispatcher, retrievedDispatcher, $"{nameof(retrievedDispatcher)} must be equal to {nameof(dispatcher)}");
        }

        [TestMethod]
        public void AllSubDispatchersAreRetrieved()
        {
            var messageMapperMock = new Mock<IFeedMessageMapper>();
            var dispatcher = new SpecificEntityDispatcher<IMatch>(messageMapperMock.Object, TestData.Cultures);

            _store.Add(dispatcher);
            dispatcher.Open();
            var soccerDispatcher = _store.Get(URN.Parse("sr:match:112"), URN.Parse("sr:sport:1"));
            Assert.AreEqual(dispatcher, soccerDispatcher, $"{nameof(soccerDispatcher)} must be equal to {nameof(dispatcher)}");

            var basketDispatcher = _store.Get(URN.Parse("sr:match:112"), URN.Parse("sr:sport:2"));
            Assert.AreEqual(dispatcher, basketDispatcher, $"{nameof(basketDispatcher)} must be equal to {nameof(dispatcher)}");

            var matchDispatcher = _store.Get(URN.Parse("sr:match:112"), URN.Parse("sr:sport:100"));
            Assert.AreEqual(dispatcher, matchDispatcher, $"{nameof(matchDispatcher)} must be equal to {nameof(dispatcher)}");
        }

        [TestMethod]
        public void SuperDispatcherIsNotRetrieved()
        {
            var messageMapperMock = new Mock<IFeedMessageMapper>();
            var dispatcher = new SpecificEntityDispatcher<ITestBallMatch>(messageMapperMock.Object, TestData.Cultures);

            _store.Add(dispatcher);
            dispatcher.Open();
            var matchDispatcher = _store.Get(URN.Parse("sr:match:100"), URN.Parse("sr:sport:100"));
            Assert.IsNull(matchDispatcher, $"{nameof(matchDispatcher)} must be null");
        }

        [TestMethod]
        public void MoreSpecificDispatcherIsUsed()
        {
            var matchDispatcher = new SpecificEntityDispatcher<IMatch>(new Mock<IFeedMessageMapper>().Object, TestData.Cultures);
            _store.Add(matchDispatcher);
            matchDispatcher.Open();

            var ballMatchDispatcher = new SpecificEntityDispatcher<ITestBallMatch>(new Mock<IFeedMessageMapper>().Object, TestData.Cultures);
            _store.Add(ballMatchDispatcher);
            ballMatchDispatcher.Open();

            var retrievedMatchDispatcher = _store.Get(URN.Parse("sr:match:100"), URN.Parse("sr:sport:100"));
            Assert.AreEqual(matchDispatcher, retrievedMatchDispatcher, $"{nameof(retrievedMatchDispatcher)} must be equal to {nameof(matchDispatcher)}");

            var retrievedSoccerDispatcher = _store.Get(URN.Parse("sr:match:100"), URN.Parse("sr:sport:1"));
            Assert.AreEqual(ballMatchDispatcher, retrievedSoccerDispatcher, $"{nameof(retrievedSoccerDispatcher)} must be equal to {nameof(ballMatchDispatcher)}");
        }

        [TestMethod]
        public void RemovedDispatcherIsNotRetrieved()
        {
            var dispatcher = new SpecificEntityDispatcher<IMatch>(new Mock<IFeedMessageMapper>().Object, TestData.Cultures);
            _store.Add(dispatcher);
            dispatcher.Open();

            dispatcher.Close();
            var retrievedDispatcher = _store.Get(URN.Parse("sr:match:100"), URN.Parse("sr:sport:100"));
            Assert.IsNull(retrievedDispatcher, $"{nameof(retrievedDispatcher)} must be null");
        }

        class TestEntityTypeMapper : EntityTypeMapper
        {
            public override Type Map(URN id, int sportId)
            {
                switch (sportId)
                {
                    case 1:
                        return typeof(ITestSoccerMatch);
                    case 2:
                        return typeof(ITestBasketMatch);
                    default:
                        return base.Map(id, sportId);
                }
            }
        }

        public interface ITestBallMatch : IMatch
        {

        }

        public interface ITestSoccerMatch : ITestBallMatch
        {

        }

        public interface ITestBasketMatch : ITestBallMatch
        {

        }
    }
}
