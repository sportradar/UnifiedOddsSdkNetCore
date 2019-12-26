/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using System.Collections.Generic;
using System.Globalization;
using Sportradar.OddsFeed.SDK.Messages;

namespace Sportradar.OddsFeed.SDK.Entities.REST
{
    /// <summary>
    /// Defines a contract implemented by classes providing information about current season
    /// </summary>
    public interface ICurrentSeasonInfo
    {
        /// <summary>
        /// Gets a <see cref="URN"/> uniquely identifying the current season
        /// </summary>
        URN Id { get; }

        /// <summary>
        /// Gets a <see cref="IReadOnlyDictionary{CultureInfo, String}"/> containing names of the season in different languages
        /// </summary>
        IReadOnlyDictionary<CultureInfo, string> Names { get; }

        /// <summary>
        /// Gets a <see cref="string"/> representation of the current season year
        /// </summary>
        string Year { get; }

        /// <summary>
        /// Gets the start date of the season represented by the current instance
        /// </summary>
        DateTime StartDate { get; }

        /// <summary>
        /// Gets the end date of the season represented by the current instance
        /// </summary>
        /// <value>The end date.</value>
        DateTime EndDate { get; }

        /// <summary>
        /// Gets the season name in the specified languages
        /// </summary>
        /// <param name="culture">A <see cref="CultureInfo"/> specifying the language of the returned name.</param>
        /// <returns>The season name in the specified languages.</returns>
        string GetName(CultureInfo culture);

        /// <summary>
        /// Gets the <see cref="ISeasonCoverage"/> instance containing information about coverage available for the season associated with the current instance
        /// </summary>
        /// <returns>The <see cref="ISeasonCoverage"/> instance containing information about coverage available for the season associated with the current instance</returns>
        ISeasonCoverage Coverage { get; }

        /// <summary>
        /// Gets the <see cref="IEnumerable{IGroup}"/> specifying groups of tournament associated with the current instance
        /// </summary>
        /// <returns>The <see cref="IEnumerable{IGroup}"/> specifying groups of tournament associated with the current instance</returns>
        IEnumerable<IGroup> Groups { get; }

        /// <summary>
        /// Gets the <see cref="IRound"/> specifying the current round of the tournament associated with the current instance
        /// </summary>
        /// <returns>The <see cref="IRound"/> specifying the current round of the tournament associated with the current instance</returns>
        IRound CurrentRound { get; }

        /// <summary>
        /// Gets the list of competitors
        /// </summary>
        /// <value>The list of competitors</value>
        IEnumerable<ICompetitor> Competitors { get; }

        /// <summary>
        /// Gets the list of all <see cref="ICompetition"/> that belongs to the season schedule
        /// </summary>
        /// <returns>The list of all <see cref="ICompetition"/> that belongs to the season schedule</returns>
        IEnumerable<ISportEvent> Schedule { get; }
    }
}
