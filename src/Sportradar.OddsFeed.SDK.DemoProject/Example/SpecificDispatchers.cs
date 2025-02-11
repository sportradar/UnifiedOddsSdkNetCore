/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using Microsoft.Extensions.Logging;
using Sportradar.OddsFeed.SDK.Api;
using Sportradar.OddsFeed.SDK.Api.Config;
using Sportradar.OddsFeed.SDK.DemoProject.Utils;
using Sportradar.OddsFeed.SDK.Entities.Rest;

namespace Sportradar.OddsFeed.SDK.DemoProject.Example;

/// <summary>
/// SpecificDispatchers example shows how to use <see cref="ISpecificEntityDispatcher{T}"/> to differently process specific <see cref="ISportEvent"/>
/// </summary>
public class SpecificDispatchers : ExampleBase
{
    public SpecificDispatchers(ILogger<SpecificDispatchers> logger)
        : base(logger)
    {
    }

    public override void Run(MessageInterest messageInterest)
    {
        Console.WriteLine(string.Empty);
        Log.LogInformation("Running the Specific Dispatchers example");

        var configuration = UofSdk.GetConfigurationBuilder().BuildFromConfigFile();
        var uofSdk = RegisterServicesAndGetUofSdk(configuration);

        LimitRecoveryRequests(uofSdk);

        AttachToGlobalEvents(uofSdk);

        Log.LogInformation("Creating IUofSessions");
        var session = uofSdk.GetSessionBuilder()
                            .SetMessageInterest(messageInterest)
                            .Build();

        Log.LogInformation("Creating entity specific dispatchers");
        var matchDispatcher = session.CreateSportSpecificMessageDispatcher<IMatch>();
        var stageDispatcher = session.CreateSportSpecificMessageDispatcher<IStage>();
        var tournamentDispatcher = session.CreateSportSpecificMessageDispatcher<ITournament>();
        var basicTournamentDispatcher = session.CreateSportSpecificMessageDispatcher<IBasicTournament>();
        var seasonDispatcher = session.CreateSportSpecificMessageDispatcher<ISeason>();

        Log.LogInformation("Creating event processors");
        var defaultEventsProcessor = new EntityProcessor<ISportEvent>(session, null, null, Log);
        var matchEventsProcessor = new SpecificEntityProcessor<IMatch>(matchDispatcher, null, null, Log);
        var stageEventsProcessor = new SpecificEntityProcessor<IStage>(stageDispatcher, null, null, Log);
        var tournamentEventsProcessor = new SpecificEntityProcessor<ITournament>(tournamentDispatcher, null, null, Log);
        var basicTournamentEventsProcessor = new SpecificEntityProcessor<IBasicTournament>(basicTournamentDispatcher, null, null, Log);
        var seasonEventsProcessor = new SpecificEntityProcessor<ISeason>(seasonDispatcher, null, null, Log);

        Log.LogInformation("Opening event processors");
        defaultEventsProcessor.Open();
        matchEventsProcessor.Open();
        stageEventsProcessor.Open();
        tournamentEventsProcessor.Open();
        basicTournamentEventsProcessor.Open();
        seasonEventsProcessor.Open();

        Log.LogInformation("Opening the sdk instance");
        uofSdk.Open();
        Log.LogInformation("Example successfully started. Hit <enter> to quit");
        Console.WriteLine(string.Empty);
        Console.ReadLine();

        Log.LogInformation("Closing / disposing the sdk instance");
        uofSdk.Close();

        DetachFromGlobalEvents(uofSdk);

        Log.LogInformation("Closing event processors");
        defaultEventsProcessor.Close();
        matchEventsProcessor.Close();
        stageEventsProcessor.Close();
        tournamentEventsProcessor.Close();
        basicTournamentEventsProcessor.Close();
        seasonEventsProcessor.Close();

        Log.LogInformation("Stopped");
    }
}