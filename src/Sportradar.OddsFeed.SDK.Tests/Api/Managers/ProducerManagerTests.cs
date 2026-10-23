// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using FluentAssertions;
using Sportradar.OddsFeed.SDK.Api;
using Sportradar.OddsFeed.SDK.Api.Internal;
using Sportradar.OddsFeed.SDK.Api.Internal.Managers;
using Sportradar.OddsFeed.SDK.Common.Extensions;
using Sportradar.OddsFeed.SDK.Common.Internal;
using Sportradar.OddsFeed.SDK.Tests.Common;
using Xunit;

namespace Sportradar.OddsFeed.SDK.Tests.API.Managers;

public class ProducerManagerTests
{
    [Fact]
    public void Init()
    {
        var producerManager = TestProducerManager.Create();
        Assert.NotNull(producerManager);
        Assert.NotNull(producerManager.Producers);
        Assert.NotEmpty(producerManager.Producers);
    }

    [Fact]
    public void UnknownProducer()
    {
        var producerManager = TestProducerManager.Create();

        var producer = producerManager.GetProducer(50);
        Assert.NotNull(producer);
        Assert.Equal(99, producer.Id);
        Assert.True(producer.IsAvailable);
        Assert.False(producer.IsDisabled);
        Assert.True(producer.IsProducerDown);
        Assert.Equal("Unknown", producer.Name);
    }

    [Fact]
    public void CompareProducers()
    {
        var producerManager = TestProducerManager.Create();

        var producer1 = producerManager.GetProducer(1);
        var producer2 = new Producer(1, "Lo", "Live Odds", "lo", true, 60, 3600, "live", 600);
        Assert.NotNull(producer1);
        Assert.Equal(1, producer1.Id);
        Assert.NotNull(producer2);
        Assert.Equal(1, producer2.Id);
        Assert.Equal(producer1.IsAvailable, producer2.IsAvailable);
        Assert.Equal(producer1.IsDisabled, producer2.IsDisabled);
        Assert.Equal(producer1.IsProducerDown, producer2.IsProducerDown);
        Assert.Equal(producer1.Name, producer2.Name, true);
        Assert.Equal(producer1.Name, producer2.Name, ignoreCase: true);
        Assert.Equal(producer1, producer2);
    }

    [Fact]
    public void GetById()
    {
        var producerManager = TestProducerManager.Create();

        var producer1 = producerManager.GetProducer(1);
        Assert.NotNull(producer1);
        Assert.Equal(1, producer1.Id);
    }

    [Fact]
    public void GetByName()
    {
        var producerManager = TestProducerManager.Create();

        var producer1 = producerManager.GetProducer("lo");
        Assert.NotNull(producer1);
        Assert.Equal(1, producer1.Id);
    }

    [Fact]
    public void ExistsById()
    {
        var producerManager = TestProducerManager.Create();

        var result = producerManager.Exists(1);
        Assert.True(result);
    }

    [Fact]
    public void ExistsByName()
    {
        var producerManager = TestProducerManager.Create();

        var result = producerManager.Exists("lo");
        Assert.True(result);
    }

    [Fact]
    public void NotExistsById()
    {
        var producerManager = TestProducerManager.Create();

        var result = producerManager.Exists(11);
        Assert.False(result);
    }

    [Fact]
    public void UpdateLocked01()
    {
        var producerId = 1;
        var producerManager = TestProducerManager.Create();

        var producer = producerManager.GetProducer(producerId);
        CheckLiveOddsProducer(producer);
        ((ProducerManager)producerManager).Lock();

        Assert.Throws<InvalidOperationException>(() => producerManager.DisableProducer(producerId));
    }

    [Fact]
    public void UpdateLocked02()
    {
        const int producerId = 1;
        var date = DateTime.Now;
        var producerManager = TestProducerManager.Create();

        var producer = producerManager.GetProducer(producerId);
        CheckLiveOddsProducer(producer);

        ((ProducerManager)producerManager).Lock();

        Assert.Throws<InvalidOperationException>(() => producerManager.AddTimestampBeforeDisconnect(producerId, date));
    }

    [Fact]
    public void UpdateIsDisabled()
    {
        const int producerId = 1;
        var producerManager = TestProducerManager.Create();

        producerManager.DisableProducer(producerId);

        var producer = producerManager.GetProducer(producerId);
        Assert.True(producer.IsDisabled);
    }

    [Fact]
    public void UpdateTimestamp()
    {
        const int producerId = 1;
        var date = DateTime.Now;
        var producerManager = TestProducerManager.Create();

        producerManager.AddTimestampBeforeDisconnect(producerId, date);

        var producer = producerManager.GetProducer(producerId);
        Assert.Equal(date, producer.LastTimestampBeforeDisconnect);
    }

