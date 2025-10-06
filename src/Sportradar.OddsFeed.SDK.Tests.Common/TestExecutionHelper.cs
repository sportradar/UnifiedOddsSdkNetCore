// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Sportradar.OddsFeed.SDK.Tests.Common;

/// <summary>
/// Helper for execution actions
/// </summary>
public static class TestExecutionHelper
{
    public static void Sleep(TimeSpan timeSpan)
    {
        Sleep((int)timeSpan.TotalMilliseconds);
    }

    private static void Sleep(int milliseconds)
    {
        var mre = new ManualResetEventSlim();
        Task.Run(async () =>
                 {
                     await Task.Delay(milliseconds);
                     mre.Set();
                 });

        mre.Wait();
    }

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
            }
            Sleep(100);
        }
        return finished;
    }

    /// <summary>
    /// Try to execute action till success or timeout
    /// </summary>
    /// <param name="action">Action to be invoked</param>
    /// <param name="delayMs">Delay step</param>
    /// <param name="timeoutMs">Maximum execution time</param>
    /// <returns>Indication if the action completed successfully</returns>
    public static bool WaitToComplete(Func<bool> action, int delayMs = 100, int timeoutMs = 10000)
    {
        var stopWatch = Stopwatch.StartNew();
        var finished = false;
        while (!finished && stopWatch.ElapsedMilliseconds < timeoutMs)
        {
            try
            {
                finished = action.Invoke();
            }
            catch
            {
                // ignored
            }
            Sleep(delayMs);
        }
        return finished;
    }

    /// <summary>
    /// Try to execute action till success or timeout
    /// </summary>
    /// <param name="action">Action to be invoked</param>
    /// <param name="delayMs">Delay step</param>
    /// <param name="timeoutMs">Maximum execution time</param>
    /// <returns>Indication if the action completed successfully</returns>
    public static async Task<bool> WaitToCompleteAsync(Func<bool> action, int delayMs = 100, int timeoutMs = 10000)
    {
        var stopWatch = Stopwatch.StartNew();
        var finished = false;
        while (!finished && stopWatch.ElapsedMilliseconds < timeoutMs)
        {
            try
            {
                finished = action.Invoke();
            }
            catch
            {
                // ignored
            }
            if (!finished)
            {
                await Task.Delay(delayMs).ConfigureAwait(false);
            }
        }
        return finished;
    }
}
