/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using System.Collections.Generic;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Internal
{
    /// <summary>
    /// Defines an interface which validates a market mapping to ensure it can be used to map a specific market
    /// </summary>
    public interface IMappingValidator
    {
        /// <summary>
        /// Validate the provided specifiers against current instance.
        /// </summary>
        /// <param name="specifiers">The <see cref="IReadOnlyDictionary{TKey,TValue}"/> containing market specifiers.</param>
        /// <returns>True if the mapping associated with the current instance can be used to map associated market; False otherwise</returns>
        /// <exception cref="InvalidOperationException">Validation cannot be performed against the provided specifiers</exception>
        bool Validate(IReadOnlyDictionary<string, string> specifiers);
    }
}
