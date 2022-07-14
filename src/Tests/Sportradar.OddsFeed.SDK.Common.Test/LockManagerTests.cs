using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sportradar.OddsFeed.SDK.Common.Internal;
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Sportradar.OddsFeed.SDK.Common.Test
{
    [TestClass]
    public class LockManagerTests
    {
        private LockManager _lockManager;

        [TestInitialize]
        public void Init()
        {
            _lockManager = new LockManager(new ConcurrentDictionary<string, DateTime>(), TimeSpan.FromSeconds(30), TimeSpan.FromMilliseconds(200));
        }

        [TestMethod]
        public void WaitTest()
        {
            var id = "some_id";

            var stopWatch = Stopwatch.StartNew();
            Task.Run(() =>
                     {
                         Task.Delay(TimeSpan.FromSeconds(5)).Wait();
                         _lockManager.Release(id);
                     });
            _lockManager.Wait(id);
            _lockManager.Wait(id);
            stopWatch.Stop();
            Assert.IsTrue(stopWatch.Elapsed > TimeSpan.FromSeconds(5));
        }

        [TestMethod]
        public void WaitTillTimeoutTest()
        {
            var id = "some_id";

            var stopWatch = Stopwatch.StartNew();
            _lockManager.Wait(id);
            _lockManager.Wait(id);
            stopWatch.Stop();
            Assert.IsTrue(stopWatch.Elapsed > TimeSpan.FromSeconds(5));
        }
    }
}
