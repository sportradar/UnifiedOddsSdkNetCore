/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/

using System.Text;
using System.Xml.Serialization;
using Sportradar.OddsFeed.SDK.Messages.Internal;

// ReSharper disable InconsistentNaming

namespace Sportradar.OddsFeed.SDK.Messages.Feed
{
    /// <summary>
    /// Represents a base class for messages received from the feed
    /// </summary>
    public abstract class FeedMessage
    {
        /// <summary>
        /// Gets or sets a <see cref="URN"/> representing the id of the sport associated with the current <see cref="FeedMessage"/> instance
        /// </summary>
        [XmlIgnore]
        public URN SportId
        {
            get;
            set;
        }

        /// <summary>
        /// When overridden in derived class, it gets a <see cref="URN"/> specifying the id of the associated sport event
        /// </summary>
        /// <value>The event urn</value>
        [XmlIgnore]
        public URN EventURN
        {
            get;
            set;
        }

        /// <summary>
        /// When overridden in derived class, gets the name of the current message
        /// </summary>
        public abstract string Name { get; }

        /// <summary>
        /// When overridden in derived class, it gets a value indicating the producer associated with current <see cref="FeedMessage"/>
        /// </summary>
        public abstract int ProducerId { get; }

        /// <summary>
        /// Gets a value specified when making a request which generated this message, or null reference if this messages is not resulted with the request
        /// </summary>
        public abstract long? RequestId { get; }

        /// <summary>
        /// When overridden in derived class, it gets a value specifying the usage requirements of the <see cref="RequestId"/> property
        /// </summary>
        public abstract PropertyUsage RequestIdUsage { get; }

        /// <summary>
        /// When overridden in derived class, it gets a value indicating whether the current <see cref="FeedMessage"/>
        /// instance is related to sport event
        /// </summary>
        public abstract bool IsEventRelated { get; }

        /// <summary>
        /// When override in derived class, it gets a value indicating whether current message is state-ful
        /// </summary>
        public abstract bool IsStateful { get; }

        /// <summary>
        /// When overridden in derived class it gets the event identifier.
        /// </summary>
        /// <value>The event identifier</value>
        public abstract string EventId { get; }

        /// <summary>
        /// Gets the timestamp of when the message was generated
        /// </summary>
        /// <value>The timestamp of the message</value>
        public abstract long GeneratedAt { get; }

        /// <summary>
        /// Gets the timestamp of when the message was sent
        /// </summary>
        /// <value>The timestamp of the message</value>
        public abstract long SentAt { get; set; }

        /// <summary>
        /// Gets the timestamp of when the message was received (picked up) by the sdk
        /// </summary>
        /// <value>The timestamp of the message</value>
        public abstract long ReceivedAt { get; set; }

        /// <summary>
        /// Returns a <see cref="string" /> that represents this instance.
        /// </summary>
        /// <returns>A <see cref="string" /> that represents this instance</returns>
        public override string ToString()
        {
            var builder = new StringBuilder();
            builder.Append(Name);
            builder.Append(" ProducerId=").Append(ProducerId);
            if (!string.IsNullOrEmpty(EventId))
            {
                builder.Append(", EventId=").Append(EventId);
            }
            builder.Append(", GeneratedAt=").Append(GeneratedAt);
            //builder.Append(", SentAt=").Append(SentAt);
            builder.Append(", ReceivedAt=").Append(ReceivedAt);
            if (RequestId.HasValue)
            {
                builder.Append(", RequestId=").Append(RequestId.Value);
            }
            return builder.ToString();
        }
    }

    [OverrideXmlNamespace(RootElementName = "alive", IgnoreNamespace = false)]
    public partial class alive : FeedMessage
    {
        public static readonly string MessageName = typeof(alive).Name;

        public override bool IsEventRelated => false;

        public override int ProducerId => product;

        public override long? RequestId => null;

        public override PropertyUsage RequestIdUsage => PropertyUsage.FORBBIDEN;

        public override bool IsStateful => false;

        public override string EventId => null;

        public override string Name => MessageName;

        public override long GeneratedAt => timestamp;

        public override long SentAt { get;  set; }

        public override long ReceivedAt { get;  set; }
    }

    [OverrideXmlNamespace(RootElementName = "snapshot_complete", IgnoreNamespace = false)]
    public partial class snapshot_complete : FeedMessage
    {
        public static readonly string MessageName = typeof(snapshot_complete).Name;

        public override bool IsEventRelated => false;

        public override int ProducerId => product;

        public override long? RequestId => request_id;

        public override PropertyUsage RequestIdUsage => PropertyUsage.REQUIRED;

        public override bool IsStateful => false;

        public override string EventId => null;

        public override string Name => MessageName;

        public override long GeneratedAt => timestamp;

        public override long SentAt { get;  set; }

        public override long ReceivedAt { get;  set; }
    }

