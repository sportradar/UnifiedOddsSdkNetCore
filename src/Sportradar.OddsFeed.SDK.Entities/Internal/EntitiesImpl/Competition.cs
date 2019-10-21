/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using System.Collections.Generic;
using Dawn;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Common.Logging;
using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Common.Internal;
using Sportradar.OddsFeed.SDK.Entities.REST;
using Sportradar.OddsFeed.SDK.Entities.REST.Enums;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching.CI;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching.Events;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.EntitiesImpl;
using Sportradar.OddsFeed.SDK.Messages;

namespace Sportradar.OddsFeed.SDK.Entities.Internal.EntitiesImpl
{
    /// <summary>
    /// Represents a sport event regardless to which sport it belongs
    /// </summary>
    internal abstract class Competition : SportEvent, ICompetitionV1
    {
        internal readonly ISportEventStatusCache SportEventStatusCache;

        /// <summary>
        /// An instance of a <see cref="ISportEntityFactory"/> used to create <see cref="ISportEvent"/> instances
        /// </summary>
        private readonly ISportEntityFactory _sportEntityFactory;

        /// <summary>
        /// The match statuses cache
        /// </summary>
        private readonly ILocalizedNamedValueCache _matchStatusesCache;

        /// <summary>
        /// Initializes a new instance of the <see cref="Competition"/> class
        /// </summary>
        /// <param name="executionLog">A <see cref="ILog"/> instance used for execution logging</param>
        /// <param name="id">A <see cref="URN"/> uniquely identifying the sport event associated with the current instance</param>
        /// <param name="sportId">A <see cref="URN"/> uniquely identifying the sport associated with the current instance</param>
        /// <param name="sportEntityFactory">An instance of a <see cref="ISportEntityFactory"/> used to create <see cref="ISportEvent"/> instances</param>
        /// <param name="sportEventStatusCache">A <see cref="ISportEventStatusCache"/> instance containing cache data information about the progress of a sport event associated with the current instance</param>
        /// <param name="sportEventCache">A <see cref="ISportEventCache"/> instance containing <see cref="CompetitionCI"/></param>
        /// <param name="cultures">A <see cref="IEnumerable{CultureInfo}"/> specifying languages the current instance supports</param>
        /// <param name="exceptionStrategy">A <see cref="ExceptionHandlingStrategy"/> enum member specifying how the initialized instance will handle potential exceptions</param>
        /// <param name="matchStatusesCache">A <see cref="ILocalizedNamedValueCache"/> cache for fetching match statuses</param>
        internal Competition(ILog executionLog,
                             URN id,
                             URN sportId,
                             ISportEntityFactory sportEntityFactory,
                             ISportEventStatusCache sportEventStatusCache,
                             ISportEventCache sportEventCache,
                             IEnumerable<CultureInfo> cultures,
                             ExceptionHandlingStrategy exceptionStrategy,
                             ILocalizedNamedValueCache matchStatusesCache)
            :base(id, sportId, executionLog, sportEventCache, cultures, exceptionStrategy)
        {
            Guard.Argument(id).NotNull();
            Guard.Argument(sportEntityFactory).NotNull();
            Guard.Argument(sportEventStatusCache).NotNull();
            Guard.Argument(matchStatusesCache).NotNull();

            _sportEntityFactory = sportEntityFactory;
            SportEventStatusCache = sportEventStatusCache;
            _matchStatusesCache = matchStatusesCache;
        }

        /// <summary>
        /// Constructs and returns a <see cref="T:System.String" /> containing the id of the current instance
        /// </summary>
        /// <returns>A <see cref="T:System.String" /> containing the id of the current instance.</returns>
        protected override string PrintI()
        {
            return $"Id={Id}";
        }

        /// <summary>
        /// Constructs and returns a <see cref="T:System.String" /> containing compacted representation of the current instance
        /// </summary>
        /// <returns>A <see cref="T:System.String" /> containing compacted representation of the current instance.</returns>
        protected override string PrintC()
        {
            var result = $"{PrintI()}, Cultures=[{string.Join(", ", Cultures.Select(c => c.TwoLetterISOLanguageName))}]"; //, Status={((SportEventStatus)Status).PrintC()}";
            return result;
        }

        /// <summary>
        /// Constructs and return a <see cref="T:System.String" /> containing details of the current instance
        /// </summary>
        /// <returns>A <see cref="T:System.String" /> containing details of the current instance.</returns>
        protected override string PrintF()
        {
            return PrintC();
        }

        /// <summary>
        /// Constructs and returns a <see cref="T:System.String" /> containing a JSON representation of the current instance
        /// </summary>
        /// <returns>a <see cref="T:System.String" /> containing a JSON representation of the current instance.</returns>
        protected override string PrintJ()
        {
            return PrintJ(GetType(), this);
        }

        /// <summary>
        /// Gets a <see cref="ICompetitionStatus"/> instance containing information about the progress of a sport event associated with the current instance
        /// </summary>
        /// <returns>A <see cref="ICompetitionStatus"/> instance containing information about the progress of the sport event</returns>
        public async Task<ICompetitionStatus> GetStatusAsync()
        {
            var item = ExceptionStrategy == ExceptionHandlingStrategy.THROW
                ? await SportEventStatusCache.GetSportEventStatusAsync(Id).ConfigureAwait(false)
                : await new Func<URN, Task<SportEventStatusCI>>(SportEventStatusCache.GetSportEventStatusAsync).SafeInvokeAsync(Id, ExecutionLog, GetFetchErrorMessage("EventStatus")).ConfigureAwait(false);

            return item == null
                ? null
                : new CompetitionStatus(item, _matchStatusesCache);
        }

