// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;

namespace Sportradar.OddsFeed.SDK.Common.Extensions
{
    /// <summary>
    /// Class defining extension methods for epoch date time
    /// </summary>
    public static class EpochTimeExtensions
    {
        /// <summary>
        /// Convert long epoch time to DateTime
        /// </summary>
        /// <param name="epochTime">The UNIX time</param>
        /// <returns>DateTime</returns>
        public static DateTime FromEpochTime(this long epochTime)
        {
            var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            return epoch.AddMilliseconds(epochTime).ToLocalTime();
        }

        /// <summary>
        /// Convert DateTime to the epoch time (in seconds)
        /// </summary>
        /// <param name="date">The date</param>
        /// <returns>System.Int64</returns>
        public static long ToEpochTime(this DateTime date)
        {
            var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            return Convert.ToInt64((date.ToUniversalTime() - epoch).TotalMilliseconds);
        }
    }
}
