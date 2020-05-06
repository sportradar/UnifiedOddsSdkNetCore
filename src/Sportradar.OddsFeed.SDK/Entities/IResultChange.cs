/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/

using System;
using Sportradar.OddsFeed.SDK.Messages;

namespace Sportradar.OddsFeed.SDK.Entities.REST
{
    /// <summary>
    /// Defines a contract implemented by classes representing result change
    /// </summary>
    public interface IResultChange : IEntityPrinter
    {
        /// <summary>
        /// Gets the <see cref="URN"/> specifying the sport event
        /// </summary>
        URN SportEventId { get; }

        /// <summary>
        /// Gets the <see cref="DateTime"/> specifying the last update time
        /// </summary>
        DateTime UpdateTime { get; }
    }
}
