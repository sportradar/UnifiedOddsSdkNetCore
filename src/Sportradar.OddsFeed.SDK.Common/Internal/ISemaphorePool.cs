/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using System.Diagnostics.Contracts;
using System.Threading;
using System.Threading.Tasks;
using Sportradar.OddsFeed.SDK.Common.Contracts;

namespace Sportradar.OddsFeed.SDK.Common.Internal
{
    /// <summary>
    /// Defines a pool of <see cref="SemaphoreSlim"/> instances which can be used to synchronize access to a shared resource
    /// </summary>
    [ContractClass(typeof(SemaphorePoolContract))]
    public interface ISemaphorePool : IDisposable
    {
        /// <summary>
        /// Acquires a <see cref="SemaphoreSlim"/> - either one already associated with the specified identifier
        /// or an unused one.
        /// </summary>
        /// <param name="id">The id to be associated with the acquired <see cref="SemaphoreSlim"/> instance.</param>
        /// <returns>A <see cref="Task{SemaphoreSlim}"/> representing an async operation</returns>
        Task<SemaphoreSlim> Acquire(string id);

        /// <summary>
        /// Releases the <see cref="SemaphoreSlim"/> previously acquired with the same id
        /// </summary>
        /// <param name="id">The Id which was used to acquire the semaphore being released .</param>
        /// <exception cref="System.ArgumentException"></exception>
        void Release(string id);
    }
}