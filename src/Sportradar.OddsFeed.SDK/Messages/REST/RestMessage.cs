// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

// ReSharper disable InconsistentNaming

using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Xml.Serialization;
using Sportradar.OddsFeed.SDK.Common.Enums;
using Sportradar.OddsFeed.SDK.Messages.Feed;
using Sportradar.OddsFeed.SDK.Messages.Internal;

namespace Sportradar.OddsFeed.SDK.Messages.Rest
{
    /// <summary>
    /// Represents all messages (entities) received from the feed REST API
    /// </summary>
    public abstract class RestMessage
    {
    }

    [SuppressMessage("Style", "IDE1006: Naming rule violation", Justification = "RestMessages defaults")]
    [OverrideXmlNamespace(RootElementName = "cashout", IgnoreNamespace = false)]
    public partial class cashout : FeedMessage
    {
        /// <summary>
        /// The message name
        /// </summary>
        public static readonly string MessageName = nameof(cashout);

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
        public override PropertyUsage RequestIdUsage => PropertyUsage.Forbidden;

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

    [SuppressMessage("Style", "IDE1006: Naming rule violation", Justification = "RestMessages defaults")]
    public partial class fixturesEndpoint : RestMessage
    {
    }

    [SuppressMessage("Style", "IDE1006: Naming rule violation", Justification = "RestMessages defaults")]
    public partial class scheduleEndpoint : RestMessage
    {
    }

    [SuppressMessage("Style", "IDE1006: Naming rule violation", Justification = "RestMessages defaults")]
    public partial class tournamentSchedule : RestMessage
    {
    }

    [SuppressMessage("Style", "IDE1006: Naming rule violation", Justification = "RestMessages defaults")]
    public partial class matchSummaryEndpoint : RestMessage
    {
    }

    [SuppressMessage("Style", "IDE1006: Naming rule violation", Justification = "RestMessages defaults")]
    public partial class sportCategoriesEndpoint : RestMessage
    {
    }

    [SuppressMessage("Style", "IDE1006: Naming rule violation", Justification = "RestMessages defaults")]
    public partial class stageSummaryEndpoint : RestMessage
    {
    }

    [SuppressMessage("Style", "IDE1006: Naming rule violation", Justification = "RestMessages defaults")]
    public partial class tournamentsEndpoint : RestMessage
    {
    }

    [SuppressMessage("Style", "IDE1006: Naming rule violation", Justification = "RestMessages defaults")]
    public partial class sportsEndpoint : RestMessage
    {
    }

    [SuppressMessage("Style", "IDE1006: Naming rule violation", Justification = "RestMessages defaults")]
    public partial class tournamentInfoEndpoint : RestMessage
    {
    }

    [SuppressMessage("Style", "IDE1006: Naming rule violation", Justification = "RestMessages defaults")]
    public partial class playerProfileEndpoint : RestMessage
    {
    }

    /// <inheritdoc />
    [SuppressMessage("Style", "IDE1006: Naming rule violation", Justification = "RestMessages defaults")]
    public partial class competitorProfileEndpoint : RestMessage
    {
    }

    [SuppressMessage("Style", "IDE1006: Naming rule violation", Justification = "RestMessages defaults")]
    public partial class simpleTeamProfileEndpoint : RestMessage
    {
    }

    [SuppressMessage("Style", "IDE1006: Naming rule violation", Justification = "RestMessages defaults")]
    [OverrideXmlNamespace(RootElementName = "draw_summary", IgnoreNamespace = false)]
    public partial class draw_summary : RestMessage
    {
    }

    [SuppressMessage("Style", "IDE1006: Naming rule violation", Justification = "RestMessages defaults")]
    [OverrideXmlNamespace(RootElementName = "draw_fixture", IgnoreNamespace = false)]
    public partial class draw_fixture : RestMessage
    {
    }

    [SuppressMessage("Style", "IDE1006: Naming rule violation", Justification = "RestMessages defaults")]
    [OverrideXmlNamespace(RootElementName = "draw_fixtures", IgnoreNamespace = false)]
    public partial class draw_fixtures : RestMessage
    {
    }

    [SuppressMessage("Style", "IDE1006: Naming rule violation", Justification = "RestMessages defaults")]
    [OverrideXmlNamespace(RootElementName = "lotteries", IgnoreNamespace = false)]
    public partial class lotteries : RestMessage
    {
    }

    [SuppressMessage("Style", "IDE1006: Naming rule violation", Justification = "RestMessages defaults")]
    [OverrideXmlNamespace(RootElementName = "lottery_schedule", IgnoreNamespace = false)]
    public partial class lottery_schedule : RestMessage
    {
    }

