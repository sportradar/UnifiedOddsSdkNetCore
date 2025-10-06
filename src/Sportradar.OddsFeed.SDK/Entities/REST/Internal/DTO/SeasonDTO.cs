// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using Dawn;
using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Common.Internal;
using Sportradar.OddsFeed.SDK.Messages.Rest;

namespace Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Dto
{
    /// <summary>
    /// A data-access-object representing a competition season
    /// </summary>
    /// <seealso cref="SportEntityDto" />
    internal class SeasonDto : SportEntityDto
    {
        /// <summary>
        /// Gets a <see cref="DateTime"/> specifying the start date of the represented season
        /// </summary>
        internal DateTime StartDate { get; }

        /// <summary>
        /// Gets a <see cref="DateTime"/> specifying the end date of the represented season
        /// </summary>
        internal DateTime EndDate { get; }

        /// <summary>
        /// Gets a string representation of the season's year
        /// </summary>
        internal string Year { get; }

        /// <summary>
        /// Gets the associated tournament identifier.
        /// </summary>
        /// <value>The associated tournament identifier.</value>
        internal Urn TournamentId { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SeasonDto"/> class
        /// </summary>
        /// <param name="season">A <see cref="seasonExtended"/> containing information about a season.</param>
        internal SeasonDto(seasonExtended season)
            : base(season.id, season.name)
        {
            Guard.Argument(season, nameof(season)).NotNull();

            StartDate = season.start_date;
            EndDate = season.end_date;
            Year = season.year;
            if (season.start_timeSpecified)
            {
                StartDate = SdkInfo.CombineDateAndTime(season.start_date, season.start_time);
            }
            if (season.end_timeSpecified)
            {
                EndDate = SdkInfo.CombineDateAndTime(season.end_date, season.end_time);
            }

            if (!string.IsNullOrEmpty(season.tournament_id))
            {
                Urn.TryParse(season.tournament_id, out var tId);
                TournamentId = tId;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SeasonDto"/> class
        /// </summary>
        /// <param name="season">A <see cref="seasonExtended"/> containing information about a season.</param>
        internal SeasonDto(currentSeason season)
            : base(season.id, season.name)
        {
            Guard.Argument(season, nameof(season)).NotNull();

            StartDate = season.start_date;
            EndDate = season.end_date;
            Year = season.year;
            if (season.start_timeSpecified)
            {
                StartDate = SdkInfo.CombineDateAndTime(season.start_date, season.start_time);
            }

            if (season.end_timeSpecified)
            {
                EndDate = SdkInfo.CombineDateAndTime(season.end_date, season.end_time);
            }

            if (!string.IsNullOrEmpty(season.tournament_id))
            {
                Urn.TryParse(season.tournament_id, out var tId);
                TournamentId = tId;
            }
        }
    }
}
