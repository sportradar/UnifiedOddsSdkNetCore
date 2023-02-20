/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Castle.Core.Internal;
using Dawn;
using Sportradar.OddsFeed.SDK.Common.Internal;
using Sportradar.OddsFeed.SDK.Entities.REST.Enums;
using Sportradar.OddsFeed.SDK.Messages.REST;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Internal.DTO
{
    /// <summary>
    /// A data-transfer-object representing a fixture
    /// </summary>
    internal class FixtureDTO : MatchDTO
    {
        internal DateTime? StartTime { get; }

        internal bool StartTimeConfirmed { get; }

        internal DateTime? NextLiveTime { get; }

        internal IReadOnlyDictionary<string, string> ExtraInfo { get; }

        internal IEnumerable<TvChannelDTO> TvChannels { get; }

        internal ProductInfoDTO ProductInfo { get; }

        internal readonly IDictionary<string, string> ReferenceIds;

        internal DelayedInfoDTO DelayedInfo { get; }

        internal IEnumerable<ScheduledStartTimeChangeDTO> ScheduledStartTimeChanges { get; }

        /// <summary>
        /// Gets a id of the parent stage associated with the current instance
        /// </summary>
        public StageDTO ParentStage { get; }

        /// <summary>
        /// Gets a <see cref="IEnumerable{T}"/> specifying the additional parent stages associated with the current instance
        /// </summary>
        public IEnumerable<StageDTO> AdditionalParents { get; }

        internal FixtureDTO(fixture fixture, DateTime? generatedAt)
            : base(fixture)
        {
            Guard.Argument(fixture, nameof(fixture)).NotNull();

            StartTime = fixture.start_timeSpecified
                ? (DateTime?)fixture.start_time.ToLocalTime()
                : null;
            if (!string.IsNullOrEmpty(fixture.next_live_time))
            {
                NextLiveTime = SdkInfo.ParseDate(fixture.next_live_time);
            }
            StartTimeConfirmed = fixture.start_time_confirmedSpecified && fixture.start_time_confirmed;
            ExtraInfo = fixture.extra_info != null && fixture.extra_info.Any()
                ? new ReadOnlyDictionary<string, string>(fixture.extra_info.ToDictionary(e => e.key, e => e.value))
                : new ReadOnlyDictionary<string, string>(new Dictionary<string, string>());
            TvChannels = fixture.tv_channels != null && fixture.tv_channels.Any()
                ? new ReadOnlyCollection<TvChannelDTO>(fixture.tv_channels.Select(t => new TvChannelDTO(t)).ToList())
                : null;
            ProductInfo = fixture.product_info != null
                ? new ProductInfoDTO(fixture.product_info)
                : null;
            Venue = fixture.venue == null
                ? null
                : new VenueDTO(fixture.venue);
            ReferenceIds = fixture.reference_ids == null
                ? null
                : new ReadOnlyDictionary<string, string>(fixture.reference_ids.ToDictionary(r => r.name, r => r.value));
            DelayedInfo = fixture.delayed_info == null
                ? null
                : new DelayedInfoDTO(fixture.delayed_info.id, fixture.delayed_info.description);
            if (fixture.scheduled_start_time_changes != null && fixture.scheduled_start_time_changes.Any())
            {
                ScheduledStartTimeChanges = fixture.scheduled_start_time_changes.Select(s => new ScheduledStartTimeChangeDTO(s));
            }
            if (generatedAt != null)
            {
                GeneratedAt = generatedAt.Value.ToLocalTime();
            }
            if (fixture.parent != null)
            {
                ParentStage = new StageDTO(fixture.parent);
            }
            if (ParentStage == null && Type != null && Type == SportEventType.Parent && fixture.tournament != null)
            {
                ParentStage = new StageDTO(new TournamentDTO(fixture.tournament));
            }
            if (!fixture.additional_parents.IsNullOrEmpty())
            {
                AdditionalParents = fixture.additional_parents.Select(s => new StageDTO(s));
            }
        }
    }
}
