using System;
using System.Collections.Generic;
using System.Text.Json;
using Sportradar.OddsFeed.SDK.Api.Config;
using Sportradar.OddsFeed.SDK.Api.EventArguments;
using Sportradar.OddsFeed.SDK.Api.Internal.FeedAccess;
using Sportradar.OddsFeed.SDK.Entities.Internal.EventArguments;
using Sportradar.OddsFeed.SDK.Messages.Feed;

namespace Sportradar.OddsFeed.SDK.Tests.Common;

internal class StubMessageReceiver : IMessageReceiver
{
    public bool IsOpened { get; set; }
    public event EventHandler<FeedMessageReceivedEventArgs> FeedMessageReceived;
    public event EventHandler<MessageDeserializationFailedEventArgs> FeedMessageDeserializationFailed;
    public event EventHandler<RawFeedMessageEventArgs> RawFeedMessageReceived;

    private MessageInterest _messageInterest;

    public StubMessageReceiver(MessageInterest messageInterest = null)
    {
        IsOpened = false;
        _messageInterest = messageInterest;
    }

    public void SimulateDispatchMessage(FeedMessage message)
    {
        var rawMessage = JsonSerializer.SerializeToUtf8Bytes(message);
        FeedMessageReceived?.Invoke(this, new FeedMessageReceivedEventArgs(message, _messageInterest, rawMessage));
    }

    public void SimulateDispatchRawMessage(FeedMessage message)
    {
        RawFeedMessageReceived?.Invoke(this, new RawFeedMessageEventArgs("*", message, _messageInterest.ToString()));
    }

    public void SimulateDispatchFeedMessageDeserializationFailed(FeedMessage message)
    {
        var rawMessage = JsonSerializer.SerializeToUtf8Bytes(message);
        FeedMessageDeserializationFailed?.Invoke(this, new MessageDeserializationFailedEventArgs(rawMessage));
    }

    public void Open(MessageInterest interest, IEnumerable<string> routingKeys)
    {
        IsOpened = true;
        _messageInterest = interest;
    }

    public void Close()
    {
        IsOpened = false;
    }
}
