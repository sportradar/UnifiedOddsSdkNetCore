// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using Sportradar.OddsFeed.SDK.Messages.Feed;

// ReSharper disable UnassignedGetOnlyAutoProperty

namespace Sportradar.OddsFeed.SDK.Tests.Common.MockFeed;

/// <summary>
/// Class RabbitMessage
/// </summary>
public class RabbitMessage
{
    /// <summary>
    /// Gets the message to be send
    /// </summary>
    /// <value>The message to be send</value>
    public FeedMessage Message { get; }

    /// <summary>
    /// Gets the value when last message was sent (used only for period messages)
    /// </summary>
    /// <value>The last message sent time</value>
    public DateTime LastSent { get; }

    /// <summary>
    /// Gets the period on which message should be send (if set)
    /// </summary>
    /// <value>The period</value>
    public TimeSpan Period { get; }

    /// <summary>
    /// Gets a value indicating whether id of the message should be randomized
    /// </summary>
    /// <value><c>true</c> if [randomize identifier]; otherwise, <c>false</c>.</value>
    public bool RandomizeId { get; }
}
