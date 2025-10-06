// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using Sportradar.OddsFeed.SDK.Api.Config;
using Sportradar.OddsFeed.SDK.Entities.Internal;
using Sportradar.OddsFeed.SDK.Entities.Internal.EventArguments;
using Sportradar.OddsFeed.SDK.Messages.Feed;

namespace Sportradar.OddsFeed.SDK.Tests.Common;

internal class StubMessageProcessor : IFeedMessageProcessor
{
    public string ProcessorId { get; set; }
    public event EventHandler<FeedMessageReceivedEventArgs> MessageProcessed;

    public StubMessageProcessor()
    {
        ProcessorId = nameof(StubMessageProcessor);
    }
    public void ProcessMessage(FeedMessage message, MessageInterest interest, byte[] rawMessage)
    {
        MessageProcessed?.Invoke(this, new FeedMessageReceivedEventArgs(message, interest, rawMessage));
    }
}
