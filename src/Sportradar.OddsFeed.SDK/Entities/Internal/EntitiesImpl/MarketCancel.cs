/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System.Collections.Generic;
using Dawn;
using System.Globalization;
using Sportradar.OddsFeed.SDK.Entities.REST;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.MarketNames;

namespace Sportradar.OddsFeed.SDK.Entities.Internal.EntitiesImpl
{
    /// <summary>
    /// Represents a result of the betting market with void reason
    /// </summary>
    internal class MarketCancel : Market, IMarketCancel
    {
        /// <summary>
        /// A <see cref="INamedValueCache"/> used to get void reason descriptions
        /// </summary>
        private readonly INamedValueCache _voidReasonsCache;

        /// <summary>
        /// A value specifying the void reason or a null reference
        /// </summary>
        private readonly int? _voidReason;

        /// <summary>
        /// Gets a <see cref="INamedValue" /> specifying the void reason, or a null reference if no void reason is specified
        /// </summary>
        public INamedValue VoidReason => _voidReason == null ? null : _voidReasonsCache.GetNamedValue(_voidReason.Value);

        /// <summary>
        /// Initializes a new instance of the <see cref="MarketWithSettlement" /> class
        /// </summary>
        /// <param name="id">a <see cref="int" /> value specifying the market type</param>
        /// <param name="specifiers">a <see cref="IReadOnlyDictionary{String, String}" /> containing additional market specifiers</param>
        /// <param name="additionalInfo">a <see cref="IReadOnlyDictionary{String, String}"/> containing additional market info</param>
        /// <param name="nameProvider">A <see cref="INameProvider"/> instance used to generate the market name(s)</param>
        /// <param name="mappingProvider">A <see cref="IMarketMappingProvider"/> instance used for providing mapped ids of markets and outcomes</param>
        /// <param name="marketDefinition">The associated market definition</param>
        /// <param name="voidReason">A value specifying the void reason or a null reference</param>
        /// <param name="voidReasonsCache">A <see cref="INamedValueCache"/> used to get void reason descriptions</param>
        /// <param name="cultures">A <see cref="IEnumerable{CultureInfo}"/> specifying languages the current instance supports</param>
        internal MarketCancel(int id,
                            IReadOnlyDictionary<string, string> specifiers,
                            IReadOnlyDictionary<string, string> additionalInfo,
                            INameProvider nameProvider,
                            IMarketMappingProvider mappingProvider,
                            IMarketDefinition marketDefinition,
                            int? voidReason,
                            INamedValueCache voidReasonsCache,
                            IEnumerable<CultureInfo> cultures)
            : base(id, specifiers, additionalInfo, nameProvider, mappingProvider, marketDefinition, cultures)
        {
            Guard.Argument(voidReasonsCache, nameof(voidReasonsCache)).NotNull();

            _voidReason = voidReason;
            _voidReasonsCache = voidReasonsCache;
        }
    }
}