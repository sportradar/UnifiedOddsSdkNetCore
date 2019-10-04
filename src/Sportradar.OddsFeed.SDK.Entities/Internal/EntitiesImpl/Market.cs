/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sportradar.OddsFeed.SDK.Common.Internal;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.MarketNames;
using Sportradar.OddsFeed.SDK.Entities.REST.MarketMapping;

namespace Sportradar.OddsFeed.SDK.Entities.Internal.EntitiesImpl
{
    /// <summary>
    ///     Represents a market with only basic informations
    /// </summary>
    internal class Market : IMarket
    {
        private readonly IEnumerable<CultureInfo> _cultures;

        /// <summary>
        /// A <see cref="INameProvider"/> instance used to generate the market name(s)
        /// </summary>
        private readonly INameProvider _nameProvider;

        /// <summary>
        /// A <see cref="IMarketMappingProvider"/> instance used for providing mapped ids of markets and outcomes
        /// </summary>
        protected readonly IMarketMappingProvider MappingProvider;

        /// <summary>
        ///     Gets a <see cref="int" /> value specifying the market type
        /// </summary>
        public int Id { get; }

        /// <summary>
        /// Gets a <see cref="IReadOnlyDictionary{String, String}" /> containing market specifiers.
        /// </summary>
        /// <remarks>Note that the <see cref="IMarket.Id" /> and <see cref="IMarket.Specifiers" /> combined uniquely identify the market within the event</remarks>
        public IReadOnlyDictionary<string, string> Specifiers { get; }

        /// <summary>
        /// A <see cref="IDictionary{CultureInfo, String}"/> containing names in different languages
        /// </summary>
        private readonly IDictionary<CultureInfo, string> _names = new ConcurrentDictionary<CultureInfo, string>();

        /// <summary>
        /// Gets the <see cref="IReadOnlyDictionary{TKey,TValue}" /> containing additional market information
        /// </summary>
        public IReadOnlyDictionary<string, string> AdditionalInfo { get; }

        /// <summary>
        /// Gets the associated market definition instance
        /// </summary>
        public IMarketDefinition MarketDefinition { get; }

        private readonly object _lock = new object();

        /// <summary>
        ///     Initializes a new instance of the <see cref="Market" /> class
        /// </summary>
        /// <param name="id">a <see cref="int" /> value specifying the market type</param>
        /// <param name="specifiers">a <see cref="IReadOnlyDictionary{String, String}" /> containing market specifiers</param>
        /// <param name="additionalInfo">a <see cref="IReadOnlyDictionary{String, String}" /> containing additional market info</param>
        /// <param name="nameProvider">A <see cref="INameProvider"/> instance used to generate the market name(s) </param>
        /// <param name="mappingProvider">A <see cref="IMarketMappingProvider"/> instance used for providing mapping ids of markets and outcomes</param>
        /// <param name="marketDefinition">The associated market definition</param>
        /// <param name="cultures">A <see cref="IEnumerable{CultureInfo}"/> specifying languages the current instance supports</param>
        internal Market(int id,
                        IReadOnlyDictionary<string, string> specifiers,
                        IReadOnlyDictionary<string, string> additionalInfo,
                        INameProvider nameProvider,
                        IMarketMappingProvider mappingProvider,
                        IMarketDefinition marketDefinition,
                        IEnumerable<CultureInfo> cultures)
        {
            Contract.Requires(nameProvider != null);
            Contract.Requires(cultures != null);
            Contract.Requires(cultures.Any());

            Id = id;
            Specifiers = specifiers;
            AdditionalInfo = additionalInfo;
            _nameProvider = nameProvider;
            MappingProvider = mappingProvider;
            MarketDefinition = marketDefinition;
            _cultures = cultures;
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>A <see cref="System.String" /> that represents this instance</returns>
        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append("Id=").Append(Id);
            if (Specifiers != null)
            {
                sb.Append(", Specifiers=").Append(string.Join(SdkInfo.SpecifiersDelimiter, Specifiers.Select(kv => kv.Key + "=" + kv.Value)));
            }
            return sb.ToString();
        }

        /// <summary>
        /// Asynchronously gets the name of the market in the specified language
        /// </summary>
        /// <param name="culture">A <see cref="CultureInfo" /> specifying the language in which to get the name</param>
        /// <returns>A <see cref="Task{String}" /> representing the async operation</returns>
        public async Task<string> GetNameAsync(CultureInfo culture)
        {
            string name;
            lock (_lock)
            {
                if (_names.TryGetValue(culture, out name))
                {
                    return name;
                }
            }

            name = await _nameProvider.GetMarketNameAsync(culture).ConfigureAwait(false);

            lock (_lock)
            {
                if (_names.ContainsKey(culture))
                {
                    _names.Remove(culture);
                }
                _names.Add(culture, name);
            }

            return name;
        }

        /// <summary>
        /// Asynchronously gets the mapping Id of the specified market
        /// </summary>
        [Obsolete("Return only the first mapping. Use GetMappedMarketIdsAsync for all possible.")]
        public async Task<IMarketMapping> GetMappedMarketIdAsync()
        {
            var mappedIds = await MappingProvider.GetMappedMarketIdAsync(_cultures).ConfigureAwait(false);
            return mappedIds?.First();
        }

        /// <summary>
        /// Asynchronously gets the mapping Ids of the specified market
        /// </summary>
        public async Task<IEnumerable<IMarketMapping>> GetMappedMarketIdsAsync()
        {
            var mappedIds = await MappingProvider.GetMappedMarketIdAsync(_cultures).ConfigureAwait(false);
            return mappedIds;
        }
    }
}