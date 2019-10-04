/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/

namespace Sportradar.OddsFeed.SDK.Entities.REST.Caching.Exportable
{
    /// <summary>
    /// Class used to export/import event player assist cache item properties
    /// </summary>
    public class ExportableEventPlayerAssistCI : ExportableCI
    {
        /// <summary>
        /// A <see cref="string"/> representing the type
        /// </summary>
        public string Type { get; set; }
    }
}
