// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using Sportradar.OddsFeed.SDK.Messages.Rest;

namespace Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Dto
{
    /// <summary>
    /// A data-access-object representing a player as a member of competitor
    /// </summary>
    /// <seealso cref="PlayerDto" />
    internal class PlayerCompetitorDto : PlayerDto
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
        /// Initializes a new instance of the <see cref="PlayerCompetitorDto"/> class
        /// </summary>
        /// <param name="record">A <see cref="playerCompetitor"/> containing information about a player as a member of competitor</param>
        internal PlayerCompetitorDto(playerCompetitor record)
            : base(new player { id = record.id, name = record.name })
        {
            Abbreviation = record.abbreviation;
            Nationality = record.nationality;
        }
    }
}
