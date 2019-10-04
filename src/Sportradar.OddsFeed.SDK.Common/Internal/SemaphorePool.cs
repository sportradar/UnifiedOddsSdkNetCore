/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Threading;
using System.Threading.Tasks;

namespace Sportradar.OddsFeed.SDK.Common.Internal
{
    /// <summary>
    /// A <see cref="ISemaphorePool"/> implementation
    /// </summary>
    public class SemaphorePool : ISemaphorePool
    {
        //private readonly ILog _executionLog = SdkLoggerFactory.GetLogger(typeof(SemaphorePool));

        /// <summary>
        /// A <see cref="List{T}"/> containing pool's semaphores
        /// </summary>
        private readonly List<SemaphoreHolder> _semaphores;

        /// <summary>
        /// A <see cref="List{T}"/> containing ids of resources currently available
        /// </summary>
        private readonly List<string> _availableSemaphoreIds;

        /// <summary>
        /// A <see cref="Semaphore"/> used to block the treads waiting for <see cref="SemaphoreSlim"/> instances to become available
        /// </summary>
        private readonly Semaphore _syncSemaphore;

        /// <summary>
        /// A <see cref="SpinWait"/> used to spin while waiting for the resource to become available
        /// </summary>
        private readonly SpinWait _spinWait;

        /// <summary>
        /// The <see cref="object"/> used to ensure thread-safety
        /// </summary>
        private readonly object _syncObject;

        /// <summary>
        /// A value indicating whether the current instance has already been disposed
        /// </summary>
        private bool _disposed;

        /// <summary>
        /// Initializes a new instance of the <see cref="SemaphorePool"/> class
        /// </summary>
        /// <param name="count">The number of <see cref="SemaphoreSlim"/> instances to be created in the pool</param>
        public SemaphorePool(int count)
        {
            _semaphores = new List<SemaphoreHolder>();
            _availableSemaphoreIds = new List<string>();
            for (var i = 0; i < count; i++)
            {
                _semaphores.Add(new SemaphoreHolder(new SemaphoreSlim(1)));
            }
            _syncSemaphore = new Semaphore(count, count);
            _spinWait = new SpinWait();
            _syncObject = new object();
        }

        /// <summary>
        /// Executes the actual acquirement
        /// </summary>
        /// <param name="id">The id under which to acquire the semaphore</param>
        /// <returns>The acquired <see cref="SemaphoreSlim"/></returns>
        /// <exception cref="InvalidOperationException">Semaphore granted entry, but there are no SemaphoreSlim objects available</exception>
        private SemaphoreSlim AcquireInternal(string id)
        {
            //_executionLog.Debug($"Waiting to enter semaphore for id={id}");
            _syncSemaphore.WaitOne();
            //_executionLog.Debug($"Semaphore granted entry to id={id}");
            lock (_syncObject)
            {
                foreach (var holder in _semaphores)
                {
                    if (holder.Id == id)
                    {
                        //_executionLog.Debug($"Returning resource for request with id={id}");
                        return holder.Semaphore;
                    }

                    if (holder.UsageCount == 0)
                    {
                        holder.Acquire();
                        holder.Id = id;
                        //_executionLog.Debug($"Returning resource for request with id={id}");
                        return holder.Semaphore;
                    }
                }
                throw new InvalidOperationException("Semaphore granted entry, but there are no SemaphoreSlim objects available");
            }
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources</param>
        protected void Dispose(bool disposing)
        {
            if (!disposing || _disposed)
            {
                return;
            }
            _disposed = true;
            lock (_syncObject)
            {
                _syncSemaphore.Dispose();
                foreach (var holder in _semaphores)
                {
                    holder.Semaphore.Release();
                }
            }
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
        }

        /// <summary>
        /// Acquires a <see cref="SemaphoreSlim"/> - either one already associated with the specified identifier or an unused one
        /// </summary>
        /// <param name="id">The id to be associated with the acquired <see cref="SemaphoreSlim"/> instance</param>
        /// <returns>A <see cref="Task{SemaphoreSlim}"/> representing an async operation</returns>
        public Task<SemaphoreSlim> Acquire(string id)
        {
            var idFound = false;
            //_executionLog.Debug($"Entering lock for request with id={id}");
            lock (_syncObject)
            {
                //_executionLog.Debug($"Lock for request with id={id} entered");
                if (_availableSemaphoreIds.Contains(id))
                {
                    idFound = true;
                }
                else
                {
                    _availableSemaphoreIds.Add(id);
                }
            }

            if (!idFound)
            {
                //_executionLog.Debug($"Creating new thread for creation of resource for id={id}");
                return Task.Run(() => AcquireInternal(id));
            }

            while(true)
            {
                lock (_syncObject)
                {
                    foreach (var holder in _semaphores)
                    {
                        if (holder.Id != id)
                        {
                            continue;
                        }
                        holder.Acquire();
                        return Task.FromResult(holder.Semaphore);
                    }
                }
                //_executionLog.Debug($"Spinning while waiting for the semaphore for id={id}");
                _spinWait.SpinOnce();
            }
        }

        /// <summary>
        /// Releases the <see cref="SemaphoreSlim"/> previously acquired with the same id
        /// </summary>
        /// <param name="id">The Id which was used to acquire the semaphore being released </param>
        /// <exception cref="System.ArgumentException"></exception>
        public void Release(string id)
        {
            lock (_syncObject)
            {
                foreach (var holder in _semaphores)
                {
                    if (holder.Id != id)
                    {
                        continue;
                    }

                    if (holder.Release() == 0)
                    {
                        holder.Id = null;
                        _availableSemaphoreIds.Remove(id);
                        //_executionLog.Debug($"Releasing the resource associated with id={id}");
                        _syncSemaphore.Release();
                    }
                    return;
                }
            }
            throw new ArgumentException($"No semaphores are aquired with Id:{id}", nameof(id));
        }

        /// <summary>
        /// A holder used to wrap <see cref="SemaphoreSlim"/> and additional information
        /// </summary>
        internal class SemaphoreHolder
        {
            /// <summary>
            /// Gets the number of the objects which have acquired the associated semaphore
            /// </summary>
            public int UsageCount { get; private set; }

            /// <summary>
            /// The <see cref="SemaphoreSlim"/>
            /// </summary>
            public SemaphoreSlim Semaphore { get; }

            /// <summary>
            /// The id associated with the current semaphore
            /// </summary>
            public string Id { get; set; }

            /// <summary>
            /// Initializes a new instance of the <see cref="SemaphoreHolder"/> class
            /// </summary>
            /// <param name="semaphore">The semaphore</param>
            public SemaphoreHolder(SemaphoreSlim semaphore)
            {
                Contract.Requires(semaphore != null);

                Semaphore = semaphore;
            }

            /// <summary>
            /// Increases the <see cref="UsageCount"/> of the current instance
            /// </summary>
            public long Acquire()
            {
                return ++UsageCount;
            }

            /// <summary>
            /// Decreases the <see cref="UsageCount"/> of the current instance
            /// </summary>
            public long Release()
            {
                return --UsageCount;
            }
        }
    }
}
