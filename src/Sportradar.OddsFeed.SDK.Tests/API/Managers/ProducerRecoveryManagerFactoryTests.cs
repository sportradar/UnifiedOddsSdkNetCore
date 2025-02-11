// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using FluentAssertions;
using Moq;
using Sportradar.OddsFeed.SDK.Api.Config;
using Sportradar.OddsFeed.SDK.Api.Internal.Managers;
using Sportradar.OddsFeed.SDK.Api.Internal.Recovery;
using Sportradar.OddsFeed.SDK.Entities.Internal;
using Sportradar.OddsFeed.SDK.Tests.Common;
using Xunit;

namespace Sportradar.OddsFeed.SDK.Tests.API.Managers;

public class ProducerRecoveryManagerFactoryTests
{
    private readonly Mock<IRecoveryRequestIssuer> _recoveryRequestIssuerMock;
    private readonly Mock<IFeedMessageMapper> _feedMessageMapperMock;
    public ProducerRecoveryManagerFactoryTests()
    {
        _recoveryRequestIssuerMock = new Mock<IRecoveryRequestIssuer>();
        _feedMessageMapperMock = new Mock<IFeedMessageMapper>();
    }

    [Fact]
    public void ProducerRecoveryManagerFactoryWhenNormalConstructor()
    {
        var factory = new ProducerRecoveryManagerFactory(_recoveryRequestIssuerMock.Object, _feedMessageMapperMock.Object, TestConfiguration.GetConfig());

        factory.Should().NotBeNull();
    }

    [Fact]
    public void GetRecoveryTrackerWhenNormalCall()
    {
        var config = TestConfiguration.GetConfig();
        var factory = new ProducerRecoveryManagerFactory(_recoveryRequestIssuerMock.Object, _feedMessageMapperMock.Object, config);
        var producer = TestProducerManager.Create().GetProducer(1);

        var recoveryTracker = factory.GetRecoveryTracker(producer, [MessageInterest.AllMessages]);

        recoveryTracker.Should().NotBeNull();
    }

    [Fact]
    public void GetRecoveryTrackerWhenProducerIsNull()
    {
        var config = TestConfiguration.GetConfig();
        var factory = new ProducerRecoveryManagerFactory(_recoveryRequestIssuerMock.Object, _feedMessageMapperMock.Object, config);

        factory.Invoking(i => i.GetRecoveryTracker(null, [MessageInterest.AllMessages]))
            .Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void GetRecoveryTrackerWhenAllInterestsIsNull()
    {
        var config = TestConfiguration.GetConfig();
        var factory = new ProducerRecoveryManagerFactory(_recoveryRequestIssuerMock.Object, _feedMessageMapperMock.Object, config);
        var producer = TestProducerManager.Create().GetProducer(1);

        factory.Invoking(i => i.GetRecoveryTracker(producer, null))
            .Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void GetRecoveryTrackerWhenAllInterestsIsEmpty()
    {
        var config = TestConfiguration.GetConfig();
        var factory = new ProducerRecoveryManagerFactory(_recoveryRequestIssuerMock.Object, _feedMessageMapperMock.Object, config);
        var producer = TestProducerManager.Create().GetProducer(1);

        factory.Invoking(i => i.GetRecoveryTracker(producer, Array.Empty<MessageInterest>()))
            .Should().Throw<ArgumentException>();
    }

    [Fact]
    public void CreateSessionMessageManagerWhenNormalCall()
    {
        var factory = new ProducerRecoveryManagerFactory(_recoveryRequestIssuerMock.Object, _feedMessageMapperMock.Object, TestConfiguration.GetConfig());

        var sessionMessageManager = factory.CreateSessionMessageManager();

        sessionMessageManager.Should().NotBeNull();
    }
}
