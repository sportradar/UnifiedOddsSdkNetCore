// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using Sportradar.OddsFeed.SDK.Entities.Rest.CustomBet;

namespace Sportradar.OddsFeed.SDK.Api.Managers
{
    /// <summary>
    /// Defines a builder for constructing a custom bet probability calculation request that supports
    /// both AND selections and OR selection groups.
    /// </summary>
    /// <remarks>
    /// The builder preserves insertion order of legs. Each call to <see cref="AndSelection"/> appends
    /// a single AND leg; each call to <see cref="AndAnyOfSelections"/> appends one OR group leg where
    /// any one of the provided selections satisfies that leg.
    /// </remarks>
    public interface ICalculateRequestBuilder
    {
        /// <summary>
        /// Appends a single AND selection leg to the request.
        /// </summary>
        /// <param name="selection">The <see cref="ISelection"/> to add as an AND leg</param>
        /// <returns>The current <see cref="ICalculateRequestBuilder"/> instance for fluent chaining</returns>
        ICalculateRequestBuilder AndSelection(ISelection selection);

        /// <summary>
        /// Appends an OR group leg to the request. Any one of the provided selections satisfies this leg.
        /// </summary>
        /// <param name="selections">The <see cref="ISelection"/> instances forming the OR group</param>
        /// <returns>The current <see cref="ICalculateRequestBuilder"/> instance for fluent chaining</returns>
        ICalculateRequestBuilder AndAnyOfSelections(params ISelection[] selections);
    }
}
