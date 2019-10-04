/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sportradar.OddsFeed.SDK.Common.Internal.Log;
using Sportradar.OddsFeed.SDK.Test.Shared;

namespace Sportradar.OddsFeed.SDK.API.Test
{
    /// <summary>
    /// Result are in log file
    /// </summary>
    [TestClass]
    public class LogProxyTest
    {
        private DemoMethods _demoClass;

        [TestInitialize]
        public void Init()
        {
            _demoClass = LogProxyFactory.Create<DemoMethods>(null, m=> m.Name.Contains("D"), LoggerType.ClientInteraction);
        }

        [TestMethod]
        public void LogInputAndOutputParametersOfVoidMethodTest()
        {
            _demoClass.DemoVoidMethod();
            var res = _demoClass.DemoIntMethod(10, 50);
            Assert.IsTrue(res > 100);
        }

        [TestMethod]
        public void LogInputAndOutputParametersOfAsyncCallerMethodTest()
        {
            var res = _demoClass.DemoLongLastingMethodAsyncCaller(45, 10);
            Assert.AreEqual(100, res);
        }

        [TestMethod]
        public void LogInputAndOutputParametersOfAsyncMethodTest()
        {
            var res = _demoClass.DemoLongLastingMethodAsync(10, 25).Result;
            res = res + _demoClass.DemoLongLastingMethodAsync(15, 20).Result;
            res = res + _demoClass.DemoLongLastingMethodAsync(40, 15).Result;
            res = res + _demoClass.DemoLongLastingMethodAsync(40, 10).Result;
            Assert.IsTrue(res > 100);
        }

        [TestMethod]
        public void LogInputAndOutputParametersOfAsyncGroupTest()
        {
            var res = 120;
            Task.Run(async () =>
            {
                var res1 = await _demoClass.DemoLongLastingMethodAsync(10, 25);
                await _demoClass.DemoLongLastingMethodAsync(15, 20);
                await _demoClass.DemoLongLastingMethodAsync(40, 15);
                await _demoClass.DemoLongLastingMethodAsync(40, 10);
                res = res1;
            }).GetAwaiter().GetResult();

            Assert.IsTrue(res > 100);
        }

        [TestMethod]
        public void LogInputAndOutParametersOfGroupAsyncMethodTest()
        {
            var tasks = new List<Task>
            {
                _demoClass.DemoLongLastingMethodAsync(10, 25),
                _demoClass.DemoLongLastingMethodAsync(15, 20),
                _demoClass.DemoLongLastingMethodAsync(40, 15),
                _demoClass.DemoLongLastingMethodAsync(40, 10)
            };
            Task.Run(async () =>
            {
                await Task.WhenAll(tasks).ConfigureAwait(false);
            }).GetAwaiter().GetResult();

            var res = tasks[0].Id;
            Assert.IsTrue(res > 0);
        }

        [TestMethod]
        public void LogInAndOutParamsOfGroupAsyncMethodTest()
        {
            var res = 120;
            Task.Run(async () =>
            {
                var res1 = await _demoClass.DemoLongLastingMethodAsync(45, 10);
                res = res1;
            }).GetAwaiter().GetResult();

            Assert.AreEqual(100, res);
        }

        [TestMethod]
        public void LogInAndOutParamsOfCustomMethodTest()
        {
            const int res = 100;
            Task.Run(async () =>
            {
                await _demoClass.DemoCustomMethodAsync(450);
            }).GetAwaiter().GetResult();

            Assert.AreEqual(100, res);
        }

        [TestMethod]
        [ExpectedException(typeof(Exception), AllowDerivedTypes = true)]
        public void MethodWithTaskThrowsExceptionAsyncTest()
        {
            var res = 100;
            Task.Run(async () =>
            {
               res = await _demoClass.DemoMethodWithTaskThrowsExceptionAsync(450);
            }).GetAwaiter().GetResult();

            Assert.AreEqual(100, res);
        }
    }
}
