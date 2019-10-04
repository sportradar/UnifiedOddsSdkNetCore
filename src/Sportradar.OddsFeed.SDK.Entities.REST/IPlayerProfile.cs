/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.Serialization;

namespace Sportradar.OddsFeed.SDK.Entities.REST
{
    /// <summary>
    /// Defines a contract implemented by classes representing a player profile
    /// </summary>
    public interface IPlayerProfile : IPlayer
    {
        /// <summary>
        /// Gets a value describing the type(e.g. forward, defense, ...) of the player represented by current instance
        /// </summary>
        [DataMember]
        string Type { get; }

        /// <summary>
        /// Gets a <see cref="DateTime"/> specifying the date of birth of the player associated with the current instance
        /// </summary>
        [DataMember]
        DateTime? DateOfBirth { get; }

        /// <summary>
        /// Gets the height in centimeters of the player represented by the current instance or a null reference if height is not known
        /// </summary>
        [DataMember]
        int? Height { get; }

        /// <summary>
        /// Gets the weight in kilograms of the player represented by the current instance or a null reference if weight is not known
        /// </summary>
        [DataMember]
        int? Weight { get; }

        /// <summary>
        /// Gets a <see cref="IReadOnlyDictionary{CultureInfo, String}"/> containing player nationality in different languages
        /// </summary>
        [DataMember]
        IReadOnlyDictionary<CultureInfo, string> Nationalities { get; }

        /// <summary>
        /// Gets the nationality of the player represented by the current instance in  the language specified by <code>culture</code>
        /// </summary>
        /// <param name="culture">A <see cref="CultureInfo"/> specifying the language of the returned nationality</param>
        /// <returns>The nationality of the player represented by the current instance in  the language specified by <code>culture</code></returns>
        string GetNationality(CultureInfo culture);
    }
}