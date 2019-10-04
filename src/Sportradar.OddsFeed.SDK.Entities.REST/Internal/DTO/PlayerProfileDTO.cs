/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using System.Diagnostics.Contracts;
using Sportradar.OddsFeed.SDK.Messages.REST;

// ReSharper disable FieldCanBeMadeReadOnly.Global

namespace Sportradar.OddsFeed.SDK.Entities.REST.Internal.DTO
{
    /// <summary>
    /// A data-transfer-object representing player's profile
    /// </summary>
    public class PlayerProfileDTO : SportEntityDTO
    {
        /// <summary>
        /// Gets a value describing the type(e.g. forward, defense, ...) of the player represented by current instance
        /// </summary>
        public string Type;

        /// <summary>
        /// Gets a <see cref="DateTime"/> specifying the date of birth of the player associated with the current instance
        /// </summary>
        public DateTime? DateOfBirth;

        /// <summary>
        /// Gets the nationality of the player represented by the current instance
        /// </summary>
        public string Nationality;

        /// <summary>
        /// Gets the height in centimeters of the player represented by the current instance or a null reference if height is not known
        /// </summary>
        public int? Height;

        /// <summary>
        /// Gets the weight in kilograms of the player represented by the current instance or a null reference if weight is not known
        /// </summary>
        public int? Weight;

        /// <summary>
        /// Gets the country code
        /// </summary>
        /// <value>The country code</value>
        public string CountryCode { get; }

        /// <summary>
        /// Gets the full name of the player
        /// </summary>
        /// <value>The full name</value>
        public string FullName { get; }

        /// <summary>
        /// Gets the nickname of the player
        /// </summary>
        /// <value>The nickname</value>
        public string Nickname { get; }

        /// <summary>
        /// Gets the jersey number
        /// </summary>
        /// <value>The jersey number</value>
        public int? JerseyNumber { get; }

        /// <summary>
        /// Gets the gender
        /// </summary>
        /// <value>The gender</value>
        public string Gender { get; }

        /// <summary>
        /// Gets the <see cref="DateTime"/> specifying when the associated message was generated (on the server side)
        /// </summary>
        public DateTime? GeneratedAt { get; }
        /// <summary>
        /// Initializes a new instance of the <see cref="PlayerProfileDTO"/> class
        /// </summary>
        /// <param name="record">A <see cref="playerExtended"/> containing information about the player</param>
        public PlayerProfileDTO(playerExtended record, DateTime? generatedAt)
            :base(record.id, record.name)
        {
            Contract.Requires(record != null);

            Type = record.type;
            DateOfBirth = string.IsNullOrEmpty(record.date_of_birth)
                ? null
                : (DateTime?) DateTime.ParseExact(record.date_of_birth, "yyyy-MM-dd", null);
            Nationality = record.nationality;
            Height = record.heightSpecified
                ? (int?) record.height
                : null;
            Weight = record.weightSpecified
                ? (int?) record.weight
                : null;
            CountryCode = record.country_code;
            FullName = record.full_name;
            Nickname = record.nickname;
            JerseyNumber = record.jersey_numberSpecified
                ? (int?) record.jersey_number
                : null;
            Gender = record.gender;
            GeneratedAt = generatedAt;
        }
    }
}
