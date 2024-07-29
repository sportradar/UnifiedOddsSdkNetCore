// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using Sportradar.OddsFeed.SDK.Entities.Rest;

namespace Sportradar.OddsFeed.SDK.Tests.Common.Mock.Feed;

public class UserReceivedMessage
{
    public long Timestamp { get; set; }

    public ISportEvent SportEvent { get; set; }

    public string MsgType { get; set; }

    public UserReceivedMessage(long timestamp, ISportEvent sportEvent, string msgType)
    {
        Timestamp = timestamp;
        SportEvent = sportEvent;
        MsgType = msgType;
    }
}
