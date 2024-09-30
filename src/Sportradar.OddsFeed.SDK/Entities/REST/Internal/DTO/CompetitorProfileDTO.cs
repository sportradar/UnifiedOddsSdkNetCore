// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Dawn;
using Sportradar.OddsFeed.SDK.Messages.Rest;

namespace Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Dto
{
    /// <summary>
    /// A data-transfer-object representing competitor's (team's) profile
    /// </summary>
    internal class CompetitorProfileDto
    {
        /// <summary>
        /// A <see cref="CompetitorDto"/> representing the competitor represented by the current profile
        /// </summary>
        public readonly CompetitorDto Competitor;

        /// <summary>
        /// Gets a <see cref="IEnumerable{PlayerProfileDto}"/> representing players which are part of the represented competitor
        /// </summary>
        public readonly ICollection<PlayerProfileDto> Players;

        /// <summary>
        /// Gets the jerseys of the players
        /// </summary>
        /// <value>The jerseys</value>
        public ICollection<JerseyDto> Jerseys { get; }

        /// <summary>
        /// Gets the manager
        /// </summary>
        /// <value>The manager</value>
        public ManagerDto Manager { get; }

        /// <summary>
        /// Gets the venue
        /// </summary>
        /// <value>The venue</value>
        public VenueDto Venue { get; }

        /// <summary>
        /// Gets the race driver profile
        /// </summary>
        /// <value>The race driver profile</value>
        public RaceDriverProfileDto RaceDriverProfile { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="CompetitorProfileDto"/> class
        /// </summary>
        /// <param name="record">A <see cref="competitorProfileEndpoint"/> containing information about the profile</param>
        public CompetitorProfileDto(competitorProfileEndpoint record)
        {
            Guard.Argument(record, nameof(record)).NotNull();
            Guard.Argument(record.competitor, nameof(record.competitor)).NotNull();

            Competitor = new CompetitorDto(record.competitor);
            if (record.players != null && record.players.Any())
            {
                Players = new ReadOnlyCollection<PlayerProfileDto>(record.players.Select(p => new PlayerProfileDto(p, record.generated_atSpecified ? record.generated_at : (DateTime?)null)).ToList());
            }
            if (record.jerseys != null)
            {
                Jerseys = new ReadOnlyCollection<JerseyDto>(record.jerseys.Select(p => new JerseyDto(p)).ToList());
            }
            if (record.manager != null)
            {
                Manager = new ManagerDto(record.manager);
            }
            if (record.venue != null)
            {
                Venue = new VenueDto(record.venue);
            }
            if (record.race_driver_profile != null)
            {
                RaceDriverProfile = new RaceDriverProfileDto(record.race_driver_profile);
            }
        }
    }
}
