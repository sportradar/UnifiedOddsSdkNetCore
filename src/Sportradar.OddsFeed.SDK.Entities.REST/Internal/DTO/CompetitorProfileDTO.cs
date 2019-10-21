/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Dawn;
using System.Linq;
using Sportradar.OddsFeed.SDK.Messages.REST;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Internal.DTO
{
    /// <summary>
    /// A data-transfer-object representing competitor's (team's) profile
    /// </summary>
    public class CompetitorProfileDTO
    {
        /// <summary>
        /// A <see cref="CompetitorDTO"/> representing the competitor represented by the current profile
        /// </summary>
        public readonly CompetitorDTO Competitor;

        /// <summary>
        /// Gets a <see cref="IEnumerable{PlayerProfileDTO}"/> representing players which are part of the represented competitor
        /// </summary>
        public readonly IEnumerable<PlayerProfileDTO> Players;

        /// <summary>
        /// Gets the jerseys of the players
        /// </summary>
        /// <value>The jerseys</value>
        public IEnumerable<JerseyDTO> Jerseys { get; }

        /// <summary>
        /// Gets the manager
        /// </summary>
        /// <value>The manager</value>
        public ManagerDTO Manager { get; }

        /// <summary>
        /// Gets the venue
        /// </summary>
        /// <value>The venue</value>
        public VenueDTO Venue { get; }

        /// <summary>
        /// Gets the race driver profile
        /// </summary>
        /// <value>The race driver profile</value>
        public RaceDriverProfileDTO RaceDriverProfile { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="CompetitorProfileDTO"/> class
        /// </summary>
        /// <param name="record">A <see cref="competitorProfileEndpoint"/> containing information about the profile</param>
        public CompetitorProfileDTO(competitorProfileEndpoint record)
        {
            Guard.Argument(record).NotNull();
            Guard.Argument(record.competitor).NotNull();

            Competitor = new CompetitorDTO(record.competitor);
            if (record.players != null && record.players.Any())
            {
                Players = new ReadOnlyCollection<PlayerProfileDTO>(record.players.Select(p => new PlayerProfileDTO(p, record.generated_atSpecified ? record.generated_at : (DateTime?) null)).ToList());
            }
            if (record.jerseys != null)
            {
                Jerseys = new ReadOnlyCollection<JerseyDTO>(record.jerseys.Select(p => new JerseyDTO(p)).ToList());
            }
            if (record.manager != null)
            {
                Manager = new ManagerDTO(record.manager);
            }
            if (record.venue != null)
            {
                Venue = new VenueDTO(record.venue);
            }
            if (record.race_driver_profile != null)
            {
                RaceDriverProfile = new RaceDriverProfileDTO(record.race_driver_profile);
            }
        }
    }
}