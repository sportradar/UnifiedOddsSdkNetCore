// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using System.Threading.Tasks;
using Moq;
using Shouldly;
using Sportradar.OddsFeed.SDK.Api.EventArguments;
using Sportradar.OddsFeed.SDK.Api.Internal;
using Sportradar.OddsFeed.SDK.Api.Internal.ApiAccess;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal;
using Sportradar.OddsFeed.SDK.Messages.Rest;
using Xunit;

namespace Sportradar.OddsFeed.SDK.Tests.Common;

public class ExecutionPathDataProviderTests
{
    private static readonly RequestOptions TimeCriticalRequestOptions = new RequestOptions(ExecutionPath.TimeCritical);
    private static readonly RequestOptions NonTimeCriticalRequestOptions = new RequestOptions(ExecutionPath.NonTimeCritical);

    [Fact]
    public void ExecutionPathDataProviderUnsubscribesFromProvidersWhenDisposed()
    {
        var criticalMock = new Mock<IDataProvider<AnyDto>>();
        var nonCriticalMock = new Mock<IDataProvider<AnyDto>>();

        var provider = new ExecutionPathDataProvider<AnyDto>(criticalMock.Object, nonCriticalMock.Object);

        var wasCalled = false;
        provider.RawApiDataReceived += (_, _) => wasCalled = true;

        var args = new RawApiDataEventArgs(new Uri("http://localhost"), It.IsAny<RestMessage>(), It.IsAny<string>(), It.IsAny<TimeSpan>(), It.IsAny<string>());

        provider.Dispose();

        criticalMock.Raise(m => m.RawApiDataReceived += null, args);
        nonCriticalMock.Raise(m => m.RawApiDataReceived += null, args);

        wasCalled.ShouldBeFalse();
    }

    [Fact]
    public async Task GetDataUsesTimeCriticalProviderWhenRequestIsTimeCritical()
    {
        var identifiers = new[] { "id1", "id2" };
        var expectedResult = new AnyDto();

        var criticalMock = new Mock<IDataProvider<AnyDto>>();
        var nonCriticalMock = new Mock<IDataProvider<AnyDto>>();
        var provider = new ExecutionPathDataProvider<AnyDto>(criticalMock.Object, nonCriticalMock.Object);

        criticalMock.Setup(m => m.GetDataAsync(identifiers)).ReturnsAsync(expectedResult);

        var result = await provider.GetDataAsync(TimeCriticalRequestOptions, identifiers);

        result.ShouldBe(expectedResult);
        criticalMock.Verify(m => m.GetDataAsync(identifiers), Times.Once);
        nonCriticalMock.Verify(m => m.GetDataAsync(It.IsAny<string[]>()), Times.Never);
    }

    [Fact]
    public async Task GetDataUsesNonTimeCriticalProviderWhenRequestIsNonTimeCritical()
    {
        var identifiers = new[] { "id1", "id2" };
        var expectedResult = new AnyDto();

        var criticalMock = new Mock<IDataProvider<AnyDto>>();
        var nonCriticalMock = new Mock<IDataProvider<AnyDto>>();
        var provider = new ExecutionPathDataProvider<AnyDto>(criticalMock.Object, nonCriticalMock.Object);

        nonCriticalMock.Setup(m => m.GetDataAsync(identifiers)).ReturnsAsync(expectedResult);

        var result = await provider.GetDataAsync(NonTimeCriticalRequestOptions, identifiers);

        result.ShouldBe(expectedResult);
        criticalMock.Verify(m => m.GetDataAsync(identifiers), Times.Never);
        nonCriticalMock.Verify(m => m.GetDataAsync(It.IsAny<string[]>()), Times.Once);
    }

    public class AnyDto
    {
    }
}
