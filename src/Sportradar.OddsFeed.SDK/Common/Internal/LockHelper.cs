using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

namespace Sportradar.OddsFeed.SDK.Common.Internal
{
    internal class LockHelper
    {
        private readonly ILogger _log = SdkLoggerFactory.GetLoggerForExecution(typeof(LockHelper));
        private readonly ConcurrentDictionary<string, object> _lockDict;
        private bool _waitAll;

        public LockHelper(ConcurrentDictionary<string, object> lockDict)
        {
            _lockDict = lockDict ?? new ConcurrentDictionary<string, object>();
            _waitAll = false;
        }

        public LockHelper()
        {
            _lockDict = new ConcurrentDictionary<string, object>();
            _waitAll = false;
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
            Clean(_lockDict);
        }

        public void Clean(ConcurrentDictionary<string, object> lockDict)
        {
            //foreach (var item in lockDict)
            //{
            //    if ((DateTime.Now - item.Value).TotalSeconds > lockTimeout.TotalSeconds)
            //    {
            //        lockDict.TryRemove(item.Key, out _);
            //    }
            //}
        }
    }
}
