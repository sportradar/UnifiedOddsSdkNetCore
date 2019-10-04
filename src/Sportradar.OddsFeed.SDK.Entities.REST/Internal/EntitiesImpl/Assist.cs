/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System.Collections.Generic;
using System.Globalization;
using Sportradar.OddsFeed.SDK.Messages;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Internal.EntitiesImpl
{
    /// <summary>
    /// Represents an assists on a sport event
    /// </summary>
    /// <seealso cref="Player" />
    /// <seealso cref="IAssist" />
    internal class Assist : Player, IAssist
    {
        /// <summary>
        /// Gets a <see cref="string" /> specifying the type of the assist
        /// </summary>
        public string Type { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Assist"/> class.
        /// </summary>
        /// <param name="id">a <see cref="URN"/> uniquely identifying the current <see cref="ICompetitor" /> instance</param>
        /// <param name="names">A <see cref="IDictionary{CultureInfo, String}"/> containing assist names in different languages</param>
        /// <param name="type">a <see cref="string" /> specifying the type of the assist</param>
        public Assist(URN id, IDictionary<CultureInfo, string> names, string type)
            : base(id, names)
        {
            Type = type;
        }

        /// <summary>
        /// Constructs and returns a <see cref="string"/> containing compacted representation of the current instance
        /// </summary>
        /// <returns>A <see cref="string"/> containing compacted representation of the current instance.</returns>
        protected override string PrintC()
        {
            return $"{base.PrintC()}, Type={Type}";
        }

        /// <summary>
        /// Constructs and return a <see cref="string"/> containing details of the current instance
        /// </summary>
        /// <returns>A <see cref="string"/> containing details of the current instance.</returns>
        protected override string PrintF()
        {
            return $"{base.PrintF()}, Type={Type}";
        }
    }
}
