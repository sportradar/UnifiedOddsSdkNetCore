/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using Sportradar.OddsFeed.SDK.API.EventArguments;
using Sportradar.OddsFeed.SDK.Entities.REST;

namespace Sportradar.OddsFeed.SDK.API
{
    /// <summary>
    /// Defines a contract implemented by classes capable of getting list of <see cref="IFixtureChange"/> and/or <see cref="IResultChange"/>
    /// </summary>
    public interface IEventChangeManager
    {
        /// <summary>
        /// Raised for <see cref="IFixtureChange"/> message
        /// </summary>
        event EventHandler<EventChangeEventArgs> FixtureChange;

        /// <summary>
        /// Raised for <see cref="IResultChange"/> message
        /// </summary>
        event EventHandler<EventChangeEventArgs> ResultChange;

        /// <summary>
        /// Gets the timestamp of last processed fixture change
        /// </summary>
        /// <value>The last fixture change.</value>
        DateTime LastFixtureChange { get; }

        /// <summary>
        /// Gets the timestamp of last processed result change
        /// </summary>
        /// <value>The last result change.</value>
        DateTime LastResultChange { get; }

        /// <summary>
        /// Gets the interval for getting new list of fixture changes
        /// </summary>
        /// <value>The fixture change interval.</value>
        TimeSpan FixtureChangeInterval { get; }

        /// <summary>
        /// Gets the interval for getting new list of result changes
        /// </summary>
        /// <value>The result change interval.</value>
        TimeSpan ResultChangeInterval { get; }

        /// <summary>
        /// Gets a value indicating whether this instance is running.
        /// </summary>
        /// <value><c>true</c> if this instance is running; otherwise, <c>false</c>.</value>
        bool IsRunning { get; }

        /// <summary>
        /// Sets the fixture change interval between two Sports API requests. Must be between 1 min and 12 hours.
        /// </summary>
        /// <param name="fixtureChangeInterval">The fixture change interval.</param>
        void SetFixtureChangeInterval(TimeSpan fixtureChangeInterval);

        /// <summary>
        /// Sets the result change interval between two Sports API requests. Must be between 1 min and 12 hours.
        /// </summary>
        /// <param name="resultChangeInterval">The result change interval.</param>
        void SetResultChangeInterval(TimeSpan resultChangeInterval);

        /// <summary>
        /// Sets the last processed fixture change timestamp.
        /// </summary>
        /// <param name="fixtureChangeTimestamp">The fixture change timestamp.</param>
        /// <remarks>It can be only set when it is stopped</remarks>
        void SetFixtureChangeTimestamp(DateTime fixtureChangeTimestamp);

        /// <summary>
        /// Sets the last processed result change timestamp.
        /// </summary>
        /// <param name="resultChangeTimestamp">The result change timestamp.</param>
        /// <remarks>It can be only set when it is stopped</remarks>
        void SetResultChangeTimestamp(DateTime resultChangeTimestamp);

        /// <summary>
        /// Starts scheduled job for fetching fixture and result changes
        /// </summary>
        void Start();

        /// <summary>
        /// Stops scheduled job for fetching fixture and result changes
        /// </summary>
        void Stop();
    }
}