/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/

using System;
using System.Collections.Generic;
using System.Globalization;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Caching.Exportable
{
    /// <summary>
    /// Class used to export/import competitor cache item properties
    /// </summary>
    [Serializable]
    public class ExportableCompetitorCI : ExportableCI
    {
        /// <summary>
        /// A <see cref="IDictionary{K, V}"/> containing competitor's country name in different languages
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
        public IEnumerable<ExportableJerseyCI> Jerseys { get; set; }

        /// <summary>
        /// A <see cref="string"/> representing the country code
        /// </summary>
        public string CountryCode { get; set; }

        /// <summary>
        /// A <see cref="ExportableManagerCI"/> representing the manager
        /// </summary>
        public ExportableManagerCI Manager { get; set; }

        /// <summary>
        /// A <see cref="ExportableVenueCI"/> representing the venue
        /// </summary>
        public ExportableVenueCI Venue { get; set; }

        /// <summary>
        /// A <see cref="string"/> representing the gender
        /// </summary>
        public string Gender { get; set; }

        /// <summary>
        /// A <see cref="string"/> representing the age group
        /// </summary>
        public string AgeGroup { get; set; }

        /// <summary>
        /// A <see cref="ExportableRaceDriverProfileCI"/> representing the race driver profile
        /// </summary>
        public ExportableRaceDriverProfileCI RaceDriverProfile { get; set; }

        /// <summary>
        /// A <see cref="IEnumerable{T}"/> representing the languages for which the current instance has translations
        /// </summary>
        public IEnumerable<CultureInfo> FetchedCultures { get; set; }

        /// <summary>
        /// A <see cref="CultureInfo"/> representing the primary culture
        /// </summary>
        public CultureInfo PrimaryCulture { get; set; }

        /// <summary>
        /// A <see cref="DateTime"/>
        /// </summary>
        public DateTime? LastTimeCompetitorProfileIsFetched { get; set; }

        /// <summary>
        /// The list of CultureInfo used to fetch competitor profiles
        /// </summary>
        public IEnumerable<CultureInfo> CultureCompetitorProfileFetched { get; set; }
    }
}
