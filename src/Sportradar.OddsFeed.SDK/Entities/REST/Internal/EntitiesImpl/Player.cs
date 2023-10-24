/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System.Collections.Generic;
using System.Globalization;
using Dawn;
using Sportradar.OddsFeed.SDK.Common;

namespace Sportradar.OddsFeed.SDK.Entities.Rest.Internal.EntitiesImpl
{
    /// <summary>
    /// Represents a player or a racer in a sport event
    /// </summary>
    /// <seealso cref="IPlayer" />
    internal class Player : BaseEntity, IPlayer
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Player"/> class
        /// </summary>
        /// <param name="id">the <see cref="Urn"/> uniquely identifying the current <see cref="ICompetitor" /> instance</param>
        /// <param name="names">A <see cref="IDictionary{CultureInfo, String}"/> containing player names in different languages</param>
        public Player(Urn id, IDictionary<CultureInfo, string> names)
            : base(id, names as IReadOnlyDictionary<CultureInfo, string>)
        {
            Guard.Argument(id, nameof(id)).NotNull();
            Guard.Argument(names, nameof(names)).NotNull();
        }
    }
}
