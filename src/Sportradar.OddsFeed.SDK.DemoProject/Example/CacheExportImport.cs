using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Sportradar.OddsFeed.SDK.Api;
using Sportradar.OddsFeed.SDK.Api.Config;
using Sportradar.OddsFeed.SDK.Common.Enums;
using Sportradar.OddsFeed.SDK.Common.Extensions;
using Sportradar.OddsFeed.SDK.Entities.Rest.Caching.Exportable;

namespace Sportradar.OddsFeed.SDK.DemoProject.Example;

/// <summary>
/// Basic cache export/import example
/// </summary>
public class CacheExportImport : ExampleBase
{
    private readonly UofClientAuthentication.IPrivateKeyJwtData _clientAuthentication;

    public CacheExportImport(ILogger<CacheExportImport> logger, UofClientAuthentication.IPrivateKeyJwtData clientAuthentication)
        : base(logger)
    {
        _clientAuthentication = clientAuthentication;
    }

    public override void Run(MessageInterest messageInterest)
    {
        Log.LogInformation("Running the Cache export/import example");

        Log.LogInformation("Retrieving configuration from application configuration file");
        var configuration = UofSdk.GetConfigurationBuilder().SetClientAuthentication(_clientAuthentication).BuildFromConfigFile();

        var uofSdk = RegisterServicesAndGetUofSdk(configuration);

        LimitRecoveryRequests(uofSdk);

        Log.LogInformation("Creating IUofSession");
        var session = uofSdk.GetSessionBuilder()
                            .SetMessageInterest(messageInterest)
                            .Build();

        AttachToGlobalEvents(uofSdk);
        AttachToSessionEvents(session);

        Log.LogInformation("Opening the sdk instance");
        uofSdk.Open();

        Log.LogInformation("Example successfully started. Waiting 60 seconds to populate");
        Task.Delay(TimeSpan.FromSeconds(60)).GetAwaiter().GetResult();

        ExportItems(uofSdk);

        ImportItems(uofSdk);

        Log.LogInformation("Closing / disposing the sdk instance");
        uofSdk.Close();

        DetachFromSessionEvents(session);
        DetachFromGlobalEvents(uofSdk);

        Log.LogInformation("Stopped");
    }

    private void ExportItems(IUofSdk uofSdk)
    {
        Log.LogInformation("Exporting cache items");
        var cacheItems = uofSdk.SportDataProvider.CacheExportAsync(CacheType.All).GetAwaiter().GetResult().ToList();
        File.WriteAllText("cache.json", JsonConvert.SerializeObject(cacheItems, new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All }));
        Log.LogInformation("Exporting {Size} cache items finished", cacheItems.Count);
    }

    private void ImportItems(IUofSdk uofSdk)
    {
        if (File.Exists("cache.json"))
        {
            Log.LogInformation("Importing cache items");
            var items = (IEnumerable<ExportableBase>)JsonConvert.DeserializeObject(File.ReadAllText("cache.json"), new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All });
            var exportableBases = items?.ToList();
            if (!exportableBases.IsNullOrEmpty())
            {
                uofSdk.SportDataProvider.CacheImportAsync(exportableBases).GetAwaiter().GetResult();
                Log.LogInformation("Importing {Size} cache items finished", exportableBases.Count());
            }
        }
    }
}
