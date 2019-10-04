/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
namespace Sportradar.OddsFeed.SDK.Entities.REST.Market
{
    /// <summary>
    /// Defines a contract implemented by classes representing market / outright / outcome specifiers
    /// representing a part of unique identifiers
    /// </summary>
    public interface ISpecifier {
        /// <summary>
        /// Gets the type name of the specifier represented by the current instance
        /// </summary>
        string Type { get; }

        /// <summary>
        /// Gets the name of the specifier represented by the current instance
        /// </summary>
        string Name { get; }
    }
}