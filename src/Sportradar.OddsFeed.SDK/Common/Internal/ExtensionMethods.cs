/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Dawn;

namespace Sportradar.OddsFeed.SDK.Common.Internal
{
    /// <summary>
    /// Defines extension methods used by the SDK
    /// </summary>
    internal static class ExtensionMethods
    {
        /// <summary>
        /// Gets a <see cref="string"/> representation of the provided <see cref="Stream"/>
        /// </summary>
        /// <param name="stream">The <see cref="Stream"/> whose content to get.</param>
        /// <returns>A <see cref="string"/> representation of the <see cref="Stream"/> content.</returns>
        public static string GetData(this Stream stream)
        {
            Guard.Argument(stream, nameof(stream)).NotNull();

            using var memoryStream = new MemoryStream();
            stream.CopyTo(memoryStream);
            return Encoding.UTF8.GetString(memoryStream.ToArray());
        }

        /// <summary>
        /// Enters the <see cref="SemaphoreSlim"/> by invoking <see cref="SemaphoreSlim.Wait()"/> in a safe manner that will
        /// not throw an exception if the semaphore is already disposed
        /// </summary>
        /// <param name="semaphore">The <see cref="SemaphoreSlim"/> on which to wait</param>
        /// <returns>True if entering the semaphore succeeded (e.g. instance was not yet disposed); otherwise false</returns>
        public static bool WaitSafe(this SemaphoreSlim semaphore)
        {
            Guard.Argument(semaphore, nameof(semaphore)).NotNull();

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
            Guard.Argument(semaphore, nameof(semaphore)).NotNull();

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
            Guard.Argument(semaphore, nameof(semaphore)).NotNull();

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
