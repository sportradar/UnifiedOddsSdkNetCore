/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Dawn;
using Microsoft.Extensions.Logging;
using Sportradar.OddsFeed.SDK.Api.Internal.Caching;
using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Common.Enums;
using Sportradar.OddsFeed.SDK.Common.Internal;
using Sportradar.OddsFeed.SDK.Common.Internal.Telemetry;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Caching.Events;

namespace Sportradar.OddsFeed.SDK.Entities.Rest.Internal.EntitiesImpl
{
    /// <summary>
    /// Represents all sport events(races, matches, tournaments, ....)
    /// </summary>
    /// <seealso cref="EntityPrinter" />
    internal class SportEvent : EntityPrinter, ISportEvent
    {
        /// <summary>
        /// The sport identifier
        /// </summary>
        protected Urn SportId;

        /// <summary>
        /// The <see cref="ILogger"/> instance used for execution logging
        /// </summary>
        protected readonly ILogger ExecutionLog;

        /// <summary>
        /// A <see cref="ExceptionHandlingStrategy"/> enum member specifying how the instance will handle potential exceptions
        /// </summary>
        protected readonly ExceptionHandlingStrategy ExceptionStrategy;

        /// <summary>
        /// Gets a <see cref="Urn"/> uniquely identifying the sport event
        /// </summary>
        public Urn Id { get; }

        /// <summary>
        /// A <see cref="IEnumerable{T}"/> specifying languages the current instance supports
        /// </summary>
        public readonly IReadOnlyCollection<CultureInfo> Cultures;

        /// <summary>
        /// A <see cref="ISportEventCache"/> instance containing <see cref="SportEventCacheItem"/>
        /// </summary>
        protected readonly ISportEventCache SportEventCache;

        /// <summary>
        /// Initializes a new instance of the <see cref="SportEvent"/> class
        /// </summary>
        /// <param name="id">A <see cref="Urn"/> uniquely identifying the sport event</param>
        /// <param name="sportId">A <see cref="Urn"/> identifying the sport current instance belong to</param>
        /// <param name="executionLog">The <see cref="ILogger"/> instance used for execution logging</param>
        /// <param name="sportEventCache">A <see cref="ISportEventCache"/> instance containing <see cref="SportEventCacheItem"/></param>
        /// <param name="cultures">A <see cref="IReadOnlyCollection{CultureInfo}"/> specifying languages the current instance supports</param>
        /// <param name="exceptionStrategy">A <see cref="ExceptionHandlingStrategy"/> enum member specifying how the instance will handle potential exceptions</param>
        public SportEvent(Urn id,
                        Urn sportId,
                        ILogger executionLog,
                        ISportEventCache sportEventCache,
                        IReadOnlyCollection<CultureInfo> cultures,
                        ExceptionHandlingStrategy exceptionStrategy)
        {
            Guard.Argument(id, nameof(id)).NotNull();
            Guard.Argument(sportEventCache, nameof(sportEventCache)).NotNull();
            Guard.Argument(cultures, nameof(cultures)).NotNull().NotEmpty();

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
            Guard.Argument(propertyName, nameof(propertyName)).NotNull().NotEmpty();

            return $"Error occurred while attempting to get {propertyName} for sport event with Id={Id} from cache";
        }

        /// <inheritdoc />
        public async Task<string> GetNameAsync(CultureInfo culture)
        {
            var sportEventCacheItem = SportEventCache.GetEventCacheItem(Id);
            if (sportEventCacheItem == null)
            {
                ExecutionLog.LogDebug($"Missing data. No sportEvent cache item for id={Id}.");
                return null;
            }

            var cultureList = new[] { culture };
            var item = ExceptionStrategy == ExceptionHandlingStrategy.Throw
                ? await sportEventCacheItem.GetNamesAsync(cultureList).ConfigureAwait(false)
                : await new Func<IEnumerable<CultureInfo>, Task<IReadOnlyDictionary<CultureInfo, string>>>(sportEventCacheItem.GetNamesAsync).SafeInvokeAsync(cultureList, ExecutionLog, GetFetchErrorMessage("Name"))
                    .ConfigureAwait(false);

            return item == null || !item.ContainsKey(culture)
                ? null
                : item[culture];
        }

