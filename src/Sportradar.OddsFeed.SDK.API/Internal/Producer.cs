/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using Sportradar.OddsFeed.SDK.Messages;

namespace Sportradar.OddsFeed.SDK.API.Internal
{
    /// <summary>
    /// Implementation of the <see cref="IProducerV1"/>
    /// </summary>
    /// <seealso cref="IProducerV1" />
    public class Producer : IProducerV1
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
        /// Gets the maximum recovery time
        /// </summary>
        /// <value>The maximum recovery time</value>
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
        internal IEnumerable<string> Scope { get; }

        /// <summary>
        /// Gets the time of last alive message received from feed
        /// </summary>
        /// <value>The time of last alive message</value>
        [Obsolete]
        public DateTime TimeOfLastAlive { get; private set; }

        /// <summary>
        /// Gets the recovery info about last recovery attempt
        /// </summary>
        /// <value>The recovery info about last recovery attempt</value>
        public IRecoveryInfo RecoveryInfo { get; internal set; }

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
        /// <param name="maxRecoveryTime">The maximum time in which recovery must be completed</param>
        /// <param name="scope">The scope of the producer</param>
        public Producer(int id, string name, string description, string apiUrl, bool active, int maxInactivitySeconds, int maxRecoveryTime, string scope)
        {
            Contract.Requires(id > 0);
            Contract.Requires(!string.IsNullOrEmpty(name));
            Contract.Requires(!string.IsNullOrEmpty(description));
            Contract.Requires(!string.IsNullOrEmpty(apiUrl));
            Contract.Requires(maxInactivitySeconds > 0);
            Contract.Requires(maxRecoveryTime > 0);

            Id = id;
            Name = name;
            Description = description;
            ApiUrl = apiUrl;
            IsAvailable = active;
            IsDisabled = false;
            IsProducerDown = true;
            TimeOfLastAlive = DateTime.MinValue;
            LastTimestampBeforeDisconnect = DateTime.MinValue;

            var tmp = ApiUrl.EndsWith("/") ? ApiUrl.Remove(apiUrl.Length - 1) : ApiUrl;
            Code = tmp.Split('/').Last();

            MaxInactivitySeconds = maxInactivitySeconds;
            MaxRecoveryTime = maxRecoveryTime;
            Scope = scope.Split('|');

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
        /// Sets the value indicating when the last alive message for this producer was received
        /// </summary>
        /// <param name="time">A <see cref="DateTime"/> specifying the time of last alive message</param>
        internal void SetTimeOfLastAlive(DateTime time)
        {
            TimeOfLastAlive = time;
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
            LastTimestampBeforeDisconnect = timestamp;
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance
        /// </summary>
        /// <returns>A <see cref="System.String" /> that represents this instance</returns>
        public override string ToString()
        {
            return $"{Id}({Name}):[IsUp={!IsProducerDown},Timestamp={LastTimestampBeforeDisconnect:dd.MM.yyyy-HH:mm:ss.fff}]";
        }

        /// <summary>
        /// Determines whether the specified <see cref="System.Object" /> is equal to this instance
        /// </summary>
        /// <param name="obj">The object to compare with the current object</param>
        /// <returns><c>true</c> if the specified <see cref="System.Object" /> is equal to this instance; otherwise, <c>false</c></returns>
        public override bool Equals(object obj)
        {
            if (!(obj is Producer))
            {
                return false;
            }
            return Equals((Producer)obj);
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
                return (Id * 397) ^ (Name != null ? Name.ToLower().GetHashCode() : 0);
            }
        }
    }
}