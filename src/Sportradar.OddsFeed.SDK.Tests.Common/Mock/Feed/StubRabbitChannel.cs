// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Moq;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Sportradar.OddsFeed.SDK.Api.Config;
using Sportradar.OddsFeed.SDK.Api.Internal.FeedAccess;
using Sportradar.OddsFeed.SDK.Common.Extensions;
using Sportradar.OddsFeed.SDK.Messages.Feed;
using Sportradar.OddsFeed.SDK.Tests.Common.Builders.Feed;

namespace Sportradar.OddsFeed.SDK.Tests.Common.Mock.Feed;

public class StubRabbitChannel : IRabbitMqChannel
{
    public bool IsOpened
    {
        get;
        private set;
    }

    public void Open(MessageInterest interest, IEnumerable<string> routingKeys)
    {
        IsOpened = true;
    }

    public void Close()
    {
        IsOpened = false;
    }

    public event EventHandler<BasicDeliverEventArgs> Received;

    public void SendMessage(FeedMessage message)
    {
        var msgXmlBody = FeedMessageBuilder.BuildMessageBody(message);
        var routingKey = FeedMessageBuilder.BuildRoutingKey(message);
        var timestamp = DateTime.Now.ToEpochTime();

        var msgBody = Encoding.UTF8.GetBytes(msgXmlBody);

        var mockBasicProperties = new Mock<IBasicProperties>();
        mockBasicProperties.Setup(s => s.IsHeadersPresent()).Returns(true);
        mockBasicProperties.SetupGet(s => s.Headers).Returns(new Dictionary<string, object> { { "timestamp_in_ms", timestamp } });

        var basicDeliverEventArgs = new BasicDeliverEventArgs
        {
            BasicProperties = mockBasicProperties.Object,
            Body = msgBody,
            RoutingKey = routingKey,
            ConsumerTag = TestConsts.AnyConsumerTag,
            DeliveryTag = 0,
            Exchange = TestConsts.DefaultRabbitExchange
        };
        SendMessage(basicDeliverEventArgs);
    }

    public void SendMessage(BasicDeliverEventArgs eventArgs)
    {
        Received?.Invoke(this, eventArgs);
    }

    public async Task WaitTillOpened()
    {
        while (!IsOpened)
        {
            await Task.Delay(20).ConfigureAwait(false);
        }
    }
}
