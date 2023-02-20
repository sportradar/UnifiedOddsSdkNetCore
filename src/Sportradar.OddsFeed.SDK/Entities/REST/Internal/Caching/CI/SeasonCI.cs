/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Dawn;
using Sportradar.OddsFeed.SDK.Entities.REST.Caching.Exportable;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.DTO;
using Sportradar.OddsFeed.SDK.Messages;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching.CI
{
    /// <summary>
    /// Defines a cache item for season
    /// </summary>
    internal class SeasonCI
    {
        /// <summary>
        /// Gets a <see cref="URN"/> representing the ID of the represented sport entity
        /// </summary>
        /// <value>The identifier</value>
        internal URN Id { get; private set; }

        /// <summary>
        /// A <see cref="IDictionary{CultureInfo, String}"/> containing season names in different languages
        /// </summary>
        internal readonly IDictionary<CultureInfo, string> Names;

        /// <summary>
        /// Gets a <see cref="DateTime"/> specifying the start date of the season
        /// </summary>
        internal DateTime StartDate { get; private set; }

        /// <summary>
        /// Gets a <see cref="DateTime"/> specifying the end date of the season
        /// </summary>
        internal DateTime EndDate { get; private set; }

        /// <summary>
        /// Gets the <see cref="string"/> representation the year of the season
        /// </summary>
        internal string Year { get; private set; }

        /// <summary>
        /// Gets the associated tournament identifier
        /// </summary>
        /// <value>The associated tournament identifier</value>
        internal URN TournamentId { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SeasonCI"/> class
        /// </summary>
        /// <param name="dto">The <see cref="SeasonDTO"/> used to create new instance</param>
        /// <param name="culture">A <see cref="CultureInfo"/> specifying the language of the season info</param>
        public SeasonCI(SeasonDTO dto, CultureInfo culture)
        {
            Guard.Argument(dto, nameof(dto)).NotNull();
            Guard.Argument(culture, nameof(culture)).NotNull();

            Names = new Dictionary<CultureInfo, string>();
            Merge(dto, culture);
        }

        /// <summary>
        /// Gets the name of the associated season in the specified language
        /// </summary>
        /// <param name="culture">A <see cref="CultureInfo"/> specifying the language in which to get the name</param>
        /// <returns>The name of the associated season in the specified language.</returns>
        public string GetName(CultureInfo culture)
        {
            Guard.Argument(culture, nameof(culture)).NotNull();

            return Names.ContainsKey(culture)
                ? Names[culture]
                : null;
        }

        /// <summary>
        /// Determines whether the current instance has translations for the specified languages
        /// </summary>
        /// <param name="cultures">A <see cref="IEnumerable{CultureInfo}"/> specifying the required languages</param>
        /// <returns>True if the current instance contains data in the required locals. Otherwise false.</returns>
        public virtual bool HasTranslationsFor(IEnumerable<CultureInfo> cultures)
        {
            return cultures.All(c => Names.ContainsKey(c));
        }

        /// <summary>
        /// Merges the information from the provided <see cref="SeasonDTO"/> to the current instance
        /// </summary>
        /// <param name="season">A <see cref="SeasonDTO"/> containing season info</param>
        /// <param name="culture">A <see cref="CultureInfo"/> specifying the language of the season info</param>
        public void Merge(SeasonDTO season, CultureInfo culture)
        {
            Guard.Argument(season, nameof(season)).NotNull();
            Guard.Argument(culture, nameof(culture)).NotNull();

            Id = season.Id;
            Names[culture] = season.Name;
            StartDate = season.StartDate;
            EndDate = season.EndDate;
            Year = season.Year;
            TournamentId = season.TournamentId;
        }

        /// <summary>
        /// Merges the information from the provided <see cref="SeasonDTO"/> to the current instance
        /// </summary>
        /// <param name="season">A <see cref="SeasonCI"/> containing season info</param>
        public void Merge(SeasonCI season)
        {
            Guard.Argument(season, nameof(season)).NotNull();

            Id = season.Id;
            foreach (var cultureName in season.Names)
            {
                Names[cultureName.Key] = cultureName.Value;
            }
            StartDate = season.StartDate;
            EndDate = season.EndDate;
            Year = season.Year;
            TournamentId = season.TournamentId;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RoundCI"/> class
        /// </summary>
        /// <param name="exportable">The <see cref="ExportableSeasonCI"/> used to create new instance</param>
        internal SeasonCI(ExportableSeasonCI exportable)
        {
            if (exportable == null)
            {
                throw new ArgumentNullException(nameof(exportable));
            }

            Id = string.IsNullOrEmpty(exportable.SeasonId) ? null : URN.Parse(exportable.SeasonId);
            Names = new Dictionary<CultureInfo, string>(exportable.Names);
            StartDate = exportable.StartDate;
            EndDate = exportable.EndDate;
            Year = exportable.Year;
            TournamentId = string.IsNullOrEmpty(exportable.TournamentId) ? null : URN.Parse(exportable.TournamentId);
        }

        /// <summary>
        /// Asynchronous export item's properties
        /// </summary>
        /// <returns>An <see cref="ExportableSeasonCI"/> instance containing all relevant properties</returns>
        public Task<ExportableSeasonCI> ExportAsync()
        {
            return Task.FromResult(new ExportableSeasonCI
            {
                SeasonId = Id?.ToString(),
                Names = new Dictionary<CultureInfo, string>(Names),
                StartDate = StartDate,
                EndDate = EndDate,
                Year = Year,
                TournamentId = TournamentId?.ToString()
            });
        }
    }
}
