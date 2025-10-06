// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Dawn;
using Sportradar.OddsFeed.SDK.Common.Extensions;
using Sportradar.OddsFeed.SDK.Common.Internal;
using Sportradar.OddsFeed.SDK.Entities.Rest.Enums;
using Sportradar.OddsFeed.SDK.Messages.Rest;

namespace Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Dto
{
    /// <summary>
    /// A data-transfer-object representing a fixture
    /// </summary>
    internal class FixtureDto : MatchDto
    {
        internal DateTime? StartTime { get; }

        internal bool StartTimeConfirmed { get; }

        internal DateTime? NextLiveTime { get; }

        internal IReadOnlyDictionary<string, string> ExtraInfo { get; }

        internal IEnumerable<TvChannelDto> TvChannels { get; }

        internal ProductInfoDto ProductInfo { get; }

        internal readonly IDictionary<string, string> ReferenceIds;

        internal DelayedInfoDto DelayedInfo { get; }

        internal IEnumerable<ScheduledStartTimeChangeDto> ScheduledStartTimeChanges { get; }

        /// <summary>
        /// Gets a id of the parent stage associated with the current instance
        /// </summary>
        public StageDto ParentStage { get; }

        /// <summary>
        /// Gets a <see cref="IEnumerable{T}"/> specifying the additional parent stages associated with the current instance
        /// </summary>
        public IEnumerable<StageDto> AdditionalParents { get; }

        internal FixtureDto(fixture fixture, DateTime? generatedAt)
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
                             ? new ReadOnlyCollection<TvChannelDto>(fixture.tv_channels.Select(t => new TvChannelDto(t)).ToList())
                             : null;
            ProductInfo = fixture.product_info != null
                              ? new ProductInfoDto(fixture.product_info)
                              : null;
            Venue = fixture.venue == null
                        ? null
                        : new VenueDto(fixture.venue);
            ReferenceIds = fixture.reference_ids == null
                               ? null
                               : new ReadOnlyDictionary<string, string>(fixture.reference_ids.ToDictionary(r => r.name, r => r.value));
            DelayedInfo = fixture.delayed_info == null
                              ? null
                              : new DelayedInfoDto(fixture.delayed_info.id, fixture.delayed_info.description);
            if (fixture.scheduled_start_time_changes != null && fixture.scheduled_start_time_changes.Any())
            {
                ScheduledStartTimeChanges = fixture.scheduled_start_time_changes.Select(s => new ScheduledStartTimeChangeDto(s));
            }
            if (generatedAt != null)
            {
                GeneratedAt = generatedAt.Value.ToLocalTime();
            }
            if (fixture.parent != null)
            {
                ParentStage = new StageDto(fixture.parent);
            }
            if (ParentStage == null && Type != null && Type == SportEventType.Parent && fixture.tournament != null)
            {
                ParentStage = new StageDto(new TournamentDto(fixture.tournament));
            }
            if (!fixture.additional_parents.IsNullOrEmpty())
            {
                AdditionalParents = fixture.additional_parents.Select(s => new StageDto(s));
            }
        }
    }
}
