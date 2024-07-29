// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Sportradar.OddsFeed.SDK.Api.Config;
using Sportradar.OddsFeed.SDK.Api.Extended;
using Xunit.Abstractions;

namespace Sportradar.OddsFeed.SDK.Tests.Common.Mock.Feed;

public class UofSdkWrapper
{
    public IUofConfiguration UofConfiguration { get; }
    public IUofSdkExtended UofSdk { get; }
    public SdkGlobalEventProcessor GlobalEventProcessor { get; }
    public SdkFeedMessageProcessor MessageProcessor { get; }
    public IServiceProvider ServiceProvider { get; }

    internal UofSdkWrapper(IServiceProvider serviceProvider, ICollection<MessageInterest> messageInterests, ITestOutputHelper outputHelper, bool openSdk)
    {
        ServiceProvider = serviceProvider;
        UofConfiguration = serviceProvider.GetRequiredService<IUofConfiguration>();
        UofSdk = new UofSdkExtended(ServiceProvider);
        GlobalEventProcessor = new SdkGlobalEventProcessor(UofSdk, outputHelper);
        GlobalEventProcessor.AttachToGlobalEvents();

        foreach (var messageInterest in messageInterests)
        {
            var session = UofSdk.GetSessionBuilder().SetMessageInterest(messageInterest).Build();
            MessageProcessor = new SdkFeedMessageProcessor(session, session.Name);
            MessageProcessor.Open();
        }

        if (openSdk)
        {
            UofSdk.Open();
            Task.Delay(5000).GetAwaiter().GetResult();
        }
    }

    public void Open(RabbitManagement rabbitMgm, ITestOutputHelper outputHelper)
    {
        if (UofSdk.IsOpen())
        {
            return;
        }
        UofSdk.Open();
        TestExecutionHelper.WaitToComplete(() => rabbitMgm.ManagementClient.GetConnectionsAsync().GetAwaiter().GetResult().Count >= 2);

        outputHelper.WriteLine("UofSdk started");
    }

    public void Close()
    {
        UofSdk.Close();
        MessageProcessor?.Close();
        GlobalEventProcessor.DetachToGlobalEvents();
    }
}
