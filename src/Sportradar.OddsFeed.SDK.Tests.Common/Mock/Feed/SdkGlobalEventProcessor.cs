// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using Sportradar.OddsFeed.SDK.Api.EventArguments;
using Sportradar.OddsFeed.SDK.Api.Extended;
using Xunit.Abstractions;

namespace Sportradar.OddsFeed.SDK.Tests.Common.Mock.Feed;

public class SdkGlobalEventProcessor
{
    private readonly IUofSdkExtended _sdk;
    private readonly ITestOutputHelper _outputHelper;
    public SdkGlobalEventProcessor(IUofSdkExtended sdk, ITestOutputHelper outputHelper)
    {
        _sdk = sdk;
        _outputHelper = outputHelper;
    }

    public void AttachToGlobalEvents()
    {
        _sdk.ProducerUp += OnProducerUp;
        _sdk.ProducerDown += OnProducerDown;
        _sdk.Disconnected += OnDisconnected;
        _sdk.Closed += OnClosed;
        _sdk.EventRecoveryCompleted += OnEventRecoveryCompleted;
        _sdk.RawFeedMessageReceived += OnRawFeedMessageReceived;
        _sdk.RawApiDataReceived += OnRawApiDataReceived;
        _sdk.RecoveryInitiated += OnRecoveryInitiated;
    }

    public void DetachToGlobalEvents()
    {
        _sdk.ProducerUp -= OnProducerUp;
        _sdk.ProducerDown -= OnProducerDown;
        _sdk.Disconnected -= OnDisconnected;
        _sdk.Closed -= OnClosed;
        _sdk.EventRecoveryCompleted -= OnEventRecoveryCompleted;
        _sdk.RawFeedMessageReceived -= OnRawFeedMessageReceived;
        _sdk.RawApiDataReceived -= OnRawApiDataReceived;
        _sdk.RecoveryInitiated -= OnRecoveryInitiated;
    }

    private void OnProducerUp(object sender, ProducerStatusChangeEventArgs e)
    {
        _outputHelper.WriteLine($"ProducerUp: {e.GetProducerStatusChange().Producer.Id}");
    }

    private void OnProducerDown(object sender, ProducerStatusChangeEventArgs e)
    {
        _outputHelper.WriteLine($"ProducerDown: {e.GetProducerStatusChange().Producer.Id}");
    }

    private void OnDisconnected(object sender, EventArgs e)
    {
        _outputHelper.WriteLine("Disconnected");
    }

    private void OnClosed(object sender, FeedCloseEventArgs e)
    {
        _outputHelper.WriteLine($"Closed: {e.GetReason()}");
    }

    private void OnEventRecoveryCompleted(object sender, EventRecoveryCompletedEventArgs e)
    {
        _outputHelper.WriteLine($"EventRecoveryCompleted: {e.GetEventId()}");
    }

    private void OnRawFeedMessageReceived(object sender, RawFeedMessageEventArgs e)
    {
        _outputHelper.WriteLine($"RawFeedMessageReceived: {e.FeedMessage}");
    }

    private void OnRawApiDataReceived(object sender, RawApiDataEventArgs e)
    {
        _outputHelper.WriteLine($"RawApiDataReceived: {e.Uri}");
    }

    private void OnRecoveryInitiated(object sender, RecoveryInitiatedEventArgs e)
    {
        _outputHelper.WriteLine($"RecoveryInitiated: {e.GetEventId()}");
    }
}