    [SuppressMessage("Style", "IDE1006: Naming rule violation", Justification = "RestMessages defaults")]
    [OverrideXmlNamespace(RootElementName = "market_descriptions", IgnoreNamespace = false)]
    public partial class market_descriptions : RestMessage
    {
    }

    [SuppressMessage("Style", "IDE1006: Naming rule violation", Justification = "RestMessages defaults")]
    [OverrideXmlNamespace(RootElementName = "variant_descriptions", IgnoreNamespace = false)]
    public partial class variant_descriptions : RestMessage
    {
    }

    [SuppressMessage("Style", "IDE1006: Naming rule violation", Justification = "RestMessages defaults")]
    [OverrideXmlNamespace(RootElementName = "bookmaker_details", IgnoreNamespace = false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public partial class bookmaker_details : RestMessage
    {
    }

    [SuppressMessage("Style", "IDE1006: Naming rule violation", Justification = "RestMessages defaults")]
    [OverrideXmlNamespace(RootElementName = "producers", IgnoreNamespace = false)]
    public partial class producers : RestMessage
    {
    }

    [SuppressMessage("Style", "IDE1006: Naming rule violation", Justification = "RestMessages defaults")]
    [OverrideXmlNamespace(RootElementName = "tournament_seasons", IgnoreNamespace = false)]
    public partial class tournamentSeasons : RestMessage
    {
    }

    [SuppressMessage("Style", "IDE1006: Naming rule violation", Justification = "RestMessages defaults")]
    [OverrideXmlNamespace(RootElementName = "match_timeline", IgnoreNamespace = false)]
    public partial class matchTimelineEndpoint : RestMessage
    {
    }

    [SuppressMessage("Style", "IDE1006: Naming rule violation", Justification = "RestMessages defaults")]
    // Classes below are only needed in order for the deserializer to work since the original declarations do not specify XmlRoot attribute
    [XmlRoot("simple_tournament_info", Namespace = "http://schemas.sportradar.com/sportsapi/v1/unified", IsNullable = false)]
    public partial class simpleTournamentInfoEndpoint
    {
    }

    [SuppressMessage("Style", "IDE1006: Naming rule violation", Justification = "RestMessages defaults")]
    [XmlRoot("standard_tournament_info", Namespace = "http://schemas.sportradar.com/sportsapi/v1/unified", IsNullable = false)]
    public partial class standardTournamentInfoEndpoint
    {
    }

    [SuppressMessage("Style", "IDE1006: Naming rule violation", Justification = "RestMessages defaults")]
    [XmlRoot("race_tournament_info", Namespace = "http://schemas.sportradar.com/sportsapi/v1/unified", IsNullable = false)]
    public partial class raceTournamentInfoEndpoint
    {
    }

    [SuppressMessage("Style", "IDE1006: Naming rule violation", Justification = "RestMessages defaults")]
    [OverrideXmlNamespace(RootElementName = "response", IgnoreNamespace = false)]
    public partial class response : RestMessage
    {
    }

    [SuppressMessage("Style", "IDE1006: Naming rule violation", Justification = "RestMessages defaults")]
    [OverrideXmlNamespace(RootElementName = "sportEventStatus", IgnoreNamespace = true)]
    public partial class restSportEventStatus
    {
    }

    [SuppressMessage("Style", "IDE1006: Naming rule violation", Justification = "RestMessages defaults")]
    [OverrideXmlNamespace(RootElementName = "period_summary", IgnoreNamespace = false)]
    public partial class stagePeriodEndpoint : RestMessage
    {
    }

    [SuppressMessage("Style", "IDE1006: Naming rule violation", Justification = "RestMessages defaults")]
    public partial class raceScheduleEndpoint : RestMessage
    {
    }

    [SuppressMessage("Style", "IDE1006: Naming rule violation", Justification = "RestMessages defaults")]
    public partial class AvailableSelectionsType : RestMessage
    {
    }

    [SuppressMessage("Style", "IDE1006: Naming rule violation", Justification = "RestMessages defaults")]
    public partial class CalculationResponseType : RestMessage
    {
    }

    public partial class FilteredCalculationResponseType : RestMessage
    {
    }

    [SuppressMessage("Style", "IDE1006: Naming rule violation", Justification = "RestMessages defaults")]
    public partial class fixtureChangesEndpoint : RestMessage
    {
    }

    [SuppressMessage("Style", "IDE1006: Naming rule violation", Justification = "RestMessages defaults")]
    public partial class sportTournamentsEndpoint : RestMessage
    {
    }

    [SuppressMessage("Style", "IDE1006: Naming rule violation", Justification = "RestMessages defaults")]
    public partial class resultChangesEndpoint : RestMessage
    {
    }
}
