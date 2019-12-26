/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Dawn;
using System.Linq;
using Sportradar.OddsFeed.SDK.Common.Exceptions;
using Sportradar.OddsFeed.SDK.Common.Internal;
using Sportradar.OddsFeed.SDK.Messages;
using Sportradar.OddsFeed.SDK.Messages.Internal;

namespace Sportradar.OddsFeed.SDK.API.Internal
{
    /// <summary>
    /// Implementation of <see cref="IProducerManager"/>
    /// </summary>
    /// <seealso cref="IProducerManager" />
    public class ProducerManager : IProducerManager
    {
        /// <summary>
        /// The producers
        /// </summary>
        private IReadOnlyCollection<IProducer> _producers;

        /// <summary>
        /// Gets the available producers
        /// </summary>
        /// <value>The producers</value>
        public IReadOnlyCollection<IProducer> Producers => _producers;

        /// <summary>
        /// The producers provider
        /// </summary>
        private readonly IProducersProvider _producersProvider;

        /// <summary>
        /// Indicates if user can still change anything
        /// </summary>
        private bool _locked;

        /// <summary>
        /// Initializes a new instance of the <see cref="ProducerManager"/> class
        /// </summary>
        /// <param name="producersProvider">The producers provider.</param>
        /// <param name="config">The <see cref="IOddsFeedConfiguration"/> used for retrieve disabled producers</param>
        [SuppressMessage("ReSharper", "PossibleMultipleEnumeration")]
        public ProducerManager(IProducersProvider producersProvider, IOddsFeedConfiguration config)
        {
            Guard.Argument(producersProvider, nameof(producersProvider)).NotNull();
            Guard.Argument(config, nameof(config)).NotNull();

            _producersProvider = producersProvider;

            LoadProducers();

            if (config.DisabledProducers != null && config.DisabledProducers.Any())
            {
                foreach (var disabledProducer in config.DisabledProducers)
                {
                    DisableProducer(disabledProducer);
                }
            }

            _locked = false;
        }

        /// <summary>
        /// Loads the producers from api
        /// </summary>
        private void LoadProducers()
        {
            var producers = _producersProvider.GetProducers();
            _producers = producers as IReadOnlyCollection<IProducer>;
        }

        /// <summary>
        /// Gets the producer
        /// </summary>
        /// <param name="id">The id of the producer to retrieve</param>
        /// <returns>An <see cref="IProducer" /></returns>
        /// <exception cref="ArgumentOutOfRangeException">id</exception>
        public IProducer Get(int id)
        {
            Guard.Argument(id, nameof(id)).Positive();

            var p = _producers?.FirstOrDefault(f => f.Id == id);
            if (p == null || p.Id != id)
            {
                return GetUnknownProducer();
            }
            return p;
        }

        /// <summary>
        /// Gets the producer by name (case insensitive)
        /// </summary>
        /// <param name="name">The name of the producer to retrieve</param>
        /// <returns>An <see cref="IProducer" /></returns>
        public IProducer Get(string name)
        {
            Guard.Argument(name, nameof(name)).NotNull().NotEmpty();

            var p = _producers.FirstOrDefault(f => string.Equals(name, f.Name, StringComparison.InvariantCultureIgnoreCase));
            if (p == null || p.Id == 0)
            {
                return GetUnknownProducer();
            }
            return p;
        }

        /// <summary>
        /// Check if the <see cref="T:Sportradar.OddsFeed.SDK.Messages.IProducer" /> exists in manager
        /// </summary>
        /// <param name="id">The id to check</param>
        /// <returns><c>true</c> if exists, <c>false</c> otherwise</returns>
        public bool Exists(int id)
        {
            return Get(id).Id == id;
        }

        /// <summary>
        /// Check if the <see cref="T:Sportradar.OddsFeed.SDK.Messages.IProducer" /> exists in manager
        /// </summary>
        /// <param name="name">The name to check</param>
        /// <returns><c>true</c> if exists, <c>false</c> otherwise</returns>
        /// <exception cref="NotImplementedException"></exception>
        public bool Exists(string name)
        {
            return Get(name).Name.Equals(name, StringComparison.InvariantCultureIgnoreCase);
        }

