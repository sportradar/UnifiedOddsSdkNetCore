/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Sportradar.OddsFeed.SDK.Api;
using Sportradar.OddsFeed.SDK.Api.Config;
using Sportradar.OddsFeed.SDK.Api.Replay;
using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Common.Extensions;
using Sportradar.OddsFeed.SDK.DemoProject.Utils;

namespace Sportradar.OddsFeed.SDK.DemoProject.Example;

/// <summary>
/// Example displaying interaction with Replay Server with single session, generic dispatcher, basic output
/// </summary>
public class ReplayServer : ExampleBase
{
    public ReplayServer(ILogger<ReplayServer> logger)
        : base(logger)
    {
    }

    public override void Run(MessageInterest messageInterest)
    {
        Log.LogInformation("Running the Replay Server example");

        Log.LogInformation("Retrieving configuration from application configuration file");
        var configuration = UofSdk.GetConfigurationBuilder().BuildFromConfigFile();
        var host = Host.CreateDefaultBuilder()
                       .ConfigureLogging((context, logging) =>
                                         {
                                             logging.ClearProviders();
                                             logging.AddLog4Net("log4net.config");
                                         })
                       .ConfigureServices(configure =>
                                          {
                                              configure.AddUofSdk(configuration);
                                          })
                       .Build();

        Log.LogInformation("Creating UofSdk instance");
        var uofSdkForReplay = new UofSdkForReplay(host.Services);

        Log.LogInformation("Creating IUofSession");
        var session = uofSdkForReplay.GetSessionBuilder()
                                     .SetMessageInterest(messageInterest)
                                     .Build();

        AttachToGlobalEvents(uofSdkForReplay);
        AttachToSessionEvents(session);

        Log.LogInformation("Opening the sdk instance");
        uofSdkForReplay.Open();

        ReplayServerInteraction(uofSdkForReplay);

        Log.LogInformation("Example successfully started. Hit <enter> to quit");
        Console.WriteLine(string.Empty);
        Console.ReadLine();

        Log.LogInformation("Stopping replay");
        uofSdkForReplay.ReplayManager.StopReplay();

        Log.LogInformation("Closing / disposing the sdk instance");
        uofSdkForReplay.Close();

        DetachFromGlobalEvents(uofSdkForReplay);
        DetachFromSessionEvents(session);

        Log.LogInformation("Stopped");
    }

    private void ReplayServerInteraction(IUofSdkForReplay uofSdkForReplay)
    {
        WriteReplayResponse(uofSdkForReplay.ReplayManager.StopAndClearReplay());
        var replayStatus = uofSdkForReplay.ReplayManager.GetStatusOfReplay();
        WriteReplayStatus(replayStatus);
        WriteReplayQueueSize(uofSdkForReplay);

        // There are two options for replaying matches: play specific scenario with predefined matches or add specific matches to be replayed (uncomment selected option).

        // Option 1:
        PlayScenario(uofSdkForReplay);

        // Option 2:
        //PlayMatches(uofSdkForReplay);

        replayStatus = uofSdkForReplay.ReplayManager.GetStatusOfReplay();
        WriteReplayStatus(replayStatus);
    }

    private void PlayScenario(IUofSdkForReplay uofSdkForReplay)
    {
        uofSdkForReplay.ReplayManager.StartReplayScenario(1, 10, 1000);
        Task.Delay(1000).GetAwaiter().GetResult();
        WriteReplayQueueSize(uofSdkForReplay);
    }

    private void PlayMatches(IUofSdkForReplay uofSdkForReplay)
    {
        // Add events from sport data provider (uncomment selected option)
        // Option 1:
        //foreach (var urn in SelectEventsFromSportDataProvider(uofSdkForReplay))
        //{
        //    WriteReplayResponse(uofSdkForReplay.ReplayManager.AddMessagesToReplayQueue(urn));
        //}

        // Option 2:
        // add example events
        foreach (var urn in SelectExampleEvents())
        {
            WriteReplayResponse(uofSdkForReplay.ReplayManager.AddMessagesToReplayQueue(urn));
        }

        WriteReplayQueueSize(uofSdkForReplay);

        WriteReplayResponse(uofSdkForReplay.ReplayManager.StartReplay(10, 1000));
    }

    private IEnumerable<Urn> SelectEventsFromSportDataProvider(IUofSdkForReplay uofSdkForReplay)
    {
        // Only matches older then 48 hours can be replayed
        var events = uofSdkForReplay.SportDataProvider.GetSportEventsByDateAsync(DateTime.Now.AddDays(-5)).Result.ToList();
        if (events.Count > 10)
        {
            for (var i = 0; i < 10; i++)
            {
                yield return events[i].Id;
            }
        }
    }

    private IEnumerable<Urn> SelectExampleEvents()
    {
        Console.WriteLine();
        Console.WriteLine("Sample events:");
        for (var i = 0; i < ExampleReplayEvents.SampleEvents.Count; i++)
        {
            Console.WriteLine($"{i,2} {ExampleReplayEvents.SampleEvents[i]}");
        }

        Console.WriteLine();

        while (true)
        {
            Console.WriteLine("Select an event to add or enter 'x' if you do not want to add additional events:");
            var additionalConsoleInput = Console.ReadLine();

            if (additionalConsoleInput.Equals("x", StringComparison.CurrentCultureIgnoreCase))
            {
                break;
            }

            if (!int.TryParse(additionalConsoleInput, out var additionalItemPosition)
                || additionalItemPosition < 0
                || additionalItemPosition > ExampleReplayEvents.SampleEvents.Count)
            {
                Console.WriteLine("Invalid input, retry");
                continue;
            }

            yield return ExampleReplayEvents.SampleEvents[additionalItemPosition].EventId;
        }
    }

    private void WriteReplayQueueSize(IUofSdkForReplay uofSdkForReplay)
    {

        var queueEvents = uofSdkForReplay.ReplayManager.GetEventsInQueue();
        Log.LogInformation("Currently {QueueSize} items in queue", queueEvents.Count().ToString());
    }

    private void WriteReplayStatus(IReplayStatus status)
    {
        Log.LogInformation("Status of replay: {PlayerStatus}. Last message for event: {EventId}", status.PlayerStatus, status.LastMessageFromEvent);
    }

    private void WriteReplayResponse(IReplayResponse response)
    {
        Log.LogInformation("Response of replay: {ResponseSuccess}. Message: {ResponseMessage}", response.Success, response.Message);
        if (!string.IsNullOrEmpty(response.ErrorMessage))
        {
            Log.LogInformation("\t{ErrorMessage}", response.ErrorMessage);
        }
    }
}
