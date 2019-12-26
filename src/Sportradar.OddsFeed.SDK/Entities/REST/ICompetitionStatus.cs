/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/

using System.Collections.Generic;
using Sportradar.OddsFeed.SDK.Entities.REST.Enums;
using Sportradar.OddsFeed.SDK.Messages;

namespace Sportradar.OddsFeed.SDK.Entities.REST
{
    /// <summary>
    /// Defines a contract implemented by classes representing the status of a <see cref="ICompetition"/>
    /// </summary>
    public interface ICompetitionStatus
    {
        /// <summary>
        /// Gets the winner identifier
        /// </summary>
        /// <value>The winner identifier, if available, otherwise null</value>
        URN WinnerId { get; }

        /// <summary>
        /// Gets a <see cref="EventStatus"/> describing the high-level status of the associated sport event
        /// </summary>
        EventStatus Status { get; }

        /// <summary>
        /// Returns a <see cref="ReportingStatus"/>  describing the reporting status of the associated sport event
        /// </summary>
        ReportingStatus ReportingStatus { get; }

        /// <summary>
        /// Gets the event results
        /// </summary>
        /// <value>The event results</value>
        IEnumerable<IEventResult> EventResults { get; }

        /// <summary>
        /// Gets the value of the property specified by it's name
        /// </summary>
        /// <param name="propertyName">The name of the property</param>
        /// <returns>A <see cref="object"/> representation of the value of the specified property, or a null reference
        /// if the value of the specified property was not specified</returns>
        object GetPropertyValue(string propertyName);

        /// <summary>
        /// Gets a <see cref="IReadOnlyDictionary{String, Object}"/> containing additional event status values
        /// </summary>
        /// <value>a <see cref="IReadOnlyDictionary{String, Object}"/> containing additional event status values</value>
        IReadOnlyDictionary<string, object> Properties { get; }
    }
}
