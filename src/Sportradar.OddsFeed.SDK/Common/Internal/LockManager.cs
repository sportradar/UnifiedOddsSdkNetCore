// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Sportradar.OddsFeed.SDK.Common.Internal
{
    internal class LockManager
    {
        private readonly ILogger _log;
        private readonly ConcurrentDictionary<string, DateTime> _uniqueItems;
        private readonly TimeSpan _lockTimeout;
        private readonly TimeSpan _lockSleep;
        private bool _waitAll;

        public LockManager(ConcurrentDictionary<string, DateTime> uniqueItems, TimeSpan lockTimeout, TimeSpan lockSleep, ILogger logger)
        {
            _uniqueItems = uniqueItems ?? new ConcurrentDictionary<string, DateTime>();
            _lockTimeout = lockTimeout.TotalSeconds < 1 ? TimeSpan.FromSeconds(60) : lockTimeout;
            _lockSleep = lockSleep.TotalMilliseconds < 10 ? TimeSpan.FromMilliseconds(50) : lockSleep;
            _waitAll = false;
            _log = logger;
        }

        public void Wait()
        {
            _waitAll = true;
        }

        public void Wait(string key)
        {
            LockInternal(key, _uniqueItems, _lockTimeout, _lockSleep);
        }

        public async Task WaitAsync(string key)
        {
            await LockInternalAsync(key, _uniqueItems, _lockTimeout, _lockSleep);
        }

        public void Release()
        {
            _waitAll = false;
        }

        public void Release(string key)
        {
            ReleaseInternal(key, _uniqueItems, _lockTimeout, _lockSleep);
        }

        public async Task ReleaseAsync(string key)
        {
            await ReleaseInternalAsync(key, _uniqueItems, _lockTimeout, _lockSleep);
        }

        private void LockInternal(string key, ConcurrentDictionary<string, DateTime> uniqueItems, TimeSpan lockTimeout, TimeSpan lockSleep)
        {
            var stopWatch = Stopwatch.StartNew();

            while (_waitAll && stopWatch.ElapsedMilliseconds < lockTimeout.TotalMilliseconds)
            {
                Task.Delay(lockSleep).ConfigureAwait(false).GetAwaiter().GetResult();
            }

            if (!uniqueItems.ContainsKey(key))
            {
                uniqueItems.AddOrUpdate(key, DateTime.Now, UpdateValueFactory);
                return;
            }

            while (_waitAll || uniqueItems.ContainsKey(key) && stopWatch.ElapsedMilliseconds < lockTimeout.TotalMilliseconds)
            {
                Task.Delay(lockSleep).ConfigureAwait(false).GetAwaiter().GetResult();
            }

            if (stopWatch.ElapsedMilliseconds > lockTimeout.TotalMilliseconds)
            {
                _log.LogWarning("Waiting for end of processing for key {} took {} ms", key, stopWatch.ElapsedMilliseconds);
            }

            uniqueItems.AddOrUpdate(key, DateTime.Now, UpdateValueFactory);
        }

        private async Task LockInternalAsync(string key, ConcurrentDictionary<string, DateTime> uniqueItems, TimeSpan lockTimeout, TimeSpan lockSleep)
        {
            var stopWatch = Stopwatch.StartNew();

            while (_waitAll && stopWatch.ElapsedMilliseconds < lockTimeout.TotalMilliseconds)
            {
                await Task.Delay(lockSleep).ConfigureAwait(false);
            }

            if (!uniqueItems.ContainsKey(key))
            {
                uniqueItems.AddOrUpdate(key, DateTime.Now, UpdateValueFactory);
                return;
            }

            while ((_waitAll || uniqueItems.ContainsKey(key)) && stopWatch.ElapsedMilliseconds < lockTimeout.TotalMilliseconds)
            {
                await Task.Delay(lockSleep).ConfigureAwait(false);
            }

            if (stopWatch.ElapsedMilliseconds > lockTimeout.TotalMilliseconds)
            {
                _log.LogWarning("Waiting for end of processing for key {} took {} ms", key, stopWatch.ElapsedMilliseconds);
            }

            uniqueItems.AddOrUpdate(key, DateTime.Now, UpdateValueFactory);
        }

        private static void ReleaseInternal(string key, ConcurrentDictionary<string, DateTime> uniqueItems, TimeSpan lockTimeout, TimeSpan lockSleep)
        {
            if (lockTimeout == TimeSpan.Zero)
            {
                lockTimeout = lockSleep;
            }
            var stopWatch = Stopwatch.StartNew();
            while (uniqueItems.ContainsKey(key) && stopWatch.ElapsedMilliseconds <= lockTimeout.TotalMilliseconds)
            {
                if (uniqueItems.TryRemove(key, out _))
                {
                    return;
                }
                Task.Delay(lockSleep).ConfigureAwait(false).GetAwaiter().GetResult();
            }
        }

        private async Task ReleaseInternalAsync(string key, ConcurrentDictionary<string, DateTime> uniqueItems, TimeSpan lockTimeout, TimeSpan lockSleep)
        {
            if (lockTimeout == TimeSpan.Zero)
            {
                lockTimeout = lockSleep;
            }
            var stopWatch = Stopwatch.StartNew();
            while (uniqueItems.ContainsKey(key) && stopWatch.ElapsedMilliseconds <= lockTimeout.TotalMilliseconds)
            {
                if (uniqueItems.TryRemove(key, out _))
                {
                    return;
                }
                await Task.Delay(lockSleep).ConfigureAwait(false);
            }
        }

        public async Task CleanAsync()
        {
            await CleanAsync(_uniqueItems, _lockTimeout);
        }

        public async Task CleanAsync(ConcurrentDictionary<string, DateTime> uniqueItems, TimeSpan lockTimeout)
        {
            try
            {
                foreach (var item in uniqueItems)
                {
                    if ((DateTime.Now - item.Value).TotalMilliseconds >= lockTimeout.TotalMilliseconds)
                    {
                        await ReleaseInternalAsync(item.Key, uniqueItems, lockTimeout, _lockSleep);
                    }
                }
            }
            catch (Exception)
            {
                // ignored
            }
        }

        private static DateTime UpdateValueFactory(string key, DateTime newDateTime)
        {
            return newDateTime;
        }
    }
}
