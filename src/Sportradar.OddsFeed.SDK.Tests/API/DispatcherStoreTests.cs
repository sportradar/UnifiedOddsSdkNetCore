/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/

using System;
using Moq;
using Sportradar.OddsFeed.SDK.API.Internal;
using Sportradar.OddsFeed.SDK.Entities.Internal;
using Sportradar.OddsFeed.SDK.Entities.REST;
using Sportradar.OddsFeed.SDK.Messages;
using Sportradar.OddsFeed.SDK.Tests.Common;
using Xunit;

namespace Sportradar.OddsFeed.SDK.Tests.API
{
    public class DispatcherStoreTests
    {
        private readonly IDispatcherStore _store;

        public DispatcherStoreTests()
        {
            _store = new DispatcherStore(new TestEntityTypeMapper());
        }

        [Fact]
        public void ClosedDispatcherIsNotRetrieved()
        {
            var messageMapperMock = new Mock<IFeedMessageMapper>();
            var dispatcher = new SpecificEntityDispatcher<ITestSoccerMatch>(messageMapperMock.Object, TestData.Cultures);

            _store.Add(dispatcher);
            var retrievedDispatcher = _store.Get(URN.Parse("sr:match:112"), URN.Parse("sr:sport:1"));
            Assert.Null(retrievedDispatcher);
        }

        [Fact]
        public void SpecificDispatcherIsRetrieved()
        {
            var messageMapperMock = new Mock<IFeedMessageMapper>();
            var dispatcher = new SpecificEntityDispatcher<ITestSoccerMatch>(messageMapperMock.Object, TestData.Cultures);

            _store.Add(dispatcher);
            dispatcher.Open();
            var retrievedDispatcher = _store.Get(URN.Parse("sr:match:112"), URN.Parse("sr:sport:1"));
            Assert.Equal(dispatcher, retrievedDispatcher);
        }

        [Fact]
        public void AllSubDispatchersAreRetrieved()
        {
            var messageMapperMock = new Mock<IFeedMessageMapper>();
            var dispatcher = new SpecificEntityDispatcher<IMatch>(messageMapperMock.Object, TestData.Cultures);

            _store.Add(dispatcher);
            dispatcher.Open();
            var soccerDispatcher = _store.Get(URN.Parse("sr:match:112"), URN.Parse("sr:sport:1"));
            Assert.Equal(dispatcher, soccerDispatcher);

            var basketDispatcher = _store.Get(URN.Parse("sr:match:112"), URN.Parse("sr:sport:2"));
            Assert.Equal(dispatcher, basketDispatcher);

            var matchDispatcher = _store.Get(URN.Parse("sr:match:112"), URN.Parse("sr:sport:100"));
            Assert.Equal(dispatcher, matchDispatcher);
        }

        [Fact]
        public void SuperDispatcherIsNotRetrieved()
        {
            var messageMapperMock = new Mock<IFeedMessageMapper>();
            var dispatcher = new SpecificEntityDispatcher<ITestBallMatch>(messageMapperMock.Object, TestData.Cultures);

            _store.Add(dispatcher);
            dispatcher.Open();
            var matchDispatcher = _store.Get(URN.Parse("sr:match:100"), URN.Parse("sr:sport:100"));
            Assert.Null(matchDispatcher);
        }

        [Fact]
        public void MoreSpecificDispatcherIsUsed()
        {
            var matchDispatcher = new SpecificEntityDispatcher<IMatch>(new Mock<IFeedMessageMapper>().Object, TestData.Cultures);
            _store.Add(matchDispatcher);
            matchDispatcher.Open();

            var ballMatchDispatcher = new SpecificEntityDispatcher<ITestBallMatch>(new Mock<IFeedMessageMapper>().Object, TestData.Cultures);
            _store.Add(ballMatchDispatcher);
            ballMatchDispatcher.Open();

            var retrievedMatchDispatcher = _store.Get(URN.Parse("sr:match:100"), URN.Parse("sr:sport:100"));
            Assert.Equal(matchDispatcher, retrievedMatchDispatcher);

            var retrievedSoccerDispatcher = _store.Get(URN.Parse("sr:match:100"), URN.Parse("sr:sport:1"));
            Assert.Equal(ballMatchDispatcher, retrievedSoccerDispatcher);
        }

        [Fact]
        public void RemovedDispatcherIsNotRetrieved()
        {
            var dispatcher = new SpecificEntityDispatcher<IMatch>(new Mock<IFeedMessageMapper>().Object, TestData.Cultures);
            _store.Add(dispatcher);
            dispatcher.Open();

            dispatcher.Close();
            var retrievedDispatcher = _store.Get(URN.Parse("sr:match:100"), URN.Parse("sr:sport:100"));
            Assert.Null(retrievedDispatcher);
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
