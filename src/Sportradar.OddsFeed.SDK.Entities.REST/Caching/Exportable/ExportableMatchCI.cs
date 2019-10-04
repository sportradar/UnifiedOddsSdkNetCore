/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/

namespace Sportradar.OddsFeed.SDK.Entities.REST.Caching.Exportable
{
    /// <summary>
    /// Class used to export/import match cache item properties
    /// </summary>
    public class ExportableMatchCI : ExportableCompetitionCI
    {
        /// <summary>
        /// A <see cref="ExportableCI"/> representation of the season
        /// </summary>
        public ExportableCI Season { get; set; }

        /// <summary>
        /// A <see cref="ExportableRoundCI"/> representation of the tournament round
        /// </summary>
        public ExportableRoundCI TournamentRound { get; set; }

        /// <summary>
        /// A <see cref="string"/> representation of the tournament id
        /// </summary>
        public string TournamentId { get; set; }

        /// <summary>
        /// A <see cref="ExportableFixtureCI"/> representation of the fixture
        /// </summary>
        public ExportableFixtureCI Fixture { get; set; }

        /// <summary>
        /// A <see cref="ExportableEventTimelineCI"/> representation of the event timeline
        /// </summary>
        public ExportableEventTimelineCI EventTimeline { get; set; }

        /// <summary>
        /// A <see cref="ExportableDelayedInfoCI"/> representation of the delayed info
        /// </summary>
        public ExportableDelayedInfoCI DelayedInfo { get; set; }
    }
}
