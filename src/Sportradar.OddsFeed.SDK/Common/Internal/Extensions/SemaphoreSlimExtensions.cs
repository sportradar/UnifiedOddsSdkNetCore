// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using System.Threading;
using System.Threading.Tasks;

namespace Sportradar.OddsFeed.SDK.Common.Internal.Extensions
{
    /// <summary>
    /// Defines extension methods used by the SDK
    /// </summary>
    internal static class SemaphoreSlimExtensions
    {
        /// <summary>
        /// Enters the <see cref="SemaphoreSlim"/> by invoking <see cref="SemaphoreSlim.Wait()"/> in a safe manner that will
        /// not throw an exception if the semaphore is already disposed
        /// </summary>
        /// <param name="semaphore">The <see cref="SemaphoreSlim"/> on which to wait</param>
        /// <returns>True if entering the semaphore succeeded (e.g. instance was not yet disposed); otherwise false</returns>
        public static bool WaitSafe(this SemaphoreSlim semaphore)
        {
            if (semaphore == null)
            {
                throw new ArgumentNullException(nameof(semaphore));
            }

            try
            {
                semaphore.Wait();
                return true;
            }
            catch (ObjectDisposedException)
            {
                return false;
            }
        }

        /// <summary>
        /// Asynchronously enters the <see cref="SemaphoreSlim"/> by invoking <see cref="SemaphoreSlim.WaitAsync()"/> in a safe manner that will
        /// not throw an exception if the semaphore is already disposed
        /// </summary>
        /// <param name="semaphore">The <see cref="SemaphoreSlim"/> on which to wait</param>
        /// <returns>True if entering the semaphore succeeded (e.g. instance was not yet disposed); otherwise false</returns>
        public static async Task<bool> WaitAsyncSafe(this SemaphoreSlim semaphore)
        {
            if (semaphore == null)
            {
                throw new ArgumentNullException(nameof(semaphore));
            }

            try
            {
                await semaphore.WaitAsync().ConfigureAwait(false);
                return true;
            }
            catch (ObjectDisposedException)
            {
                return false;
            }
        }

        /// <summary>
        /// Asynchronously enters the <see cref="SemaphoreSlim"/> by invoking <see cref="SemaphoreSlim.WaitAsync()"/> in a safe manner that will
        /// not throw an exception if the semaphore is already disposed
        /// </summary>
        /// <param name="semaphore">The <see cref="SemaphoreSlim"/> on which to wait</param>
        /// <param name="timeout">The timeout for the semaphore</param>
        /// <returns>True if entering the semaphore succeeded (e.g. instance was not yet disposed); otherwise false</returns>
        public static async Task<bool> WaitAsyncSafe(this SemaphoreSlim semaphore, TimeSpan timeout)
        {
            if (semaphore == null)
            {
                throw new ArgumentNullException(nameof(semaphore));
            }

            try
            {
                if (timeout.TotalSeconds > 0)
                {
                    await semaphore.WaitAsync(timeout).ConfigureAwait(false);
                }
                else
                {
                    await semaphore.WaitAsync().ConfigureAwait(false);
                }
                return true;
            }
            catch (ObjectDisposedException)
            {
                return false;
            }
        }

        /// <summary>
        /// Releases the <see cref="SemaphoreSlim"/> by invoking <see cref="SemaphoreSlim.Release()"/> in a safe manner that will
        /// not throw an exception if the semaphore is already disposed
        /// </summary>
        /// <param name="semaphore">The <see cref="SemaphoreSlim"/> to be released</param>
        /// <returns>True if releasing the semaphore succeeded (e.g. instance was not yet disposed); otherwise false</returns>
        public static bool ReleaseSafe(this SemaphoreSlim semaphore)
        {
            try
            {
                semaphore?.Release();
                return true;
            }
            catch (ObjectDisposedException)
            {
                return false;
            }
        }
    }
}
