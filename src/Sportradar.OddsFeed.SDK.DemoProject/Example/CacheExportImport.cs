using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Newtonsoft.Json;
using Sportradar.OddsFeed.SDK.API;
using Sportradar.OddsFeed.SDK.Entities;
using Sportradar.OddsFeed.SDK.Entities.REST.Caching.Exportable;

namespace Sportradar.OddsFeed.SDK.DemoProject.Example
{
    /// <summary>
    /// Basic cache export/import example
    /// </summary>
    public class CacheExportImport
    {
        private readonly ILogger _log;
        private readonly ILoggerFactory _loggerFactory;

        public CacheExportImport(ILoggerFactory loggerFactory = null)
        {
            _loggerFactory = loggerFactory;
            _log = _loggerFactory?.CreateLogger(typeof(CacheExportImport)) ?? new NullLogger<CacheExportImport>();
        }

        public void Run(MessageInterest messageInterest)
        {
            _log.LogInformation("Running the OddsFeed SDK Export/import example");

            _log.LogInformation("Retrieving configuration from application configuration file");
            var configuration = Feed.GetConfigurationBuilder().BuildFromConfigFile();
            //you can also create the IOddsFeedConfiguration instance by providing required values
            //var configuration = Feed.CreateConfiguration("myAccessToken", new[] {"en"});

            _log.LogInformation("Creating Feed instance");
            var oddsFeed = new Feed(configuration, _loggerFactory);
            
            if (File.Exists("cache.json"))
            {
                _log.LogInformation("Importing cache items");
                var items = JsonConvert.DeserializeObject(File.ReadAllText("cache.json"), new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All });
                oddsFeed.SportDataProvider.CacheImportAsync(items as IEnumerable<ExportableCI>).Wait();
            }

            var sports = oddsFeed.SportDataProvider.GetSportsAsync().Result;
            _log.LogInformation("Exporting cache items");
            var cacheItems = oddsFeed.SportDataProvider.CacheExportAsync(CacheType.All).Result.ToList();
            File.WriteAllText("cache.json", JsonConvert.SerializeObject(cacheItems, new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All }));
        }
    }
}
