// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;

namespace Sportradar.OddsFeed.SDK.Entities.Rest.Caching.Exportable
{
    /// <summary>
    /// Class used to export/import match cache item properties
    /// </summary>
    [Serializable]
    public class ExportableMatch : ExportableCompetition
    {
        /// <summary>
        /// A <see cref="ExportableSeason"/> representation of the season
        /// </summary>
        public ExportableSeason Season { get; set; }

        /// <summary>
        /// A <see cref="ExportableRound"/> representation of the tournament round
        /// </summary>
        public ExportableRound TournamentRound { get; set; }

        /// <summary>
        /// A <see cref="string"/> representation of the tournament id
        /// </summary>
        public string TournamentId { get; set; }

        /// <summary>
        /// A <see cref="ExportableFixture"/> representation of the fixture
        /// </summary>
        public ExportableFixture Fixture { get; set; }

        /// <summary>
        /// A <see cref="ExportableEventTimeline"/> representation of the event timeline
        /// </summary>
        public ExportableEventTimeline EventTimeline { get; set; }

        /// <summary>
        /// A <see cref="ExportableDelayedInfo"/> representation of the delayed info
        /// </summary>
        public ExportableDelayedInfo DelayedInfo { get; set; }

        /// <summary>
        /// A <see cref="ExportableCoverageInfo"/> representation of the coverage info
        /// </summary>
        public ExportableCoverageInfo CoverageInfo { get; set; }
    }
}
