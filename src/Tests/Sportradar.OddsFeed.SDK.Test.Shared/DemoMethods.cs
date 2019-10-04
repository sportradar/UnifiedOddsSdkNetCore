/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using System.Threading;
using System.Threading.Tasks;
using Sportradar.OddsFeed.SDK.Common.Internal.Log;

namespace Sportradar.OddsFeed.SDK.Test.Shared
{
    public class DemoMethods : MarshalByRefObject
    {
        [Log(LoggerType.Cache)]
        public void DemoVoidMethod()
        {
        }

        public int DemoIntMethod(int a, int b)
        {
            return a * b;
        }

        public int DemoLongLastingMethodAsyncCaller(int seed, int steps)
        {
            return DemoLongLastingMethodAsync(seed, steps).Result;
        }

        public async Task<int> DemoLongLastingMethodAsync(int seed, int steps)
        {
            return await Task.Run(() =>
                {
                    while (steps > 0)
                    {
                        seed += steps;
                        steps--;
                        Thread.Sleep(20);
                    }

                    return seed;
                }
            );
        }

        public void DemoLongLastingVoidMethod(int sleep)
        {
            Thread.Sleep(sleep);
        }

        public async Task<int> DemoLongLastingSleepMethodAsync(int sleep)
        {
            return await Task.Run(() =>
                {
                    Thread.Sleep(sleep);
                    return sleep;
                }
            );
        }

        public async Task<int> DemoMethodWithTaskThrowsExceptionAsync(int sleep)
        {
            return await Task.Run(() =>
                {
                    Thread.Sleep(sleep);
                    throw new Exception("DemoMethodWithTaskThrowsExceptionAsync exception.");
#pragma warning disable 162
                    return sleep;
#pragma warning restore 162
                }
            );
        }

        [Log(LoggerType.RestTraffic)]
        public async Task<DemoMethods> DemoCustomMethodAsync(int sleep)
        {
            return await Task.Run(() =>
                {
                    Thread.Sleep(sleep);
                    return new DemoMethods();
                }
            );
        }

        public override string ToString()
        {
            return new Random().Next().ToString();
        }
    }
}
