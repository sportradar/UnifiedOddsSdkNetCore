/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using System.Collections.Generic;
using System.Globalization;
using Sportradar.OddsFeed.SDK.Messages;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching.Sports
{
    /// <summary>
    /// Contains basic information about a sport tournament
    /// </summary>
    /// <seealso cref="SportEntityData" />
    public class TournamentData : SportEntityData
    {
        /// <summary>
        /// Gets a <see cref="DateTime"/> specifying the scheduled start time of the associated tournament or a null reference if start time is not known
        /// </summary>
        public DateTime? Scheduled;

        /// <summary>
        /// Gets a <see cref="DateTime"/> specifying the scheduled end time of the associated tournament or a null reference if end time is not known
        /// </summary>
        public DateTime? ScheduledEnd;

        /// <summary>
        /// Initializes a new instance of the <see cref="TournamentData"/> class.
        /// </summary>
        /// <param name="id">A <see cref="URN"/> specifying the id of the associated tournament</param>
        /// <param name="scheduled">a <see cref="DateTime"/> specifying the scheduled start time of the associated tournament or a null reference if start time is not known</param>
        /// <param name="scheduledEnd">a <see cref="DateTime"/> specifying the scheduled end time of the associated tournament or a null reference if end time is not known</param>
        /// <param name="names">a <see cref="IReadOnlyDictionary{CultureInfo, String}"/> containing translated sport name</param>
        public TournamentData(URN id, DateTime? scheduled, DateTime? scheduledEnd, IReadOnlyDictionary<CultureInfo, string> names)
            : base(id, names)
        {
            Scheduled = scheduled;
            ScheduledEnd = scheduledEnd;
        }
    }
}