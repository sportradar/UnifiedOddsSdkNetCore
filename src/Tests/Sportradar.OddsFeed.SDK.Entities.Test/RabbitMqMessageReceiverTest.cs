/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using RabbitMQ.Client.Events;
using Sportradar.OddsFeed.SDK.Entities.Internal;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal;
using Sportradar.OddsFeed.SDK.Messages;
using Sportradar.OddsFeed.SDK.Messages.Feed;
using Sportradar.OddsFeed.SDK.Test.Shared;

namespace Sportradar.OddsFeed.SDK.Entities.Test
{
    [TestClass]
    public class RabbitMqMessageReceiverTest
    {
        private IMessageReceiver _messageReceiver;
        private Mock<IRabbitMqChannel> _mock;

        [TestInitialize]
        public void Init()
        {
            var deserializer = new Deserializer<FeedMessage>();
            var keyParser = new RegexRoutingKeyParser();
            _mock = new Mock<IRabbitMqChannel>();

            _messageReceiver = new RabbitMqMessageReceiver(_mock.Object, deserializer, keyParser, TestProducerManager.Create());
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

        [TestMethod]
        public void NullOrEmptyDataDoesNotRaiseDeserializationFailedEvent()
        {

            _messageReceiver.Open(MessageInterest.AllMessages, FeedRoutingKeyBuilder.GetStandardKeys());

            var deserializationFailed = false;

            _messageReceiver.FeedMessageDeserializationFailed += (sender, args) => { deserializationFailed = true; };
            _mock.Raise(mock => mock.Received += null, new BasicDeliverEventArgs("", 1, false, ",", null, null, null));
            _mock.Raise(mock => mock.Received += null, new BasicDeliverEventArgs("", 1, false, ",", null, null, new byte[0]));

            Assert.IsFalse(deserializationFailed, "deserializationFailed should be false");
        }

        [TestMethod]
        public void EventsAreNotRaisedBeforeTheReceiverIsOpened()
        {
            var messageReceived = false;
            var deserializationFailed = false;

            var messageData = GetFileContent("odds_change.xml");
            _messageReceiver.FeedMessageReceived += (sender, args) => { messageReceived = true; };
            _messageReceiver.FeedMessageDeserializationFailed += (sender, args) => { deserializationFailed = false; };

            _mock.Raise(mock => mock.Received += null, new BasicDeliverEventArgs("", 1, false, ",", null, null, messageData));
            _mock.Raise(mock => mock.Received += null, new BasicDeliverEventArgs("", 1, false, ",", null, null, new[] { (byte)1 }));

            Assert.IsFalse(messageReceived, "messageReceived should be false");
            Assert.IsFalse(deserializationFailed, "deserializationFailed should be false");
        }

        [TestMethod]
        public void EventsAreNotRaisedAfterTheReceiverIsClosed()
        {
            _messageReceiver.Open(MessageInterest.AllMessages, FeedRoutingKeyBuilder.GetStandardKeys());
            _messageReceiver.Close();
            EventsAreNotRaisedBeforeTheReceiverIsOpened();
        }

        [TestMethod]
        public void EventsAreRaisedWhenTheReceiverIsOpened()
        {
            _messageReceiver.Open(MessageInterest.AllMessages, FeedRoutingKeyBuilder.GetStandardKeys());

            var messageReceived = false;
            var deserializationFailed = false;

            var messageData = GetFileContent("odds_change.xml");
            _messageReceiver.FeedMessageReceived += (sender, args) => { messageReceived = true; };
            _messageReceiver.FeedMessageDeserializationFailed += (sender, args) => { deserializationFailed = true; };

            _mock.Raise(mock => mock.Received += null, new BasicDeliverEventArgs("", 1, false, ",", null, null, messageData));
            _mock.Raise(mock => mock.Received += null, new BasicDeliverEventArgs("", 1, false, ",", null, null, new []{(byte)1}));

            Assert.IsTrue(messageReceived, "messageReceived should be true");
            Assert.IsTrue(deserializationFailed, "deserializationFailed should be true");
        }

        [TestMethod]
        public void EventsAreRaisedAfterReceiverIsReOpened()
        {
            _messageReceiver.Open(MessageInterest.AllMessages, FeedRoutingKeyBuilder.GetStandardKeys());
            _messageReceiver.Close();
            EventsAreRaisedWhenTheReceiverIsOpened();
        }

        [TestMethod]
        public void MessageReceivedEventIsRaisedForOddsChange()
        {
            _messageReceiver.Open(MessageInterest.AllMessages, FeedRoutingKeyBuilder.GetStandardKeys());
            var routingKey = "hi.-.live.odds_change.2.sr:match.9900415";
            FeedMessage message = null;

            _messageReceiver.FeedMessageReceived += (sender, args) => { message = args.Message; };
            _mock.Raise(mock => mock.Received += null, new BasicDeliverEventArgs("", 1, false, ",", routingKey, null, GetFileContent("odds_change.xml")));

            Assert.IsNotNull(message, "message should not be a null reference");
            var expectedSportId = URN.Parse("sr:sport:2");
            Assert.AreEqual(expectedSportId, message.SportId, $"SportId on the messages should be {expectedSportId}");
        }

        [TestMethod]
        public void MessageReceivedEventIsRaisedForNullRoutingKey()
        {
            _messageReceiver.Open(MessageInterest.AllMessages, FeedRoutingKeyBuilder.GetStandardKeys());
            var eventRaised = false;

            _messageReceiver.FeedMessageReceived += (sender, args) => { eventRaised = true; };
            _mock.Raise(mock => mock.Received += null, new BasicDeliverEventArgs("", 1, false, ",", null, null, GetFileContent("odds_change.xml")));

            Assert.IsTrue(eventRaised, "eventRaised flag should be set to true");
        }

        [TestMethod]
        public void MessageReceivedEventIsRaisedForUnparsableRoutingKey()
        {
            _messageReceiver.Open(MessageInterest.AllMessages, FeedRoutingKeyBuilder.GetStandardKeys());
            var eventRaised = false;

            _messageReceiver.FeedMessageReceived += (sender, args) => { eventRaised = true; };
            _mock.Raise(mock => mock.Received += null, new BasicDeliverEventArgs("", 1, false, ",", "routing_key", null, GetFileContent("odds_change.xml")));

            Assert.IsTrue(eventRaised, "eventRaised flag should be set to true");
        }

        [TestMethod]
        public void CorrectDataIsPassedToDeserializationFailedEvent()
        {
            _messageReceiver.Open(MessageInterest.AllMessages, FeedRoutingKeyBuilder.GetStandardKeys());

            byte[] data = null;
            var messageData = new byte[] {1, 2, 3, 4, 100, 99 };

            _messageReceiver.FeedMessageDeserializationFailed += (sender, args) => { data = args.RawData.ToArray(); };
            _mock.Raise(mock => mock.Received += null, new BasicDeliverEventArgs("", 1, false, ",", null, null, messageData));

            var areEqual = messageData.SequenceEqual(data);
            Assert.IsTrue(areEqual, "both arrays should be equal");
        }
    }
}
