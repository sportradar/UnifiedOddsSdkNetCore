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
    internal class MarketWithOdds : Market, IMarketWithOdds
    {
        /// <summary>
        /// Gets a <see cref="MarketStatus"/> enum member specifying the status of the market associated with the current <see cref="IMarketWithOdds"/> instance
        /// </summary>
        public MarketStatus Status { get; }

        /// <summary>
        /// Gets a <see cref="CashoutStatus" /> enum member specifying the availability of cashout, or a null reference
        /// </summary>
        /// <value>The cashout status.</value>
        public CashoutStatus? CashoutStatus { get; }

        /// <summary>
        /// Gets a <see cref="IEnumerable{IOutcomeOdds}"/> where each <see cref="IOutcomeOdds"/> instance specifies the odds
        /// for one outcome associated with the current <see cref="IMarketWithOdds"/> instance
        /// </summary>
        public IEnumerable<IOutcomeOdds> OutcomeOdds { get; }

        /// <summary>
        /// Gets the market metadata which contains the additional market information
        /// </summary>
        /// <value>The market metadata which contains the additional market information</value>
        public IMarketMetadata MarketMetadata { get; }

        /// <summary>
        /// Gets a value indicating whether the market associated with the current instance is the favorite market (i.e. the one with most balanced odds)
        /// </summary>
        public bool IsFavorite { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="MarketWithOdds"/> class
        /// </summary>
        /// <param name="id">a <see cref="int"/> value specifying the market type</param>
        /// <param name="specifiers">a <see cref="IReadOnlyDictionary{String, String}"/> containing market specifiers.</param>
        /// <param name="additionalInfo">a <see cref="IReadOnlyDictionary{String, String}"/> containing additional market info.</param>
        /// <param name="nameProvider">A <see cref="INameProvider"/> instance used to generate the market name(s) </param>
        /// <param name="mappingProvider">A <see cref="IMarketMappingProvider"/> instance used for providing mapped ids of markets and outcomes</param>
        /// <param name="status">a <see cref="MarketStatus"/> enum member specifying the status of the market associated with the current <see cref="IMarketWithOdds"/> instance</param>
        /// <param name="cashoutStatus">A <see cref="CashoutStatus"/> to be set</param>
        /// <param name="isFavorite">Gets a value indicating whether the market associated with the current <see cref="IMarketWithOdds"/> instance is the most
        /// balanced market.</param>
        /// <param name="outcomeOdds">a <see cref="IEnumerable{IOutcomeOdds}"/> where each <see cref="IOutcomeOdds"/> instance specifies the odds
        /// for one outcome associated with the current <see cref="IMarketWithOdds"/> instance</param>
        /// <param name="marketMetadata">A <see cref="IMarketMetadata"/> to be set</param>
        /// <param name="marketDefinition">The associated market definition</param>
        /// <param name="cultures">A <see cref="IEnumerable{CultureInfo}"/> specifying languages the current instance supports</param>
        internal MarketWithOdds(int id,
                                IReadOnlyDictionary<string, string> specifiers,
                                IReadOnlyDictionary<string, string> additionalInfo,
                                INameProvider nameProvider,
                                IMarketMappingProvider mappingProvider,
                                MarketStatus status,
                                CashoutStatus? cashoutStatus,
                                bool isFavorite,
                                IEnumerable<IOutcomeOdds> outcomeOdds,
                                IMarketMetadata marketMetadata,
                                IMarketDefinition marketDefinition,
                                IEnumerable<CultureInfo> cultures)
            : base(id, specifiers, additionalInfo, nameProvider, mappingProvider, marketDefinition, cultures)
        {
            Status = status;
            CashoutStatus = cashoutStatus;

            if (outcomeOdds != null)
            {
                OutcomeOdds = outcomeOdds as ReadOnlyCollection<IOutcomeOdds> ?? new ReadOnlyCollection<IOutcomeOdds>(outcomeOdds.ToList());
            }
            IsFavorite = isFavorite;

            MarketMetadata = marketMetadata;
        }
    }
}