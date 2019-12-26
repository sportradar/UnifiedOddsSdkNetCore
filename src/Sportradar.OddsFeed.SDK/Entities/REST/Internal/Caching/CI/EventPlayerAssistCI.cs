/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/

using System.Collections.Generic;
using Dawn;
using System.Globalization;
using System.Threading.Tasks;
using Sportradar.OddsFeed.SDK.Entities.REST.Caching.Exportable;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.DTO;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching.CI
{
    /// <summary>
    /// A cache item for event player assist
    /// </summary>
    /// <seealso cref="CacheItem" />
    public class EventPlayerAssistCI : CacheItem
    {
        public string Type { get; }

        public EventPlayerAssistCI(EventPlayerAssistDTO dto, CultureInfo culture)
            : base(dto.Id, dto.Name, culture)
        {
            Guard.Argument(dto, nameof(dto)).NotNull();

            Type = dto.Type;
        }

        public EventPlayerAssistCI(ExportableEventPlayerAssistCI exportable)
            : base(exportable)
        {
            Type = exportable.Type;
        }

        /// <summary>
        /// Asynchronous export item's properties
        /// </summary>
        /// <returns>An <see cref="ExportableCI"/> instance containing all relevant properties</returns>
        public Task<ExportableEventPlayerAssistCI> ExportAsync()
        {
            return Task.FromResult(new ExportableEventPlayerAssistCI
            {
                Id = Id.ToString(),
                Name = new Dictionary<CultureInfo, string>(Name ?? new Dictionary<CultureInfo, string>()),
                Type = Type
            });
        }
    }
}
