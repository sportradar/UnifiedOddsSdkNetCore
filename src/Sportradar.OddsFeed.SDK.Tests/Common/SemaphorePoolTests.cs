/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Common.Internal;
using Xunit;
using Xunit.Abstractions;

namespace Sportradar.OddsFeed.SDK.Tests.Common
{
    public class SemaphorePoolTests
    {
        private readonly ITestOutputHelper _testOutputHelper;

        public SemaphorePoolTests(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
        }

        [Fact]
        public void SemaphoreCanBeAcquiredWithSameId()
        {
            var pool = new SemaphorePool(1, ExceptionHandlingStrategy.THROW);

            var task1 = pool.AcquireAsync("1");
            var task2 = pool.AcquireAsync("1");

            Task.Delay(10).GetAwaiter().GetResult();

            var semaphore1 = task1.GetAwaiter().GetResult();
            var semaphore2 = task2.GetAwaiter().GetResult();
            Assert.Equal(semaphore1, semaphore2);
            Assert.Single(pool.SemaphoreHolders);
            Assert.Single(pool.AvailableSemaphoreIds);
            Assert.Equal("1", pool.AvailableSemaphoreIds.First());
            Assert.Equal("1", pool.SemaphoreHolders[0].Id);
            Assert.Equal(2, pool.SemaphoreHolders[0].UsageCount);
        }

        [Fact]
        public void UsedSemaphoresCannotBeAcquiredWithDifferentId()
        {
            var pool = new SemaphorePool(3, ExceptionHandlingStrategy.THROW);

            var task1 = pool.AcquireAsync("1");
            var task2 = pool.AcquireAsync("2");
            var task3 = pool.AcquireAsync("3");

            var semaphore1 = task1.GetAwaiter().GetResult();
            var semaphore2 = task2.GetAwaiter().GetResult();
            var semaphore3 = task3.GetAwaiter().GetResult();

            Assert.NotNull(semaphore1);
            Assert.NotNull(semaphore2);
            Assert.NotNull(semaphore3);

            var task4 = pool.AcquireAsync("4");
            Task.Delay(5).GetAwaiter().GetResult();
            Assert.False(task4.IsCompleted);

            Assert.NotEqual(semaphore2, semaphore1);
            Assert.NotEqual(semaphore3, semaphore1);
            Assert.NotEqual(semaphore3, semaphore2);
        }

        [Fact]
        public void UsedSemaphoreCanBeAcquiredWithSameId()
        {
            var pool = new SemaphorePool(1, ExceptionHandlingStrategy.THROW);

            var task1 = pool.AcquireAsync("1");
            var task3 = pool.AcquireAsync("1");

            var semaphore1 = task1.GetAwaiter().GetResult();
            var semaphore3 = task3.GetAwaiter().GetResult();

            var task2 = pool.AcquireAsync("2");
            Task.Delay(5).GetAwaiter().GetResult();
            Assert.False(task2.IsCompleted);

            Assert.NotNull(semaphore1);
            Assert.Equal(semaphore3, semaphore1);
            Assert.Single(pool.SemaphoreHolders);
            Assert.Equal(2, pool.AvailableSemaphoreIds.Count);
            Assert.Equal("1", pool.AvailableSemaphoreIds.First());
            Assert.Equal("1", pool.SemaphoreHolders[0].Id);
            Assert.Equal(2, pool.SemaphoreHolders[0].UsageCount);
        }

