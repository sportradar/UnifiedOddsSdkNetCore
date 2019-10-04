/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System.Diagnostics.Contracts;
using Sportradar.OddsFeed.SDK.API.Contracts;
using Sportradar.OddsFeed.SDK.Entities.REST;

namespace Sportradar.OddsFeed.SDK.API
{
    /// <summary>
    /// Represent a root object of the unified odds feed
    /// </summary>
    /// <remarks>Interface will be merged into base <see cref="ITimelineEvent"/> in next major version scheduled for January 2019</remarks>
    [ContractClass(typeof(OddsFeedV1Contract))]
    public interface IOddsFeedV1 : IOddsFeed
    {
        /// <summary>
        /// Gets a <see cref="IBookmakerDetails"/> instance used to get info about bookmaker and token used
        /// </summary>
        IBookmakerDetails BookmakerDetails { get; }
    }
}
