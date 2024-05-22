// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Dawn;
using Sportradar.OddsFeed.SDK.Entities.Rest.Caching.Exportable;
using Sportradar.OddsFeed.SDK.Entities.Rest.Enums;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Dto;

namespace Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Caching.CI
{
    /// <summary>
    /// Defines a cache item for event timeline (aka MatchTimeline)
    /// </summary>
    internal class EventTimelineCacheItem
    {
        private List<TimelineEventCacheItem> _timeline;
        private bool _isFinalized;
        private readonly List<CultureInfo> _fetchedCultures;

        /// <summary>
        /// Initializes a new instance of the <see cref="EventTimelineCacheItem"/> class.
        /// </summary>
        /// <param name="dto">The events.</param>
        /// <param name="culture"></param>
        public EventTimelineCacheItem(MatchTimelineDto dto, CultureInfo culture)
        {
            _fetchedCultures = new List<CultureInfo>();
            Merge(dto, culture);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EventTimelineCacheItem"/> class.
        /// </summary>
        /// <param name="exportable">The events.</param>
        public EventTimelineCacheItem(ExportableEventTimeline exportable)
        {
            if (exportable == null)
            {
                throw new ArgumentNullException(nameof(exportable));
            }

            _timeline = exportable.Timeline?.Select(t => new TimelineEventCacheItem(t)).ToList();
            _isFinalized = exportable.IsFinalized;
            _fetchedCultures = new List<CultureInfo>(exportable.FetchedCultures ?? new List<CultureInfo>());
        }

        /// <summary>
        /// Gets the list of timeline events
        /// </summary>
        /// <returns>The list of <see cref="TimelineEventCacheItem"/></returns>
        public IEnumerable<TimelineEventCacheItem> Timeline => _timeline;

        /// <summary>
        /// Gets a value indicating whether this instance is finalized
        /// </summary>
        /// <value><c>true</c> if this instance is finalized; otherwise, <c>false</c>.</value>
        public bool IsFinalized => _isFinalized;

        /// <summary>
        /// Gets the list already fetched cultures
        /// </summary>
        /// <value>The list already fetched cultures</value>
        public IEnumerable<CultureInfo> FetchedCultureInfos => _fetchedCultures;

        /// <summary>
        /// Merges the specified dto
        /// </summary>
        /// <param name="dto">The dto</param>
        /// <param name="culture">The culture</param>
        public void Merge(MatchTimelineDto dto, CultureInfo culture)
        {
            Guard.Argument(dto, nameof(dto)).NotNull();

            if (dto.BasicEvents != null && dto.BasicEvents.Any())
            {
                if (_timeline == null)
                {
                    _timeline = dto.BasicEvents.Select(s => new TimelineEventCacheItem(s, culture)).ToList();
                }
                else
                {
                    // delete obsolete timeline events
                    _timeline.RemoveAll(timelineEvent => dto.BasicEvents.All(basicEvent => basicEvent.Id != timelineEvent.Id));
                    foreach (var basicEvent in dto.BasicEvents)
                    {
                        var timelineEvent = _timeline.FirstOrDefault(s => s.Id == basicEvent.Id);
                        if (timelineEvent != null && timelineEvent.Id == basicEvent.Id)
                        {
                            timelineEvent.Merge(basicEvent, culture);
                        }
                        else
                        {
                            _timeline.Add(new TimelineEventCacheItem(basicEvent, culture));
                        }
                    }
                }
            }

            if (!_isFinalized && dto.SportEventStatus != null)
            {
                if (dto.SportEventStatus.Status == EventStatus.Closed
                 || dto.SportEventStatus.Status == EventStatus.Ended)
                {
                    _isFinalized = true;
                }
            }

            _fetchedCultures.Add(culture);
        }

        /// <summary>
        /// Asynchronous export item's properties
        /// </summary>
        /// <returns>An <see cref="ExportableBase"/> instance containing all relevant properties</returns>
        public async Task<ExportableEventTimeline> ExportAsync()
        {
            var timelineTasks = _timeline?.Select(async t => await t.ExportAsync().ConfigureAwait(false));

            return new ExportableEventTimeline
            {
                FetchedCultures = new List<CultureInfo>(_fetchedCultures ?? new List<CultureInfo>()),
                Timeline = timelineTasks != null ? await Task.WhenAll(timelineTasks) : null,
                IsFinalized = _isFinalized
            };
        }
    }
}
