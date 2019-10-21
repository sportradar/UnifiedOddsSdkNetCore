/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using Dawn;
using System.Linq;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.DTO;
using Sportradar.OddsFeed.SDK.Messages.REST;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Internal.Mapping
{
    /// <summary>
    /// Maps <see cref="scheduleEndpoint"/> instances to <see cref="SportEventSummaryDTO" /> instance
    /// </summary>
    internal class ListSportEventsMapper : ISingleTypeMapper<EntityList<SportEventSummaryDTO>>
    {
        /// <summary>
        /// A <see cref="scheduleEndpoint"/> instance containing schedule info
        /// </summary>
        private readonly scheduleEndpoint _data;

        /// <summary>
        /// Initializes a new instance of the <see cref="ListSportEventsMapper"/> class
        /// </summary>
        /// <param name="data">>A <see cref="scheduleEndpoint"/> instance containing schedule info</param>
        internal ListSportEventsMapper(scheduleEndpoint data)
        {
            Guard.Argument(data).NotNull();

            _data = data;
        }

        /// <summary>
        /// Maps it's data to <see cref="EntityList{SportEventSummaryDTO}"/> instance
        /// </summary>
        /// <returns>Constructed <see cref="EntityList{SportEventSummaryDTO}"/> instance</returns>
        public EntityList<SportEventSummaryDTO> Map()
        {
            var events = _data.sport_event.Select(RestMapperHelper.MapSportEvent).ToList();
            return new EntityList<SportEventSummaryDTO>(events);
        }
    }
}
