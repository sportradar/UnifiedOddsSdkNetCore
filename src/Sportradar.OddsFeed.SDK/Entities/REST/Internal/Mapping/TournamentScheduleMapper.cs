/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System.Collections.Generic;
using System.Linq;
using Dawn;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Dto;
using Sportradar.OddsFeed.SDK.Messages.Rest;

namespace Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Mapping
{
    /// <summary>
    /// Maps <see cref="tournamentSchedule"/> instances to <see cref="SportEventSummaryDto" /> instance
    /// </summary>
    internal class TournamentScheduleMapper : ISingleTypeMapper<EntityList<SportEventSummaryDto>>
    {
        /// <summary>
        /// A <see cref="tournamentSchedule"/> instance containing tournament schedule info
        /// </summary>
        private readonly tournamentSchedule _data;

        /// <summary>
        /// Initializes a new instance of the <see cref="TournamentScheduleMapper"/> class.
        /// </summary>
        /// <param name="data">>A <see cref="tournamentSchedule"/> instance containing tournament schedule info</param>
        internal TournamentScheduleMapper(tournamentSchedule data)
        {
            Guard.Argument(data, nameof(data)).NotNull();

            _data = data;
        }

        /// <summary>
        /// Maps it's data to <see cref="EntityList{SportEventSummaryDto}"/> instance
        /// </summary>
        /// <returns>Constructed <see cref="EntityList{SportEventSummaryDto}"/> instance</returns>
        public EntityList<SportEventSummaryDto> Map()
        {
            if (_data.sport_events == null || !_data.sport_events.Any())
            {
                return new EntityList<SportEventSummaryDto>(new List<SportEventSummaryDto>());
            }
            var events = _data.sport_events.Select(RestMapperHelper.MapSportEvent).ToList();
            return new EntityList<SportEventSummaryDto>(events);
        }
    }
}
