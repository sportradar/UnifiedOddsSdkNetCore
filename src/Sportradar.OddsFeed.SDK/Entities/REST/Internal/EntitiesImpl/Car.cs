/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using System.Runtime.Serialization;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching.CI;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Internal.EntitiesImpl
{
    /// <summary>
    /// Represents a car
    /// </summary>
    [DataContract]
    internal class Car : ICar
    {
        public Car(CarCI car)
        {
            if (car == null)
                throw new ArgumentNullException(nameof(car));

            Name = car.Name;
            Chassis = car.Chassis;
            EngineName = car.EngineName;
        }

        /// <summary>
        /// Gets the car name
        /// </summary>
        /// <value>The car name</value>
        public string Name { get; }

        /// <summary>
        /// Gets the car chassis
        /// </summary>
        /// <value>The car chassis</value>
        public string Chassis { get; }

        /// <summary>
        /// Gets the car engine name
        /// </summary>
        /// <value>The car engine name</value>
        public string EngineName { get; }
    }
}
