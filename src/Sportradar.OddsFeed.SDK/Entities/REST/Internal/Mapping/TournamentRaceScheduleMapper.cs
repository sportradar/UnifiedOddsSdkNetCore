/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System.Collections.Generic;
using Dawn;
using System.Linq;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.DTO;
using Sportradar.OddsFeed.SDK.Messages.REST;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Internal.Mapping
{
    /// <summary>
    /// Maps <see cref="raceScheduleEndpoint"/> instances to <see cref="SportEventSummaryDTO" /> instance
    /// </summary>
    internal class TournamentRaceScheduleMapper : ISingleTypeMapper<EntityList<SportEventSummaryDTO>>
    {
        /// <summary>
        /// A <see cref="raceScheduleEndpoint"/> instance containing tournament schedule info
        /// </summary>
        private readonly raceScheduleEndpoint _data;

        /// <summary>
        /// Initializes a new instance of the <see cref="TournamentRaceScheduleMapper"/> class.
        /// </summary>
        /// <param name="data">>A <see cref="raceScheduleEndpoint"/> instance containing tournament schedule info</param>
        internal TournamentRaceScheduleMapper(raceScheduleEndpoint data)
        {
            Guard.Argument(data, nameof(data)).NotNull();

            _data = data;
        }

        /// <summary>
        /// Maps it's data to <see cref="EntityList{SportEventSummaryDTO}"/> instance
        /// </summary>
        /// <returns>Constructed <see cref="EntityList{SportEventSummaryDTO}"/> instance</returns>
        public EntityList<SportEventSummaryDTO> Map()
        {
            if(_data.sport_events == null || !_data.sport_events.Any())
            {
                return new EntityList<SportEventSummaryDTO>(new List<SportEventSummaryDTO>());
            }
            var events = _data.sport_events.Select(s => new StageDTO(s)).ToList();
            return new EntityList<SportEventSummaryDTO>(events);
        }
    }
}
