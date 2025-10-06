// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using Sportradar.OddsFeed.SDK.Common;

namespace Sportradar.OddsFeed.SDK.Entities.Rest
{
    /// <summary>
    /// Defines a contract implemented by classes representing result change
    /// </summary>
    public interface IResultChange : IEntityPrinter
    {
        /// <summary>
        /// Gets the <see cref="Urn"/> specifying the sport event
        /// </summary>
        Urn SportEventId { get; }

        /// <summary>
        /// Gets the <see cref="DateTime"/> specifying the last update time
        /// </summary>
        DateTime UpdateTime { get; }
    }
}
