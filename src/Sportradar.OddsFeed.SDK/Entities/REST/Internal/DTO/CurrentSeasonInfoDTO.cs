// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using System.Collections.Generic;
using Dawn;
using Sportradar.OddsFeed.SDK.Common.Internal;
using Sportradar.OddsFeed.SDK.Messages.Rest;

namespace Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Dto
{
    /// <summary>
    /// Class CurrentSeasonInfoDto
    /// </summary>
    /// <seealso cref="SportEntityDto" />
    internal class CurrentSeasonInfoDto : SportEntityDto
    {
        //TODO: review if this is needed and maybe can be replaced with SeasonDto
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
        /// Gets the <see cref="SeasonCoverageDto"/> instance containing information about coverage available for the season associated with the current instance
        /// </summary>
        /// <returns>The <see cref="SeasonCoverageDto"/> instance containing information about coverage available for the season associated with the current instance</returns>
        public SeasonCoverageDto SeasonCoverage { get; }

        /// <summary>
        /// Gets the <see cref="IEnumerable{GroupDto}"/> specifying groups of tournament associated with the current instance
        /// </summary>
        /// <returns>The <see cref="IEnumerable{GroupDto}"/> specifying groups of tournament associated with the current instance</returns>
        public IEnumerable<GroupDto> Groups { get; }

        /// <summary>
        /// Gets the <see cref="RoundDto"/> specifying the current round of the tournament associated with the current instance
        /// </summary>
        /// <returns>The <see cref="RoundDto"/> specifying the current round of the tournament associated with the current instance</returns>
        public RoundDto CurrentRound { get; }

        /// <summary>
        /// Gets the list of competitors
        /// </summary>
        /// <value>The list of competitors</value>
        public IEnumerable<CompetitorDto> Competitors { get; }

        /// <summary>
        /// Gets the list of all <see cref="CompetitionDto"/> that belongs to the season schedule
        /// </summary>
        /// <returns>The list of all <see cref="CompetitionDto"/> that belongs to the season schedule</returns>
        public IEnumerable<CompetitionDto> Schedule { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="CurrentSeasonInfoDto"/> class
        /// </summary>
        /// <param name="season">The season</param>
        public CurrentSeasonInfoDto(seasonExtended season)
            : base(season.id, season.name)
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
        /// Initializes a new instance of the <see cref="CurrentSeasonInfoDto"/> class
        /// </summary>
        /// <param name="season">The season</param>
        public CurrentSeasonInfoDto(SeasonDto season)
            : base(season.Id.ToString(), season.Name)
        {
            Year = season.Year;
            StartDate = season.StartDate;
            EndDate = season.EndDate;
        }
    }
}
