/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.MarketNames;
using Sportradar.OddsFeed.SDK.Entities.REST.MarketMapping;

namespace Sportradar.OddsFeed.SDK.Entities.Internal.EntitiesImpl
{
    /// <summary>
    /// Represents a betting market outcome (selection)
    /// </summary>
    internal abstract class Outcome : IOutcomeV1
    {
        private readonly IEnumerable<CultureInfo> _cultures;

        /// <summary>
        /// A <see cref="INameProvider"/> used to generate the outcome name(s)
        /// </summary>
        private readonly INameProvider _nameProvider;

        /// <summary>
        /// A <see cref="IMarketMappingProvider"/> instance used for providing mapped ids of markets and outcomes
        /// </summary>
        private readonly IMarketMappingProvider _mappingProvider;

        /// <summary>
        /// A <see cref="IDictionary{TKey,TValue}"/> containing names in different languages
        /// </summary>
        private readonly IDictionary<CultureInfo, string> _names = new ConcurrentDictionary<CultureInfo, string>();

        private IOutcomeDefinition _outcomeDefinition;

        /// <summary>
        /// Gets the value uniquely identifying the current <see cref="Outcome" /> instance
        /// </summary>
        public string Id { get; }

        /// <summary>
        /// Gets the associated outcome definition instance
        /// </summary>
        public IOutcomeDefinition OutcomeDefinition { get; }

        private readonly object _lock = new object();

        /// <summary>
        ///     Initializes a new instance of the <see cref="Outcome" /> class
        /// </summary>
        /// <param name="id">the value uniquely identifying the current <see cref="Outcome" /> instance</param>
        /// <param name="nameProvider">A <see cref="INameProvider"/> used to generate the outcome name(s)</param>
        /// <param name="mappingProvider">A <see cref="IMarketMappingProvider"/> instance used for providing mapping ids of markets and outcomes</param>
        /// <param name="cultures">A <see cref="IEnumerable{CultureInfo}"/> specifying languages the current instance supports</param>
        /// <param name="outcomeDefinition"></param>
        protected Outcome(string id, INameProvider nameProvider, IMarketMappingProvider mappingProvider, IEnumerable<CultureInfo> cultures, IOutcomeDefinition outcomeDefinition)
        {
            Contract.Requires(nameProvider != null);
            Contract.Requires(cultures != null);
            Contract.Requires(cultures.Any());

            Id = id;
            _nameProvider = nameProvider;
            _mappingProvider = mappingProvider;
            _cultures = cultures;
            OutcomeDefinition = outcomeDefinition;
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

            name = await _nameProvider.GetOutcomeNameAsync(Id, culture).ConfigureAwait(false);

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
        /// Asynchronously gets the mapped Id of the specified outcome
        /// </summary>
        [Obsolete("Return only the first mapping. Use GetMappedOutcomeIdsAsync for all possible.")]
        public virtual async Task<IOutcomeMapping> GetMappedOutcomeIdAsync()
        {
            var mappedId = await _mappingProvider.GetMappedOutcomeIdAsync(Id, _cultures).ConfigureAwait(false);
            return mappedId?.First();
        }

        /// <summary>
        /// Asynchronously gets the mapping Ids of the specified outcome
        /// </summary>
        public async Task<IEnumerable<IOutcomeMapping>> GetMappedOutcomeIdsAsync()
        {
            var mappedIds = await _mappingProvider.GetMappedOutcomeIdAsync(Id, _cultures).ConfigureAwait(false);
            return mappedIds;
        }

        //TODO: remove this
        //public async Task<IOutcomeDefinition> GetOutcomeDefinitionAsync()
        //{
        //    if (_outcomeDefinition != null)
        //    {
        //        return _outcomeDefinition;
        //    }

        //    _outcomeDefinition = await _nameProvider.GetOutcomeNameAsync(Id, _cultures).ConfigureAwait(false);

        //    return _outcomeDefinition;
        //}
    }
}