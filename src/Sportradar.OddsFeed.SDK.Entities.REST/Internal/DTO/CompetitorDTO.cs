/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.Linq;
using Sportradar.OddsFeed.SDK.Messages.REST;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Internal.DTO
{
    /// <summary>
    /// A data-access-object representing a sport competitor
    /// </summary>
    /// <seealso cref="PlayerDTO" />
    public class CompetitorDTO : PlayerDTO
    {
        /// <summary>
        /// Gets the competitor's abbreviation
        /// </summary>
        /// <value>The abbreviation</value>
        public string Abbreviation { get; }

        /// <summary>
        /// Gets the name of the competitor's country
        /// </summary>
        public string CountryName { get; }

        /// <summary>
        /// Gets a value indicating whether current competitor is virtual
        /// </summary>
        public bool IsVirtual { get; }

        /// <summary>
        /// The reference ids
        /// </summary>
        public readonly IDictionary<string, string> ReferenceIds;

        /// <summary>
        /// Gets the country code
        /// </summary>
        /// <value>The country code</value>
        public string CountryCode { get; }

        /// <summary>
        /// Gets the players
        /// </summary>
        /// <value>The players</value>
        public IEnumerable<PlayerCompetitorDTO> Players { get; }

        /// <summary>
        /// Gets the gender
        /// </summary>
        /// <value>The gender</value>
        public string Gender { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="CompetitorDTO"/> class from the <see cref="team"/> instance
        /// </summary>
        /// <param name="record">A <see cref="team"/> containing information about a team</param>
        internal CompetitorDTO(team record)
            :base(new player {id = record.id, name = record.name })
        {
            Contract.Requires(record != null);

            Abbreviation = record.abbreviation;
            CountryName = record.country;
            IsVirtual = record.virtualSpecified && record.@virtual;

            ReferenceIds = record.reference_ids == null
                ? null
                : new ReadOnlyDictionary<string, string>(record.reference_ids.ToDictionary(r => r.name, r => r.value));
            CountryCode = record.country_code;

            if (record.players != null && record.players.Any())
            {
                Players = record.players.Select(s => new PlayerCompetitorDTO(s));
            }
            Gender = record.gender;
        }
    }
}