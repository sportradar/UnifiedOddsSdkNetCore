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
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching.Events;
using Sportradar.OddsFeed.SDK.Messages;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Internal.EntitiesImpl
{
    /// <summary>
    /// Represents all sport events(races, matches, tournaments, ....)
    /// </summary>
    /// <seealso cref="EntityPrinter" />
    internal class SportEvent : EntityPrinter, ISportEventV1
    {
        /// <summary>
        /// The sport identifier
        /// </summary>
        protected URN SportId;

        /// <summary>
        /// The <see cref="ILog"/> instance used for execution logging
        /// </summary>
        protected readonly ILog ExecutionLog;

        /// <summary>
        /// A <see cref="ExceptionHandlingStrategy"/> enum member specifying how the instance will handle potential exceptions
        /// </summary>
        protected readonly ExceptionHandlingStrategy ExceptionStrategy;

        /// <summary>
        /// Gets a <see cref="URN"/> uniquely identifying the sport event
        /// </summary>
        public URN Id { get; }

        /// <summary>
        /// A <see cref="IEnumerable{T}"/> specifying languages the current instance supports
        /// </summary>
        public readonly IEnumerable<CultureInfo> Cultures;

        /// <summary>
        /// A <see cref="ISportEventCache"/> instance containing <see cref="SportEventCI"/>
        /// </summary>
        protected readonly ISportEventCache SportEventCache;

        /// <summary>
        /// Initializes a new instance of the <see cref="SportEvent"/> class
        /// </summary>
        /// <param name="id">A <see cref="URN"/> uniquely identifying the sport event</param>
        /// <param name="sportId">A <see cref="URN"/> identifying the sport current instance belong to</param>
        /// <param name="executionLog">The <see cref="ILog"/> instance used for execution logging</param>
        /// <param name="sportEventCache">A <see cref="ISportEventCache"/> instance containing <see cref="SportEventCI"/></param>
        /// <param name="cultures">A <see cref="IEnumerable{CultureInfo}"/> specifying languages the current instance supports</param>
        /// <param name="exceptionStrategy">A <see cref="ExceptionHandlingStrategy"/> enum member specifying how the instance will handle potential exceptions</param>
        public SportEvent(URN id,
                        URN sportId,
                        ILog executionLog,
                        ISportEventCache sportEventCache,
                        IEnumerable<CultureInfo> cultures,
                        ExceptionHandlingStrategy exceptionStrategy)
        {
            Guard.Argument(id).NotNull();
            Guard.Argument(sportEventCache).NotNull();
            Guard.Argument(cultures).NotNull().NotEmpty();

            Id = id;
            SportId = sportId;
            ExecutionLog = executionLog ?? SdkLoggerFactory.GetLoggerForExecution(typeof(SportEvent));
            ExceptionStrategy = exceptionStrategy;
            Cultures = cultures;
            SportEventCache = sportEventCache;
        }

        /// <summary>
        /// Constructs and returns an error message for errors which occur while retrieving cached values
        /// </summary>
        /// <param name="propertyName">The name of the property being retrieved</param>
        /// <returns>An error message for errors which occur while retrieving cached values</returns>
        protected string GetFetchErrorMessage(string propertyName)
        {
            Guard.Argument(propertyName).NotNull().NotEmpty();

            return $"Error occurred while attempting to get {propertyName} for sport event with Id={Id} from cache";
        }

        /// <inheritdoc />
        public async Task<string> GetNameAsync(CultureInfo culture)
        {
            var sportEventCI = SportEventCache.GetEventCacheItem(Id);
            if (sportEventCI == null)
            {
                ExecutionLog.Debug($"Missing data. No sportEvent cache item for id={Id}.");
                return null;
            }

            var item = ExceptionStrategy == ExceptionHandlingStrategy.THROW
                ? await sportEventCI.GetNamesAsync(Cultures).ConfigureAwait(false)
                : await new Func<IEnumerable<CultureInfo>, Task<IReadOnlyDictionary<CultureInfo, string>>>(sportEventCI
                        .GetNamesAsync).SafeInvokeAsync(Cultures, ExecutionLog, GetFetchErrorMessage("Name"))
                    .ConfigureAwait(false);

            return item == null || !item.ContainsKey(culture)
                ? null
                : item[culture];
        }

        /// <summary>
        /// Asynchronously gets a <see cref="URN"/> uniquely identifying the sport associated with the current instance
        /// </summary>
        /// <returns>Task&lt;URN&gt;</returns>
        public async Task<URN> GetSportIdAsync()
        {
            if (SportId == null)
            {
                var sportEventCI = SportEventCache.GetEventCacheItem(Id);
                if (sportEventCI == null)
                {
                    ExecutionLog.Debug($"Missing data. No sportEvent cache item for id={Id}.");
                    return null;
                }
                SportId = await sportEventCI.GetSportIdAsync().ConfigureAwait(false);
            }

            return SportId;
        }

        /// <summary>
        /// When overridden in derived class, it asynchronously gets a <see cref="T:System.DateTime" /> instance specifying for when the sport event associated with the current instance is
        /// scheduled or a null reference if the value is not known
        /// </summary>
        /// <returns>A <see cref="T:System.Threading.Tasks.Task`1" /> representing the retrieval operation</returns>
        public async Task<DateTime?> GetScheduledTimeAsync()
        {
            var sportEventCI = SportEventCache.GetEventCacheItem(Id);
            if (sportEventCI == null)
            {
                ExecutionLog.Debug($"Missing data. No sportEvent cache item for id={Id}.");
                return null;
            }
            return ExceptionStrategy == ExceptionHandlingStrategy.THROW
                ? await sportEventCI.GetScheduledAsync().ConfigureAwait(false)
                : await new Func<Task<DateTime?>>(sportEventCI.GetScheduledAsync).SafeInvokeAsync(ExecutionLog, GetFetchErrorMessage("ScheduledTime")).ConfigureAwait(false);
        }

        /// <summary>
        /// When overridden in derived class, it asynchronously gets a <see cref="T:System.DateTime" /> instance specifying for when the sport event associated with the current instance is
        /// scheduled to end or a null reference if the value is not known
        /// </summary>
        /// <returns>A <see cref="T:System.Threading.Tasks.Task`1" /> representing the retrieval operation</returns>
        public async Task<DateTime?> GetScheduledEndTimeAsync()
        {
            var sportEventCI = SportEventCache.GetEventCacheItem(Id);
            if (sportEventCI == null)
            {
                ExecutionLog.Debug($"Missing data. No sportEvent cache item for id={Id}.");
                return null;
            }
            return ExceptionStrategy == ExceptionHandlingStrategy.THROW
                ? await sportEventCI.GetScheduledEndAsync().ConfigureAwait(false)
                : await new Func<Task<DateTime?>>(sportEventCI.GetScheduledEndAsync).SafeInvokeAsync(ExecutionLog, GetFetchErrorMessage("ScheduledEnd")).ConfigureAwait(false);
        }

        /// <summary>
        /// Constructs and returns a <see cref="string" /> containing the id of the current instance
        /// </summary>
        /// <returns>A <see cref="string" /> containing the id of the current instance</returns>
        protected override string PrintI()
        {
            return $"Id={Id}";
        }

        /// <summary>
        /// Constructs and returns a <see cref="string" /> containing compacted representation of the current instance
        /// </summary>
        /// <returns>A <see cref="string" /> containing compacted representation of the current instance</returns>
        protected override string PrintC()
        {
            var detailsCultures = string.Join(", ", Cultures.Select(k => k.TwoLetterISOLanguageName));
            string result = $"Id={Id}, Sport={GetSportIdAsync().Result}, ScheduledStartTime={GetScheduledTimeAsync().Result}, ScheduledEndTime={GetScheduledEndTimeAsync().Result}";
            result += $", CulturesLoaded=[{detailsCultures}]";
            return result;
        }

        /// <summary>
        /// Constructs and return a <see cref="string" /> containing details of the current instance
        /// </summary>
        /// <returns>A <see cref="string" /> containing details of the current instance</returns>
        protected override string PrintF()
        {
            return PrintC();
        }

        /// <summary>
        /// Constructs and returns a <see cref="string" /> containing a JSON representation of the current instance
        /// </summary>
        /// <returns>a <see cref="string" /> containing a JSON representation of the current instance</returns>
        protected override string PrintJ()
        {
            return PrintJ(GetType(), this);
        }

        /// <summary>
        /// Asynchronously gets a <see cref="Nullable{bool}"/> specifying if the start time to be determined is set for the associated sport event.
        /// </summary>
        /// <returns>A <see cref="Nullable{bool}"/> specifying if the start time to be determined is set for the associated sport event.</returns>
        public async Task<bool?> GetStartTimeTbdAsync()
        {
            var sportEventCI = SportEventCache.GetEventCacheItem(Id);
            if (sportEventCI == null)
            {
                ExecutionLog.Debug($"Missing data. No sportEvent cache item for id={Id}.");
                return null;
            }
            return ExceptionStrategy == ExceptionHandlingStrategy.THROW
                ? await sportEventCI.GetStartTimeTbdAsync().ConfigureAwait(false)
                : await new Func<Task<bool?>>(sportEventCI.GetStartTimeTbdAsync).SafeInvokeAsync(ExecutionLog, GetFetchErrorMessage("StartTimeTbd")).ConfigureAwait(false);
        }

        /// <summary>
        /// Asynchronously gets a <see cref="URN"/> specifying the replacement sport event for the associated sport event.
        /// </summary>
        /// <returns>A <see cref="URN"/> specifying the replacement sport event for the associated sport event.</returns>
        public async Task<URN> GetReplacedByAsync()
        {
            var sportEventCI = SportEventCache.GetEventCacheItem(Id);
            if (sportEventCI == null)
            {
                ExecutionLog.Debug($"Missing data. No sportEvent cache item for id={Id}.");
                return null;
            }
            return ExceptionStrategy == ExceptionHandlingStrategy.THROW
                ? await sportEventCI.GetReplacedByAsync().ConfigureAwait(false)
                : await new Func<Task<URN>>(sportEventCI.GetReplacedByAsync).SafeInvokeAsync(ExecutionLog, GetFetchErrorMessage("ReplacedBy")).ConfigureAwait(false);
        }
    }
}