        /// <summary>
        /// Asynchronously gets a <see cref="Urn"/> uniquely identifying the sport associated with the current instance
        /// </summary>
        /// <returns>Returns the sport id</returns>
        public async Task<Urn> GetSportIdAsync()
        {
            if (SportId == null)
            {
                var sportEventCacheItem = SportEventCache.GetEventCacheItem(Id);
                if (sportEventCacheItem == null)
                {
                    ExecutionLog.LogDebug($"Missing data. No sportEvent cache item for id={Id}.");
                    return null;
                }
                SportId = await sportEventCacheItem.GetSportIdAsync().ConfigureAwait(false);
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
            var sportEventCacheItem = SportEventCache.GetEventCacheItem(Id);
            if (sportEventCacheItem == null)
            {
                ExecutionLog.LogDebug($"Missing data. No sportEvent cache item for id={Id}.");
                return null;
            }
            return ExceptionStrategy == ExceptionHandlingStrategy.Throw
                ? await sportEventCacheItem.GetScheduledAsync().ConfigureAwait(false)
                : await new Func<Task<DateTime?>>(sportEventCacheItem.GetScheduledAsync).SafeInvokeAsync(ExecutionLog, GetFetchErrorMessage("ScheduledTime")).ConfigureAwait(false);
        }

        /// <summary>
        /// When overridden in derived class, it asynchronously gets a <see cref="T:System.DateTime" /> instance specifying for when the sport event associated with the current instance is
        /// scheduled to end or a null reference if the value is not known
        /// </summary>
        /// <returns>A <see cref="T:System.Threading.Tasks.Task`1" /> representing the retrieval operation</returns>
        public async Task<DateTime?> GetScheduledEndTimeAsync()
        {
            var sportEventCacheItem = SportEventCache.GetEventCacheItem(Id);
            if (sportEventCacheItem == null)
            {
                ExecutionLog.LogDebug($"Missing data. No sportEvent cache item for id={Id}.");
                return null;
            }
            return ExceptionStrategy == ExceptionHandlingStrategy.Throw
                ? await sportEventCacheItem.GetScheduledEndAsync().ConfigureAwait(false)
                : await new Func<Task<DateTime?>>(sportEventCacheItem.GetScheduledEndAsync).SafeInvokeAsync(ExecutionLog, GetFetchErrorMessage("ScheduledEnd")).ConfigureAwait(false);
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
            var result = $"Id={Id}, Sport={GetSportIdAsync().GetAwaiter().GetResult()}, ScheduledStartTime={GetScheduledTimeAsync().GetAwaiter().GetResult()}, ScheduledEndTime={GetScheduledEndTimeAsync().GetAwaiter().GetResult()}";
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
        /// Asynchronously gets a <see cref="bool"/> specifying if the start time to be determined is set for the associated sport event.
        /// </summary>
        /// <returns>A <see cref="bool"/> specifying if the start time to be determined is set for the associated sport event.</returns>
        public async Task<bool?> GetStartTimeTbdAsync()
        {
            var sportEventCacheItem = SportEventCache.GetEventCacheItem(Id);
            if (sportEventCacheItem == null)
            {
                ExecutionLog.LogDebug($"Missing data. No sportEvent cache item for id={Id}.");
                return null;
            }
            return ExceptionStrategy == ExceptionHandlingStrategy.Throw
                ? await sportEventCacheItem.GetStartTimeTbdAsync().ConfigureAwait(false)
                : await new Func<Task<bool?>>(sportEventCacheItem.GetStartTimeTbdAsync).SafeInvokeAsync(ExecutionLog, GetFetchErrorMessage("StartTimeTbd")).ConfigureAwait(false);
        }

        /// <summary>
        /// Asynchronously gets a <see cref="Urn"/> specifying the replacement sport event for the associated sport event.
        /// </summary>
        /// <returns>A <see cref="Urn"/> specifying the replacement sport event for the associated sport event.</returns>
        public async Task<Urn> GetReplacedByAsync()
        {
            var sportEventCacheItem = SportEventCache.GetEventCacheItem(Id);
            if (sportEventCacheItem == null)
            {
                ExecutionLog.LogDebug($"Missing data. No sportEvent cache item for id={Id}.");
                return null;
            }
            return ExceptionStrategy == ExceptionHandlingStrategy.Throw
                ? await sportEventCacheItem.GetReplacedByAsync().ConfigureAwait(false)
                : await new Func<Task<Urn>>(sportEventCacheItem.GetReplacedByAsync).SafeInvokeAsync(ExecutionLog, GetFetchErrorMessage("ReplacedBy")).ConfigureAwait(false);
        }
    }
}
