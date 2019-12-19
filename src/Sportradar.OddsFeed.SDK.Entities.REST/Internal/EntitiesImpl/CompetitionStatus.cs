/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System.Collections.Generic;
using Dawn;
using System.Linq;
using Sportradar.OddsFeed.SDK.Entities.REST.Enums;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching.Events;
using Sportradar.OddsFeed.SDK.Messages;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Internal.EntitiesImpl
{
    /// <summary>
    /// Class CompetitionStatus
    /// </summary>
    /// <seealso cref="ICompetitionStatus" />
    public class CompetitionStatus : ICompetitionStatus
    {
        /// <summary>
        /// The sport event status dto
        /// </summary>
        public SportEventStatusCI SportEventStatusCI { get; }

        /// <summary>
        /// Gets the winner identifier
        /// </summary>
        /// <value>The winner identifier, if available, otherwise null</value>
        public URN WinnerId { get; }

        /// <summary>
        /// Gets a <see cref="EventStatus" /> describing the high-level status of the associated sport event
        /// </summary>
        /// <value>The status.</value>
        public EventStatus Status { get; }

        /// <summary>
        /// Returns a <see cref="ReportingStatus" />  describing the reporting status of the associated sport event
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
        /// Gets the value of the property specified by it's name
        /// </summary>
        /// <param name="propertyName">The name of the property</param>
        /// <returns>A <see cref="object" /> representation of the value of the specified property, or a null reference
        /// if the value of the specified property was not specified</returns>
        public object GetPropertyValue(string propertyName)
        {
            Guard.Argument(propertyName).NotNull().NotEmpty();

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
        public CompetitionStatus(SportEventStatusCI ci, ILocalizedNamedValueCache matchStatusesCache)
        {
            Guard.Argument(ci).NotNull();

            SportEventStatusCI = ci;

            WinnerId = ci.WinnerId;
            Status = ci.Status;
            ReportingStatus = ci.ReportingStatus;
            if (ci.EventResults != null)
            {
                EventResults = ci.EventResults.Select(s => new EventResult(s, matchStatusesCache));
            }
            Properties = ci.Properties;
        }
    }
}
