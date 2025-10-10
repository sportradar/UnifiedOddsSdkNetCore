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
/// An example demonstrating getting and displaying all Markets with Outcomes
/// </summary>
/// <seealso cref="MarketWriter"/>
public class ShowMarketNames : ExampleBase
{
    private readonly UofClientAuthentication.IPrivateKeyJwtData _clientAuthentication;
    private MarketWriter _marketWriter;

    public ShowMarketNames(ILogger<ShowMarketNames> logger, UofClientAuthentication.IPrivateKeyJwtData clientAuthentication)
        : base(logger)
    {
        _clientAuthentication = clientAuthentication;
    }

    public override void Run(MessageInterest messageInterest)
    {
        Log.LogInformation("Running the Display Markets Names example");

        Log.LogInformation("Retrieving configuration from application configuration file");
        var configuration = UofSdk.GetConfigurationBuilder().SetClientAuthentication(_clientAuthentication).BuildFromConfigFile();
        var uofSdk = RegisterServicesAndGetUofSdk(configuration);
        AttachToGlobalEvents(uofSdk);

        Log.LogInformation("Creating IUofSessions");
        var session = uofSdk.GetSessionBuilder()
                            .SetMessageInterest(messageInterest)
                            .Build();

        _marketWriter = new MarketWriter(TaskProcessor, configuration.DefaultLanguage, Log);

        var defaultEventsProcessor = new EntityProcessor<ISportEvent>(session, null, _marketWriter, Log);

        AttachToGlobalEvents(uofSdk);
        Log.LogInformation("Opening event processors");
        defaultEventsProcessor.Open();
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

        Log.LogInformation("Waiting for asynchronous operations to complete");
        var waitResult = TaskProcessor.WaitForTasks();
        Log.LogInformation("Waiting for tasks completed. Result:{WaitResult}", waitResult);
        Log.LogInformation("Stopped");
    }
}