        /// <summary>
        /// Gets the event status asynchronous
        /// </summary>
        /// <returns>Get the event status</returns>
        public async Task<EventStatus?> GetEventStatusAsync()
        {
            var status = await GetStatusAsync().ConfigureAwait(false);
            return status?.Status;
        }

        /// <summary>
        /// Asynchronously gets a <see cref="BookingStatus"/> enum member providing booking status for the associated @event or a null reference if booking status is not known
        /// </summary>
        /// <returns></returns>
        public async Task<BookingStatus?> GetBookingStatusAsync()
        {
            var competitionCI = (CompetitionCI)SportEventCache.GetEventCacheItem(Id);
            if (competitionCI == null)
            {
                ExecutionLog.Debug($"Missing data. No sportEvent cache item for id={Id}.");
                return null;
            }
            return ExceptionStrategy == ExceptionHandlingStrategy.THROW
                ? await competitionCI.GetBookingStatusAsync().ConfigureAwait(false)
                : await new Func<Task<BookingStatus?>>(competitionCI.GetBookingStatusAsync).SafeInvokeAsync(ExecutionLog, GetFetchErrorMessage("BookingStatus")).ConfigureAwait(false);
        }

        /// <summary>
        /// Asynchronously gets a <see cref="IVenue"/> instance representing a venue where the sport event associated with the
        /// current instance will take place
        /// </summary>
        /// <returns>A <see cref="Task{IVenue}"/> representing the retrieval operation</returns>
        public async Task<IVenue> GetVenueAsync()
        {
            var competitionCI = (CompetitionCI)SportEventCache.GetEventCacheItem(Id);
            if (competitionCI == null)
            {
                ExecutionLog.Debug($"Missing data. No sportEvent cache item for id={Id}.");
                return null;
            }
            var item = ExceptionStrategy == ExceptionHandlingStrategy.THROW
                ? await competitionCI.GetVenueAsync(Cultures).ConfigureAwait(false)
                : await new Func<IEnumerable<CultureInfo>, Task<VenueCI>>(competitionCI.GetVenueAsync).SafeInvokeAsync(Cultures, ExecutionLog, "Venue").ConfigureAwait(false);

            return item == null
                ? null
                : new Venue(item, Cultures);
        }

        /// <summary>
        /// Asynchronously gets a <see cref="ISportEventConditions"/> instance representing live conditions of the sport event associated with the current instance
        /// </summary>
        /// <returns>A <see cref="Task{IVenue}"/> representing the retrieval operation</returns>
        /// <remarks>A Fixture is a sport event that has been arranged for a particular time and place</remarks>
        public async Task<ISportEventConditions> GetConditionsAsync()
        {
            var competitionCI = (CompetitionCI)SportEventCache.GetEventCacheItem(Id);
            if (competitionCI == null)
            {
                ExecutionLog.Debug($"Missing data. No sportEvent cache item for id={Id}.");
                return null;
            }
            var item = ExceptionStrategy == ExceptionHandlingStrategy.THROW
                ? await competitionCI.GetConditionsAsync(Cultures).ConfigureAwait(false)
                : await new Func<IEnumerable<CultureInfo>, Task<SportEventConditionsCI>>(competitionCI.GetConditionsAsync).SafeInvokeAsync(Cultures, ExecutionLog, GetFetchErrorMessage("EventConditions")).ConfigureAwait(false);

            return item == null
                ? null
                : new SportEventConditions(item, Cultures);
        }

        /// <summary>
        /// Asynchronously gets a <see cref="IEnumerable{T}"/> representing competitors in the sport event associated with the current instance
        /// </summary>
        /// <returns>A <see cref="Task{T}"/> representing the retrieval operation</returns>
        public async Task<IEnumerable<ICompetitor>> GetCompetitorsAsync()
        {
            var competitionCI = (CompetitionCI)SportEventCache.GetEventCacheItem(Id);
            if (competitionCI == null)
            {
                ExecutionLog.Debug($"Missing data. No sportEvent cache item for id={Id}.");
                return null;
            }
            var items = ExceptionStrategy == ExceptionHandlingStrategy.THROW
                ? await competitionCI.GetCompetitorsAsync(Cultures).ConfigureAwait(false)
                : await new Func<IEnumerable<CultureInfo>, Task<IEnumerable<URN>>>(competitionCI.GetCompetitorsAsync).SafeInvokeAsync(Cultures, ExecutionLog, GetFetchErrorMessage("Competitors")).ConfigureAwait(false);

            var competitorUrns = items as List<URN>;
            if (competitorUrns == null || !competitorUrns.Any())
            {
                return null;
            }

            var tasks = competitorUrns.Select(s =>
                                              {
                                                  var t = _sportEntityFactory.BuildTeamCompetitorAsync(s, Cultures, competitionCI);
                                                  t.ConfigureAwait(false);
                                                  return t;
                                              }).ToList();
            await Task.WhenAll(tasks).ConfigureAwait(false);

            return tasks.Select(s=>s.Result);
        }
    }
}