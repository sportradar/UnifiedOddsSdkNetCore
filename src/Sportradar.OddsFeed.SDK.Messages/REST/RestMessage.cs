/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/

// ReSharper disable InconsistentNaming

using System;
using System.ComponentModel;
using Sportradar.OddsFeed.SDK.Messages.Feed;
using Sportradar.OddsFeed.SDK.Messages.Internal;

namespace Sportradar.OddsFeed.SDK.Messages.REST
{
    /// <summary>
    /// Represents all messages (entities) received from the feed's REST API
    /// </summary>
    public abstract class RestMessage
    {
    }

    [OverrideXmlNamespace(RootElementName = "cashout", IgnoreNamespace = false)]
    public partial class cashout : FeedMessage
    {
        /// <summary>
        /// The message name
        /// </summary>
        public static readonly string MessageName = typeof(cashout).Name;

        /// <summary>
        /// When overridden in derived class, it gets a value indicating whether the current <see cref="FeedMessage" />
        /// instance is related to sport event
        /// </summary>
        /// <value><c>true</c> if this instance is event related; otherwise, <c>false</c>.</value>
        public override bool IsEventRelated => true;

        /// <summary>
        /// When overridden in derived class, it gets a value indicating the producer associated with current <see cref="FeedMessage" />
        /// </summary>
        /// <value>The producer identifier.</value>
        public override int ProducerId => product;

        /// <summary>
        /// Gets a value specified when making a request which generated this message, or null reference if this messages is not resulted with the request
        /// </summary>
        /// <value>The request identifier.</value>
        public override long? RequestId => null;

        /// <summary>
        /// When overridden in derived class, it gets a value specifying the usage requirements of the <see cref="RequestId" /> property
        /// </summary>
        /// <value>The request identifier usage.</value>
        public override PropertyUsage RequestIdUsage => PropertyUsage.FORBBIDEN;

        /// <summary>
        /// When override in derived class, it gets a value indicating whether current message is state-ful
        /// </summary>
        /// <value><c>true</c> if this instance is stateful; otherwise, <c>false</c>.</value>
        public override bool IsStateful => false;

        /// <summary>
        /// When overridden in derived class it gets the event identifier.
        /// </summary>
        /// <value>The event identifier</value>
        public override string EventId => event_id;

        /// <summary>
        /// When overridden in derived class, gets the name of the current message
        /// </summary>
        /// <value>The name.</value>
        public override string Name => MessageName;

        /// <summary>
        /// Gets the timestamp of the message
        /// </summary>
        /// <value>The timestamp of the message</value>
        public override long GeneratedAt => timestamp;

        /// <summary>
        /// Gets the timestamp of when the message was sent
        /// </summary>
        /// <value>The timestamp of the message</value>
        public override long SentAt { get; set; }

        /// <summary>
        /// Gets the timestamp of when the message was received (picked up) by the sdk
        /// </summary>
        /// <value>The timestamp of the message</value>
        public override long ReceivedAt { get; set; }
    }

    public partial class fixturesEndpoint : RestMessage
    {
    }

    public partial class scheduleEndpoint : RestMessage
    {
    }

    public partial class tournamentSchedule : RestMessage
    {
    }

    public partial class  matchSummaryEndpoint : RestMessage
    {
    }

    public partial class sportCategoriesEndpoint : RestMessage
    {
    }

    public partial class stageSummaryEndpoint : RestMessage
    {
    }

    public partial class tournamentsEndpoint : RestMessage
    {
    }

    public partial class sportsEndpoint : RestMessage
    {
    }

    public partial class tournamentInfoEndpoint : RestMessage
    {
    }

    public partial class playerProfileEndpoint : RestMessage
    {
    }

    public partial class competitorProfileEndpoint : RestMessage
    {
    }
    
    public partial class simpleTeamProfileEndpoint : RestMessage
    {
    }

    [OverrideXmlNamespace(RootElementName = "draw_summary", IgnoreNamespace = false)]
    public partial class draw_summary : RestMessage
    {
    }

    [OverrideXmlNamespace(RootElementName = "draw_fixture", IgnoreNamespace = false)]
    public partial class draw_fixture : RestMessage
    {
    }

    [OverrideXmlNamespace(RootElementName = "draw_fixtures", IgnoreNamespace = false)]
    public partial class draw_fixtures : RestMessage
    {
    }

    [OverrideXmlNamespace(RootElementName = "lotteries", IgnoreNamespace = false)]
    public partial class lotteries : RestMessage
    {
    }

    [OverrideXmlNamespace(RootElementName = "lottery_schedule", IgnoreNamespace = false)]
    public partial class lottery_schedule : RestMessage
    {
    }

    [OverrideXmlNamespace(RootElementName = "market_descriptions", IgnoreNamespace = false)]
    public partial class market_descriptions : RestMessage
    {
    }

    [OverrideXmlNamespace(RootElementName = "variant_descriptions", IgnoreNamespace = false)]
    public partial class variant_descriptions : RestMessage
    {
    }

    [OverrideXmlNamespace(RootElementName = "bookmaker_details", IgnoreNamespace = false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public partial class bookmaker_details : RestMessage
    {
    }

    [OverrideXmlNamespace(RootElementName = "producers", IgnoreNamespace = false)]
    public partial class producers : RestMessage
    {
    }

    [OverrideXmlNamespace(RootElementName = "tournament_seasons", IgnoreNamespace = false)]
    public partial class tournamentSeasons : RestMessage
    {
    }

    [OverrideXmlNamespace(RootElementName = "match_timeline", IgnoreNamespace = false)]
    public partial class matchTimelineEndpoint : RestMessage
    {
    }

    // Classes below are only needed in order for the deserializer to work since the original declarations do not specify XmlRoot attribute
    [System.Xml.Serialization.XmlRoot("simple_tournament_info", Namespace = "http://schemas.sportradar.com/sportsapi/v1/unified", IsNullable = false)]
    public partial class simpleTournamentInfoEndpoint
    {
    }

    [System.Xml.Serialization.XmlRoot("standard_tournament_info", Namespace = "http://schemas.sportradar.com/sportsapi/v1/unified", IsNullable = false)]
    public partial class standardTournamentInfoEndpoint
    {
    }

    [System.Xml.Serialization.XmlRoot("race_tournament_info", Namespace = "http://schemas.sportradar.com/sportsapi/v1/unified", IsNullable = false)]
    public partial class raceTournamentInfoEndpoint
    {
    }

    [OverrideXmlNamespace(RootElementName = "response", IgnoreNamespace = false)]
    public partial class response : RestMessage
    {
    }

    [OverrideXmlNamespace(RootElementName = "sportEventStatus", IgnoreNamespace = true)]
    public partial class restSportEventStatus
    {
    }

    public partial class AvailableSelectionsType : RestMessage
    {
    }

    public partial class CalculationResponseType : RestMessage
    {
    }

    public partial class fixtureChangesEndpoint : RestMessage
    {
    }

    public partial class sportTournamentsEndpoint : RestMessage
    {
    }

    [Obsolete("Not used")]
    public partial class teamExtended
    {
    }

    [Obsolete("Not used")]
    public partial class playerSubstitute
    {
    }

    [Obsolete("Not used")]
    public partial class playerLineup
    {
    }

    [Obsolete("Not used")]
    public partial class resultEndpoint
    {
    }

    [Obsolete("Not used")]
    public partial class resultsEndpoint
    {
    }

    [Obsolete("Not used")]
    public partial class venueSummaryEndpoint
    {
    }

    [Obsolete("Not used")]
    public partial class ResponseType
    {
    }
}
