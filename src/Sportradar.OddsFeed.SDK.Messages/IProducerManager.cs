/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using Sportradar.OddsFeed.SDK.Messages.Contracts;

namespace Sportradar.OddsFeed.SDK.Messages
{
    /// <summary>
    /// Defines a contract implemented by classes used to manage feed producers
    /// </summary>
    [ContractClass(typeof(ProducerManagerContract))]
    public interface IProducerManager
    {
        /// <summary>
        /// Gets the available producers
        /// </summary>
        /// <value>The producers</value>
        IReadOnlyCollection<IProducer> Producers { get; }

        /// <summary>
        /// Disables the producer (no recovery will be made and not message will be received)
        /// </summary>
        /// <param name="id">The id of the producer</param>
        /// <exception cref="ArgumentOutOfRangeException">The id of the producer is not found</exception>
        void DisableProducer(int id);

        /// <summary>
        /// Gets the producer if exists, otherwise 'unknown' producer is returned
        /// </summary>
        /// <param name="id">The id of the producer to retrieve</param>
        /// <returns>An <see cref="IProducer"/></returns>
        IProducer Get(int id);

        /// <summary>
        /// Gets the producer by name if exists, otherwise 'unknown' producer is returned
        /// </summary>
        /// <param name="name">The name of the producer to retrieve</param>
        /// <remarks>Search is case insensitive</remarks>
        /// <returns>An <see cref="IProducer"/></returns>
        IProducer Get(string name);

        /// <summary>
        /// Check if the <see cref="IProducer"/> exists in manager
        /// </summary>
        /// <param name="id">The id to check</param>
        /// <returns><c>true</c> if exists, <c>false</c> otherwise</returns>
        bool Exists(int id);

        /// <summary>
        /// Check if the <see cref="IProducer"/> exists in manager
        /// </summary>
        /// <param name="name">The name to check</param>
        /// <returns><c>true</c> if exists, <c>false</c> otherwise</returns>
        bool Exists(string name);

        /// <summary>
        /// Sets the timestamp of the last processed message for a specific producer
        /// </summary>
        /// <param name="id">The id of the <see cref="IProducer" /> for which to set timestamp</param>
        /// <param name="timestamp">A <see cref="DateTime" /> specifying the message timestamp</param>
        /// <exception cref="ArgumentOutOfRangeException">The timestamp is in the future or to far in the past</exception>
        void AddTimestampBeforeDisconnect(int id, DateTime timestamp);

        /// <summary>
        /// Removes the timestamp of the last message processed for a specific producer
        /// </summary>
        /// <param name="id">An id of the <see cref="IProducer"/> for which to remove the timestamp</param>
        /// <exception cref="ArgumentOutOfRangeException">The id of the producer is not found</exception>
        void RemoveTimestampBeforeDisconnect(int id);
    }
}