/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/

using System;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Caching.Exportable
{
    /// <summary>
    /// Class used to export/import car item properties
    /// </summary>
    [Serializable]
    public class ExportableCarCI
    {
        /// <summary>
        /// A <see cref="string"/> representing the name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// A <see cref="string"/> representing the chassis
        /// </summary>
        public string Chassis { get; set; }

        /// <summary>
        /// A <see cref="string"/> representing the engine name
        /// </summary>
        public string EngineName { get; set; }
    }
}