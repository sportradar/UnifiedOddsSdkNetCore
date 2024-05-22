// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Sportradar.OddsFeed.SDK.Common.Internal
{
    /// <summary>
    /// Helper for execution actions
    /// </summary>
    internal static class ExecutionHelper
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

            while (stopWatch.ElapsedMilliseconds < timeoutMs)
            {
                try
                {
                    action.Invoke();
                    return true;
                }
                catch
                {
                    // ignored
                    Task.Delay(100).GetAwaiter().GetResult();
                }
            }
            return false;
        }

        /// <summary>
        /// Try to safely execute action (exception ignored)
        /// </summary>
        /// <param name="action">Action to be invoked</param>
        /// <returns>Indication if the action completed successfully</returns>
        public static bool Safe(Action action)
        {
            try
            {
                action.Invoke();
                return true;
            }
            catch
            {
                // ignored
            }
            return false;
        }

        /// <summary>
        /// Try to safely execute action (exception ignored)
        /// </summary>
        /// <param name="asyncFunction">Action to be invoked</param>
        /// <returns>Indication if the action completed successfully</returns>
        public static async Task<bool> SafeAsync(Func<Task> asyncFunction)
        {
            try
            {
                await asyncFunction().ConfigureAwait(false);
                return true;
            }
            catch
            {
                // ignored
            }
            return false;
        }

        /// <summary>
        /// Try to safely execute action (exception ignored)
        /// </summary>
        /// <param name="asyncFunction">Action to be invoked</param>
        /// <param name="functionArg1">The first argument for function</param>
        /// <returns>Indication if the action completed successfully</returns>
        public static async Task<bool> SafeAsync<T1>(Func<T1, Task> asyncFunction, T1 functionArg1)
        {
            try
            {
                await asyncFunction(functionArg1).ConfigureAwait(false);
                return true;
            }
            catch
            {
                // ignored
            }
            return false;
        }

        /// <summary>
        /// Try to safely execute action (exception ignored)
        /// </summary>
        /// <param name="asyncFunction">Action to be invoked</param>
        /// <param name="functionArg1">The first argument for function</param>
        /// <param name="functionArg2">The second argument for function</param>
        /// <returns>Indication if the action completed successfully</returns>
        public static async Task<bool> SafeAsync<T1, T2>(Func<T1, T2, Task> asyncFunction, T1 functionArg1, T2 functionArg2)
        {
            try
            {
                await asyncFunction(functionArg1, functionArg2).ConfigureAwait(false);
                return true;
            }
            catch
            {
                // ignored
            }
            return false;
        }
    }
}
