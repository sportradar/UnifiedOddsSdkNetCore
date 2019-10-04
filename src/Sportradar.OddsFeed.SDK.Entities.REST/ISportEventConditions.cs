/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/

namespace Sportradar.OddsFeed.SDK.Entities.REST
{
    /// <summary>
    /// Defines a contract implemented by classes representing sport event conditions
    /// </summary>
    public interface ISportEventConditions : IEntityPrinter
    {
        /// <summary>
        /// Gets a <see cref="string"/> specifying the attendance of the associated sport event
        /// </summary>
        string Attendance { get; }

        /// <summary>
        /// TODO: Add comments
        /// </summary>
        string EventMode { get; }

        /// <summary>
        /// Gets the <see cref="IReferee"/> instance representing the referee presiding over the associated sport event
        /// </summary>
        IReferee Referee { get; }

        /// <summary>
        /// Gets a <see cref="IWeatherInfo"/> instance representing the expected weather on the associated sport event
        /// </summary>
        IWeatherInfo WeatherInfo { get; }
    }
}
