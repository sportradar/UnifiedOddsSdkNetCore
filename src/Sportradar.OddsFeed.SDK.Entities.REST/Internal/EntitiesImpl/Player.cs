/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Runtime.Serialization;
using Sportradar.OddsFeed.SDK.Messages;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Internal.EntitiesImpl
{
    /// <summary>
    /// Represents a player or a racer in a sport event
    /// </summary>
    /// <seealso cref="IPlayer" />
    [DataContract]
    internal class Player : BaseEntity, IPlayer
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Player"/> class
        /// </summary>
        /// <param name="id">the <see cref="URN"/> uniquely identifying the current <see cref="ICompetitor" /> instance</param>
        /// <param name="names">A <see cref="IDictionary{CultureInfo, String}"/> containing player names in different languages</param>
        public Player(URN id, IDictionary<CultureInfo, string> names)
            : base(id, names as IReadOnlyDictionary<CultureInfo, string>)
        {
            Contract.Requires(id != null);
            Contract.Requires(names != null);
        }
    }
}
