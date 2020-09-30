using System;
using System.Collections.Generic;
using System.Text;
using Sportradar.OddsFeed.SDK.Entities.REST.Caching.Exportable;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.DTO;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching.CI
{
    /// <summary>
    /// A cache item representing a hole (used in golf course)
    /// </summary>
    internal class HoleCI
    {
        /// <summary>
        /// Gets the number of the hole
        /// </summary>
        /// <value>The number</value>
        internal int Number { get; }

        /// <summary>
        /// Gets the par
        /// </summary>
        /// <value>The par</value>
        internal int Par { get; }

        internal HoleCI(HoleDTO hole)
        {
            Number = hole.Number;
            Par = hole.Par;
        }

        internal HoleCI(ExportableHoleCI hole)
        {
            Number = hole.Number;
            Par = hole.Par;
        }
    }
}
