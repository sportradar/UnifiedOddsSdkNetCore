// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using Microsoft.Extensions.Logging;
using Moq;
using Sportradar.OddsFeed.SDK.Api.Config;
using Sportradar.OddsFeed.SDK.Api.Internal.ApiAccess;
using Sportradar.OddsFeed.SDK.Api.Internal.Managers;
using Sportradar.OddsFeed.SDK.Common.Enums;
using Sportradar.OddsFeed.SDK.Entities.Rest;
using Sportradar.OddsFeed.SDK.Tests.Common.Builders.CustomBet;

namespace Sportradar.OddsFeed.SDK.Tests.Common.Builders.Extensions;

internal static class CustomBetManagerBuilderExtensions
{
    public static CustomBetManagerBuilder AddDefaultDependencies(this CustomBetManagerBuilder builder)
    {
        builder.WithConfiguration(new Mock<IUofConfiguration>().Object)
               .WithSelectionBuilderFactory(new Mock<ICustomBetSelectionBuilderFactory>().Object)
               .WithDataFetcher(new Mock<IDataFetcher>().Object)
               .WithClientLogger(new Mock<ILogger>().Object)
               .WithExecutionLogger(new Mock<ILogger>().Object);

        return builder;
    }

    public static CustomBetManagerBuilder WithConfigurationProvidingAnyBookmakerIdAndStrategyThrow(this CustomBetManagerBuilder builder)
    {
        return builder.WithConfigurationProviding(TestConsts.AnyBookmakerId, ExceptionHandlingStrategy.Throw);
    }

    public static CustomBetManagerBuilder WithConfigurationProvidingAnyBookmakerIdAndStrategyCatch(this CustomBetManagerBuilder builder)
    {
        return builder.WithConfigurationProviding(TestConsts.AnyBookmakerId, ExceptionHandlingStrategy.Catch);
    }

    public static CustomBetManagerBuilder WithConfigurationProviding(this CustomBetManagerBuilder builder, int bookmakerId, ExceptionHandlingStrategy exceptionHandlingStrategy)
    {
        var bookmakerDetailsMock = new Mock<IBookmakerDetails>();
        bookmakerDetailsMock.SetupGet(b => b.BookmakerId).Returns(bookmakerId);

        var uofConfigurationMock = new Mock<IUofConfiguration>();
        uofConfigurationMock.SetupGet(c => c.ExceptionHandlingStrategy).Returns(exceptionHandlingStrategy);
        uofConfigurationMock.SetupGet(c => c.BookmakerDetails).Returns(bookmakerDetailsMock.Object);

        builder.WithConfiguration(uofConfigurationMock.Object);

        return builder;
    }
}
