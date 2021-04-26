/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
namespace Sportradar.OddsFeed.SDK.Entities.REST
{
    /// <summary>
    /// Defines a contract implemented by classes representing a hole of a golf course
    /// </summary>
    public interface IHole
    {
        /// <summary>
        /// Gets the number of the hole
        /// </summary>
        int Number { get; }

        /// <summary>
        /// Gets the par
        /// </summary>
        /// <value>The par</value>
        int Par { get; }
    }
}
