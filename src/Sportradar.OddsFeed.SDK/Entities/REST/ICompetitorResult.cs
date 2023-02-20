/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
namespace Sportradar.OddsFeed.SDK.Entities.REST
{
    /// <summary>
    /// Defines contract used by classes that provide competitor result information
    /// </summary>
    public interface ICompetitorResult
    {
        /// <summary>
        /// Get the type
        /// </summary>
        public string Type { get; }

        /// <summary>
        /// Gets the value
        /// </summary>
        /// <value>The value</value>
        public string Value { get; }

        /// <summary>
        /// Gets the specifiers
        /// </summary>
        /// <value>The specifiers</value>
        public string Specifiers { get; }
    }
}
