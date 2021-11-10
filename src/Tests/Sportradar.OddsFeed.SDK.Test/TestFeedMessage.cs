/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/

using Sportradar.OddsFeed.SDK.Messages.Internal;

// ReSharper disable InconsistentNaming

namespace Sportradar.OddsFeed.SDK.Test
{
    /// <summary>
    /// Represents a base class for messages received from the feed
    /// </summary>
    public abstract class TestFeedMessage
    {
    }

    [OverrideXmlNamespace(RootElementName = "alive", IgnoreNamespace = false)]
    public partial class alive : TestFeedMessage
    {
    }

    [OverrideXmlNamespace(RootElementName = "snapshot_complete", IgnoreNamespace = false)]
    public partial class snapshot_complete : TestFeedMessage
    {
    }

    [OverrideXmlNamespace(RootElementName = "odds_change", IgnoreNamespace = false)]
    public partial class odds_change : TestFeedMessage
    {
    }

    [OverrideXmlNamespace(RootElementName = "bet_stop", IgnoreNamespace = false)]
    public partial class bet_stop : TestFeedMessage
    {
    }

    [OverrideXmlNamespace(RootElementName = "bet_settlement", IgnoreNamespace = false)]
    public partial class bet_settlement : TestFeedMessage
    {
    }

    [OverrideXmlNamespace(RootElementName = "rollback_bet_settlement", IgnoreNamespace = false)]
    public partial class rollback_bet_settlement : TestFeedMessage
    {
    }

    [OverrideXmlNamespace(RootElementName = "bet_cancel", IgnoreNamespace = false)]
    public partial class bet_cancel : TestFeedMessage
    {
    }

    [OverrideXmlNamespace(RootElementName = "rollback_bet_cancel", IgnoreNamespace = false)]
    public partial class rollback_bet_cancel : TestFeedMessage
    {
    }

    [OverrideXmlNamespace(RootElementName = "fixture_change", IgnoreNamespace = false)]
    public partial class fixture_change : TestFeedMessage
    {
    }
}
