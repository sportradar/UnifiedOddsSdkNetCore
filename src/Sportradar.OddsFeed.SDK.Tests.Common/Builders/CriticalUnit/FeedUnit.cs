// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System.Collections.Generic;
using System.Linq;
using Sportradar.OddsFeed.SDK.Api.Config;
using Sportradar.OddsFeed.SDK.Api.Internal;
using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Tests.Common.Mock.Feed;

namespace Sportradar.OddsFeed.SDK.Tests.Common.Builders.CriticalUnit;

public class FeedUnit : IOpenable
{
    private readonly Dictionary<UofSession, SdkFeedMessageProcessor> _sessions = [];

    internal FeedUnit(Dictionary<MessageInterest, UofSession> sessions)
    {
        foreach (var session in sessions.Values)
        {
            var feedMessageProcessor = new SdkFeedMessageProcessor(session, session.Name);
            _sessions.Add(session, feedMessageProcessor);
        }
        IsOpened = false;
    }

    public bool IsOpened
    {
        get;
        private set;
    }

    public void Open()
    {
        foreach (var session in _sessions)
        {
            session.Value.Open();
            session.Key.Open();
        }
        IsOpened = true;
    }

    public void Close()
    {
        foreach (var session in _sessions)
        {
            session.Key.Close();
            session.Value.Close();
        }
        IsOpened = false;
    }

    internal UofSession GetSession(MessageInterest messageInterest)
    {
        return _sessions.First(f => f.Key.MessageInterest.Equals(messageInterest)).Key;
    }

    internal SdkFeedMessageProcessor GetMessageProcessor(MessageInterest messageInterest)
    {
        return _sessions.First(f => f.Key.MessageInterest.Equals(messageInterest)).Value;
    }
}
