/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using System.Collections.Generic;
using System.Globalization;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching.CI;
using Sportradar.OddsFeed.SDK.Messages;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Internal.EntitiesImpl
{
    /// <summary>
    /// Class SeasonInfo
    /// </summary>
    /// <seealso cref="ISeasonInfo" />
    internal class SeasonInfo : BaseEntity, ISeasonInfo
    {
        /// <summary>
        /// Gets the start date of the season represented by the current instance
        /// </summary>
        public DateTime StartDate { get; }

        /// <summary>
        /// Gets the end date of the season represented by the current instance
        /// </summary>
        /// <value>The end date.</value>
        public DateTime EndDate { get; }

        /// <summary>
        /// Gets a <see cref="string"/> representation of the current season year
        /// </summary>
        public string Year { get; }

        /// <summary>
        /// Gets the associated tournament identifier.
        /// </summary>
        /// <value>The associated tournament identifier.</value>
        public URN TournamentId { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SeasonInfo"/> class
        /// </summary>
        /// <param name="seasonCI"></param>
        public SeasonInfo(SeasonCI seasonCI)
            : base(seasonCI.Id, seasonCI.Names as IReadOnlyDictionary<CultureInfo, string>)
        {
            StartDate = seasonCI.StartDate;
            EndDate = seasonCI.EndDate;
            Year = seasonCI.Year;
            TournamentId = seasonCI.TournamentId;
        }
    }
}
