/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using System.Collections.Generic;
using Sportradar.OddsFeed.SDK.Entities.REST.Enums;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Caching.Exportable
{
    /// <summary>
    /// Class used to export/import competition cache item properties
    /// </summary>
    [Serializable]
    public class ExportableCompetitionCI : ExportableSportEventCI
    {
        /// <summary>
        /// Gets the <see cref="BookingStatus"/> specifying the booking status
        /// </summary>
        public BookingStatus? BookingStatus { get; set; }

        /// <summary>
        /// Gets the <see cref="ExportableVenueCI"/> specifying the venue
        /// </summary>
        public ExportableVenueCI Venue { get; set; }

        /// <summary>
        /// Gets the <see cref="ExportableSportEventConditionsCI"/> specifying the conditions
        /// </summary>
        public ExportableSportEventConditionsCI Conditions { get; set; }

        /// <summary>
        /// Gets the <see cref="IEnumerable{T}"/> specifying the competitors
        /// </summary>
        public IEnumerable<string> Competitors { get; set; }

        /// <summary>
        /// Gets the <see cref="IDictionary{K, V}"/> specifying the reference ids
        /// </summary>
        public IDictionary<string, string> ReferenceId { get; set; }

        /// <summary>
        /// Gets the <see cref="IDictionary{K, V}"/> specifying the competitors qualifiers
        /// </summary>
        public IDictionary<string, string> CompetitorsQualifiers { get; set; }

        /// <summary>
        /// Gets the <see cref="IDictionary{K, V}"/> specifying the competitors qualifiers
        /// </summary>
        public IDictionary<string, IDictionary<string, string>> CompetitorsReferences { get; set; }

        /// <summary>
        /// Gets a liveOdds
        /// </summary>
        /// <returns>A liveOdds</returns>
        public string LiveOdds { get; set; }

        /// <summary>
        /// Gets a <see cref="SportEventType"/> for the associated sport event.
        /// </summary>
        /// <returns>A <see cref="SportEventType"/> for the associated sport event.</returns>
        public SportEventType? SportEventType { get; set; }

        /// <summary>
        /// Gets a <see cref="StageType"/> for the associated sport event.
        /// </summary>
        /// <returns>A <see cref="StageType"/> for the associated sport event.</returns>
        public StageType? StageType { get; set; }
    }
}