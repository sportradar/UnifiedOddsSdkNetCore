/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using System.Collections.Generic;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Caching.Exportable
{
    /// <summary>
    /// Class used to export/import product info cache item properties
    /// </summary>
    [Serializable]
    public class ExportableProductInfoCI
    {
        /// <summary>
        /// A <see cref="bool"/> indicating if the product is auto traded
        /// </summary>
        public bool IsAutoTraded;

        /// <summary>
        /// A <see cref="bool"/> indicating if the product is in hosted statistics
        /// </summary>
        public bool IsInHostedStatistics;

        /// <summary>
        /// A <see cref="bool"/> indicating if the product is in live center soccer
        /// </summary>
        public bool IsInLiveCenterSoccer;

        /// <summary>
        /// The <see cref="bool"/> indicating if the product is in live match tracker
        /// </summary>
        public bool IsInLiveMatchTracker;

        /// <summary>
        /// A <see cref="bool"/> indicating if the product is in live score
        /// </summary>
        public bool IsInLiveScore;

        /// <summary>
        /// A <see cref="IEnumerable{T}"/> representation of the links
        /// </summary>
        public IEnumerable<ExportableProductInfoLinkCI> Links;

        /// <summary>
        /// A <see cref="IEnumerable{T}"/> representation of the channels
        /// </summary>
        public IEnumerable<ExportableStreamingChannelCI> Channels;
    }
}
