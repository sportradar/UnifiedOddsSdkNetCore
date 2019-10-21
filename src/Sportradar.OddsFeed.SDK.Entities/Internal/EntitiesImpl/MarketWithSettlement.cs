/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Dawn;
using System.Globalization;
using System.Linq;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.MarketNames;

namespace Sportradar.OddsFeed.SDK.Entities.Internal.EntitiesImpl
{
    /// <summary>
    ///     Represents a result of the betting market
    /// </summary>
    internal class MarketWithSettlement : MarketCancel, IMarketWithSettlement
    {
        /// <summary>
        /// Gets an <see cref="IEnumerable{IOutcomeSettlement}" /> where each <see cref="IOutcomeSettlement" /> instance providing outcome settling information
        /// </summary>
        public IEnumerable<IOutcomeSettlement> OutcomeSettlements { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="MarketWithSettlement" /> class
        /// </summary>
        /// <param name="id">a <see cref="int" /> value specifying the market type</param>
        /// <param name="specifiers">a <see cref="IReadOnlyDictionary{String, String}" /> containing additional market specifiers</param>
        /// <param name="additionalInfo">a <see cref="IReadOnlyDictionary{String, String}"/> containing additional market info</param>
        /// <param name="outcomes">An <see cref="IEnumerable{IOutcomeSettlement}" /> where each <see cref="IOutcomeSettlement" /> instance providing outcome settling information
        /// </param>
        /// <param name="nameProvider">A <see cref="INameProvider"/> instance used to generate the market name(s)</param>
        /// <param name="mappingProvider">A <see cref="IMarketMappingProvider"/> instance used for providing mapped ids of markets and outcomes</param>
        /// <param name="marketDefinition">The associated market definition</param>
        /// <param name="voidReason">A value specifying the void reason or a null reference</param>
        /// <param name="voidReasonsCache">A <see cref="INamedValueCache"/> used to get void reason descriptions</param>
        /// <param name="cultures">A <see cref="IEnumerable{CultureInfo}"/> specifying languages the current instance supports</param>
        internal MarketWithSettlement(int id,
                                    IReadOnlyDictionary<string, string> specifiers,
                                    IReadOnlyDictionary<string, string> additionalInfo,
                                    IEnumerable<IOutcomeSettlement> outcomes,
                                    INameProvider nameProvider,
                                    IMarketMappingProvider mappingProvider,
                                    IMarketDefinition marketDefinition,
                                    int? voidReason,
                                    INamedValueCache voidReasonsCache,
                                    IEnumerable<CultureInfo> cultures)
            : base(id, specifiers, additionalInfo, nameProvider, mappingProvider, marketDefinition, voidReason, voidReasonsCache, cultures)
        {
            Guard.Argument(outcomes != null && outcomes.Any());

            var readonlyOutcomes = outcomes as IReadOnlyCollection<IOutcomeSettlement>;
            OutcomeSettlements = readonlyOutcomes ?? new ReadOnlyCollection<IOutcomeSettlement>(outcomes.ToList());
        }
    }
}