using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Sportradar.OddsFeed.SDK.Common
{
    /// <summary>
    /// Helper for execution actions
    /// </summary>
    public static class ExecutionHelper
    {
        /// <summary>
        /// Try to execute action till success or timeout
        /// </summary>
        /// <param name="action">Action to be invoked</param>
        /// <param name="timeoutMs">Maximum execution time</param>
        /// <returns>Indication if the action completed successfully</returns>
        public static bool WaitToComplete(Action action, int timeoutMs = 10000)
        {
            var stopWatch = Stopwatch.StartNew();
            var finished = false;
            while (!finished && stopWatch.ElapsedMilliseconds < timeoutMs)
            {
                try
                {
                    action.Invoke();
                    finished = true;
                }
                catch
                {
                    // ignored
                    Task.Delay(100).GetAwaiter().GetResult();
                }
            }
            return finished;
        }

        /// <summary>
        /// Try to safely execute action (exception ignored)
        /// </summary>
        /// <param name="action">Action to be invoked</param>
        /// <returns>Indication if the action completed successfully</returns>
        public static bool Safe(Action action)
        {
            var finished = false;
            try
            {
                action.Invoke();
                finished = true;
            }
            catch
            {
                // ignored
            }
            return finished;
        }

        /// <summary>
        /// Try to safely execute action (exception ignored)
        /// </summary>
        /// <param name="action">Action to be invoked</param>
        /// <returns>Indication if the action completed successfully</returns>
        public static async Task<bool> SafeAsync(Action action)
        {
            var finished = false;
            try
            {
                await Task.Run(action).ConfigureAwait(false);
                finished = true;
            }
            catch
            {
                // ignored
            }
            return finished;
        }
    }
}
