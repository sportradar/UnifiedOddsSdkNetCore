/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using System.Diagnostics.Contracts;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Sportradar.OddsFeed.SDK.Common.Internal
{
    /// <summary>
    /// Defines extension methods used by the SDK
    /// </summary>
    public static class ExtensionMethods
    {
        /// <summary>
        /// Gets a <see cref="string"/> representation of the provided <see cref="Stream"/>
        /// </summary>
        /// <param name="stream">The <see cref="Stream"/> whose content to get.</param>
        /// <returns>A <see cref="string"/> representation of the <see cref="Stream"/> content.</returns>
        public static string GetData(this Stream stream)
        {
            Contract.Requires(stream != null);
            Contract.Ensures(Contract.Result<string>() != null);

            using (var memoryStream = new MemoryStream())
            {
                stream.CopyTo(memoryStream);
                return Encoding.UTF8.GetString(memoryStream.ToArray());
            }
        }

        /// <summary>
        /// Enters the <see cref="SemaphoreSlim"/> by invoking <see cref="SemaphoreSlim.Wait()"/> in a safe manner that will
        /// not throw an exception if the semaphore is already disposed
        /// </summary>
        /// <param name="semaphore">The <see cref="SemaphoreSlim"/> on which to wait</param>
        /// <returns>True if entering the semaphore succedded (e.g. isntance was not yet disposed); otherwise false</returns>
        public static bool WaitSafe(this SemaphoreSlim semaphore)
        {
            Contract.Requires(semaphore != null);
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
        /// <returns>True if entering the semaphore succedded (e.g. isntance was not yet disposed); otherwise false</returns>

        public static async Task<bool> WaitAsyncSafe(this SemaphoreSlim semaphore)
        {
            Contract.Requires(semaphore != null);
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
        /// Releases the <see cref="SemaphoreSlim"/> by invoking <see cref="SemaphoreSlim.Release()"/> in a safe manner that will
        /// not throw an exception if the semaphore is already disposed
        /// </summary>
        /// <param name="semaphore">The <see cref="SemaphoreSlim"/> to be released</param>
        /// <returns>True if releasing the semaphore succedded (e.g. isntance was not yet disposed); otherwise false</returns>

        public static bool ReleaseSafe(this SemaphoreSlim semaphore)
        {
            Contract.Requires(semaphore != null);
            try
            {
                semaphore.Release();
                return true;
            }
            catch (ObjectDisposedException)
            {
                return false;
            }
        }

    }
}
