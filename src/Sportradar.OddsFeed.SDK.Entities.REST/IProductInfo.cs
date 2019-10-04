/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System.Collections.Generic;

namespace Sportradar.OddsFeed.SDK.Entities.REST
{
    /// <summary>
    /// Defines a contract implemented by classes providing product information
    /// </summary>
    public interface IProductInfo : IEntityPrinter
    {
        /// <summary>
        /// TODO: Add comments
        /// </summary>
        bool IsAutoTraded { get; }

        /// <summary>
        /// Gets a value indicating whether the sport event associated with the current instance is available in hosted solutions
        /// </summary>
        bool IsInHostedStatistics { get; }

        /// <summary>
        /// Gets a value indicating whether the sport event associated with the current instance is available in LiveCenterSoccer solution
        /// </summary>
        bool IsInLiveCenterSoccer { get; }

        /// <summary>
        /// Gets a value indicating whether the sport event associated with the current instance is available in LiveScore solution
        /// </summary>
        bool IsInLiveScore { get; }

        /// <summary>
        /// Gets a <see cref="IEnumerable{IProductInfoLink}"/> representing links to the product represented by current instance
        /// </summary>
        IEnumerable<IProductInfoLink> Links { get; }

        /// <summary>
        /// Gets a <see cref="IEnumerable{IStreamingChannel}"/> representing streaming channel associated with product
        /// </summary>
        IEnumerable<IStreamingChannel> Channels { get; }
    }
}
