/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sportradar.OddsFeed.SDK.Common.Internal;

namespace Sportradar.OddsFeed.SDK.Common.Test
{
    [TestClass]
    public class SemaphorePoolTests
    {
        [TestMethod]
        public void semaphore_can_be_acquired_with_same_id()
        {
            var pool = new SemaphorePool(1);


            var task1 = pool.Acquire("1");
            var task2 = pool.Acquire("1");

            Thread.Sleep(10);

            var semaphore1 = task1.Result;
            var semaphore2 = task2.Result;
            Assert.AreEqual(semaphore1, semaphore2, "semaphore1 and semaphore2 should be equal");
        }

        [TestMethod]
        public void used_semaphores_cannot_be_acquired_with_different_id()
        {
            var pool = new SemaphorePool(3);

            var task1 = pool.Acquire("1");
            var task2 = pool.Acquire("2");
            var task3 = pool.Acquire("3");

            var semaphore1 = task1.Result;
            var semaphore2 = task2.Result;
            var semaphore3 = task3.Result;

            Assert.IsNotNull(semaphore1);
            Assert.IsNotNull(semaphore2);
            Assert.IsNotNull(semaphore3);

            var task4 = pool.Acquire("4");
            Thread.Sleep(5);
            Assert.IsFalse(task4.IsCompleted);

            Assert.AreNotEqual(semaphore2, semaphore1, "Semaphore1 and Semaphore2 must not be equal");
            Assert.AreNotEqual(semaphore3, semaphore1, "Semaphore1 and Semaphore3 must not be equal");
            Assert.AreNotEqual(semaphore3, semaphore2, "Semaphore2 and Semaphore3 must not be equal");
        }

        [TestMethod]
        public void used_semaphore_can_be_acquired_with_same_id()
        {
            var pool = new SemaphorePool(1);

            var task1 = pool.Acquire("1");
            var task3 = pool.Acquire("1");

            var semaphore1 = task1.Result;
            var semaphore3 = task3.Result;

            var task2 = pool.Acquire("2");
            Thread.Sleep(5);
            Assert.IsFalse(task2.IsCompleted);

            Assert.IsNotNull(semaphore1);
            Assert.AreEqual(semaphore3, semaphore1, "Semaphore1 and Semaphore3 must be equal");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void releasing_unused_semaphore_throws()
        {
            var pool = new SemaphorePool(1);
            pool.Release("1");
        }

        [TestMethod]
        public void released_semaphore_can_be_re_acquired()
        {
            var pool = new SemaphorePool(1);

            var task1 = pool.Acquire("1");
            Thread.Sleep(5);
            var task2 = pool.Acquire("2");

            var semaphore1 = task1.Result;
            Assert.IsNotNull(semaphore1);
            Assert.IsFalse(task2.IsCompleted);

            pool.Release("1");
            var semaphore2 = task2.Result;
            Assert.AreEqual(semaphore2, semaphore1, "Semaphore1 and Semaphore2 should be equal");
        }

        [TestMethod]
        public void semaphore_can_be_acquired_only_after_all_acquisitions_are_released()
        {
            var pool = new SemaphorePool(1);

            var task11 = pool.Acquire("1");
            var task12 = pool.Acquire("1");
            var task13 = pool.Acquire("1");


            var semaphore1 = task11.Result;
            Assert.IsNotNull(semaphore1);
            Assert.AreEqual(semaphore1, task12.Result, "both semaphores should be equal");
            Assert.AreEqual(semaphore1, task13.Result, "Both semaphores should be equal");

            var task2 = pool.Acquire("2");
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
        }

        [TestMethod]
        public void complex_usage_test()
        {
            var pool = new SemaphorePool(2);

            var task11 = pool.Acquire("1");
            var task21 = pool.Acquire("2");
            Thread.Sleep(5);
            var task12 = pool.Acquire("1");
            var task22 = pool.Acquire("2");
            var task3 = pool.Acquire("3");


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

            var task4 = pool.Acquire("4");
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

            var task1 = pool.Acquire("1");
            var task2 = pool.Acquire("2");

            Thread.Sleep(5);

            Assert.IsTrue(task1.IsCompleted);
            Assert.IsTrue(task2.IsCompleted);
        }

        [TestMethod]
        public void no_error_when_many_requests_are_made()
        {
            var pool = new SemaphorePool(15);
            var tasks = new List<Task>();

            var id = 0;
            for (var i = 0; i < 300; i++)
            {
                tasks.Add(Task.Run(() =>
                {
                    var stringId = id.ToString();
                    id++;
                    var semaphore = pool.Acquire(stringId).Result;
                    Assert.IsNotNull(semaphore);
                    Thread.Sleep(10);
                    pool.Release(stringId);
                }));
            }
            Task.WaitAll(tasks.ToArray());
        }
    }
}
