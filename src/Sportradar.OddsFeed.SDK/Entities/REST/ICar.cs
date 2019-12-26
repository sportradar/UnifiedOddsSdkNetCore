/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/

namespace Sportradar.OddsFeed.SDK.Entities.REST
{
    /// <summary>
    /// Represents a car
    /// </summary>
    public interface ICar
    {
        /// <summary>
        /// Gets the car name
        /// </summary>
        /// <value>The car name</value>
        string Name { get; }

        /// <summary>
        /// Gets the car chassis
        /// </summary>
        /// <value>The car chassis</value>
        string Chassis { get; }

        /// <summary>
        /// Gets the car engine name
        /// </summary>
        /// <value>The car engine name</value>
        string EngineName { get; }
    }
}
