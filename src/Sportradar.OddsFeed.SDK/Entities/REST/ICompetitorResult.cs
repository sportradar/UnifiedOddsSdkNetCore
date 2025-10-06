// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

namespace Sportradar.OddsFeed.SDK.Entities.Rest
{
    /// <summary>
    /// Defines contract used by classes that provide competitor result information
    /// </summary>
    public interface ICompetitorResult
    {
        /// <summary>
        /// Get the type
        /// </summary>
        string Type { get; }

        /// <summary>
        /// Gets the value
        /// </summary>
        /// <value>The value</value>
        string Value { get; }

        /// <summary>
        /// Gets the specifiers
        /// </summary>
        /// <value>The specifiers</value>
        string Specifiers { get; }
    }
}
