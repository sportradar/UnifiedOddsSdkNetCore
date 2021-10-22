/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sportradar.OddsFeed.SDK.Common.Internal;
using Sportradar.OddsFeed.SDK.Test.Shared;

namespace Sportradar.OddsFeed.SDK.Common.Test
{
    [TestClass]
    public class SemaphorePoolTests
    {
        [TestMethod]
        public void SemaphoreCanBeAcquiredWithSameId()
        {
            var pool = new SemaphorePool(1, ExceptionHandlingStrategy.THROW);

            var task1 = pool.AcquireAsync("1");
            var task2 = pool.AcquireAsync("1");

            Thread.Sleep(10);

            var semaphore1 = task1.Result;
            var semaphore2 = task2.Result;
            Assert.AreEqual(semaphore1, semaphore2, "semaphore1 and semaphore2 should be equal");
            Assert.AreEqual(1, pool.SemaphoreHolders.Count);
            Assert.AreEqual(1, pool.AvailableSemaphoreIds.Count);
            Assert.AreEqual("1", pool.AvailableSemaphoreIds.First());
            Assert.AreEqual("1", pool.SemaphoreHolders[0].Id);
            Assert.AreEqual(2, pool.SemaphoreHolders[0].UsageCount);
        }

        [TestMethod]
        public void UsedSemaphoresCannotBeAcquiredWithDifferentId()
        {
            var pool = new SemaphorePool(3, ExceptionHandlingStrategy.THROW);

            var task1 = pool.AcquireAsync("1");
            var task2 = pool.AcquireAsync("2");
            var task3 = pool.AcquireAsync("3");

            var semaphore1 = task1.Result;
            var semaphore2 = task2.Result;
            var semaphore3 = task3.Result;

            Assert.IsNotNull(semaphore1);
            Assert.IsNotNull(semaphore2);
            Assert.IsNotNull(semaphore3);

            var task4 = pool.AcquireAsync("4");
            Thread.Sleep(5);
            Assert.IsFalse(task4.IsCompleted);

            Assert.AreNotEqual(semaphore2, semaphore1, "Semaphore1 and Semaphore2 must not be equal");
            Assert.AreNotEqual(semaphore3, semaphore1, "Semaphore1 and Semaphore3 must not be equal");
            Assert.AreNotEqual(semaphore3, semaphore2, "Semaphore2 and Semaphore3 must not be equal");
        }

