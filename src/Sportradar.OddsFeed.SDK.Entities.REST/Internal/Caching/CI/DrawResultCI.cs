/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/

using System;
using System.Collections.Generic;
using Dawn;
using System.Globalization;
using System.Threading.Tasks;
using Sportradar.OddsFeed.SDK.Entities.REST.Caching.Exportable;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.DTO.Lottery;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching.CI
{
    /// <summary>
    /// Defines a cache item object for lottery draw result
    /// </summary>
    internal class DrawResultCI
    {
        /// <summary>
        /// Gets the value of the draw
        /// </summary>
        public int? Value { get; private set; }

        /// <summary>
        /// A <see cref="IDictionary{TKey,TValue}"/> containing result name in different languages
        /// </summary>
        public readonly IDictionary<CultureInfo, string> Names;

        /// <summary>
        /// Initializes a new instance of the <see cref="DrawInfoCI"/> class
        /// </summary>
        /// <param name="dto">A <see cref="DrawResultDTO"/> instance containing information about the draw result</param>
        /// <param name="culture">The culture of the <see cref="DrawResultDTO"/> used to create new instance</param>
        public DrawResultCI(DrawResultDTO dto, CultureInfo culture)
        {
            Guard.Argument(dto).NotNull();

            Names = new Dictionary<CultureInfo, string>();
            Merge(dto, culture);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DrawInfoCI"/> class
        /// </summary>
        /// <param name="exportable">A <see cref="ExportableDrawResultCI"/> instance containing information about the draw result</param>
        public DrawResultCI(ExportableDrawResultCI exportable)
        {
            if (exportable == null)
                throw new ArgumentNullException(nameof(exportable));

            Value = exportable.Value;
            Names = exportable.Names != null ? new Dictionary<CultureInfo, string>(exportable.Names) : null;
        }

        /// <summary>
        /// Merges the specified <see cref="DrawResultDTO"/> into instance
        /// </summary>
        /// <param name="dto">The <see cref="DrawResultDTO"/> used to merge into instance</param>
        /// <param name="culture">The culture of the <see cref="DrawResultDTO"/> used to merge</param>
        internal void Merge(DrawResultDTO dto, CultureInfo culture)
        {
            Guard.Argument(dto).NotNull();
            Guard.Argument(culture).NotNull();

            if (dto.Value.HasValue)
            {
                Value = dto.Value;
            }
            Names[culture] = dto.Name;
        }

        /// <summary>
        /// Gets the name of the player in the specified language
        /// </summary>
        /// <param name="culture">A <see cref="CultureInfo"/> specifying the language of the returned name</param>
        /// <returns>The name of the player in the specified language if it exists. Null otherwise.</returns>
        public string GetName(CultureInfo culture)
        {
            Guard.Argument(culture).NotNull();

            return Names.ContainsKey(culture)
                ? Names[culture]
                : null;
        }

        /// <summary>
        /// Asynchronous export item's properties
        /// </summary>
        /// <returns>An <see cref="ExportableCI"/> instance containing all relevant properties</returns>
        public Task<ExportableDrawResultCI> ExportAsync()
        {
            return Task.FromResult(new ExportableDrawResultCI
            {
                Value = Value,
                Names = new Dictionary<CultureInfo, string>(Names ?? new Dictionary<CultureInfo, string>())
            });
        }
    }
}
