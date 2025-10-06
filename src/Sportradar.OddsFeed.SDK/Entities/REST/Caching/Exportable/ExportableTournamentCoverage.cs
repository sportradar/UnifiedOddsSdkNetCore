// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;

namespace Sportradar.OddsFeed.SDK.Entities.Rest.Caching.Exportable
{
    /// <summary>
    /// Class used to export/import tournament coverage cache item properties
    /// </summary>
    [Serializable]
    public class ExportableTournamentCoverage
    {
        /// <summary>
        /// A <see cref="bool"/> indicating if the tournament has live coverage
        /// </summary>
        public bool LiveCoverage { get; set; }
    }
}
