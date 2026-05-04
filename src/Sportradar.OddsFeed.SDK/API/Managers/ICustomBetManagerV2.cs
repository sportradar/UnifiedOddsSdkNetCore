// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System.Threading.Tasks;
using Sportradar.OddsFeed.SDK.Entities.Rest.CustomBet;

namespace Sportradar.OddsFeed.SDK.Api.Managers
{
    /// <summary>
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
    }
}

