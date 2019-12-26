/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.MarketNames;

namespace Sportradar.OddsFeed.SDK.Entities.Internal.EntitiesImpl
{
    internal class MarketWithProbabilities : Market, IMarketWithProbabilities
    {
        /// <summary>
        /// Gets a <see cref="MarketStatus" /> enum member specifying the status of the market associated with the current market
        /// </summary>
        public MarketStatus Status { get; }

        /// <summary>
        /// Gets an <see cref="IEnumerable{IOutcomeProbabilities}" /> where each <see cref="IOutcomeProbabilities" /> instance provides
        /// probabilities information for one outcome(selection)
        /// </summary>
        public IEnumerable<IOutcomeProbabilities> OutcomeProbabilities { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="MarketWithProbabilities"/> class
        /// </summary>
        /// <param name="id">a <see cref="int"/> value specifying the market type</param>
        /// <param name="specifiers">a <see cref="IReadOnlyDictionary{String, String}"/> containing market specifiers.</param>
        /// <param name="additionalInfo">a <see cref="IReadOnlyDictionary{String, String}"/> containing additional market info.</param>
        /// <param name="nameProvider">A <see cref="INameProvider"/> instance used to generate the market name(s) </param>
        /// <param name="mappingProvider">A <see cref="IMarketMappingProvider"/> instance used for providing mapped ids of markets and outcomes</param>
        /// <param name="status">a <see cref="MarketStatus"/> enum member specifying the status of the market associated with the current <see cref="IMarketWithProbabilities"/> instance</param>
        /// <param name="outcomeProbabilities">a <see cref="IEnumerable{IOutcomeProbabilities}"/> where each <see cref="IOutcomeProbabilities"/> instance specifies the odds
        ///     for one outcome associated with the current <see cref="IMarketWithProbabilities"/> instance</param>
        /// <param name="marketDefinition">The associated market definition</param>
        /// <param name="cultures">A <see cref="IEnumerable{CultureInfo}"/> specifying languages the current instance supports</param>
        internal MarketWithProbabilities(int id,
                                         IReadOnlyDictionary<string, string> specifiers,
                                         IReadOnlyDictionary<string, string> additionalInfo,
                                         INameProvider nameProvider,
                                         IMarketMappingProvider mappingProvider,
                                         MarketStatus status,
                                         IEnumerable<IOutcomeProbabilities> outcomeProbabilities,
                                         IMarketDefinition marketDefinition,
                                         IEnumerable<CultureInfo> cultures)
            : base(id, specifiers, additionalInfo, nameProvider, mappingProvider, marketDefinition, cultures)
        {
            Status = status;

            if (outcomeProbabilities != null)
            {
                OutcomeProbabilities = outcomeProbabilities as ReadOnlyCollection<IOutcomeProbabilities> ?? new ReadOnlyCollection<IOutcomeProbabilities>(outcomeProbabilities.ToList());
            }
        }
    }
}