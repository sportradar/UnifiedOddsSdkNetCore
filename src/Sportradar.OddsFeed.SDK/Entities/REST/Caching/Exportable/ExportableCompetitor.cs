// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using System.Collections.Generic;
using System.Globalization;

namespace Sportradar.OddsFeed.SDK.Entities.Rest.Caching.Exportable
{
    /// <summary>
    /// Class used to export/import competitor cache item properties
    /// </summary>
    [Serializable]
    public class ExportableCompetitor : ExportableBase
    {
        /// <summary>
        /// A <see cref="IDictionary{K,V}"/> containing competitor's country name in different languages
        /// </summary>
        public IDictionary<CultureInfo, string> CountryNames { get; set; }

        /// <summary>
        /// A <see cref="IDictionary{K, V}"/> containing competitor abbreviations in different languages
        /// </summary>
        public IDictionary<CultureInfo, string> Abbreviations { get; set; }

        /// <summary>
        /// A <see cref="IEnumerable{T}"/> containing associated player ids
        /// </summary>
        public IEnumerable<string> AssociatedPlayerIds { get; set; }

        /// <summary>
        /// A <see cref="IDictionary{K, V}"/> containing associated player jersey numbers
        /// </summary>
        public IDictionary<string, int> AssociatedPlayersJerseyNumbers { get; set; }

        /// <summary>
        /// A <see cref="bool"/> indicating whether represented competitor is virtual
        /// </summary>
        public bool IsVirtual { get; set; }

        /// <summary>
        /// A <see cref="IDictionary{K, V}"/> containing reference ids
        /// </summary>
        public IDictionary<string, string> ReferenceIds { get; set; }

        /// <summary>
        /// A <see cref="IEnumerable{T}"/> containing jerseys
        /// </summary>
        public IEnumerable<ExportableJersey> Jerseys { get; set; }

        /// <summary>
        /// A <see cref="string"/> representing the country code
        /// </summary>
        public string CountryCode { get; set; }

        /// <summary>
        /// A <see cref="string"/> representing the state
        /// </summary>
        public string State { get; set; }

        /// <summary>
        /// A <see cref="ExportableManager"/> representing the manager
        /// </summary>
        public ExportableManager Manager { get; set; }

        /// <summary>
        /// A <see cref="ExportableVenue"/> representing the venue
        /// </summary>
        public ExportableVenue Venue { get; set; }

        /// <summary>
        /// A <see cref="string"/> representing the gender
        /// </summary>
        public string Gender { get; set; }

        /// <summary>
        /// A <see cref="string"/> representing the age group
        /// </summary>
        public string AgeGroup { get; set; }

        /// <summary>
        /// A <see cref="ExportableRaceDriverProfile"/> representing the race driver profile
        /// </summary>
        public ExportableRaceDriverProfile RaceDriverProfile { get; set; }

        /// <summary>
        /// A <see cref="CultureInfo"/> representing the primary culture
        /// </summary>
        public CultureInfo PrimaryCulture { get; set; }

        /// <summary>
        /// The list of CultureInfo used to fetch competitor profiles
        /// </summary>
        public IDictionary<CultureInfo, DateTime> CultureCompetitorProfileFetched { get; set; }

        /// <summary>
        /// A <see cref="string"/> representing the sport id
        /// </summary>
        public string SportId { get; set; }

        /// <summary>
        /// A <see cref="string"/> representing the category id
        /// </summary>
        public string CategoryId { get; set; }

        /// <summary>
        /// Gets the short name
        /// </summary>
        /// <value>The short name</value>
        public string ShortName { get; set; }

        /// <summary>
        /// GEts the division
        /// </summary>
        public ExportableDivision Division { get; set; }
    }
}
