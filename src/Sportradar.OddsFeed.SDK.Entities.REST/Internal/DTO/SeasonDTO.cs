/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using Dawn;
using Sportradar.OddsFeed.SDK.Common.Internal;
using Sportradar.OddsFeed.SDK.Messages.REST;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Internal.DTO
{
    /// <summary>
    /// A data-access-object representing a competition season
    /// </summary>
    /// <seealso cref="SportEntityDTO" />
    public class SeasonDTO : SportEntityDTO
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
        /// Initializes a new instance of the <see cref="SeasonDTO"/> class
        /// </summary>
        /// <param name="season">A <see cref="season"/> containing information about a season.</param>
        internal SeasonDTO(seasonExtended season)
            : base(season.id, season.name)
        {
            Guard.Argument(season).NotNull();
            Guard.Argument(season.year).NotNull().NotEmpty();

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
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SeasonDTO"/> class
        /// </summary>
        /// <param name="season">A <see cref="season"/> containing information about a season.</param>
        internal SeasonDTO(currentSeason season)
            : base(season.id, season.name)
        {
            Guard.Argument(season).NotNull();

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
        }
    }
}
