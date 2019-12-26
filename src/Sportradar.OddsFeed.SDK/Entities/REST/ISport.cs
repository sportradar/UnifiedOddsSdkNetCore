/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System.Collections.Generic;

namespace Sportradar.OddsFeed.SDK.Entities.REST
{
    /// <summary>
    /// Defines a contract implemented by classes representing a sport
    /// </summary>
    public interface ISport : ISportSummary
    {
        /// <summary>
        /// Gets a <see cref="IEnumerable{ICategory}"/> representing categories
        /// which belong to the sport represented by the current instance
        /// </summary>
        IEnumerable<ICategory> Categories { get; }
    }
}