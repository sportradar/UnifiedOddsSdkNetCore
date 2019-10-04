/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sportradar.OddsFeed.SDK.API.Internal;

namespace Sportradar.OddsFeed.SDK.API.Test
{
    [TestClass]
    public class MessageDataExtractorTest
    {
        private static readonly IMessageDataExtractor DataExtractor = new MessageDataExtractor();

        [TestMethod]
        public void EmptyOrNullDataDoesNotCauseException()
        {
            var result = DataExtractor.GetBasicMessageData(null);
            Assert.IsNotNull(result, "result must not be null");

            var data = new byte[0];
            result = DataExtractor.GetBasicMessageData(data);
            Assert.IsNotNull(result, "result must not be null");
        }

        [TestMethod]
        public void MessageTypeIsExtractedFromShortMessageStart()
        {
            var message = "<alive 'rest of the message";
            var result = DataExtractor.GetBasicMessageData(Encoding.UTF8.GetBytes(message));
            Assert.AreEqual(MessageType.ALIVE, result.MessageType, "messageType must be 'alive'");
        }

        [TestMethod]
        public void MessageTypeIsExtractedFromLongMessageStart()
        {
            var message = "'xml header here' <bet_settlement> 'rest of the message here";
            var result = DataExtractor.GetBasicMessageData(Encoding.UTF8.GetBytes(message));
            Assert.AreEqual(MessageType.BET_SETTLEMENT, result.MessageType, "messageType must be 'alive'");
        }

        [TestMethod]
        public void MessageTypeIsExtractedFromMessageEnd()
        {
            var message = "'start of the message'</product_down>";
            var result = DataExtractor.GetBasicMessageData(Encoding.UTF8.GetBytes(message));
            Assert.AreEqual(MessageType.UNKNOWN, result.MessageType, "messageType must be alive");
        }

        [TestMethod]
        public void AttributeValueIsCorrectWhenStartQuoteIsMissing()
        {
            var message = "<alive product=1";
            var result = DataExtractor.GetBasicMessageData(Encoding.UTF8.GetBytes(message));

            Assert.IsNull(result.ProducerId, "product must be null");
        }

        [TestMethod]
        public void AttributeValueIsCorrectWhenEndQuoteIsMissing()
        {
            var message = "<alive product=\"1";
            var result = DataExtractor.GetBasicMessageData(Encoding.UTF8.GetBytes(message));

            Assert.IsNull(result.ProducerId, "product must be null");
        }

        [TestMethod]
        public void DataExtractedFromValidMessageIsCorrect()
        {
            var message = "<bet_stop product=\"1\" event_id=\"sr:match:9578495\" 'remaining message' />";
            var result = DataExtractor.GetBasicMessageData(Encoding.UTF8.GetBytes(message));

            Assert.IsNotNull(result, "Extraction result cannot be null");

            Assert.AreEqual(MessageType.BET_STOP, result.MessageType, "messageType must be bet_stop");
            Assert.AreEqual("1", result.ProducerId, "Product must be 1");
            Assert.AreEqual("sr:match:9578495", result.EventId, "EventId must be sr:match:9578495");
        }
    }
}
