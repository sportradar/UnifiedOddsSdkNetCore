/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System.Globalization;
using System.Threading.Tasks;

namespace Sportradar.OddsFeed.SDK.Entities.REST
{
    /// <summary>
    /// Defines a contract implemented by classes representing a stage status
    /// </summary>
    /// <seealso cref="ICompetitionStatus" />
    public interface IStageStatus : ICompetitionStatus
    {
        /// <summary>
        /// Asynchronously gets the stage match status
        /// </summary>
        /// <param name="culture">The culture used to get stage match status id and description</param>
        /// <returns>Returns the stage match status id and description in selected culture</returns>
        Task<ILocalizedNamedValue> GetMatchStatusAsync(CultureInfo culture);
    }
}
