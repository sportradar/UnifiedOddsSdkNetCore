// /*
// * Copyright (C) Sportradar AG. See LICENSE for full license governing this code
// */

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Common.Internal.Log;
using Sportradar.OddsFeed.SDK.Tests.Common;
using Xunit;

namespace Sportradar.OddsFeed.SDK.Tests.API
{
    /// <summary>
    /// Result are in log file
    /// </summary>
    public class LogProxyTests
    {
        private readonly DemoMethods _demoClass;

        public LogProxyTests()
        {
            _demoClass = LogInterceptorFactory.Create<DemoMethods>(null, m => m.Name.Contains("D"), LoggerType.ClientInteraction);
        }

        [Fact]
        public void LogInputAndOutputParametersOfVoidMethodTest()
        {
            _demoClass.DemoVoidMethod();
            var res = _demoClass.DemoIntMethod(10, 50);
            Assert.True(res > 100);
        }

        [Fact]
        public void LogInputAndOutputParametersOfAsyncCallerMethodTest()
        {
            var res = _demoClass.DemoLongLastingMethodAsyncCaller(45, 10);
            Assert.Equal(100, res);
        }

        [Fact]
        public async Task LogInputAndOutputParametersOfAsyncMethodTest()
        {
            var res = await _demoClass.DemoLongLastingMethodAsync(10, 25);
            res += await _demoClass.DemoLongLastingMethodAsync(15, 20);
            res += await _demoClass.DemoLongLastingMethodAsync(40, 15);
            res += await _demoClass.DemoLongLastingMethodAsync(40, 10);
            Assert.True(res > 100);
        }

        [Fact]
        public async Task LogInputAndOutputParametersOfAsyncGroupTest()
        {
            var res1 = await _demoClass.DemoLongLastingMethodAsync(10, 25);
            await _demoClass.DemoLongLastingMethodAsync(15, 20);
            await _demoClass.DemoLongLastingMethodAsync(40, 15);
            await _demoClass.DemoLongLastingMethodAsync(40, 10);
            var res = res1;

            Assert.True(res > 100);
        }

        [Fact]
        public async Task LogInputAndOutParametersOfGroupAsyncMethodTest()
        {
            var tasks = new List<Task>
             {
                 _demoClass.DemoLongLastingMethodAsync(10, 25),
                 _demoClass.DemoLongLastingMethodAsync(15, 20),
                 _demoClass.DemoLongLastingMethodAsync(40, 15),
                 _demoClass.DemoLongLastingMethodAsync(40, 10)
             };

            await Task.WhenAll(tasks).ConfigureAwait(false);

            var res = tasks[0].Id;
            Assert.True(res > 0);
        }

        [Fact]
        public async Task LogInAndOutParamsOfGroupAsyncMethodTest()
        {
            var res = await _demoClass.DemoLongLastingMethodAsync(45, 10);
            Assert.Equal(100, res);
        }

        //not sure what this proves?
        [Fact]
        public void LogInAndOutParamsOfCustomMethodTest()
        {
            const int res = 100;
            Task.Run(async () =>
            {
                await _demoClass.DemoCustomMethodAsync(450);
            }).GetAwaiter().GetResult();

            Assert.Equal(100, res);
        }

        [Fact]
        public void MethodWithTaskThrowsExceptionAsyncTest()
        {
            var res = 100;
            Task.Run(async () =>
            {
                await Assert.ThrowsAsync<Exception>(async () => res = await _demoClass.DemoMethodWithTaskThrowsExceptionAsync(450).ConfigureAwait(false));
            }).GetAwaiter().GetResult();

            Assert.Equal(100, res);
        }
    }
}
