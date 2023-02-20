/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Dawn;
using Microsoft.Extensions.Logging;
using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Messages;

namespace Sportradar.OddsFeed.SDK.API.Internal
{
    /// <summary>
    /// Implementation of the <see cref="IProducer"/>
    /// </summary>
    /// <seealso cref="IProducer" />
    internal class Producer : IProducer
    {
        /// <summary>
        /// Gets the id of the producer
        /// </summary>
        /// <value>The id</value>
        public int Id { get; }

        /// <summary>
        /// Gets the name of the producer
        /// </summary>
        /// <value>The name</value>
        public string Name { get; }

        /// <summary>
        /// Gets the description of the producer
        /// </summary>
        /// <value>The description</value>
        public string Description { get; }

        /// <summary>
        /// Gets a value indicating whether the producer is available on feed
        /// </summary>
        /// <value><c>true</c> if this instance is active; otherwise, <c>false</c></value>
        public bool IsAvailable { get; }

        /// <summary>
        /// Gets a value indicating whether the producer is disabled
        /// </summary>
        /// <value><c>true</c> if this instance is disabled; otherwise, <c>false</c></value>
        public bool IsDisabled { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the producer is marked as down
        /// </summary>
        /// <value><c>true</c> if this instance is down; otherwise, <c>false</c></value>
        public bool IsProducerDown { get; private set; }

        /// <summary>
        /// Gets the last timestamp before disconnect for this producer
        /// </summary>
        /// <value>The last timestamp before disconnect</value>
        public DateTime LastTimestampBeforeDisconnect { get; private set; }

        /// <summary>
        /// Gets the maximum recovery time in seconds
        /// </summary>
        /// <value>The maximum recovery time in seconds</value>
        public int MaxRecoveryTime { get; }

        /// <summary>
        /// Gets the maximum inactivity seconds
        /// </summary>
        /// <value>The maximum inactivity seconds</value>
        public int MaxInactivitySeconds { get; }

        /// <summary>
        /// Gets the API URL.
        /// </summary>
        /// <value>The API URL</value>
        internal string ApiUrl { get; }

        /// <summary>
        /// Gets the code.
        /// </summary>
        /// <value>The code</value>
        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        internal string Code { get; }

        /// <summary>
        /// Gets a value indicating whether we should ignore recovery (true for ReplayServer)
        /// </summary>
        /// <value><c>true</c> if [ignore recovery]; otherwise, <c>false</c></value>
        internal bool IgnoreRecovery { get; set; }

        /// <summary>
        /// Gets the scope of the producer
        /// </summary>
        /// <value>The scope</value>
        public IReadOnlyCollection<string> Scope { get; }

        /// <summary>
        /// Gets the recovery info about last recovery attempt
        /// </summary>
        /// <value>The recovery info about last recovery attempt</value>
        public IRecoveryInfo RecoveryInfo { get; internal set; }

        /// <inheritdoc />
        public int StatefulRecoveryWindow { get; }

        internal ConcurrentDictionary<long, URN> EventRecoveries { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Producer"/> class
        /// </summary>
        /// <param name="id">The identifier</param>
        /// <param name="name">The name</param>
        /// <param name="description">The description</param>
        /// <param name="apiUrl">The API URL</param>
        /// <param name="active">if set to <c>true</c> [active]</param>
        /// <param name="maxInactivitySeconds">The maximum time between two alive messages before the producer is marked as down</param>
        /// <param name="maxRecoveryTime">The maximum time in seconds in which recovery must be completed</param>
        /// <param name="scope">The scope of the producer</param>
        /// <param name="statefulRecoveryWindowInMinutes">The stateful recovery window in minutes</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Major Code Smell", "S107:Methods should not have too many parameters", Justification = "Allowed here")]
        public Producer(int id, string name, string description, string apiUrl, bool active, int maxInactivitySeconds, int maxRecoveryTime, string scope, int statefulRecoveryWindowInMinutes)
        {
            Guard.Argument(id).Positive();
            Guard.Argument(name, nameof(name)).NotNull().NotEmpty();
            Guard.Argument(description, nameof(description)).NotNull().NotEmpty();
            Guard.Argument(apiUrl, nameof(apiUrl)).NotNull().NotEmpty();
            Guard.Argument(maxInactivitySeconds, nameof(maxInactivitySeconds)).Positive();
            Guard.Argument(maxRecoveryTime, nameof(maxRecoveryTime)).Positive();

            Id = id;
            Name = name;
            Description = description;
            ApiUrl = apiUrl;
            IsAvailable = active;
            IsDisabled = false;
            IsProducerDown = true;
            LastTimestampBeforeDisconnect = DateTime.MinValue;

            if (!string.IsNullOrEmpty(apiUrl))
            {
                var tmp = ApiUrl.EndsWith("/", StringComparison.InvariantCultureIgnoreCase) ? ApiUrl.Remove(apiUrl.Length - 1) : ApiUrl;
                Code = tmp.Split('/').Last();
            }

            MaxInactivitySeconds = maxInactivitySeconds;
            MaxRecoveryTime = maxRecoveryTime;
            Scope = scope.Split('|');
            StatefulRecoveryWindow = statefulRecoveryWindowInMinutes;

            IgnoreRecovery = false;
            EventRecoveries = new ConcurrentDictionary<long, URN>();
        }

        /// <summary>
        /// Sets the disabled
        /// </summary>
        /// <param name="disabled">if set to <c>true</c> [disabled]</param>
        internal void SetDisabled(bool disabled)
        {
            IsDisabled = disabled;
        }

        /// <summary>
        /// Sets the producer down.
        /// </summary>
        /// <param name="down">if set to <c>true</c> [down]</param>
        internal void SetProducerDown(bool down)
        {
            IsProducerDown = down;
        }

        /// <summary>
        /// Sets the last timestamp before disconnect
        /// </summary>
        /// <param name="timestamp">The timestamp</param>
        internal void SetLastTimestampBeforeDisconnect(DateTime timestamp)
        {
            if (timestamp >= LastTimestampBeforeDisconnect)
            {
                LastTimestampBeforeDisconnect = timestamp;
            }
            else if (timestamp < LastTimestampBeforeDisconnect.AddSeconds(-MaxInactivitySeconds))
            {
                var logger = SdkLoggerFactory.GetLoggerForExecution(typeof(Producer));
                logger.LogWarning($"Suspicious feed message timestamp arrived for producer {Id}. Current={LastTimestampBeforeDisconnect}. Arrived={timestamp}");
            }
        }

        /// <summary>
        /// Returns a <see cref="string" /> that represents this instance
        /// </summary>
        /// <returns>A <see cref="string" /> that represents this instance</returns>
        public override string ToString()
        {
            return $"{Id}({Name}):[IsUp={!IsProducerDown},Timestamp={LastTimestampBeforeDisconnect:dd.MM.yyyy-HH:mm:ss.fff}]";
        }

        public string ToString(string format, IFormatProvider formatProvider = null)
        {
            //Supported formats: C - compact, F - full, I - only id, J - json
            if (format == null)
            {
                format = "G";
            }
            format = format.ToLowerInvariant();

            if (formatProvider?.GetFormat(GetType()) is ICustomFormatter formatter)
            {
                return formatter.Format(format, this, formatProvider);
            }

            return format switch
            {
                "c" => $"{Id}-{Name}",
                "f" => $"{Id}({Name}):[IsUp={!IsProducerDown},Timestamp={LastTimestampBeforeDisconnect:dd.MM.yyyy-HH:mm:ss.fff}]",
                "i" => Id.ToString(),
                _ => $"{Id}({Name}):[IsUp={!IsProducerDown},Timestamp={LastTimestampBeforeDisconnect:dd.MM.yyyy-HH:mm:ss.fff}]",
            };
        }

        /// <summary>
        /// Determines whether the specified <see cref="object" /> is equal to this instance
        /// </summary>
        /// <param name="obj">The object to compare with the current object</param>
        /// <returns><c>true</c> if the specified <see cref="object" /> is equal to this instance; otherwise, <c>false</c></returns>
        public override bool Equals(object obj)
        {
            if (!(obj is Producer producer))
            {
                return false;
            }
            return Equals(producer);
        }

        /// <summary>
        /// Equals the specified other
        /// </summary>
        /// <param name="other">The other</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise</returns>
        protected bool Equals(Producer other)
        {
            if (other == null)
            {
                return false;
            }
            return Id == other.Id && string.Equals(Name, other.Name, StringComparison.InvariantCultureIgnoreCase);
        }

        /// <summary>
        /// Returns a hash code for this instance
        /// </summary>
        /// <returns>A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table</returns>
        public override int GetHashCode()
        {
            unchecked
            {
                return (Id * 397) ^ (Name != null ? Name.ToLowerInvariant().GetHashCode() : 0);
            }
        }
    }
}
