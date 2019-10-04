/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/

namespace Sportradar.OddsFeed.SDK.Entities.REST
{
    /// <summary>
    /// Defines methods used by classes that provide event result information
    /// </summary>
    public interface IEventResultV1 : IEventResult
    {
        /// <summary>
        /// Gets the grid
        /// </summary>
        /// <value>The grid</value>
        int? Grid { get; }
    }
}
