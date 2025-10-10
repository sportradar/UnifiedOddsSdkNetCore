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
/// An example demonstrating getting and displaying all info regarding <see cref="ICompetition"/>
/// </summary>
public class ShowEventInfo : ExampleBase
{
    private readonly UofClientAuthentication.IPrivateKeyJwtData _clientAuthentication;

    public ShowEventInfo(ILogger<ShowEventInfo> logger, UofClientAuthentication.IPrivateKeyJwtData clientAuthentication)
        : base(logger)
    {
        _clientAuthentication = clientAuthentication;
    }

    public override void Run(MessageInterest messageInterest)
    {
        Console.WriteLine(string.Empty);
        Log.LogInformation("Running the SportEvent Info example");

        var configuration = UofSdk.GetConfigurationBuilder().SetClientAuthentication(_clientAuthentication).BuildFromConfigFile();
        var uofSdk = RegisterServicesAndGetUofSdk(configuration);
        AttachToGlobalEvents(uofSdk);

        Log.LogInformation("Creating IUofSessions");
        var session = uofSdk.GetSessionBuilder()
                            .SetMessageInterest(messageInterest)
                            .Build();

        var sportEntityWriter = new SportEntityWriter(TaskProcessor, configuration.DefaultLanguage, false, Log);

        Log.LogInformation("Creating entity specific dispatchers");
        var matchDispatcher = session.CreateSportSpecificMessageDispatcher<IMatch>();
        var stageDispatcher = session.CreateSportSpecificMessageDispatcher<IStage>();
        var tournamentDispatcher = session.CreateSportSpecificMessageDispatcher<ITournament>();
        var basicTournamentDispatcher = session.CreateSportSpecificMessageDispatcher<IBasicTournament>();
        var seasonDispatcher = session.CreateSportSpecificMessageDispatcher<ISeason>();

        Log.LogInformation("Creating event processors");
        var defaultEventsProcessor = new EntityProcessor<ISportEvent>(session, sportEntityWriter, null, Log);
        var matchEventsProcessor = new SpecificEntityProcessor<IMatch>(matchDispatcher, sportEntityWriter, null, Log);
        var stageEventsProcessor = new SpecificEntityProcessor<IStage>(stageDispatcher, sportEntityWriter, null, Log);
        var tournamentEventsProcessor = new SpecificEntityProcessor<ITournament>(tournamentDispatcher, sportEntityWriter, null, Log);
        var basicTournamentEventsProcessor = new SpecificEntityProcessor<IBasicTournament>(basicTournamentDispatcher, sportEntityWriter, null, Log);
        var seasonEventsProcessor = new SpecificEntityProcessor<ISeason>(seasonDispatcher, sportEntityWriter, null, Log);

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

        Log.LogInformation("Waiting for asynchronous operations to complete");
        var waitResult = TaskProcessor.WaitForTasks();
        Log.LogInformation("Waiting for tasks completed. Result:{WaitResult}", waitResult);

        Log.LogInformation("Stopped");
    }
}
