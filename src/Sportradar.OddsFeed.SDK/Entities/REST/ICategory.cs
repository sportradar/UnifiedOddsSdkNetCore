/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System.Collections.Generic;

namespace Sportradar.OddsFeed.SDK.Entities.REST
{
    /// <summary>
    /// Defines a contract implemented by classes representing sport category
    /// </summary>
    public interface ICategory : ICategorySummary
    {
        /// <summary>
        /// Gets a <see cref="IEnumerable{ISportEvent}"/> representing the tournaments which belong to
        /// the category represented by the current instance
        /// </summary>
        IEnumerable<ISportEvent> Tournaments { get; }
    }
}
