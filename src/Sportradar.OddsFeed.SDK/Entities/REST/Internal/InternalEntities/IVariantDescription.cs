// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System.Collections.Generic;
using Sportradar.OddsFeed.SDK.Entities.Rest.Market;
using Sportradar.OddsFeed.SDK.Entities.Rest.MarketMapping;

namespace Sportradar.OddsFeed.SDK.Entities.Rest.Internal.InternalEntities
{
    /// <summary>
    /// Defines a contract implemented by classes representing variant description
    /// </summary>
    internal interface IVariantDescription
    {
        /// <summary>
        /// Gets the id of the market described by the current instance
        /// </summary>
        string Id { get; }

        /// <summary>
        /// Gets the <see cref="IEnumerable{IOutcomeDescription}"/> describing the outcomes of the market
        /// described by the current instance
        /// </summary>
        IEnumerable<IOutcomeDescription> Outcomes { get; }

        /// <summary>
        /// Gets the <see cref="IEnumerable{IMarketMapping}"/> representing the mappings of the market
        /// described by the current instance
        /// </summary>
        IEnumerable<IMarketMappingData> Mappings { get; }
    }
}
