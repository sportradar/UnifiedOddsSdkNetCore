/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/

using System;
using System.Collections.Generic;
using Sportradar.OddsFeed.SDK.Entities.REST.Enums;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Caching.Exportable
{
    /// <summary>
    /// Class used to export/import pitcher cache item properties
    /// </summary>
    [Serializable]
    public class ExportableTimelineEventCI
    {
        /// <summary>
        /// A <see cref="int" /> specifying the id
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// A <see cref="decimal" /> specifying the home score
        /// </summary>
        public decimal? HomeScore { get; set; }

        /// <summary>
        /// A <see cref="decimal" /> specifying the away score
        /// </summary>
        public decimal? AwayScore { get; set; }

        /// <summary>
        /// A <see cref="int" /> specifying the match time
        /// </summary>
        public int? MatchTime { get; set; }

        /// <summary>
        /// A <see cref="string" /> specifying the period
        /// </summary>
        public string Period { get; set; }

        /// <summary>
        /// A <see cref="string" /> specifying the period name
        /// </summary>
        public string PeriodName { get; set; }

        /// <summary>
        /// A <see cref="string" /> specifying the points
        /// </summary>
        public string Points { get; set; }

        /// <summary>
        /// A <see cref="string" /> specifying the stoppage time
        /// </summary>
        public string StoppageTime { get; set; }

        /// <summary>
        /// A <see cref="HomeAway" /> specifying the team
        /// </summary>
        public HomeAway? Team { get; set; }

        /// <summary>
        /// A <see cref="string" /> specifying the type
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// A <see cref="string" /> specifying the value
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        /// A <see cref="int" /> specifying the x
        /// </summary>
        public int? X { get; set; }

        /// <summary>
        /// A <see cref="int" /> specifying the y
        /// </summary>
        public int? Y { get; set; }

        /// <summary>
        /// A <see cref="DateTime" /> specifying the time
        /// </summary>
        public DateTime Time { get; set; }

        /// <summary>
        /// A <see cref="IEnumerable{T}" /> specifying the assists
        /// </summary>
        public IEnumerable<ExportableEventPlayerAssistCI> Assists { get; set; }

        /// <summary>
        /// A <see cref="ExportableCI" /> specifying the goal scorer
        /// </summary>
        public ExportableCI GoalScorer { get; set; }

        /// <summary>
        /// A <see cref="ExportableCI" /> specifying the player
        /// </summary>
        public ExportableCI Player { get; set; }

        /// <summary>
        /// A <see cref="int" /> specifying the match status code
        /// </summary>
        public int? MatchStatusCode { get; set; }

        /// <summary>
        /// A <see cref="string" /> specifying the match clock
        /// </summary>
        public string MatchClock { get; set; }
    }
}
