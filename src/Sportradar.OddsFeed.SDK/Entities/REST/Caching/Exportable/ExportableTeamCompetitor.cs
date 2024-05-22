// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;

namespace Sportradar.OddsFeed.SDK.Entities.Rest.Caching.Exportable
{
    /// <summary>
    /// Class used to export/import team competitor cache item properties
    /// </summary>
    [Serializable]
    public class ExportableTeamCompetitor : ExportableCompetitor
    {
        /// <summary>
        /// A <see cref="string"/> representing the qualifier additionally describing the competitor (e.g. home, away, ...)
        /// </summary>
        public string Qualifier { get; set; }
    }
}
