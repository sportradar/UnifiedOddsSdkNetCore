/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Sportradar.OddsFeed.SDK.Common.Internal
{
    internal class LockHelper
    {
        private readonly ILogger _log = SdkLoggerFactory.GetLoggerForExecution(typeof(LockHelper));
        private readonly ConcurrentDictionary<string, object> _lockDict;
        private readonly TimeSpan _lockTimeout;
        private readonly TimeSpan _lockSleep;
        private bool _waitAll;

        public LockHelper(ConcurrentDictionary<string, object> lockDict, TimeSpan lockTimeout, TimeSpan lockSleep)
        {
            _lockDict = lockDict ?? new ConcurrentDictionary<string, object>();
            _lockTimeout = lockTimeout.TotalSeconds < 1 ? TimeSpan.FromSeconds(60) : lockTimeout;
            _lockSleep = lockSleep.TotalMilliseconds < 10 ? TimeSpan.FromMilliseconds(50) : lockSleep;
            _waitAll = false;
        }

        public LockHelper()
        {
            _lockDict = new ConcurrentDictionary<string, object>();
            _lockTimeout = TimeSpan.FromSeconds(60);
            _lockSleep = TimeSpan.FromMilliseconds(50);
            _waitAll = false;
        }

        public void Wait()
        {
            _waitAll = true;
            LockInternal("all", _lockDict, _lockTimeout, _lockSleep);
        }

        public void Wait(string key)
        {
            LockInternal(key, _lockDict, _lockTimeout, _lockSleep);
        }

        public void Wait(string key, TimeSpan lockTimeout, TimeSpan lockSleep)
        {
            if (lockTimeout == TimeSpan.Zero)
            {
                lockTimeout = _lockTimeout;
            }

            if (lockSleep == TimeSpan.Zero)
            {
                lockSleep = _lockSleep;
            }

            LockInternal(key, _lockDict, lockTimeout, lockSleep);
        }

        private void LockInternal(string key, ConcurrentDictionary<string, object> uniqueItems, TimeSpan lockTimeout, TimeSpan lockSleep)
        {
            if (!uniqueItems.ContainsKey(key))
            {
                uniqueItems.AddOrUpdate(key, new object(), UpdateValueFactory);
                return;
            }

            var stopWatch = Stopwatch.StartNew();
            while (_waitAll || (uniqueItems.ContainsKey(key) && stopWatch.ElapsedMilliseconds < lockTimeout.TotalMilliseconds))
            {
                Task.Delay(lockSleep).GetAwaiter().GetResult();
            }

            if (stopWatch.ElapsedMilliseconds > lockTimeout.TotalMilliseconds)
            {
                _log.LogWarning("Waiting for end of processing for key {} took {} ms.", key, stopWatch.ElapsedMilliseconds);
            }

            uniqueItems.AddOrUpdate(key, new object(), UpdateValueFactory);
        }

        /// <summary>
        /// Return a lock object for a named synchronization block.
        /// </summary>
        /// <param name="name">The lock name.</param>
        /// <returns>object</returns>
        public object GetLock(string name)
        {
            return _lockDict.GetOrAdd(name, s => new object());
        }

        public object GetLock()
        {
            _waitAll = true;
            return GetLock("all");
        }

        public void Release()
        {
            _waitAll = false;
        }

        public void Clean()
        {
            Clean(_lockDict, _lockTimeout);
        }

        public void Clean(ConcurrentDictionary<string, object> lockDict, TimeSpan lockTimeout)
        {
            foreach (var item in lockDict)
            {
                if ((DateTime.Now - (DateTime)item.Value).TotalSeconds > lockTimeout.TotalSeconds)
                {
                    lockDict.TryRemove(item.Key, out _);
                }
            }
        }

        private object UpdateValueFactory(string arg1, object arg2)
        {
            return arg2;
        }
    }
}