    [OverrideXmlNamespace(RootElementName = "odds_change", IgnoreNamespace = false)]
    public partial class odds_change : FeedMessage
    {
        public static readonly string MessageName = typeof(odds_change).Name;

        public override bool IsEventRelated => true;

        public override int ProducerId => product;

        public override long? RequestId => request_idSpecified ? (long?)request_id : null;

        public override PropertyUsage RequestIdUsage => PropertyUsage.OPTIONAL;

        public override bool IsStateful => false;

        public override string EventId => event_id;

        public override string Name => MessageName;

        public override long GeneratedAt => timestamp;

        public override long SentAt { get;  set; }

        public override long ReceivedAt { get;  set; }
    }

    [OverrideXmlNamespace(RootElementName = "bet_stop", IgnoreNamespace = false)]
    public partial class bet_stop : FeedMessage
    {
        public static readonly string MessageName = typeof(bet_stop).Name;

        public override bool IsEventRelated => true;

        public override int ProducerId => product;

        public override long? RequestId => request_idSpecified ? (long?)request_id : null;

        public override PropertyUsage RequestIdUsage => PropertyUsage.OPTIONAL;

        public override bool IsStateful => false;

        public override string EventId => event_id;

        public override string Name => MessageName;

        public override long GeneratedAt => timestamp;

        public override long SentAt { get;  set; }

        public override long ReceivedAt { get;  set; }
    }

    [OverrideXmlNamespace(RootElementName = "bet_settlement", IgnoreNamespace = false)]
    public partial class bet_settlement : FeedMessage
    {
        public static readonly string MessageName = typeof(bet_settlement).Name;

        public override bool IsEventRelated => true;

        public override int ProducerId => product;

        public override long? RequestId => request_idSpecified ? (long?)request_id : null;

        public override PropertyUsage RequestIdUsage => PropertyUsage.OPTIONAL;

        public override bool IsStateful => true;

        public override string EventId => event_id;

        public override string Name => MessageName;

        public override long GeneratedAt => timestamp;

        public override long SentAt { get;  set; }

        public override long ReceivedAt { get;  set; }
    }

    [OverrideXmlNamespace(RootElementName = "rollback_bet_settlement", IgnoreNamespace = false)]
    public partial class rollback_bet_settlement : FeedMessage
    {
        public static readonly string MessageName = typeof(rollback_bet_settlement).Name;

        public override bool IsEventRelated => true;

        public override int ProducerId => product;

        public override long? RequestId => request_idSpecified ? (long?)request_id : null;

        public override PropertyUsage RequestIdUsage => PropertyUsage.OPTIONAL;

        public override bool IsStateful => true;

        public override string EventId => event_id;

        public override string Name => MessageName;

        public override long GeneratedAt => timestamp;

        public override long SentAt { get;  set; }

        public override long ReceivedAt { get;  set; }
    }

    [OverrideXmlNamespace(RootElementName = "bet_cancel", IgnoreNamespace = false)]
    public partial class bet_cancel : FeedMessage
    {
        public static readonly string MessageName = typeof(bet_cancel).Name;

        public override bool IsEventRelated => true;

        public override int ProducerId => product;

        public override long? RequestId => request_idSpecified ? (long?)request_id : null;

        public override PropertyUsage RequestIdUsage => PropertyUsage.OPTIONAL;

        public override bool IsStateful => true;

        public override string EventId => event_id;

        public override string Name => MessageName;

        public override long GeneratedAt => timestamp;

        public override long SentAt { get;  set; }

        public override long ReceivedAt { get;  set; }
    }

    [OverrideXmlNamespace(RootElementName = "rollback_bet_cancel", IgnoreNamespace = false)]
    public partial class rollback_bet_cancel : FeedMessage
    {
        public static readonly string MessageName = typeof(rollback_bet_cancel).Name;

        public override bool IsEventRelated => true;

        public override int ProducerId => product;

        public override long? RequestId => request_idSpecified ? (long?)request_id : null;

        public override PropertyUsage RequestIdUsage => PropertyUsage.OPTIONAL;

        public override bool IsStateful => true;

        public override string EventId => event_id;

        public override string Name => MessageName;

        public override long GeneratedAt => timestamp;

        public override long SentAt { get;  set; }

        public override long ReceivedAt { get;  set; }
    }

    [OverrideXmlNamespace(RootElementName = "fixture_change", IgnoreNamespace = false)]
    public partial class fixture_change : FeedMessage
    {
        public static readonly string MessageName = typeof(fixture_change).Name;

        public override bool IsEventRelated => true;

        public override int ProducerId => product;

        public override long? RequestId => request_idSpecified ? (long?)request_id : null;

        public override PropertyUsage RequestIdUsage => PropertyUsage.OPTIONAL;

        public override bool IsStateful => false;

        public override string EventId => event_id;

        public override string Name => MessageName;

        public override long GeneratedAt => timestamp;

        public override long SentAt { get;  set; }

        public override long ReceivedAt { get;  set; }
    }
}