        [TestMethod]
        public void UsedSemaphoreCanBeAcquiredWithSameId()
        {
            var pool = new SemaphorePool(1, ExceptionHandlingStrategy.THROW);

            var task1 = pool.AcquireAsync("1");
            var task3 = pool.AcquireAsync("1");

            var semaphore1 = task1.Result;
            var semaphore3 = task3.Result;

            var task2 = pool.AcquireAsync("2");
            Thread.Sleep(5);
            Assert.IsFalse(task2.IsCompleted);

            Assert.IsNotNull(semaphore1);
            Assert.AreEqual(semaphore3, semaphore1, "Semaphore1 and Semaphore3 must be equal");
            Assert.AreEqual(1, pool.SemaphoreHolders.Count);
            Assert.AreEqual(2, pool.AvailableSemaphoreIds.Count);
            Assert.AreEqual("1", pool.AvailableSemaphoreIds.First());
            Assert.AreEqual("1", pool.SemaphoreHolders[0].Id);
            Assert.AreEqual(2, pool.SemaphoreHolders[0].UsageCount);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void ReleasingUnusedSemaphoreThrows()
        {
            var pool = new SemaphorePool(1, ExceptionHandlingStrategy.THROW);
            pool.Release("1");
        }

        [TestMethod]
        public void ReleasedSemaphoreCanBeReacquired()
        {
            var pool = new SemaphorePool(1, ExceptionHandlingStrategy.THROW);

            var task1 = pool.AcquireAsync("1");
            Thread.Sleep(5);
            var task2 = pool.AcquireAsync("2");

            var semaphore1 = task1.Result;
            Assert.IsNotNull(semaphore1);
            Assert.IsFalse(task2.IsCompleted);

            pool.Release("1");
            var semaphore2 = task2.Result;
            Assert.AreEqual(semaphore2, semaphore1, "Semaphore1 and Semaphore2 should be equal");
            Assert.AreEqual(1, pool.SemaphoreHolders.Count);
            Assert.AreEqual(1, pool.AvailableSemaphoreIds.Count);
            Assert.AreEqual("2", pool.AvailableSemaphoreIds.First());
            Assert.AreEqual("2", pool.SemaphoreHolders[0].Id);
            Assert.AreEqual(1, pool.SemaphoreHolders[0].UsageCount);
        }

        [TestMethod]
        public void SemaphoreCanBeAcquiredOnlyAfterAllAcquisitionsAreReleased()
        {
            var pool = new SemaphorePool(1, ExceptionHandlingStrategy.THROW);

            var task11 = pool.AcquireAsync("1");
            var task12 = pool.AcquireAsync("1");
            var task13 = pool.AcquireAsync("1");

            var semaphore1 = task11.Result;
            Assert.IsNotNull(semaphore1);
            Assert.AreEqual(semaphore1, task12.Result, "both semaphores should be equal");
            Assert.AreEqual(semaphore1, task13.Result, "Both semaphores should be equal");
            Assert.AreEqual(1, pool.SemaphoreHolders.Count);
            Assert.AreEqual(1, pool.AvailableSemaphoreIds.Count);
            Assert.AreEqual("1", pool.AvailableSemaphoreIds.First());
            Assert.AreEqual("1", pool.SemaphoreHolders[0].Id);
            Assert.AreEqual(3, pool.SemaphoreHolders[0].UsageCount);

            var task2 = pool.AcquireAsync("2");
            Thread.Sleep(5);
            Assert.IsFalse(task2.IsCompleted);

            pool.Release("1");
            Thread.Sleep(5);
            Assert.IsFalse(task2.IsCompleted);

            pool.Release("1");
            Thread.Sleep(5);
            Assert.IsFalse(task2.IsCompleted);

            pool.Release("1");
            var semaphore2 = task2.Result;

            Assert.AreEqual(semaphore2, semaphore1, "Semaphore1 and Semaphore2 should be equal");
            Assert.AreEqual(1, pool.SemaphoreHolders.Count);
            Assert.AreEqual(1, pool.AvailableSemaphoreIds.Count);
            Assert.AreEqual("2", pool.SemaphoreHolders[0].Id);
            Assert.AreEqual(1, pool.SemaphoreHolders[0].UsageCount);
        }

        [TestMethod]
        public void ComplexUsageTest()
        {
            var pool = new SemaphorePool(2, ExceptionHandlingStrategy.THROW);

            var task11 = pool.AcquireAsync("1");
            var task21 = pool.AcquireAsync("2");
            Thread.Sleep(5);
            var task12 = pool.AcquireAsync("1");
            var task22 = pool.AcquireAsync("2");
            var task3 = pool.AcquireAsync("3");

            Thread.Sleep(5);
            var semaphore11 = task11.Result;
            var semaphore12 = task12.Result;
            var semaphore21 = task21.Result;
            var semaphore22 = task22.Result;

            Assert.IsNotNull(semaphore11);
            Assert.AreEqual(semaphore12, semaphore11, "semaphore11 and semaphore12 should be equal");
            Assert.IsNotNull(semaphore21);
            Assert.AreEqual(semaphore21, semaphore22, "semaphore21 and semaphore22 should be equal");

            Assert.IsFalse(task3.IsCompleted);

            pool.Release("1");
            Thread.Sleep(5);
            Assert.IsFalse(task3.IsCompleted);

            pool.Release("1");
            Thread.Sleep(5);
            Assert.IsTrue(task3.IsCompleted);

            var task4 = pool.AcquireAsync("4");
            Thread.Sleep(5);
            Assert.IsFalse(task4.IsCompleted);

            pool.Release("2");
            Thread.Sleep(5);
            Assert.IsFalse(task4.IsCompleted);

            pool.Release("2");
            Thread.Sleep(5);
            Assert.IsTrue(task4.IsCompleted);

            pool.Release("3");
            pool.Release("4");
            Thread.Sleep(5);

            var task1 = pool.AcquireAsync("1");
            var task2 = pool.AcquireAsync("2");

            Thread.Sleep(5);

            Assert.IsTrue(task1.IsCompleted);
            Assert.IsTrue(task2.IsCompleted);
            pool.Release("1");
            pool.Release("2");
            Assert.AreEqual(2, pool.SemaphoreHolders.Count);
            Assert.AreEqual(0, pool.AvailableSemaphoreIds.Count);
            Assert.IsTrue(pool.SemaphoreHolders.All(a => string.IsNullOrEmpty(a.Id) && a.UsageCount == 0));
        }

        [TestMethod]
        public void NoErrorWhenManyRequestsAreMade()
        {
            var pool = new SemaphorePool(10, ExceptionHandlingStrategy.THROW);
            var tasks = new List<Task>();

            var id = 0;
            for (var i = 0; i < 300; i++)
            {
                tasks.Add(Task.Run(() =>
                {
                    var stringId = id.ToString();
                    id++;
                    var semaphore = pool.AcquireAsync(stringId).Result;
                    Assert.IsNotNull(semaphore);
                    Thread.Sleep(10);
                    pool.Release(stringId);
                }));
            }
            Task.WaitAll(tasks.ToArray());
            Assert.IsTrue(tasks.All(a=>a.IsCompletedSuccessfully));
            Assert.AreEqual(10, pool.SemaphoreHolders.Count);
            Assert.AreEqual(0, pool.AvailableSemaphoreIds.Count);
            Assert.IsNull(pool.SemaphoreHolders[0].Id);
            Assert.IsTrue(pool.SemaphoreHolders.All(a => string.IsNullOrEmpty(a.Id) && a.UsageCount == 0));
        }

        [TestMethod]
        public void NoErrorWhenManySimilarRequestsAreMade()
        {
            var pool = new SemaphorePool(10, ExceptionHandlingStrategy.THROW);
            var tasks = new List<Task>();

            for (var i = 0; i < 300; i++)
            {
                tasks.Add(Task.Run(async () =>
                                   {
                                       var stringId = StaticRandom.S(10);
                                       var semaphore = await pool.AcquireAsync(stringId).ConfigureAwait(false);
                                       Assert.IsNotNull(semaphore);
                                       await semaphore.WaitAsync().ConfigureAwait(false);
                                       Assert.IsTrue(pool.AvailableSemaphoreIds.Contains(stringId));
                                       Thread.Sleep(StaticRandom.I100);
                                       semaphore.ReleaseSafe();
                                       pool.Release(stringId);
                                   }));
            }
            Task.WaitAll(tasks.ToArray());
            Assert.IsTrue(tasks.All(a=>a.IsCompletedSuccessfully));
            Assert.AreEqual(10, pool.SemaphoreHolders.Count);
            Assert.AreEqual(0, pool.AvailableSemaphoreIds.Count);
            Assert.IsNull(pool.SemaphoreHolders[0].Id);
            Assert.IsTrue(pool.SemaphoreHolders.All(a => string.IsNullOrEmpty(a.Id) && a.UsageCount == 0));
        }
    }
}
