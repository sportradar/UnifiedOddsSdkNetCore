/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System.Collections.Generic;
using System.Linq;
using Castle.Core.Internal;
using Castle.DynamicProxy.Generators.Emitters.SimpleAST;
using Sportradar.OddsFeed.SDK.Entities.Internal.EntitiesImpl;
using Sportradar.OddsFeed.SDK.Entities.REST.Enums;
using Sportradar.OddsFeed.SDK.Messages;
using Sportradar.OddsFeed.SDK.Messages.REST;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Internal.DTO
{
    /// <summary>
    /// A data-transfer-object containing basic information about a sport event
    /// </summary>
    internal class StageDTO : CompetitionDTO
    {
        private readonly URN _parentStageId;

        /// <summary>
        /// Gets a id of the parent stage associated with the current instance
        /// </summary>
        public URN ParentStageId => _parentStageId;

        /// <summary>
        /// Gets a <see cref="IEnumerable{T}"/> specifying the additional parent stages associated with the current instance
        /// </summary>
        public IEnumerable<URN> AdditionalParentIds { get; }

        /// <summary>
        /// Gets a <see cref="IEnumerable{T}"/> specifying the child stages associated with the current instance
        /// </summary>
        public IEnumerable<StageDTO> Stages { get; }

        /// <summary>
        /// Gets a <see cref="TournamentDTO"/> representing the tournament to which the associated sport event belongs to
        /// </summary>
        public TournamentDTO Tournament { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="StageDTO"/> class
        /// </summary>
        /// <param name="sportEvent">A <see cref="sportEvent"/> containing basic information about the event</param>
        internal StageDTO(sportEvent sportEvent)
            : base(sportEvent)
        {
            if (sportEvent.parent != null)
            {
                URN.TryParse(sportEvent.parent.id, out _parentStageId);
            }
            if (sportEvent.races != null && sportEvent.races.Any())
            {
                Stages = sportEvent.races.Select(s => new StageDTO(s));
            }
            if (sportEvent.tournament != null)
            {
                Tournament = new TournamentDTO(sportEvent.tournament);
            }
            if (!sportEvent.additional_parents.IsNullOrEmpty())
            {
                AdditionalParentIds = sportEvent.additional_parents.Select(s => URN.Parse(s.id));
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StageDTO"/> class
        /// </summary>
        /// <param name="stageEvent">A <see cref="stageSummaryEndpoint"/> containing basic information about the event</param>
        internal StageDTO(stageSummaryEndpoint stageEvent)
            : base(stageEvent)
        {
            if (stageEvent.sport_event == null)
            {
                return;
            }
            if (stageEvent.sport_event.parent != null)
            {
                URN.TryParse(stageEvent.sport_event.parent.id, out _parentStageId);
            }
            if (stageEvent.sport_event.races != null && stageEvent.sport_event.races.Any())
            {
                Stages = stageEvent.sport_event.races.Select(s => new StageDTO(s));
            }
            if (stageEvent.sport_event?.tournament != null)
            {
                Tournament = new TournamentDTO(stageEvent.sport_event.tournament);
            }
            if (!stageEvent.sport_event.additional_parents.IsNullOrEmpty())
            {
                AdditionalParentIds = stageEvent.sport_event.additional_parents.Select(s => URN.Parse(s.id));
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StageDTO"/> class
        /// </summary>
        /// <param name="childStage">A <see cref="sportEventChildrenSport_event"/> containing basic information about the event</param>
        protected StageDTO(sportEventChildrenSport_event childStage)
            : base(childStage)
        {
        }
    }
}