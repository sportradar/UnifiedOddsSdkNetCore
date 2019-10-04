/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using Sportradar.OddsFeed.SDK.Entities.REST.Enums;

namespace Sportradar.OddsFeed.SDK.Entities.REST
{
    /// <summary>
    /// Defines a contract for classes implementing the draw info
    /// </summary>
    public interface IDrawInfo
    {
        /// <summary>
        /// Gets the type of the draw
        /// </summary>
        /// <value>The type of the draw</value>
        DrawType DrawType { get; }

        /// <summary>
        /// Gets the type of the time
        /// </summary>
        /// <value>The type of the time</value>
        TimeType TimeType { get; }

        /// <summary>
        /// Gets the type of the game
        /// </summary>
        /// <value>The type of the game</value>
        string GameType { get; }
    }
}
