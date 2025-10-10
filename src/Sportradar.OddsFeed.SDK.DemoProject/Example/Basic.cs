/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using Microsoft.Extensions.Logging;
using Sportradar.OddsFeed.SDK.Api;
using Sportradar.OddsFeed.SDK.Api.Config;

namespace Sportradar.OddsFeed.SDK.DemoProject.Example;

/// <summary>
/// Basic example with single session, generic dispatcher, basic output
/// </summary>
public class Basic : ExampleBase
{
    private readonly UofClientAuthentication.IPrivateKeyJwtData _clientAuthentication;

    public Basic(ILogger<Basic> logger, UofClientAuthentication.IPrivateKeyJwtData clientAuthentication)
        : base(logger)
    {
        _clientAuthentication = clientAuthentication;
    }

    public override void Run(MessageInterest messageInterest)
    {
        Log.LogInformation("Running the Basic example");

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
        Log.LogInformation("Example successfully started. Hit <enter> to quit");
        Console.WriteLine(string.Empty);
        Console.ReadLine();

        Log.LogInformation("Closing / disposing the sdk instance");
        uofSdk.Close();

        DetachFromSessionEvents(session);
        DetachFromGlobalEvents(uofSdk);

        Log.LogInformation("Stopped");
    }
}
