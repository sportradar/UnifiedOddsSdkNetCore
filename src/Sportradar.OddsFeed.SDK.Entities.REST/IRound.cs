/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System.Collections.Generic;
using System.Globalization;

namespace Sportradar.OddsFeed.SDK.Entities.REST
{
    /// <summary>
    /// Defines a contract implemented by classes providing basic tournament round information
    /// </summary>
    public interface IRound : IEntityPrinter
    {
        /// <summary>
        /// Gets the type of the round
        /// </summary>
        string Type { get; }

        /// <summary>
        /// Gets a value specifying the round number or a null reference if round number is not defined
        /// </summary>
        int? Number { get; }

        /// <summary>
        /// Gets the name of the group associated with the current round
        /// </summary>
        string GroupName { get; }

        /// <summary>
        /// Gets the id of the other match
        /// </summary>
        string OtherMatchId { get; }


        /// <summary>
        /// Gets the name of the current <see cref="IRound"/> per locale
        /// </summary>
        IDictionary<CultureInfo, string> Name { get; }

        /// <summary>
        /// Gets a value specifying the number of matches in the current cup round or a null reference
        /// if number of matches is not applicable to current <see cref="IRound"/> instance
        /// </summary>
        int? CupRoundMatches { get; }

        /// <summary>
        /// Gets a value specifying the number of the match in the current cup round or a null reference
        /// if match number is not applicable to current <see cref="IRound"/> instance
        /// </summary>
        int? CupRoundMatchNumber { get; }

        /// <summary>
        /// Gets the betradar identifier
        /// </summary>
        int BetradarId { get; }

        /// <summary>
        /// Gets the phase or group long name of the current <see cref="IRound"/> per locale
        /// </summary>
        IDictionary<CultureInfo, string> PhaseOrGroupLongName { get; }

        /// <summary>
        /// Gets the name for specific locale
        /// </summary>
        /// <param name="culture">The culture</param>
        /// <returns>Return the Name if exists, or null</returns>
        string GetName(CultureInfo culture);

        /// <summary>
        /// Gets the phase or group long name for specific locale
        /// </summary>
        /// <param name="culture">The culture</param>
        /// <returns>Return the phase or group long name if exists, or null</returns>
        string GetPhaseOrGroupLongName(CultureInfo culture);
    }
}