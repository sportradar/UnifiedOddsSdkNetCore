/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using Dawn;
using Sportradar.OddsFeed.SDK.Messages.REST;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Internal.DTO
{
    /// <summary>
    /// A data-access-object representing a player as a member of competitor
    /// </summary>
    /// <seealso cref="PlayerDTO" />
    public class PlayerCompetitorDTO : PlayerDTO
    {
        /// <summary>
        /// Gets the abbreviation
        /// </summary>
        /// <value>The abbreviation</value>
        public string Abbreviation { get; }

        /// <summary>
        /// Gets the nationality
        /// </summary>
        /// <value>The nationality</value>
        public string Nationality { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="PlayerCompetitorDTO"/> class
        /// </summary>
        /// <param name="record">A <see cref="playerCompetitor"/> containing information about a player as a member of competitor</param>
        internal PlayerCompetitorDTO(playerCompetitor record)
            :base(new player {id = record.id, name = record.name })
        {
            Guard.Argument(record).NotNull();

            Abbreviation = record.abbreviation;
            Nationality = record.nationality;
        }
    }
}