        /// <summary>
        /// Disables the producer (no recovery will be made and not message will be received)
        /// </summary>
        /// <param name="id">The id of the producer</param>
        public void DisableProducer(int id)
        {
            if (_locked)
            {
                throw new InvalidOperationException("Change to producer is not allowed anymore.");
            }
            var p = (Producer)Get(id);
            p.SetDisabled(true);
        }

        /// <summary>
        /// Sets the timestamp of the last processed message for a specific producer
        /// </summary>
        /// <param name="id">The <see cref="IProducer" /> associated with the message</param>
        /// <param name="timestamp">A <see cref="DateTime" /> specifying the message timestamp</param>
        /// <exception cref="ArgumentOutOfRangeException">The timestamp is in the future or to far in the past</exception>
        public void AddTimestampBeforeDisconnect(int id, DateTime timestamp)
        {
            if (_locked)
            {
                throw new InvalidOperationException("Change to producer is not allowed anymore.");
            }

            Guard.Argument(id, nameof(id)).Positive();
            Guard.Argument(timestamp, nameof(timestamp)).Require(timestamp > DateTime.MinValue);

            var p = (Producer) Get(id);
            if (timestamp > DateTime.Now)
            {
                throw new ArgumentOutOfRangeException(nameof(timestamp), $"The value {timestamp} specifies the time in the future");
            }
            if (timestamp < DateTime.Now.Subtract(p.MaxAfterAge()))
            {
                throw new ArgumentOutOfRangeException(nameof(timestamp), $"The value {timestamp} specifies the time to far in the past. Timestamp must be greater then {DateTime.Now.Subtract(p.MaxAfterAge())}");
            }
            p.SetLastTimestampBeforeDisconnect(timestamp);
        }

        /// <summary>
        /// Removes the timestamp of the last message processed for a specific producer
        /// </summary>
        /// <param name="id">An id of the <see cref="IProducer" /> for which to remove the timestamp</param>
        public void RemoveTimestampBeforeDisconnect(int id)
        {
            if (_locked)
            {
                throw new InvalidOperationException("Change to producer is not allowed anymore.");
            }

            Guard.Argument(id, nameof(id)).Positive();

            var p = (Producer) Get(id);
            p.SetLastTimestampBeforeDisconnect(DateTime.MinValue);
        }

        /// <summary>
        /// Locks this instance from user changes
        /// </summary>
        /// <remarks>Also checks if at least 1 producer is enabled and if all LastTimestamps are valid</remarks>
        public void Lock()
        {
            if (_producers == null)
            {
                throw new CommunicationException("No producer available.", "API:AvailableProducers", null);
            }
            if (!_producers.Any() || _producers.Count(c => c.IsAvailable && !c.IsDisabled) == 0)
            {
                throw new InvalidOperationException("No producer available or all are disabled.");
            }
            foreach (var producer in _producers)
            {
                if (producer.LastTimestampBeforeDisconnect != DateTime.MinValue && producer.LastTimestampBeforeDisconnect < DateTime.Now.Subtract(producer.MaxAfterAge()))
                {
                    var err = $"Recovery timestamp for producer {producer} is too far in the past. TimeStamp={producer.LastTimestampBeforeDisconnect}";
                    throw new InvalidOperationException(err);
                }
            }
            _locked = true;
        }

        /// <summary>
        /// Sets the ignore recovery to one specific producer or all
        /// </summary>
        /// <param name="id">The identifier of producer or 0 for all</param>
        public void SetIgnoreRecovery(int id)
        {
            if (id == 0)
            {
                foreach (var producer in _producers)
                {
                    ((Producer) producer).IgnoreRecovery = true;
                    ((Producer) producer).SetProducerDown(false);
                }
            }
            else
            {
                var p = (Producer)Get(id);
                p.IgnoreRecovery = true;
                p.SetProducerDown(false);
            }
        }

        /// <summary>
        /// Gets the unknown producer
        /// </summary>
        /// <returns>The <see cref="IProducer"/> instance</returns>
        public static IProducer GetUnknownProducer()
        {
            return new Producer(id: SdkInfo.UnknownProducerId, name: "Unknown", description: "Unknown producer", apiUrl: "unknown", active: true, maxInactivitySeconds: 20, maxRecoveryTime: 3600, scope: "live|prematch|virtual");
        }
    }
}
