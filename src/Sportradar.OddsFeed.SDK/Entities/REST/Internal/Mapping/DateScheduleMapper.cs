/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System.Linq;
using Dawn;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Dto;
using Sportradar.OddsFeed.SDK.Messages.Rest;

namespace Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Mapping
{
    /// <summary>
    /// Maps <see cref="scheduleEndpoint"/> instances to <see cref="SportEventSummaryDto" /> instance
    /// </summary>
    internal class DateScheduleMapper : ISingleTypeMapper<EntityList<SportEventSummaryDto>>
    {
        /// <summary>
        /// A <see cref="scheduleEndpoint"/> instance containing schedule for a day
        /// </summary>
        private readonly scheduleEndpoint _data;

        /// <summary>
        /// Initializes a new instance of the <see cref="DateScheduleMapper"/> class.
        /// </summary>
        /// <param name="data">>A <see cref="scheduleEndpoint"/> instance containing schedule for a day</param>
        internal DateScheduleMapper(scheduleEndpoint data)
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
            var events = _data.sport_event.Select(RestMapperHelper.MapSportEvent);
            return new EntityList<SportEventSummaryDto>(events);
        }
    }
}
