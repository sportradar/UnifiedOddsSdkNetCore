// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System.Collections.Generic;
using System.Globalization;
using Sportradar.OddsFeed.SDK.Common;

namespace Sportradar.OddsFeed.SDK.Entities.Rest
{
    /// <summary>
    /// Defines a contract implemented by classes representing a golf course
    /// </summary>
    public interface ICourse : IEntityPrinter
    {
        /// <summary>
        /// Gets a <see cref="Urn"/> uniquely identifying the current <see cref="ICourse"/> instance
        /// </summary>
        Urn Id { get; }

        /// <summary>
        /// Gets a <see cref="IReadOnlyDictionary{CultureInfo,String}"/> containing course's names in different languages
        /// </summary>
        IReadOnlyDictionary<CultureInfo, string> Names { get; }

        /// <summary>
        /// Get the list of holes associated with this course
        /// </summary>
        ICollection<IHole> Holes { get; }
    }
}
