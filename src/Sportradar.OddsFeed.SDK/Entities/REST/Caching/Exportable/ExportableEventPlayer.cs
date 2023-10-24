/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;

namespace Sportradar.OddsFeed.SDK.Entities.Rest.Caching.Exportable
{
    /// <summary>
    /// Class used to export/import event player cache item properties
    /// </summary>
    [Serializable]
    public class ExportableEventPlayer : ExportableBase
    {
        /// <summary>
        /// A <see cref="string"/> representing the bench value
        /// </summary>
        public string Bench { get; set; }

        /// <summary>
        /// A <see cref="string"/> representing the method value
        /// </summary>
        public string Method { get; set; }
    }
}
