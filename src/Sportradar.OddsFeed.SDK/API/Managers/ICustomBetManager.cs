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

        /// <summary>
        /// Returns an <see cref="IPrebuiltBetsRequestBuilder"/> instance used to build request for prebuilt bets
        /// </summary>
        /// <returns>An <see cref="IPrebuiltBetsRequestBuilder"/> instance used to build request for  prebuilt bets</returns>
        IPrebuiltBetsRequestBuilder PrebuiltBetsRequestBuilder { get; }

        /// <summary>
        /// Returns prebuilt bet recommendations for the specified request.
        /// </summary>
        /// <param name="prebuiltBetsRequest">
        /// The <see cref="IPrebuiltBetsRequest"/> containing the event identifier and optional parameters.
        /// </param>
        /// <returns>
        /// A <see cref="IPrebuiltBets"/> object for the given request
        /// </returns>
        Task<IPrebuiltBets> GetPrebuiltBets(IPrebuiltBetsRequest prebuiltBetsRequest);

        /// <summary>
        /// Returns a new <see cref="ICalculateRequestBuilder"/> instance used to build a probability
        /// calculation request that may contain both AND selections and OR selection groups.
        /// </summary>
        /// <returns>A new <see cref="ICalculateRequestBuilder"/> instance</returns>
        ICalculateRequestBuilder GetCalculateRequestBuilder();

        /// <summary>
        /// Returns an <see cref="ICalculation"/> instance providing the probability for the request built
        /// via <see cref="GetCalculateRequestBuilder"/>. Supports both AND selections and OR selection groups.
        /// </summary>
        /// <param name="request">The <see cref="ICalculateRequestBuilder"/> containing the legs of the bet</param>
        /// <returns>An <see cref="ICalculation"/> providing the probability for the specified request</returns>
        Task<ICalculation> CalculateProbabilityAsync(ICalculateRequestBuilder request);

        /// <summary>
        /// Returns an <see cref="ICalculationFilter"/> instance providing the probability for the request built
        /// via <see cref="GetCalculateRequestBuilder"/>, filtering out conflicting outcomes.
        /// Supports both AND selections and OR selection groups.
        /// </summary>
        /// <param name="request">The <see cref="ICalculateRequestBuilder"/> containing the legs of the bet</param>
        /// <returns>An <see cref="ICalculationFilter"/> providing the probability for the specified request</returns>
        Task<ICalculationFilter> CalculateProbabilityFilterAsync(ICalculateRequestBuilder request);
    }
}
