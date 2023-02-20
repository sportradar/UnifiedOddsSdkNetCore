/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using Dawn;
using Sportradar.OddsFeed.SDK.Messages;
using Sportradar.OddsFeed.SDK.Messages.REST;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Internal.DTO
{
    /// <summary>
    /// A data-transfer-object containing basic information about a sport event
    /// </summary>
    internal class SportEventSummaryDTO
    {
        /// <summary>
        /// Gets a <see cref="URN"/> specifying the id of the sport event associated with the current instance
        /// </summary>
        public URN Id { get; }

        /// <summary>
        /// Gets a <see cref="URN"/> specifying the id of the sport associated with the current instance
        /// </summary>
        public URN SportId { get; }

        /// <summary>
        /// Gets a <see cref="DateTime"/> specifying the scheduled start date for the sport event associated with the current instance
        /// </summary>
        public DateTime? Scheduled { get; }

        /// <summary>
        /// Gets a <see cref="DateTime"/> specifying the scheduled end time of the sport event associated with the current instance
        /// </summary>
        public DateTime? ScheduledEnd { get; }

        /// <summary>
        /// Gets a name o f the associated sport event
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets a <see cref="bool"/> specifying if the start time to be determined is set for the associated sport event
        /// </summary>
        public bool? StartTimeTbd { get; }

        /// <summary>
        /// Gets a <see cref="URN"/> specifying the replacement sport event
        /// for the associated sport event
        /// </summary>
        public URN ReplacedBy { get; }

        /// <summary>
        /// Gets the status (directly from sportEvent)
        /// </summary>
        /// <value>The status (directly from sportEvent)</value>
        public string StatusOnEvent { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SportEventSummaryDTO"/> class
        /// </summary>
        /// <param name="sportEvent">A <see cref="sportEvent"/> containing basic information about the event</param>
        internal SportEventSummaryDTO(sportEvent sportEvent)
        {
            Guard.Argument(sportEvent, nameof(sportEvent)).NotNull();
            Guard.Argument(sportEvent.id, nameof(sportEvent.id)).NotNull().NotEmpty();

            Id = URN.Parse(sportEvent.id);
            Scheduled = sportEvent.scheduledSpecified
                ? (DateTime?)sportEvent.scheduled.ToLocalTime()
                : null;
            ScheduledEnd = sportEvent.scheduled_endSpecified
                ? (DateTime?)sportEvent.scheduled_end.ToLocalTime()
                : null;
            if (sportEvent.tournament?.sport != null && URN.TryParse(sportEvent.tournament.sport.id, out var sportId))
            {
                SportId = sportId;
            }
            Name = sportEvent.name;
            if (!string.IsNullOrEmpty(sportEvent.replaced_by) && URN.TryParse(sportEvent.replaced_by, out var replacedBy))
            {
                ReplacedBy = replacedBy;
            }
            StartTimeTbd = sportEvent.start_time_tbdSpecified ? (bool?)sportEvent.start_time_tbd : null;
            StatusOnEvent = sportEvent.status;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SportEventSummaryDTO"/> class
        /// </summary>
        /// <param name="parentStage">A <see cref="parentStage"/> containing basic information about the event</param>
        protected SportEventSummaryDTO(parentStage parentStage)
        {
            Guard.Argument(parentStage, nameof(parentStage)).NotNull();
            Guard.Argument(parentStage.id, nameof(parentStage.id)).NotNull().NotEmpty();

            Id = URN.Parse(parentStage.id);
            Scheduled = parentStage.scheduledSpecified
                            ? (DateTime?)parentStage.scheduled.ToLocalTime()
                            : null;
            ScheduledEnd = parentStage.scheduled_endSpecified
                               ? (DateTime?)parentStage.scheduled_end.ToLocalTime()
                               : null;
            Name = parentStage.name;
            if (!string.IsNullOrEmpty(parentStage.replaced_by) && URN.TryParse(parentStage.replaced_by, out var replacedBy))
            {
                ReplacedBy = replacedBy;
            }
            StartTimeTbd = parentStage.start_time_tbdSpecified ? (bool?)parentStage.start_time_tbd : null;

            StatusOnEvent = null;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SportEventSummaryDTO"/> class
        /// </summary>
        /// <param name="childStage">A <see cref="sportEventChildrenSport_event"/> containing basic information about the event</param>
        protected SportEventSummaryDTO(sportEventChildrenSport_event childStage)
        {
            Guard.Argument(childStage, nameof(childStage)).NotNull();
            Guard.Argument(childStage.id, nameof(childStage.id)).NotNull().NotEmpty();

            Id = URN.Parse(childStage.id);
            Scheduled = childStage.scheduledSpecified
                            ? (DateTime?)childStage.scheduled.ToLocalTime()
                            : null;
            ScheduledEnd = childStage.scheduled_endSpecified
                               ? (DateTime?)childStage.scheduled_end.ToLocalTime()
                               : null;
            Name = childStage.name;
            if (!string.IsNullOrEmpty(childStage.replaced_by) && URN.TryParse(childStage.replaced_by, out var replacedBy))
            {
                ReplacedBy = replacedBy;
            }
            StartTimeTbd = childStage.start_time_tbdSpecified ? (bool?)childStage.start_time_tbd : null;

            StatusOnEvent = null;
        }
    }
}
