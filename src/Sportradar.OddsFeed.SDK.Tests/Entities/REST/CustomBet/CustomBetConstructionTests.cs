// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using Microsoft.Extensions.Logging;
using Moq;
using Shouldly;
using Sportradar.OddsFeed.SDK.Api.Config;
using Sportradar.OddsFeed.SDK.Api.Internal.ApiAccess;
using Sportradar.OddsFeed.SDK.Api.Internal.Managers;
using Xunit;

namespace Sportradar.OddsFeed.SDK.Tests.Entities.Rest.CustomBet;

public class CustomBetConstructionTests
{
    [Fact]
    public void CustomBetManagerClientInteractionLoggerCannotBeNull()
    {
        var exception = Should.Throw<ArgumentNullException>(() =>
                                                                new CustomBetManager(new Mock<IDataRouterManager>().Object, new Mock<IUofConfiguration>().Object,
                                                                                     new Mock<ICustomBetSelectionBuilderFactory>().Object,
                                                                                     null,
                                                                                     new Mock<ILogger>().Object));
        exception.ParamName.ShouldBe("clientLog");
    }

    [Fact]
    public void CustomBetManagerExecutionLoggerCannotBeNull()
    {
        var exception = Should.Throw<ArgumentNullException>(() =>
                                                                new CustomBetManager(new Mock<IDataRouterManager>().Object, new Mock<IUofConfiguration>().Object,
                                                                                     new Mock<ICustomBetSelectionBuilderFactory>().Object,
                                                                                     new Mock<ILogger>().Object,
                                                                                     null));
        exception.ParamName.ShouldBe("executionLog");
    }
}
