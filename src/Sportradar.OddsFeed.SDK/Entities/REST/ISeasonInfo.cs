// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using System.Collections.Generic;
using System.Globalization;
using Sportradar.OddsFeed.SDK.Common;

namespace Sportradar.OddsFeed.SDK.Entities.Rest
{
    /// <summary>
    /// Defines a contract for classes implementing
    /// </summary>
    public interface ISeasonInfo
    {
        /// <summary>
        /// Gets the <see cref="Urn"/> identifying the current instance
        /// </summary>
        /// <value>The <see cref="Urn"/> identifying the current instance</value>
        Urn Id { get; }

        /// <summary>
        /// Gets the name.
        /// </summary>
        /// <param name="culture">The culture</param>
        /// <returns>System.String</returns>
        string GetName(CultureInfo culture);

        /// <summary>
        /// Gets the list of translated names
        /// </summary>
        /// <value>The list of translated names</value>
        IReadOnlyDictionary<CultureInfo, string> Names { get; }

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
        /// Gets a <see cref="string"/> representation of the current season year
        /// </summary>
        string Year { get; }

        /// <summary>
        /// Gets the associated tournament identifier.
        /// </summary>
        /// <value>The associated tournament identifier.</value>
        Urn TournamentId { get; }
    }
}
