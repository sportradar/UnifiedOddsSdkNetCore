// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System.Collections.Generic;
using System.Threading.Tasks;
using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Entities.Rest.CustomBet;

namespace Sportradar.OddsFeed.SDK.Api.Managers
{
    /// <summary>
    /// Defines methods used to perform various custom bet operations
    /// </summary>
    public interface ICustomBetManager
    {
        /// <summary>
        /// Returns an <see cref="ICustomBetSelectionBuilder"/> instance used to build selections
        /// </summary>
        /// <returns>An <see cref="ICustomBetSelectionBuilder"/> instance used to build selections</returns>
        ICustomBetSelectionBuilder CustomBetSelectionBuilder { get; }

        /// <summary>
        /// Returns an <see cref="IAvailableSelections"/> instance providing the available selections for the event associated with the provided <see cref="Urn"/> identifier
        /// </summary>
        /// <param name="eventId">The <see cref="Urn"/> identifier of the event for which the available selections should be returned</param>
        /// <returns>An <see cref="IAvailableSelections"/> providing the available selections of the associated event</returns>
        Task<IAvailableSelections> GetAvailableSelectionsAsync(Urn eventId);

        /// <summary>
        /// Returns an <see cref="ICalculation"/> instance providing the probability for the specified selections
        /// </summary>
        /// <param name="selections">The <see cref="IEnumerable{ISelection}"/> containing selections for which the probability should be calculated</param>
        /// <returns>An <see cref="ICalculation"/> providing the probability for the specified selections</returns>
        Task<ICalculation> CalculateProbabilityAsync(IEnumerable<ISelection> selections);

        /// <summary>
        /// Returns an <see cref="ICalculationFilter"/> instance providing the probability for the specified selections and filter out conflicting outcomes.
        /// </summary>
        /// <param name="selections">The <see cref="IEnumerable{ISelection}"/> containing selections for which the probability should be calculated</param>
        /// <returns>An <see cref="ICalculationFilter"/> providing the probability for the specified selections</returns>
        Task<ICalculationFilter> CalculateProbabilityFilterAsync(IEnumerable<ISelection> selections);
    }
}
