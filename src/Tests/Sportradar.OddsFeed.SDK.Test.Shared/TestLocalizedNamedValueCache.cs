/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Sportradar.OddsFeed.SDK.Entities.REST;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching.Events;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.EntitiesImpl;

namespace Sportradar.OddsFeed.SDK.Test.Shared
{
    public class TestLocalizedNamedValueCache : ILocalizedNamedValueCache
    {
        public Task<ILocalizedNamedValue> GetAsync(int matchStatus, IEnumerable<CultureInfo> cultures)
        {
            var translations = new Dictionary<CultureInfo, string>();
            var dic = translations.Where(s => cultures.Contains(s.Key)).ToDictionary(t => t.Key, t => t.Value);
            var ms = new LocalizedNamedValue(1, dic, cultures.ToList().First());
            return Task.FromResult<ILocalizedNamedValue>(null);
        }

        public bool IsValueDefined(int id)
        {
            throw new System.NotImplementedException();
        }
    }
}
