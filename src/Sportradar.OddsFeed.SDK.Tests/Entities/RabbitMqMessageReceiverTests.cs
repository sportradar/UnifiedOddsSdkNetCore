/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/

using System.IO;
using System.Linq;
using Moq;
using RabbitMQ.Client.Events;
using Sportradar.OddsFeed.SDK.Entities;
using Sportradar.OddsFeed.SDK.Entities.Internal;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal;
using Sportradar.OddsFeed.SDK.Messages;
using Sportradar.OddsFeed.SDK.Messages.Feed;
using Sportradar.OddsFeed.SDK.Tests.Common;
using Xunit;

namespace Sportradar.OddsFeed.SDK.Tests.Entities
{
    public class RabbitMqMessageReceiverTests
    {
        private readonly IMessageReceiver _messageReceiver;
        private readonly Mock<IRabbitMqChannel> _mock;

        public RabbitMqMessageReceiverTests()
        {
            var deserializer = new Deserializer<FeedMessage>();
            var keyParser = new RegexRoutingKeyParser();
            _mock = new Mock<IRabbitMqChannel>();

            _messageReceiver = new RabbitMqMessageReceiver(_mock.Object, deserializer, keyParser, TestProducerManager.Create(), false);
        }

        private static byte[] GetFileContent(string fileName)
        {
            var stream = FileHelper.OpenFile(TestData.FeedXmlPath, fileName);
            using (var ms = new MemoryStream())
            {
                stream.CopyTo(ms);
                return ms.ToArray();
            }
        }

        [Fact]
        public void NullOrEmptyDataDoesNotRaiseDeserializationFailedEvent()
        {

            _messageReceiver.Open(MessageInterest.AllMessages, FeedRoutingKeyBuilder.GetStandardKeys());

            var deserializationFailed = false;

            _messageReceiver.FeedMessageDeserializationFailed += (sender, args) => { deserializationFailed = true; };
            _mock.Raise(mock => mock.Received += null, new BasicDeliverEventArgs("", 1, false, ",", null, null, null));
            _mock.Raise(mock => mock.Received += null, new BasicDeliverEventArgs("", 1, false, ",", null, null, new byte[0]));

            Assert.False(deserializationFailed, "deserializationFailed should be false");
        }

        [Fact]
        public void EventsAreNotRaisedBeforeTheReceiverIsOpened()
        {
            var messageReceived = false;
            var deserializationFailed = false;

            var messageData = GetFileContent("odds_change.xml");
            _messageReceiver.FeedMessageReceived += (sender, args) => { messageReceived = true; };
            _messageReceiver.FeedMessageDeserializationFailed += (sender, args) => { deserializationFailed = false; };

            _mock.Raise(mock => mock.Received += null, new BasicDeliverEventArgs("", 1, false, ",", null, null, messageData));
            _mock.Raise(mock => mock.Received += null, new BasicDeliverEventArgs("", 1, false, ",", null, null, new[] { (byte)1 }));

            Assert.False(messageReceived, "messageReceived should be false");
            Assert.False(deserializationFailed, "deserializationFailed should be false");
        }

        [Fact]
        public void EventsAreNotRaisedAfterTheReceiverIsClosed()
        {
            _messageReceiver.Open(MessageInterest.AllMessages, FeedRoutingKeyBuilder.GetStandardKeys());
            _messageReceiver.Close();
            EventsAreNotRaisedBeforeTheReceiverIsOpened();
        }

        [Fact]
        public void EventsAreRaisedWhenTheReceiverIsOpened()
        {
            _messageReceiver.Open(MessageInterest.AllMessages, FeedRoutingKeyBuilder.GetStandardKeys());

            var messageReceived = false;
            var deserializationFailed = false;

            var messageData = GetFileContent("odds_change.xml");
            _messageReceiver.FeedMessageReceived += (sender, args) => { messageReceived = true; };
            _messageReceiver.FeedMessageDeserializationFailed += (sender, args) => { deserializationFailed = true; };

            _mock.Raise(mock => mock.Received += null, new BasicDeliverEventArgs("", 1, false, ",", null, null, messageData));
            _mock.Raise(mock => mock.Received += null, new BasicDeliverEventArgs("", 1, false, ",", null, null, new[] { (byte)1 }));

            Assert.True(messageReceived, "messageReceived should be true");
            Assert.True(deserializationFailed, "deserializationFailed should be true");
        }

        [Fact]
        public void EventsAreRaisedAfterReceiverIsReOpened()
        {
            _messageReceiver.Open(MessageInterest.AllMessages, FeedRoutingKeyBuilder.GetStandardKeys());
            _messageReceiver.Close();
            EventsAreRaisedWhenTheReceiverIsOpened();
        }

        [Fact]
        public void MessageReceivedEventIsRaisedForOddsChange()
        {
            _messageReceiver.Open(MessageInterest.AllMessages, FeedRoutingKeyBuilder.GetStandardKeys());
            var routingKey = "hi.-.live.odds_change.2.sr:match.9900415";
            FeedMessage message = null;

            _messageReceiver.FeedMessageReceived += (sender, args) => { message = args.Message; };
            _mock.Raise(mock => mock.Received += null, new BasicDeliverEventArgs("", 1, false, ",", routingKey, null, GetFileContent("odds_change.xml")));

            Assert.NotNull(message);
            var expectedSportId = URN.Parse("sr:sport:2");
            Assert.Equal(expectedSportId, message.SportId);
        }

        [Fact]
        public void MessageReceivedEventIsRaisedForNullRoutingKey()
        {
            _messageReceiver.Open(MessageInterest.AllMessages, FeedRoutingKeyBuilder.GetStandardKeys());
            var eventRaised = false;

            _messageReceiver.FeedMessageReceived += (sender, args) => { eventRaised = true; };
            _mock.Raise(mock => mock.Received += null, new BasicDeliverEventArgs("", 1, false, ",", null, null, GetFileContent("odds_change.xml")));

            Assert.True(eventRaised, "eventRaised flag should be set to true");
        }

        [Fact]
        public void MessageReceivedEventIsRaisedForUnparsableRoutingKey()
        {
            _messageReceiver.Open(MessageInterest.AllMessages, FeedRoutingKeyBuilder.GetStandardKeys());
            var eventRaised = false;

            _messageReceiver.FeedMessageReceived += (sender, args) => { eventRaised = true; };
            _mock.Raise(mock => mock.Received += null, new BasicDeliverEventArgs("", 1, false, ",", "routing_key", null, GetFileContent("odds_change.xml")));

            Assert.True(eventRaised, "eventRaised flag should be set to true");
        }

        [Fact]
        public void CorrectDataIsPassedToDeserializationFailedEvent()
        {
            _messageReceiver.Open(MessageInterest.AllMessages, FeedRoutingKeyBuilder.GetStandardKeys());

            byte[] data = null;
            var messageData = new byte[] { 1, 2, 3, 4, 100, 99 };

            _messageReceiver.FeedMessageDeserializationFailed += (sender, args) => { data = args.RawData.ToArray(); };
            _mock.Raise(mock => mock.Received += null, new BasicDeliverEventArgs("", 1, false, ",", null, null, messageData));

            var areEqual = messageData.SequenceEqual(data);
            Assert.True(areEqual, "both arrays should be equal");
        }
    }
}
