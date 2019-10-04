/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Sportradar.OddsFeed.SDK.DemoProject.Utils
{
    /// <summary>
    /// A class used keep track of the ongoing asynchronous operations
    /// </summary>
    internal class TaskProcessor
    {
        /// <summary>
        /// The number of currently running tasks
        /// </summary>
        private long _runningTaskCount;

        /// <summary>
        /// A <see cref="AutoResetEvent"/> used to wait for un-completed tasks
        /// </summary>
        private readonly AutoResetEvent _autoReset = new AutoResetEvent(true);

        /// <summary>
        /// A <see cref="TimeSpan"/> defining the max wait time
        /// </summary>
        private readonly TimeSpan _maxWaitTime;

        /// <summary>
        /// Initializes a new instance of the <see cref="TaskProcessor"/> class
        /// </summary>
        /// <param name="maxWaitTime"> A <see cref="TimeSpan"/> defining the max wait time</param>
        public TaskProcessor(TimeSpan maxWaitTime)
        {
            Contract.Requires(maxWaitTime > TimeSpan.Zero);

            _maxWaitTime = maxWaitTime;
        }

        /// <summary>
        /// Defined field invariants needed by code contracts
        /// </summary>
        [ContractInvariantMethod]
        private void ObjectInvariant()
        {
            Contract.Invariant(_autoReset != null);
            Contract.Invariant(_maxWaitTime != null);
        }

        /// <summary>
        /// Waits for the provided <see cref="Task{T}"/> to complete and returns it's result
        /// </summary>
        /// <typeparam name="T">The type returned by the task</typeparam>
        /// <param name="task">A <see cref="Task{T}"/> on from which to get the result</param>
        /// <returns>A <see cref="T"/> representing the result of the task</returns>
        public T GetTaskResult<T>(Task<T> task)
        {
            Contract.Requires(task != null);

            Interlocked.Increment(ref _runningTaskCount);
            try
            {
                return task.Result;
            }
            catch (AggregateException ex)
            {
                throw ex.InnerExceptions.First();
            }
            finally
            {
                if (Interlocked.Decrement(ref _runningTaskCount) == 0)
                {
                    _autoReset.Set();
                }
                else
                {
                    _autoReset.Reset();
                }
            }
        }

        /// <summary>
        /// Waits for all un-finished tasks
        /// </summary>
        /// <returns>True if all unfinished tasks completed within the allocated time. Otherwise false</returns>
        public bool WaitForTasks()
        {
            return _autoReset.WaitOne(_maxWaitTime);
        }
    }
}