    [Fact]
    public void UpdateTimestampWhenDateInFutureThenThrow()
    {
        const int producerId = 1;
        var date = DateTime.Now.AddSeconds(1);
        var producerManager = TestProducerManager.Create();

        producerManager.Invoking(i => i.AddTimestampBeforeDisconnect(producerId, date)).Should().Throw<ArgumentException>();
    }

    [Fact]
    public void UpdateTimestampWhenDateMinThenThrow()
    {
        const int producerId = 1;
        var producerManager = TestProducerManager.Create();

        producerManager.Invoking(i => i.AddTimestampBeforeDisconnect(producerId, DateTime.MinValue)).Should().Throw<ArgumentException>();
    }

    [Fact]
    public void UpdateTimestampWhenDateEarlierThenMaxDateThenThrow()
    {
        const int producerId = 1;
        var producerManager = TestProducerManager.Create();
        var producer = producerManager.GetProducer(producerId);
        var date = DateTime.Now.Subtract(producer.MaxAfterAge());

        producerManager.Invoking(i => i.AddTimestampBeforeDisconnect(producerId, date)).Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void UpdateTimestampWhenValidAfterWithMaxInactivitySecondsThenUpdate()
    {
        const int producerId = 1;
        var producerManager = TestProducerManager.Create();
        var producer = producerManager.GetProducer(producerId);
        var date = DateTime.Now.AddSeconds(-producer.MaxInactivitySeconds);

        producerManager.AddTimestampBeforeDisconnect(producerId, date);

        Assert.Equal(date, producer.LastTimestampBeforeDisconnect);
    }

    [Fact]
    public void UpdateTimestampWhenValidMaxAfterThenUpdate()
    {
        const int producerId = 1;
        var producerManager = TestProducerManager.Create();
        var producer = producerManager.GetProducer(producerId);
        var date = DateTime.Now.Subtract(producer.MaxAfterAge()).AddSeconds(1); // add 1 second to avoid diff in milliseconds

        producerManager.AddTimestampBeforeDisconnect(producerId, date);

        Assert.Equal(date, producer.LastTimestampBeforeDisconnect);
    }

    [Fact]
    public void UpdateTimestampWhenValidMAfterAndUnknownProducerThenNotUpdate()
    {
        const int producerId = SdkInfo.UnknownProducerId;
        var producerManager = TestProducerManager.Create();
        var producer = producerManager.GetProducer(producerId);
        var date = DateTime.Now.Subtract(TimeSpan.FromHours(1));

        producerManager.AddTimestampBeforeDisconnect(producerId, date);

        Assert.Equal(DateTime.MinValue, producer.LastTimestampBeforeDisconnect);
    }

    [Fact]
    public void LockWhenNoProducerAvailableThenThrow()
    {
        var producerManager = (ProducerManager)TestProducerManager.Create();
        foreach (var producer in producerManager.Producers)
        {
            producerManager.DisableProducer(producer.Id);
        }

        producerManager.Invoking(i => i.Lock()).Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void LockWhenLockedCanNotUpdateDisabledProducers()
    {
        var producerManager = (ProducerManager)TestProducerManager.Create();
        producerManager.Lock();

        producerManager.Invoking(i => i.DisableProducer(1)).Should().Throw<InvalidOperationException>();
    }

    // [Fact]
    // [ExpectedException(typeof(InvalidOperationException))]
    // public void UpdateLocked01()
    // {
    //     var producerId = 1;
    //     var producerManager = TestProducerManager.Create();
    //
    //     var producer = producerManager.Get(producerId);
    //     CheckLiveOddsProducer(producer);
    //
    //     ((ProducerManager)producerManager).Lock();
    //     producerManager.DisableProducer(producerId);
    //     Assert.Equal(true, producer.IsDisabled);
    // }
    //
    // [Fact]
    // [ExpectedException(typeof(InvalidOperationException))]
    // public void UpdateLocked02()
    // {
    //     var producerId = 1;
    //     var date = DateTime.Now;
    //     var producerManager = TestProducerManager.Create();
    //
    //     var producer = producerManager.Get(producerId);
    //     CheckLiveOddsProducer(producer);
    //
    //     ((ProducerManager)producerManager).Lock();
    //     producerManager.AddTimestampBeforeDisconnect(producerId, date);
    //     Assert.Equal(date, producer.LastTimestampBeforeDisconnect);
    // }

    private static void CheckLiveOddsProducer(IProducer producer)
    {
        Assert.NotNull(producer);
        Assert.Equal(1, producer.Id);
        Assert.True(producer.IsAvailable);
        Assert.False(producer.IsDisabled);
        Assert.True(producer.IsProducerDown);
        Assert.Equal("LO", producer.Name);
        Assert.Equal(DateTime.MinValue, producer.LastTimestampBeforeDisconnect);
    }
}
