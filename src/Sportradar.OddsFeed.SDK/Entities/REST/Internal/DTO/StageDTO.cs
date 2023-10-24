/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using System.Collections.Generic;
using System.Linq;
using Sportradar.OddsFeed.SDK.Common.Extensions;
using Sportradar.OddsFeed.SDK.Entities.Rest.Enums;
using Sportradar.OddsFeed.SDK.Messages.Rest;

namespace Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Dto
{
    /// <summary>
    /// A data-transfer-object containing basic information about a sport event
    /// </summary>
    internal class StageDto : CompetitionDto
    {
        /// <summary>
        /// Gets a id of the parent stage associated with the current instance
        /// </summary>
        public StageDto ParentStage { get; }

        /// <summary>
        /// Gets a <see cref="IEnumerable{T}"/> specifying the additional parent stages associated with the current instance
        /// </summary>
        public IEnumerable<StageDto> AdditionalParents { get; }

        /// <summary>
        /// Gets a <see cref="IEnumerable{T}"/> specifying the child stages associated with the current instance
        /// </summary>
        public IEnumerable<StageDto> Stages { get; }

        /// <summary>
        /// Gets a <see cref="TournamentDto"/> representing the tournament to which the associated sport event belongs to
        /// </summary>
        public TournamentDto Tournament { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="StageDto"/> class
        /// </summary>
        /// <param name="sportEvent">A <see cref="sportEvent"/> containing basic information about the event</param>
        internal StageDto(sportEvent sportEvent)
            : base(sportEvent)
        {
            if (sportEvent.races != null && sportEvent.races.Any())
            {
                Stages = sportEvent.races.Select(s => new StageDto(s));
            }
            if (sportEvent.tournament != null)
            {
                Tournament = new TournamentDto(sportEvent.tournament);
            }
            if (sportEvent.parent != null)
            {
                ParentStage = new StageDto(sportEvent.parent);
            }
            if (ParentStage == null && Type != null && Type == SportEventType.Parent && sportEvent.tournament != null)
            {
                ParentStage = new StageDto(new TournamentDto(sportEvent.tournament));
            }
            if (!sportEvent.additional_parents.IsNullOrEmpty())
            {
                AdditionalParents = sportEvent.additional_parents.Select(s => new StageDto(s));
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StageDto"/> class
        /// </summary>
        /// <param name="stageEvent">A <see cref="stageSummaryEndpoint"/> containing basic information about the event</param>
        internal StageDto(stageSummaryEndpoint stageEvent)
            : base(stageEvent)
        {
            if (stageEvent == null)
            {
                return;
            }
            if (stageEvent.sport_event == null)
            {
                return;
            }
            if (stageEvent.sport_event.races != null && stageEvent.sport_event.races.Any())
            {
                Stages = stageEvent.sport_event.races.Select(s => new StageDto(s));
            }
            if (stageEvent.sport_event?.tournament != null)
            {
                Tournament = new TournamentDto(stageEvent.sport_event.tournament);
            }
            if (stageEvent.sport_event.parent != null)
            {
                ParentStage = new StageDto(stageEvent.sport_event.parent);
            }
            if (ParentStage == null && Type != null && Type == SportEventType.Parent && stageEvent.sport_event.tournament != null)
            {
                ParentStage = new StageDto(new TournamentDto(stageEvent.sport_event.tournament));
            }
            if (!stageEvent.sport_event.additional_parents.IsNullOrEmpty())
            {
                AdditionalParents = stageEvent.sport_event.additional_parents.Select(s => new StageDto(s));
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StageDto"/> class
        /// </summary>
        /// <param name="childStage">A <see cref="sportEventChildrenSport_event"/> containing basic information about the event</param>
        internal StageDto(sportEventChildrenSport_event childStage)
            : base(childStage)
        {
        }

        internal StageDto(parentStage parentStage)
            : base(parentStage)
        {
        }

        internal StageDto(TournamentDto tournament)
            : this(new sportEvent
            {
                id = tournament.Id.ToString(),
                name = tournament.Name,
                //type = tournament.type,
                scheduledSpecified = tournament.Scheduled != null,
                scheduled = tournament.Scheduled.GetValueOrDefault(DateTime.MinValue),
                scheduled_endSpecified = tournament.ScheduledEnd != null,
                scheduled_end = tournament.ScheduledEnd.GetValueOrDefault(DateTime.MinValue),
                //liveodds = tournament.liveodds,
                //season = tournament.season,
                tournament = new tournament
                {
                    id = tournament.Id.ToString(),
                    name = tournament.Name,
                    scheduledSpecified = tournament.Scheduled != null,
                    scheduled = tournament.Scheduled.GetValueOrDefault(DateTime.MinValue),
                    scheduled_endSpecified = tournament.ScheduledEnd != null,
                    scheduled_end = tournament.ScheduledEnd.GetValueOrDefault(DateTime.MinValue),
                    category = new category
                    {
                        id = tournament.Category.Id.ToString(),
                        name = tournament.Category.Name,
                        country_code = tournament.Category.CountryCode
                    },
                    sport = new sport
                    {
                        id = tournament.Sport.Id.ToString(),
                        name = tournament.Sport.Name
                    }
                }
            })
        {
            Tournament = tournament;
        }
    }
}
