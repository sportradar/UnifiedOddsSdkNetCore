/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using System.Collections.Generic;
using Dawn;
using Sportradar.OddsFeed.SDK.Common.Internal;
using Sportradar.OddsFeed.SDK.Messages.REST;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Internal.DTO
{
    /// <summary>
    /// Class CurrentSeasonInfoDTO
    /// </summary>
    /// <seealso cref="SportEntityDTO" />
    public class CurrentSeasonInfoDTO : SportEntityDTO
    {
        /// <summary>
        /// Gets a <see cref="string"/> representation of the current season year
        /// </summary>
        public string Year { get; }

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
        /// Gets the <see cref="SeasonCoverageDTO"/> instance containing information about coverage available for the season associated with the current instance
        /// </summary>
        /// <returns>The <see cref="SeasonCoverageDTO"/> instance containing information about coverage available for the season associated with the current instance</returns>
        public SeasonCoverageDTO SeasonCoverage { get; }

        /// <summary>
        /// Gets the <see cref="IEnumerable{GroupDTO}"/> specifying groups of tournament associated with the current instance
        /// </summary>
        /// <returns>The <see cref="IEnumerable{GroupDTO}"/> specifying groups of tournament associated with the current instance</returns>
        public IEnumerable<GroupDTO> Groups { get; }

        /// <summary>
        /// Gets the <see cref="RoundDTO"/> specifying the current round of the tournament associated with the current instance
        /// </summary>
        /// <returns>The <see cref="RoundDTO"/> specifying the current round of the tournament associated with the current instance</returns>
        public RoundDTO CurrentRound { get; }

        /// <summary>
        /// Gets the list of competitors
        /// </summary>
        /// <value>The list of competitors</value>
        public IEnumerable<CompetitorDTO> Competitors { get; }

        /// <summary>
        /// Gets the list of all <see cref="CompetitionDTO"/> that belongs to the season schedule
        /// </summary>
        /// <returns>The list of all <see cref="CompetitionDTO"/> that belongs to the season schedule</returns>
        public IEnumerable<CompetitionDTO> Schedule { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="CurrentSeasonInfoDTO"/> class
        /// </summary>
        /// <param name="season">The season</param>
        public CurrentSeasonInfoDTO(seasonExtended season)
            : base (season.id, season.name)
        {
            Guard.Argument(season, nameof(season)).NotNull();

            Year = season.year;
            StartDate = season.start_date;
            EndDate = season.end_date;
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
        /// Initializes a new instance of the <see cref="CurrentSeasonInfoDTO"/> class
        /// </summary>
        /// <param name="season">The season</param>
        public CurrentSeasonInfoDTO(SeasonDTO season)
            : base(season.Id.ToString(), season.Name)
        {
            Year = season.Year;
            StartDate = season.StartDate;
            EndDate = season.EndDate;
        }
    }
}
