/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Threading.Tasks;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching.Events;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.Enums;
using Sportradar.OddsFeed.SDK.Messages;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Contracts
{
    [ContractClassFor(typeof(ISportEventStatusCache))]
    abstract class SportEventStatusCacheContract : ISportEventStatusCache
    {
        public Task<SportEventStatusCI> GetSportEventStatusAsync(URN eventId)
        {
            Contract.Requires(eventId != null);
            Contract.Ensures(Contract.Result<Task<SportEventStatusCI>>() != null);
            return Contract.Result<Task<SportEventStatusCI>>();
        }

        public void AddSportEventStatus(URN eventId, SportEventStatusCI sportEventStatus)
        {
            Contract.Requires(eventId != null);
            Contract.Requires(sportEventStatus != null);
        }

        public void RemoveSportEventStatus(URN eventId)
        {
            Contract.Requires(eventId != null);
        }

        public void RemoveSportEventFromCache(URN eventId)
        {
            Contract.Requires(eventId != null);
        }

        public IEnumerable<DtoType> RegisteredDtoTypes => Contract.Result<IEnumerable<DtoType>>();

        public string CacheName => Contract.Result<string>();

        /// <summary>
        /// Registers the cache in the <see cref="CacheManager"/>
        /// </summary>
        public void RegisterCache()
        {
        }

        /// <summary>
        /// Set the list of <see cref="DtoType"/> in the this cache
        /// </summary>
        public void SetDtoTypes()
        {
        }

        public Task<bool> CacheAddDtoAsync(URN id, object item, CultureInfo culture, DtoType dtoType, ISportEventCI requester)
        {
            Contract.Requires(id != null);
            Contract.Requires(item != null);
            return Contract.Result<Task<bool>>();
        }

        public void CacheDeleteItem(URN id, CacheItemType cacheItemType)
        {
            Contract.Requires(id != null);
        }

        public bool CacheHasItem(URN id, CacheItemType cacheItemType)
        {
            Contract.Requires(id != null);
            return Contract.Result<bool>();
        }

        public void CacheDeleteItem(string id, CacheItemType cacheItemType)
        {
            Contract.Requires(!string.IsNullOrEmpty(id));
        }

        public bool CacheHasItem(string id, CacheItemType cacheItemType)
        {
            Contract.Requires(!string.IsNullOrEmpty(id));
            return Contract.Result<bool>();
        }
    }
}
