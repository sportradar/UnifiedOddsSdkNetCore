// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Dawn;
using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Entities.Rest.Caching.Exportable;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Dto;

namespace Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Caching.CI
{
    /// <summary>
    /// Provides information about referee (cache item)
    /// </summary>
    internal class RefereeCacheItem : SportEntityCacheItem
    {
        /// <summary>
        /// A <see cref="IDictionary{CultureInfo,String}"/> containing referee nationality in different languages
        /// </summary>
        private readonly IDictionary<CultureInfo, string> _nationality;

        /// <summary>
        /// Gets the name of the referee
        /// </summary>
        /// <value>The name.</value>
        public string Name { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="RefereeCacheItem"/> class.
        /// </summary>
        /// <param name="referee">A <see cref="RefereeDto"/> containing information about the referee</param>
        /// <param name="culture">A <see cref="CultureInfo"/> specifying the language of the referee info</param>
        internal RefereeCacheItem(RefereeDto referee, CultureInfo culture)
            : base(referee)
        {
            Guard.Argument(referee, nameof(referee)).NotNull();
            Guard.Argument(culture, nameof(culture)).NotNull();

            _nationality = new Dictionary<CultureInfo, string>();
            Merge(referee, culture);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RefereeCacheItem"/> class.
        /// </summary>
        /// <param name="exportable">A <see cref="ExportableReferee"/> containing information about the referee</param>
        internal RefereeCacheItem(ExportableReferee exportable)
            : base(Urn.Parse(exportable.Id))
        {
            _nationality = new Dictionary<CultureInfo, string>(exportable.Nationality);
            Name = exportable.Name;
        }

        /// <summary>
        /// Gets the nationality of the referee in the specified language
        /// </summary>
        /// <param name="culture">A <see cref="CultureInfo"/> specifying the language of the returned nationality.</param>
        /// <returns>The nationality of the referee in the specified language.</returns>
        internal string GetNationality(CultureInfo culture)
        {
            Guard.Argument(culture, nameof(culture)).NotNull();

            return _nationality.TryGetValue(culture, out var value)
                       ? value
                       : null;
        }

        /// <summary>
        /// Merges the provided information about the current referee
        /// </summary>
        /// <param name="referee">A <see cref="RefereeDto"/> containing referee info.</param>
        /// <param name="culture">A <see cref="CultureInfo"/> specifying the language of the referee info.</param>
        internal void Merge(RefereeDto referee, CultureInfo culture)
        {
            Guard.Argument(referee, nameof(referee)).NotNull();
            Guard.Argument(culture, nameof(culture)).NotNull();

            Name = referee.Name;
            _nationality[culture] = referee.Nationality;
        }

        /// <summary>
        /// Determines whether the current instance has translations for the specified languages
        /// </summary>
        /// <param name="cultures">A <see cref="IEnumerable{CultureInfo}" /> specifying the required languages</param>
        /// <returns>True if the current instance contains data in the required locals. Otherwise false.</returns>
        public bool HasTranslationsFor(IEnumerable<CultureInfo> cultures)
        {
            return cultures.All(c => _nationality.ContainsKey(c));
        }

        /// <summary>
        /// Asynchronous export item's properties
        /// </summary>
        /// <returns>An <see cref="ExportableBase"/> instance containing all relevant properties</returns>
        public Task<ExportableReferee> ExportAsync()
        {
            return Task.FromResult(new ExportableReferee
            {
                Id = Id.ToString(),
                Nationality = new Dictionary<CultureInfo, string>(_nationality ?? new Dictionary<CultureInfo, string>()),
                Name = Name
            });
        }
    }
}
