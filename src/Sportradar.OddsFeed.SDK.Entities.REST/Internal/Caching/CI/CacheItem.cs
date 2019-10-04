/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Linq;
using Sportradar.OddsFeed.SDK.Entities.REST.Caching.Exportable;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.DTO;
using Sportradar.OddsFeed.SDK.Messages;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching.CI
{
    /// <summary>
    /// Base class for cached representation of the sport hierarchy entity (sport, category, tournament)
    /// </summary>
    public class CacheItem
    {
        /// <summary>
        /// Gets a <see cref="URN"/> representing id of the related entity
        /// </summary>
        public URN Id { get;}

        /// <summary>
        /// Gets a <see cref="IDictionary{CultureInfo, String}"/> containing translated name of the item
        /// </summary>
        public IDictionary<CultureInfo, string> Name { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="CacheItem"/> class.
        /// </summary>
        /// <param name="id">A <see cref="URN"/> representing the id of the item</param>
        /// <param name="name">The name of the item</param>
        /// <param name="culture">A <see cref="CultureInfo"/> specifying the language of the provided data</param>
        public CacheItem(URN id, string name, CultureInfo culture)
        {
            Contract.Requires(id != null);
            //Contract.Requires(!string.IsNullOrEmpty(name)); // there were tournaments with empty name!
            Contract.Requires(culture != null);

            Id = id;
            Name = new Dictionary<CultureInfo, string> {{culture, name}};
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CacheItem"/> class.
        /// </summary>
        /// <param name="exportable">A <see cref="ExportableCI"/> representing the cache item</param>
        public CacheItem(ExportableCI exportable)
        {
            if (exportable == null)
                throw new ArgumentNullException(nameof(exportable));

            Id = URN.Parse(exportable.Id);
            Name = new Dictionary<CultureInfo, string>(exportable.Name);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CacheItem"/> class.
        /// </summary>
        /// <param name="id">A <see cref="URN"/> representing the id of the item</param>
        /// <param name="name">The name of the item</param>
        protected CacheItem(URN id, IDictionary<CultureInfo, string> name)
        {
            if (id == null)
                throw new ArgumentNullException(nameof(id));
            if (name == null)
                throw new ArgumentNullException(nameof(name));
            Id = id;
            Name = name;
        }

        /// <summary>
        /// Defined field invariants needed by code contracts
        /// </summary>
        [ContractInvariantMethod]
        private void ObjectInvariant()
        {
            Contract.Invariant(Id != null);
            Contract.Invariant(Name != null);
            Contract.Invariant(Name.Any());
        }

        /// <summary>
        /// Merges the information from the provided <see cref="CacheItem"/> to data held by current instance
        /// </summary>
        /// <param name="item">A <see cref="CacheItem"/> containing the data to be merged to current instance</param>
        /// <param name="culture">A <see cref="CultureInfo"/> specifying the culture of data in the passed <see cref="CacheItem"/></param>
        public virtual void Merge(CacheItem item, CultureInfo culture)
        {
            Contract.Requires(culture != null);
            Contract.Requires(item != null);
            Contract.Requires(item.Name != null);
            Contract.Requires(item.Name.Any());

            if (item.Name.Count == 1) // must be only 1 name (received from mapper)
            {
                Name[culture] = item.Name.FirstOrDefault().Value;
                return;
            }

            foreach (var k in item.Name.Keys)
            {
                Name[k] = item.Name[k];
            }
        }

        /// <summary>
        /// Merges the specified <see cref="SportEntityDTO"/> into instance
        /// </summary>
        /// <param name="dto">The <see cref="SportEntityDTO"/> used for merge</param>
        /// <param name="culture">The culture of the input <see cref="SportEntityDTO"/></param>
        internal void Merge(SportEntityDTO dto, CultureInfo culture)
        {
            Contract.Requires(dto != null);
            Name[culture] = dto.Name;
        }

        /// <summary>
        /// Determines whether the current instance has translations for the specified languages
        /// </summary>
        /// <param name="cultures">A <see cref="IEnumerable{CultureInfo}"/> specifying the required languages</param>
        /// <returns>True if the current instance contains data in the required locals. Otherwise false.</returns>
        public virtual bool HasTranslationsFor(IEnumerable<CultureInfo> cultures)
        {
            return cultures.All(c => Name.ContainsKey(c));
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>A <see cref="System.String" /> that represents this instance.</returns>
        public override string ToString()
        {
            return $"Id={Id}";
        }
    }
}