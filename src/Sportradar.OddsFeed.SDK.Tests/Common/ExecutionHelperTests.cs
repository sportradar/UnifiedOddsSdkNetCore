// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using System.Threading.Tasks;
using Sportradar.OddsFeed.SDK.Common.Internal;
using Xunit;

namespace Sportradar.OddsFeed.SDK.Tests.Common;

[Collection(NonParallelCollectionFixture.NonParallelTestCollection)]
public class ExecutionHelperTests
{
    [Fact]
    public void SafeWhenActionSucceedThenReturnTrue()
    {
        var result = ExecutionHelper.Safe(() => { });

        Assert.True(result);
    }

    [Fact]
    public void SafeWhenActionFailThenReturnFalse()
    {
        var result = ExecutionHelper.Safe(() => throw new InvalidOperationException());

        Assert.False(result);
    }

    [Fact]
    public async Task SafeAsyncWhenFunctionSucceedThenReturnTrue()
    {
        var result = await ExecutionHelper.SafeAsync(SuccessAsyncMethod);

        Assert.True(result);
    }

    [Fact]
    public async Task SafeAsyncWhenFunctionFailThenReturnFalse()
    {
        var result = await ExecutionHelper.SafeAsync(ThrowAsyncMethod);

        Assert.False(result);
    }

    [Fact]
    public async Task SafeAsyncWithOneArgumentWhenFunctionSucceedThenReturnTrue()
    {
        var result = await ExecutionHelper.SafeAsync(AsyncMethodWithOneArgument, 10);

        Assert.True(result);
    }

    [Fact]
    public async Task SafeAsyncWithOneArgumentWhenFunctionFailThenReturnFalse()
    {
        var result = await ExecutionHelper.SafeAsync(AsyncMethodWithOneArgument, -1);

        Assert.False(result);
    }

    [Fact]
    public async Task SafeAsyncWithTwoArgumentsWhenFunctionSucceedThenReturnTrue()
    {
        var result = await ExecutionHelper.SafeAsync(AsyncMethodWithTwoArguments, 10, 10);

        Assert.True(result);
    }

    [Fact]
    public async Task SafeAsyncWithTwoArgumentsWhenFunctionFailThenReturnFalse()
    {
        var result = await ExecutionHelper.SafeAsync(AsyncMethodWithTwoArguments, 30, -1);

        Assert.False(result);
    }

    private static async Task SuccessAsyncMethod()
    {
        await Task.Delay(50);
    }

    private static async Task ThrowAsyncMethod()
    {
        await Task.Delay(50);
        throw new InvalidOperationException();
    }

    private static async Task AsyncMethodWithOneArgument(int mode)
    {
        await Task.Delay(50);
        if (mode == -1)
        {
            throw new InvalidOperationException();
        }
    }

    private static async Task AsyncMethodWithTwoArguments(int delay, int mode)
    {
        await Task.Delay(delay);
        if (mode == -1)
        {
            throw new InvalidOperationException();
        }
    }
}
