/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using Dawn;
using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Entities.REST;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching.Events;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.DTO;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.EntitiesImpl;

namespace Sportradar.OddsFeed.SDK.Tests.Common
{
    public class TestLocalizedNamedValueCache : ILocalizedNamedValueCache
    {
        private readonly IDictionary<int, IDictionary<CultureInfo, string>> _namedValues;
        private readonly IReadOnlyCollection<CultureInfo> _cultures;
        private readonly ICollection<CultureInfo> _loadedCultures;
        private readonly ExceptionHandlingStrategy _exceptionStrategy;

        public TestLocalizedNamedValueCache(IDictionary<int, IDictionary<CultureInfo, string>> namedValues, IReadOnlyCollection<CultureInfo> cultures, ExceptionHandlingStrategy exceptionStrategy)
        {
            _namedValues = namedValues;
            _cultures = cultures;
            _exceptionStrategy = exceptionStrategy;
            _loadedCultures = new List<CultureInfo>();
        }

        public Task<ILocalizedNamedValue> GetAsync(int id, IEnumerable<CultureInfo> cultures = null)
        {
            if (_namedValues.TryGetValue(id, out var values))
            {
                var wantedCultures = cultures ?? _cultures;
                var localizedNamedValue = new LocalizedNamedValue(id, values.Where(s => wantedCultures.Contains(s.Key)).ToDictionary(d => d.Key, d => d.Value), wantedCultures.First());
                return Task.FromResult<ILocalizedNamedValue>(localizedNamedValue);
            }
            return Task.FromResult<ILocalizedNamedValue>(null);
        }

        public bool IsValueDefined(int id)
        {
            return _namedValues.ContainsKey(id);
        }

        internal static ILocalizedNamedValueCache CreateMatchStatusCache(IReadOnlyCollection<CultureInfo> cultures, ExceptionHandlingStrategy exceptionStrategy)
        {
            IDictionary<int, IDictionary<CultureInfo, string>> namedValues = new Dictionary<int, IDictionary<CultureInfo, string>>();
            foreach (var culture in cultures)
            {
                var fileName = $"match_status_descriptions_{culture.TwoLetterISOLanguageName}.xml";
                var xmlElementName = "match_status";
                var records = GetFromFile(fileName, xmlElementName);
                foreach (var item in records.Items)
                {
                    if (namedValues.TryGetValue(item.Id, out var trans))
                    {
                        trans[culture] = item.Description;
                    }
                    else
                    {
                        trans = new Dictionary<CultureInfo, string> { { culture, item.Description } };
                        namedValues.Add(item.Id, trans);
                    }
                }
            }

            return new TestLocalizedNamedValueCache(namedValues, cultures, exceptionStrategy);
        }

        private static EntityList<NamedValueDTO> GetFromFile(string fileName, string xmlElementName)
        {
            var stream = FileHelper.GetResource(fileName);
            if (stream != null)
            {
                var document = new XmlDocument();
                document.Load(stream);

                if (document.DocumentElement != null)
                {
                    var nodes = document.DocumentElement.SelectNodes(xmlElementName);
                    var result = from XmlNode m in nodes
                                 where m.Attributes?["id"] != null && m.Attributes["description"] != null
                                 let id = int.Parse(m.Attributes["id"].Value)
                                 let desc = m.Attributes?["description"].Value
                                 select new NamedValueDTO(id, desc);
                    return new EntityList<NamedValueDTO>(result);
                }
            }

            return null;
        }
    }
}
