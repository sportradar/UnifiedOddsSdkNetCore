/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Dawn;
using Microsoft.Extensions.Logging;
using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Entities.REST;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.DTO;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.Enums;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.Mapping;
using Sportradar.OddsFeed.SDK.Messages;
using Sportradar.OddsFeed.SDK.Messages.Feed;

namespace Sportradar.OddsFeed.SDK.Entities.Internal
{
    /// <summary>
    /// Caches <see cref="ISportEventStatus"/> instances by inspecting odds change messages received from the feed
    /// </summary>
    /// <seealso cref="MessageProcessorBase" />
    /// <seealso cref="IFeedMessageProcessor" />
    internal class CacheMessageProcessor : MessageProcessorBase, IFeedMessageProcessor
    {
        /// <summary>
        /// The execution log
        /// </summary>
        private static readonly ILogger ExecutionLog = SdkLoggerFactory.GetLoggerForExecution(typeof(CacheMessageProcessor));

        /// <summary>
        /// A <see cref="ISingleTypeMapperFactory{removeSportEventStatus, SportEventStatusDTO}"/> used to created <see cref="ISingleTypeMapper{SportEventStatusDTO}"/> instances
        /// </summary>
        private readonly ISingleTypeMapperFactory<sportEventStatus, SportEventStatusDTO> _mapperFactory;

        /// <summary>
        /// The processor identifier
        /// </summary>
        /// <value>The processor identifier</value>
        public string ProcessorId { get; }

        /// <summary>
        /// The sport event cache
        /// </summary>
        private readonly SportEventCache _sportEventCache;

        /// <summary>
        /// The cache manager
        /// </summary>
        private readonly ICacheManager _cacheManager;

        private readonly IFeedMessageHandler _feedMessageHandler;
        private readonly ISportEventStatusCache _sportEventStatusCache;
        private readonly IReadOnlyCollection<int> _ignoredProducersForFixtureEndpoint;

        /// <summary>
        /// Initializes a new instance of the <see cref="CacheMessageProcessor"/> class
        /// </summary>
        /// <param name="mapperFactory">A <see cref="ISingleTypeMapperFactory{removeSportEventStatus, SportEventStatusDTO}"/> used to created <see cref="ISingleTypeMapper{SportEventStatusDTO}"/> instances</param>
        /// <param name="sportEventCache">A <see cref="ISportEventCache"/> used to handle <see cref="fixture_change"/></param>
        /// <param name="cacheManager">A <see cref="ICacheManager"/> used to interact among caches</param>
        /// <param name="feedMessageHandler">A <see cref="IFeedMessageHandler"/> for handling special cases</param>
        /// <param name="sportEventStatusCache">A <see cref="ISportEventStatusCache"/> used to save eventId from BetPal to ignore timeline SES</param>
        /// <param name="producerManager">The <see cref="IProducerManager"/> to get 'virtual' producers to ignore for fixture_change_fixture endpoint</param>
        public CacheMessageProcessor(ISingleTypeMapperFactory<sportEventStatus, SportEventStatusDTO> mapperFactory,
                                     ISportEventCache sportEventCache,
                                     ICacheManager cacheManager,
                                     IFeedMessageHandler feedMessageHandler,
                                     ISportEventStatusCache sportEventStatusCache,
                                     IProducerManager producerManager)
        {
            Guard.Argument(mapperFactory, nameof(mapperFactory)).NotNull();
            Guard.Argument(sportEventCache, nameof(sportEventCache)).NotNull();
            Guard.Argument(cacheManager, nameof(cacheManager)).NotNull();
            Guard.Argument(feedMessageHandler, nameof(feedMessageHandler)).NotNull();
            Guard.Argument(sportEventStatusCache, nameof(sportEventStatusCache)).NotNull();
            Guard.Argument(producerManager, nameof(producerManager)).NotNull();

            ProcessorId = "CMP" + Guid.NewGuid().ToString().Substring(0, 4);

            _mapperFactory = mapperFactory;
            _sportEventCache = (SportEventCache)sportEventCache;
            _cacheManager = cacheManager;
            _feedMessageHandler = feedMessageHandler;
            _sportEventStatusCache = sportEventStatusCache;
            var ignoredProducerIdsList = producerManager.Producers.Where(w => w.Scope.Contains("virtual", StringComparer.InvariantCultureIgnoreCase))
                                                      .Select(s => s.Id)
                                                      .ToList();
            _ignoredProducersForFixtureEndpoint = new ReadOnlyCollection<int>(ignoredProducerIdsList);
        }

