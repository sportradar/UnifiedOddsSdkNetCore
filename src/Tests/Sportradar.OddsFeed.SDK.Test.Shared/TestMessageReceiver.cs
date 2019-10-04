/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using System.Collections.Generic;
using Sportradar.OddsFeed.SDK.Entities;
using Sportradar.OddsFeed.SDK.Entities.Internal;
using Sportradar.OddsFeed.SDK.Entities.Internal.EventArguments;
using Sportradar.OddsFeed.SDK.Messages.EventArguments;
using Sportradar.OddsFeed.SDK.Messages.Feed;

namespace Sportradar.OddsFeed.SDK.Test.Shared
{
    public class TestMessageReceiver : IMessageReceiver
    {
        private bool _isOpened;
        public bool IsOpened => _isOpened;

        public event EventHandler<FeedMessageReceivedEventArgs> FeedMessageReceived;

        public event EventHandler<MessageDeserializationFailedEventArgs> FeedMessageDeserializationFailed;

        public event EventHandler<RawFeedMessageEventArgs> RawFeedMessageReceived;

        public void Open(MessageInterest msgInterest)
        {
            _isOpened = true;
        }

        public void Open(MessageInterest interest, IEnumerable<string> routingKeys)
        {
            _isOpened = true;
        }

        public void Close()
        {
            _isOpened = false;
        }

        public void ProcessMessage(FeedMessage message, MessageInterest interest, byte[] rawMessage)
        {
            FeedMessageReceived?.Invoke(this, new FeedMessageReceivedEventArgs(message, interest, rawMessage));
        }

        public void DeserializationFailedMessage(FeedMessage message)
        {
            FeedMessageDeserializationFailed?.Invoke(this, null);
        }
    }
}
