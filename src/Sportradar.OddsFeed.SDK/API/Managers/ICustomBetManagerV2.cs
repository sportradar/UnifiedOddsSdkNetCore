// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System.Threading.Tasks;
using Sportradar.OddsFeed.SDK.Entities.Rest.CustomBet;

namespace Sportradar.OddsFeed.SDK.Api.Managers
{
    /// <summary>
    /// Defines methods used to perform various custom bet operations
    /// Extends <see cref="ICustomBetManager"/> with additional methods for prebuilt bets functionality.
    /// This interface provides a safe upgrade path for clients with custom implementations of <see cref="ICustomBetManager"/>.
    /// </summary>
    public interface ICustomBetManagerV2 : ICustomBetManager
    {
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
