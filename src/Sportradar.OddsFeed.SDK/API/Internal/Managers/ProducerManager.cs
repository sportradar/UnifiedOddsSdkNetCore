// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Linq;
using Dawn;
using Microsoft.Extensions.Logging;
using Sportradar.OddsFeed.SDK.Api.Config;
using Sportradar.OddsFeed.SDK.Api.EventArguments;
using Sportradar.OddsFeed.SDK.Api.Managers;
using Sportradar.OddsFeed.SDK.Common.Extensions;
using Sportradar.OddsFeed.SDK.Common.Internal;
using Sportradar.OddsFeed.SDK.Common.Internal.Telemetry;

namespace Sportradar.OddsFeed.SDK.Api.Internal.Managers
{
    /// <summary>
    /// Implementation of <see cref="IProducerManager"/>
    /// </summary>
    /// <seealso cref="IProducerManager" />
    internal class ProducerManager : IProducerManager
    {
        /// <summary>
        /// A <see cref="ILogger"/> instance used for logging
        /// </summary>
        private static readonly ILogger Log = SdkLoggerFactory.GetLoggerForExecution(typeof(ProducerManager));

        /// <summary>
        /// Occurs when a recovery initiation completes
        /// </summary>
        public event EventHandler<RecoveryInitiatedEventArgs> RecoveryInitiated;

        /// <summary>
        /// The producers
        /// </summary>
        private readonly IReadOnlyCollection<IProducer> _producers;

        /// <summary>
        /// Gets the available producers
        /// </summary>
        /// <value>The producers</value>
        public IReadOnlyCollection<IProducer> Producers => _producers;

        /// <summary>
        /// Indicates if user can still change anything
        /// </summary>
        private bool _locked;

        /// <summary>
        /// Initializes a new instance of the <see cref="ProducerManager"/> class
        /// </summary>
        /// <param name="config">The <see cref="IUofConfiguration"/> used for retrieve disabled producers</param>
        public ProducerManager(IUofConfiguration config)
        {
            Guard.Argument(config, nameof(config)).NotNull();

            if (config.Producer.Producers.IsNullOrEmpty())
            {
                throw new ArgumentException("Missing available producers");
            }

            _producers = config.Producer.Producers;

            if (!config.Producer.DisabledProducers.IsNullOrEmpty())
            {
                foreach (var disabledProducer in config.Producer.DisabledProducers)
                {
                    DisableProducer(disabledProducer);
                }
            }

            _locked = false;

            _ = UofSdkTelemetry.DefaultMeter.CreateObservableGauge(UofSdkTelemetry.MetricNameForProducerStatus, GetProducersStatuses, description: UofSdkTelemetry.MetricDescForProducerStatus);
            _ = UsageTelemetry.UsageMeter.CreateObservableGauge(UofSdkTelemetry.MetricNameForProducerStatus, GetProducersStatuses, description: UofSdkTelemetry.MetricDescForProducerStatus);
        }

        private IEnumerable<Measurement<int>> GetProducersStatuses()
        {
            return _producers.Where(w => w.IsAvailable && !w.IsDisabled)
                             .Select(producer => new Measurement<int>(producer.IsProducerDown ? 0 : 1,
                                                                      new KeyValuePair<string, object>("producer", $"{producer.Id}-{producer.Name}"),
                                                                      new KeyValuePair<string, object>("reason", producer.IsProducerDown ? "unknown" : string.Empty)));
        }

        /// <summary>
        /// Gets the producer
        /// </summary>
        /// <param name="id">The id of the producer to retrieve</param>
        /// <returns>An <see cref="IProducer" /></returns>
        /// <exception cref="ArgumentOutOfRangeException">id</exception>
        public IProducer GetProducer(int id)
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
        public IProducer GetProducer(string name)
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
            return GetProducer(id).Id == id;
        }

        /// <summary>
        /// Check if the <see cref="T:Sportradar.OddsFeed.SDK.Messages.IProducer" /> exists in manager
        /// </summary>
        /// <param name="name">The name to check</param>
        /// <returns><c>true</c> if exists, <c>false</c> otherwise</returns>
        /// <exception cref="NotImplementedException"></exception>
        public bool Exists(string name)
        {
            return GetProducer(name).Name.Equals(name, StringComparison.InvariantCultureIgnoreCase);
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
            var p = (Producer)GetProducer(id);
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

            var p = (Producer)GetProducer(id);
            if (p.Id.Equals(SdkInfo.UnknownProducerId))
            {
                return;
            }
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

            var p = (Producer)GetProducer(id);
            p.SetLastTimestampBeforeDisconnect(DateTime.MinValue);
        }

        /// <summary>
        /// Locks this instance from user changes
        /// </summary>
        /// <remarks>Also checks if at least 1 producer is enabled and if all LastTimestamps are valid</remarks>
        public void Lock()
        {
            if (!_producers.Any() || _producers.Count(c => c.IsAvailable && !c.IsDisabled) == 0)
            {
                throw new InvalidOperationException("No producer available or all are disabled");
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
                    ((Producer)producer).IgnoreRecovery = true;
                    ((Producer)producer).SetProducerDown(false);
                }
            }
            else
            {
                var p = (Producer)GetProducer(id);
                if (p != null)
                {
                    p.IgnoreRecovery = true;
                    p.SetProducerDown(false);
                }
            }
        }

        /// <summary>
        /// Gets the unknown producer
        /// </summary>
        /// <returns>The <see cref="IProducer"/> instance</returns>
        private static IProducer GetUnknownProducer()
        {
            return new Producer(id: SdkInfo.UnknownProducerId, name: "Unknown", description: "Unknown producer", apiUrl: "unknown", active: true, maxInactivitySeconds: 20, maxRecoveryTime: 3600, scope: "live|prematch|virtual", statefulRecoveryWindowInMinutes: 100);
        }

        /// <summary>
        /// Dispatches the <c>RecoveryInitiated</c>
        /// </summary>
        /// <param name="eventArgs">Event arguments</param>
        public void InvokeRecoveryInitiated(RecoveryInitiatedEventArgs eventArgs)
        {
            if (RecoveryInitiated == null)
            {
                Log.LogDebug("Cannot invoke RecoveryInitiated because no event listeners are attached to associated event handler");
                return;
            }

            var stopwatch = Stopwatch.StartNew();
            try
            {
                RecoveryInitiated(this, eventArgs);
                stopwatch.Stop();
                Log.LogInformation("Successfully called RecoveryInitiated event. Duration: {ElapsedMilliseconds} ms", stopwatch.ElapsedMilliseconds);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                Log.LogWarning(ex, "Event handler throw an exception while invoking RecoveryInitiated");
            }
        }
    }
}
