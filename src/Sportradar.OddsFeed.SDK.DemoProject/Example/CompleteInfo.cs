/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using System.Globalization;
using Microsoft.Extensions.Logging;
using Sportradar.OddsFeed.SDK.Api;
using Sportradar.OddsFeed.SDK.Api.Config;
using Sportradar.OddsFeed.SDK.DemoProject.Utils;
using Sportradar.OddsFeed.SDK.Entities.Rest;

namespace Sportradar.OddsFeed.SDK.DemoProject.Example;

///<summary>
/// A complete example using <see cref="ISpecificEntityDispatcher{T}"/> for various <see cref="ISportEvent"/> displaying all <see cref="ICompetition"/> info with Markets and Outcomes
/// </summary>
public class CompleteInfo : ExampleBase
{
    private readonly CultureInfo _culture;

    public CompleteInfo(ILogger<CompleteInfo> logger, CultureInfo culture)
        : base(logger)
    {
        _culture = culture;
    }

    public override void Run(MessageInterest messageInterest)
    {
        Console.WriteLine(string.Empty);
        Log.LogInformation("Running the Complete example");

        var configuration = UofSdk.GetConfigurationBuilder().BuildFromConfigFile();
        var uofSdk = RegisterServicesAndGetUofSdk(configuration);

        LimitRecoveryRequests(uofSdk);

        AttachToGlobalEvents(uofSdk);

        Log.LogInformation("Creating IUofSession");
        var session = uofSdk.GetSessionBuilder()
                            .SetMessageInterest(messageInterest)
                            .Build();

        var marketWriter = new MarketWriter(TaskProcessor, _culture, Log);
        var sportEntityWriter = new SportEntityWriter(TaskProcessor, _culture, false, Log);

        Log.LogInformation("Creating entity specific dispatchers");
        var matchDispatcher = session.CreateSportSpecificMessageDispatcher<IMatch>();
        var stageDispatcher = session.CreateSportSpecificMessageDispatcher<IStage>();
        var tournamentDispatcher = session.CreateSportSpecificMessageDispatcher<ITournament>();
        var basicTournamentDispatcher = session.CreateSportSpecificMessageDispatcher<IBasicTournament>();
        var seasonDispatcher = session.CreateSportSpecificMessageDispatcher<ISeason>();

        Log.LogInformation("Creating event processors");
        var defaultEventsProcessor = new EntityProcessor<ISportEvent>(session, sportEntityWriter, marketWriter, Log);
        var matchEventsProcessor = new SpecificEntityProcessor<IMatch>(matchDispatcher, sportEntityWriter, marketWriter, Log);
        var stageEventsProcessor = new SpecificEntityProcessor<IStage>(stageDispatcher, sportEntityWriter, marketWriter, Log);
        var tournamentEventsProcessor = new SpecificEntityProcessor<ITournament>(tournamentDispatcher, sportEntityWriter, marketWriter, Log);
        var basicTournamentEventsProcessor = new SpecificEntityProcessor<IBasicTournament>(basicTournamentDispatcher, sportEntityWriter, marketWriter, Log);
        var seasonEventsProcessor = new SpecificEntityProcessor<ISeason>(seasonDispatcher, sportEntityWriter, marketWriter, Log);

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
        Log.LogInformation("Waiting for tasks completed. Result: {Result}", waitResult);

        Log.LogInformation("Stopped");
    }
}