        /// <summary>
        /// Processes and dispatches the provided <see cref="FeedMessage" /> instance
        /// </summary>
        /// <param name="message">A <see cref="FeedMessage" /> instance to be processed</param>
        /// <param name="interest">A <see cref="MessageInterest"/> specifying the interest of the associated session</param>
        /// <param name="rawMessage">A raw message received from the feed</param>
        public void ProcessMessage(FeedMessage message, MessageInterest interest, byte[] rawMessage)
        {
            Guard.Argument(message, nameof(message)).NotNull();
            Guard.Argument(interest, nameof(interest)).NotNull();

            if (message.IsEventRelated)
            {
                _sportEventStatusCache.AddEventIdForTimelineIgnore(message.EventURN, message.ProducerId, message.GetType());
            }

            // process odds_change
            var oddsChange = message as odds_change;
            if (oddsChange?.sport_event_status != null)
            {
                var status = _mapperFactory.CreateMapper(oddsChange.sport_event_status).Map();
                _cacheManager.SaveDto(oddsChange.EventURN, status, CultureInfo.CurrentCulture, DtoType.SportEventStatus, null);
            }

            var betStop = message as bet_stop;
            if (betStop != null)
            {
                // draw event is still removed
                RemoveCacheItem(betStop.EventURN, removeEvent: false, removeSportEventStatus: true);
            }

            var betSettlement = message as bet_settlement;
            if (betSettlement != null)
            {
                // draw event is still removed
                RemoveCacheItem(betSettlement.EventURN, removeEvent: false, removeSportEventStatus: false);
            }

            // process fixtureChange
            var fixtureChange = message as fixture_change;
            if (fixtureChange != null)
            {
                RemoveCacheItem(fixtureChange.EventURN, removeEvent: true, removeSportEventStatus: true);
                if (!_ignoredProducersForFixtureEndpoint.Contains(fixtureChange.ProducerId))
                {
                    _sportEventCache.AddFixtureTimestamp(fixtureChange.EventURN);
                }

                if (fixtureChange.EventURN.TypeGroup == ResourceTypeGroup.TOURNAMENT)
                {
                    HandleTournamentFixtureChange(fixtureChange.EventURN);
                }

                if (_feedMessageHandler.StopProcessingFixtureChange(fixtureChange))
                {
                    // fixtureChange was already dispatched (via another session)
                }
            }
        }

        /// <summary>
        /// Fetches sport events for specific tournament (prefetching it - useful for virtual feeds)
        /// </summary>
        /// <param name="urn">The tournament id</param>
        private void HandleTournamentFixtureChange(URN urn)
        {
            try
            {
                Task.Run(async () => await _sportEventCache.GetEventIdsAsync(urn, (IEnumerable<CultureInfo>)null)).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                ExecutionLog.LogDebug(e, "Error fetching tournament schedule");
            }
        }

        /// <summary>
        /// Removes the cache item (SportEventStatus or SportEvent)
        /// </summary>
        /// <param name="eventId">The event identifier</param>
        /// <param name="removeEvent">if set to <c>true</c> [remove event]</param>
        /// <param name="removeSportEventStatus">if set to <c>true</c> [sport event status]</param>
        private void RemoveCacheItem(URN eventId, bool removeEvent, bool removeSportEventStatus)
        {
            if (removeSportEventStatus)
            {
                _cacheManager.RemoveCacheItem(eventId, CacheItemType.SportEventStatus, ProcessorId);
            }

            if (removeEvent || eventId.TypeGroup == ResourceTypeGroup.DRAW)
            {
                _cacheManager.RemoveCacheItem(eventId, CacheItemType.SportEvent, ProcessorId);
            }
        }
    }
}
