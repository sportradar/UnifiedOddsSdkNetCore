/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System.Diagnostics.Contracts;
using System.Linq;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.DTO;
using Sportradar.OddsFeed.SDK.Messages.REST;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Internal.Mapping
{
    /// <summary>
    /// Maps <see cref="scheduleEndpoint" /> instances to list of <see cref="SportEventSummaryDTO" /> instances
    /// </summary>
    internal class SportEventsScheduleMapper : ISingleTypeMapper<EntityList<SportEventSummaryDTO>>
    {
        /// <summary>
        /// A <see cref="scheduleEndpoint"/> instance containing schedule for a day
        /// </summary>
        private readonly scheduleEndpoint _data;

        /// <summary>
        /// Initializes a new instance of the <see cref="SportEventsScheduleMapper"/> class.
        /// </summary>
        /// <param name="data">A <see cref="scheduleEndpoint"/> instance containing schedule information</param>
        internal SportEventsScheduleMapper(scheduleEndpoint data)
        {
            Contract.Requires(data != null);

            _data = data;
        }

        ///// <summary>
        ///// Initializes a new instance of the <see cref="SportEventsScheduleMapper"/> class.
        ///// </summary>
        ///// <param name="data">A <see cref="tournamentScheduleEndpoint"/> instance containing tournament schedule information</param>
        //internal SportEventsScheduleMapper(tournamentScheduleEndpoint data)
        //{
        //    Contract.Requires(data != null);

        //    _events = data.sport_events.Select(e => new SportEventSummaryDTO(e)).ToList();
        //}

        /// <summary>
        /// Defines object invariants used by the code contracts
        /// </summary>
        [ContractInvariantMethod]
        private void ObjectInvariant()
        {
            Contract.Invariant(_data != null);
        }

        /// <summary>
        /// Maps it's data to <see cref="EntityList{SportEventSummaryDTO}"/> instance
        /// </summary>
        /// <returns>The created <see cref="EntityList{SportEventSummaryDTO}"/> instance</returns>
        public EntityList<SportEventSummaryDTO> Map()
        {
            var events = _data.sport_event.Select(e => RestMapperHelper.MapSportEvent(e)).ToList();
            return new EntityList<SportEventSummaryDTO>(events);
        }
    }
}
