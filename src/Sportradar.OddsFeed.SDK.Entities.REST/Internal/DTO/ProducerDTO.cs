/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using System.Collections.Generic;
using Dawn;
using Sportradar.OddsFeed.SDK.Messages.REST;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Internal.DTO
{
    /// <summary>
    /// A data-transfer-object representing producer info returned by Sports API
    /// </summary>
    public class ProducerDTO
    {
        /// <summary>
        /// Gets the Id of the associated producer
        /// </summary>
        internal int Id { get; }

        /// <summary>
        /// Gets the name of the associated producer
        /// </summary>
        internal string Name { get; }

        /// <summary>
        /// Gets the description of the associated producer
        /// </summary>
        internal string Description { get; }

        /// <summary>
        /// Gets a value indicating whether the associated producer is active
        /// </summary>
        internal bool Active { get; }

        /// <summary>
        /// Gets the url of the associated producer
        /// </summary>
        internal string Url { get; }

        /// <summary>
        /// Gets a <see cref="IEnumerable{T}"/> containing the scope names to which the producer belongs to.
        /// </summary>
        internal IEnumerable<string> ScopeNames { get; }

        /// <summary>
        /// Gets the stateful recovery window in minutes
        /// </summary>
        /// <value>The stateful recovery window in minutes</value>
        internal int StatefulRecoveryWindow { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ProducerDTO"/> class
        /// </summary>
        /// <param name="producer">A <see cref="producer"/> containing deserialized response from 'available producers' endpoint</param>
        internal ProducerDTO(producer producer)
        {
            Guard.Argument(producer).NotNull();

            Id = (int)producer.id;
            Name = producer.name;
            Description = producer.description;
            Active = producer.active;
            Url = producer.api_url;
            ScopeNames = string.IsNullOrEmpty(producer.scope)
                ? null
                : producer.scope.Split(new[] {"|"}, StringSplitOptions.RemoveEmptyEntries);
            StatefulRecoveryWindow = producer.stateful_recovery_window_in_minutes;
        }
    }
}
