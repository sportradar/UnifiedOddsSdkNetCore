// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using System.Threading.Tasks;

namespace Sportradar.OddsFeed.SDK.Tests.Common;

public class DemoMethods
{
    public void DemoVoidMethod()
    {
        // Method intentionally left empty.
    }

    public int DemoIntMethod(int a, int b)
    {
        return a * b;
    }

    public int DemoLongLastingMethodAsyncCaller(int seed, int steps)
    {
        return DemoLongLastingMethodAsync(seed, steps).GetAwaiter().GetResult();
    }

    public async Task<int> DemoLongLastingMethodAsync(int seed, int steps)
    {
        return await Task.Run(async () =>
                              {
                                  while (steps > 0)
                                  {
                                      seed += steps;
                                      steps--;
                                      await Task.Delay(10).ConfigureAwait(false);
                                  }

                                  return seed;
                              });
    }

    public void DemoLongLastingVoidMethod(int sleep)
    {
        Task.Delay(sleep).GetAwaiter().GetResult();
    }

    public async Task<int> DemoLongLastingSleepMethodAsync(int sleep)
    {
        await Task.Delay(sleep).ConfigureAwait(false);
        return sleep;
    }

    public async Task<int> DemoMethodWithTaskThrowsExceptionAsync(int sleep)
    {
        await Task.Run(async () =>
                       {
                           await Task.Delay(sleep).ConfigureAwait(false);
                           throw new Exception("DemoMethodWithTaskThrowsExceptionAsync exception.");
                       });
        return sleep;
    }

    public async Task<DemoMethods> DemoCustomMethodAsync(int sleep)
    {
        await Task.Delay(sleep).ConfigureAwait(false);
        return new DemoMethods();
    }

    public override string ToString()
    {
        return StaticRandom.S10000P;
    }
}
