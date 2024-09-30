// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System.Collections.Generic;
using Sportradar.OddsFeed.SDK.Common.Extensions;
using Sportradar.OddsFeed.SDK.Entities;
using Sportradar.OddsFeed.SDK.Entities.Rest;

namespace Sportradar.OddsFeed.SDK.Tests.Common.Mock.Feed;

public class UserReceivedMessage
{
    public long Timestamp { get; set; }

    public ISportEvent SportEvent { get; set; }

    public string MsgType { get; set; }

    public ICollection<IMarket> Markets { get; set; }

    public UserReceivedMessage(long timestamp, ISportEvent sportEvent, string msgType)
    {
        Timestamp = timestamp;
        SportEvent = sportEvent;
        MsgType = msgType;
        Markets = null;
    }

    public UserReceivedMessage(long timestamp, ISportEvent sportEvent, string msgType, List<IMarketWithOdds> markets)
        : this(timestamp, sportEvent, msgType)
    {
        InitializeMarkets(markets);
    }

    public UserReceivedMessage(long timestamp, ISportEvent sportEvent, string msgType, List<IMarketCancel> markets)
        : this(timestamp, sportEvent, msgType)
    {
        InitializeMarkets(markets);
    }

    public UserReceivedMessage(long timestamp, ISportEvent sportEvent, string msgType, List<IMarketWithSettlement> markets)
        : this(timestamp, sportEvent, msgType)
    {
        InitializeMarkets(markets);
    }

    private void InitializeMarkets<T>(List<T> markets) where T : IMarket
    {
        if (markets.IsNullOrEmpty())
        {
            return;
        }
        Markets = new List<IMarket>();
        foreach (var market in markets)
        {
            Markets.Add(market);
        }
    }
}
