/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/

using System;
using System.Collections.Generic;
using Dawn;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Sportradar.OddsFeed.SDK.Entities.REST.Caching.Exportable;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.DTO;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching.CI
{
    /// <summary>
    /// Defines a cache item for fixture delayed info
    /// </summary>
    public class DelayedInfoCI
    {
        /// <summary>
        /// Gets the identifier
        /// </summary>
        public int Id { get; private set; }

        /// <summary>
        /// A <see cref="IDictionary{CultureInfo,String}"/> containing descriptions in different languages
        /// </summary>
        public readonly IDictionary<CultureInfo, string> Descriptions;

        /// <summary>
        /// Initializes a new instance of the <see cref="DelayedInfoCI"/> class
        /// </summary>
        /// <param name="dto">The <see cref="DelayedInfoCI"/> used to create new instance</param>
        /// <param name="culture">The culture of the input <see cref="RoundDTO"/></param>
        internal DelayedInfoCI(DelayedInfoDTO dto, CultureInfo culture)
        {
            Guard.Argument(dto, nameof(dto)).NotNull();
            Guard.Argument(culture, nameof(culture)).NotNull();

            Descriptions = new Dictionary<CultureInfo, string>();
            Merge(dto, culture);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DelayedInfoCI"/> class
        /// </summary>
        /// <param name="exportable">The <see cref="ExportableDelayedInfoCI"/> used to create new instance</param>
        internal DelayedInfoCI(ExportableDelayedInfoCI exportable)
        {
            if (exportable == null)
                throw new ArgumentNullException(nameof(exportable));

            Id = exportable.Id;
            Descriptions = new Dictionary<CultureInfo, string>(exportable.Descriptions ?? new Dictionary<CultureInfo, string>());
        }

        /// <summary>
        /// Merges the specified <see cref="DelayedInfoCI"/> into instance
        /// </summary>
        /// <param name="dto">The <see cref="DelayedInfoCI"/> used fro merging</param>
        /// <param name="culture">The culture of the input <see cref="DelayedInfoCI"/></param>
        internal void Merge(DelayedInfoDTO dto, CultureInfo culture)
        {
            Guard.Argument(dto, nameof(dto)).NotNull();
            Id = dto.Id;
            Descriptions[culture] = dto.Description;
        }

        /// <summary>
        /// Gets the name for specific locale
        /// </summary>
        /// <param name="culture">The culture</param>
        /// <returns>Return the Name if exists, or null</returns>
        public string GetDescription(CultureInfo culture)
        {
            Guard.Argument(culture, nameof(culture)).NotNull();

            return Descriptions == null || !Descriptions.ContainsKey(culture)
                ? null
                : Descriptions[culture];
        }

        /// <summary>
        /// Determines whether the current instance has translations for the specified languages
        /// </summary>
        /// <param name="cultures">A <see cref="IEnumerable{CultureInfo}"/> specifying the required languages</param>
        /// <returns>True if the current instance contains data in the required locals. Otherwise false</returns>
        public virtual bool HasTranslationsFor(IEnumerable<CultureInfo> cultures)
        {
            return cultures.All(c => Descriptions.ContainsKey(c));
        }

        /// <summary>
        /// Asynchronous export item's properties
        /// </summary>
        /// <returns>An <see cref="ExportableCI"/> instance containing all relevant properties</returns>
        public Task<ExportableDelayedInfoCI> ExportAsync()
        {
            return Task.FromResult(new ExportableDelayedInfoCI
            {
                Id = Id,
                Descriptions = new Dictionary<CultureInfo, string>(Descriptions ?? new Dictionary<CultureInfo, string>())
            });
        }
    }
}