        [Fact]
        public void ReleasingUnusedSemaphoreThrows()
        {
            var pool = new SemaphorePool(1, ExceptionHandlingStrategy.THROW);
            Action action = () => pool.Release("1");
            action.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void ReleasedSemaphoreCanBeReacquired()
        {
            var pool = new SemaphorePool(1, ExceptionHandlingStrategy.THROW);

            var task1 = pool.AcquireAsync("1");
            Task.Delay(5).GetAwaiter().GetResult();
            var task2 = pool.AcquireAsync("2");

            var semaphore1 = task1.GetAwaiter().GetResult();
            Assert.NotNull(semaphore1);
            Assert.False(task2.IsCompleted);

            pool.Release("1");
            var semaphore2 = task2.GetAwaiter().GetResult();
            Assert.Equal(semaphore2, semaphore1);
            Assert.Single(pool.SemaphoreHolders);
            Assert.Single(pool.AvailableSemaphoreIds);
            Assert.Equal("2", pool.AvailableSemaphoreIds.First());
            Assert.Equal("2", pool.SemaphoreHolders[0].Id);
            Assert.Equal(1, pool.SemaphoreHolders[0].UsageCount);
        }

        [Fact]
        public void SemaphoreCanBeAcquiredOnlyAfterAllAcquisitionsAreReleased()
        {
            var pool = new SemaphorePool(1, ExceptionHandlingStrategy.THROW);

            var task11 = pool.AcquireAsync("1");
            var task12 = pool.AcquireAsync("1");
            var task13 = pool.AcquireAsync("1");

            var semaphore1 = task11.GetAwaiter().GetResult();
            Assert.NotNull(semaphore1);
            Assert.Equal(semaphore1, task12.GetAwaiter().GetResult());
            Assert.Equal(semaphore1, task13.GetAwaiter().GetResult());
            Assert.Single(pool.SemaphoreHolders);
            Assert.Single(pool.AvailableSemaphoreIds);
            Assert.Equal("1", pool.AvailableSemaphoreIds.First());
            Assert.Equal("1", pool.SemaphoreHolders[0].Id);
            Assert.Equal(3, pool.SemaphoreHolders[0].UsageCount);

            var task2 = pool.AcquireAsync("2");
            Task.Delay(5).GetAwaiter().GetResult();
            Assert.False(task2.IsCompleted);

            pool.Release("1");
            Task.Delay(5).GetAwaiter().GetResult();
            Assert.False(task2.IsCompleted);

            pool.Release("1");
            Task.Delay(5).GetAwaiter().GetResult();
            Assert.False(task2.IsCompleted);

            pool.Release("1");
            var semaphore2 = task2.GetAwaiter().GetResult();

            Assert.Equal(semaphore2, semaphore1);
            Assert.Single(pool.SemaphoreHolders);
            Assert.Single(pool.AvailableSemaphoreIds);
            Assert.Equal("2", pool.SemaphoreHolders[0].Id);
            Assert.Equal(1, pool.SemaphoreHolders[0].UsageCount);
        }

        // TODO this test appears to be unreliable in linux environment
        //[Fact]
        public void ComplexUsage()
        {
            var pool = new SemaphorePool(2, ExceptionHandlingStrategy.THROW);

            var task11 = pool.AcquireAsync("1");
            var task21 = pool.AcquireAsync("2");
            Task.Delay(5).GetAwaiter().GetResult();
            var task12 = pool.AcquireAsync("1");
            var task22 = pool.AcquireAsync("2");
            var task3 = pool.AcquireAsync("3");

            Task.Delay(5).GetAwaiter().GetResult();
            var semaphore11 = task11.GetAwaiter().GetResult();
            var semaphore12 = task12.GetAwaiter().GetResult();
            var semaphore21 = task21.GetAwaiter().GetResult();
            var semaphore22 = task22.GetAwaiter().GetResult();

            Assert.NotNull(semaphore11);
            Assert.Equal(semaphore12, semaphore11);
            Assert.NotNull(semaphore21);
            Assert.Equal(semaphore21, semaphore22);

            Assert.False(task3.IsCompleted);

            pool.Release("1");
            Task.Delay(5).GetAwaiter().GetResult();
            Assert.False(task3.IsCompleted);

            pool.Release("1");
            Task.Delay(5).GetAwaiter().GetResult();
            Assert.True(task3.IsCompleted);

            var task4 = pool.AcquireAsync("4");
            Task.Delay(5).GetAwaiter().GetResult();
            Assert.False(task4.IsCompleted);

            pool.Release("2");
            Task.Delay(5).GetAwaiter().GetResult();
            Assert.False(task4.IsCompleted);

            pool.Release("2");
            Task.Delay(5).GetAwaiter().GetResult();
            Assert.True(task4.IsCompleted);

            pool.Release("3");
            pool.Release("4");
            Task.Delay(5).GetAwaiter().GetResult();

            var task1 = pool.AcquireAsync("1");
            var task2 = pool.AcquireAsync("2");

            Task.Delay(5).GetAwaiter().GetResult();

            Assert.True(task1.IsCompleted);
            Assert.True(task2.IsCompleted);
            pool.Release("1");
            pool.Release("2");
            Assert.Equal(2, pool.SemaphoreHolders.Count);
            Assert.Empty(pool.AvailableSemaphoreIds);
            Assert.True(pool.SemaphoreHolders.All(a => string.IsNullOrEmpty(a.Id) && a.UsageCount == 0));
        }

        //TODO
        //[Fact(Timeout = 30000)]
        public async Task NoErrorWhenManyRequestsAreMade()
        {
            var stopWatch = Stopwatch.StartNew();
            var pool = new SemaphorePool(20, ExceptionHandlingStrategy.THROW);
            var tasks = new List<Task>();

            for (var i = 0; i < 300; i++)
            {
                var id = i.ToString();
                tasks.Add(Task.Run(() => GetAndRunSemaphoreAction(pool, id, stopWatch).ConfigureAwait(false)));
            }
            await Task.WhenAll(tasks.ToArray()).ConfigureAwait(false);
            Assert.True(tasks.All(a => a.IsCompletedSuccessfully));
            Assert.Equal(20, pool.SemaphoreHolders.Count);
            Assert.Empty(pool.AvailableSemaphoreIds);
            Assert.Null(pool.SemaphoreHolders[0].Id);
            Assert.True(pool.SemaphoreHolders.All(a => string.IsNullOrEmpty(a.Id) && a.UsageCount == 0));
        }

        //TODO 
        //[Fact]
        public async Task NoErrorWhenManySimilarRequestsAreMade()
        {
            var stopWatch = Stopwatch.StartNew();
            var pool = new SemaphorePool(10, ExceptionHandlingStrategy.THROW);
            var tasks = new List<Task>();

            for (var i = 0; i < 300; i++)
            {
                var id = StaticRandom.S(10);
                tasks.Add(Task.Run(() => GetAndRunSemaphoreAction(pool, id, stopWatch).ConfigureAwait(false)));
            }
            await Task.WhenAll(tasks.ToArray()).ConfigureAwait(false);
            Assert.True(tasks.All(a => a.IsCompletedSuccessfully));
            Assert.Equal(10, pool.SemaphoreHolders.Count);
            Assert.Empty(pool.AvailableSemaphoreIds);
            Assert.Null(pool.SemaphoreHolders[0].Id);
            Assert.True(pool.SemaphoreHolders.All(a => string.IsNullOrEmpty(a.Id) && a.UsageCount == 0));
        }

        private async Task<string> GetAndRunSemaphoreAction(SemaphorePool pool, string semaphoreKey, Stopwatch stopWatch)
        {
            var semaphore = await pool.AcquireAsync(semaphoreKey).ConfigureAwait(false);
            Assert.NotNull(semaphore);
            await semaphore.WaitAsync().ConfigureAwait(false);
            var start = stopWatch.ElapsedMilliseconds;
            _testOutputHelper.WriteLine($"{start} Semaphore for key {semaphoreKey} acquired - {pool.SemaphoreHolders.Count}/{pool.AvailableSemaphoreIds.Count}");
            await Task.Delay(10).ConfigureAwait(false);
            semaphore.ReleaseSafe();
            pool.Release(semaphoreKey);
            var stop = stopWatch.ElapsedMilliseconds;
            _testOutputHelper.WriteLine($"{stop} Semaphore for key {semaphoreKey} released - {stop - start} ms");
            return semaphoreKey;
        }
    }
}
