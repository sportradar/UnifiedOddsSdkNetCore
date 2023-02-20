/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Dawn;
using Sportradar.OddsFeed.SDK.Messages;
using Sportradar.OddsFeed.SDK.Messages.REST;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Internal.DTO
{
    /// <summary>
    /// A data-transfer-object representing a sport competitor
    /// </summary>
    /// <seealso cref="PlayerDTO" />
    internal class CompetitorDTO : PlayerDTO
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
        /// Gets the state
        /// </summary>
        /// <value>The state</value>
        public string State { get; protected set; }

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
        /// Gets the age group
        /// </summary>
        /// <value>The age group</value>
        public string AgeGroup { get; }

        /// <summary>
        /// Gets the sport id
        /// </summary>
        /// <value>The sport id</value>
        public URN SportId { get; }

        /// <summary>
        /// Gets the category id
        /// </summary>
        /// <value>The category id</value>
        public URN CategoryId { get; }

        /// <summary>
        /// Gets the short name
        /// </summary>
        /// <value>The short name</value>
        public string ShortName { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="CompetitorDTO"/> class from the <see cref="team"/> instance
        /// </summary>
        /// <param name="record">A <see cref="team"/> containing information about a team</param>
        internal CompetitorDTO(team record)
            : base(new player { id = record.id, name = record.name })
        {
            Guard.Argument(record, nameof(record)).NotNull();

            Abbreviation = record.abbreviation;
            CountryName = record.country;
            IsVirtual = record.virtualSpecified && record.@virtual;

            ReferenceIds = record.reference_ids == null
                ? null
                : new ReadOnlyDictionary<string, string>(record.reference_ids.ToDictionary(r => r.name, r => r.value));
            CountryCode = record.country_code;
            State = record.state;

            if (record.players != null && record.players.Any())
            {
                Players = record.players.Select(s => new PlayerCompetitorDTO(s));
            }
            Gender = record.gender;
            AgeGroup = record.age_group;

            if (record is teamExtended extended)
            {
                SportId = extended.sport == null
                    ? null
                    : URN.Parse(extended.sport.id);
                CategoryId = extended.category == null
                    ? null
                    : URN.Parse(extended.category.id);
            }

            if (!string.IsNullOrEmpty(record.short_name))
            {
                ShortName = record.short_name;
            }
        }
    }
}
