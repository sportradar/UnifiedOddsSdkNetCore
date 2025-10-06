// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System.Collections.Generic;
using System.Linq;
using Dawn;
using Sportradar.OddsFeed.SDK.Api.Internal.Caching;
using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Entities.Rest.Enums;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Caching.Events;

namespace Sportradar.OddsFeed.SDK.Entities.Rest.Internal.EntitiesImpl
{
    /// <summary>
    /// Class CompetitionStatus
    /// </summary>
    /// <seealso cref="ICompetitionStatus" />
    internal class CompetitionStatus : ICompetitionStatus
    {
        /// <summary>
        /// The sport event status dto
        /// </summary>
        public SportEventStatusCacheItem SportEventStatusCacheItem { get; }

        /// <summary>
        /// Gets the winner identifier
        /// </summary>
        /// <value>The winner identifier, if available, otherwise null</value>
        public Urn WinnerId { get; }

        /// <summary>
        /// Gets a <see cref="EventStatus" /> describing the high-level status of the associated sport event
        /// </summary>
        /// <value>The status.</value>
        public EventStatus Status { get; }

        /// <summary>
        /// Returns a <see cref="ReportingStatus" /> describing the reporting status of the associated sport event
        /// </summary>
        /// <value>The reporting status.</value>
        public ReportingStatus ReportingStatus { get; }

        /// <summary>
        /// Gets the event results
        /// </summary>
        /// <value>The event results</value>
        public IEnumerable<IEventResult> EventResults { get; }

        /// <summary>
        /// Gets a <see cref="IReadOnlyDictionary{String, Object}" /> containing additional event status values
        /// </summary>
        /// <value>a <see cref="IReadOnlyDictionary{String, Object}" /> containing additional event status values</value>
        public IReadOnlyDictionary<string, object> Properties { get; }

        /// <summary>
        /// Gets the period of ladder.
        /// </summary>
        /// <value>The period of ladder.</value>
        public int? PeriodOfLadder { get; }

        /// <summary>
        /// The cache for match statuses
        /// </summary>
        protected readonly ILocalizedNamedValueCache MatchStatusCache;

        /// <summary>
        /// Gets the value of the property specified by it's name
        /// </summary>
        /// <param name="propertyName">The name of the property</param>
        /// <returns>A <see cref="object" /> representation of the value of the specified property, or a null reference
        /// if the value of the specified property was not specified</returns>
        public object GetPropertyValue(string propertyName)
        {
            Guard.Argument(propertyName, nameof(propertyName)).NotNull().NotEmpty();

            if (Properties != null && Properties.ContainsKey(propertyName))
            {
                return Properties[propertyName];
            }
            return null;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CompetitionStatus"/> class.
        /// </summary>
        /// <param name="ci">The status cache item</param>
        /// <param name="matchStatusesCache">The <see cref="ILocalizedNamedValueCache"/> used to get match status id and description</param>
        public CompetitionStatus(SportEventStatusCacheItem ci, ILocalizedNamedValueCache matchStatusesCache)
        {
            Guard.Argument(ci, nameof(ci)).NotNull();
            Guard.Argument(matchStatusesCache, nameof(matchStatusesCache)).NotNull();

            SportEventStatusCacheItem = ci;
            if (matchStatusesCache != null)
            {
                MatchStatusCache = matchStatusesCache;
            }

            WinnerId = ci.WinnerId;
            Status = ci.Status;
            ReportingStatus = ci.ReportingStatus;
            if (ci.EventResults != null)
            {
                EventResults = ci.EventResults.Select(s => new EventResult(s, MatchStatusCache));
            }
            Properties = ci.Properties;
            PeriodOfLadder = ci.PeriodOfLadder;
        }
    }
}
