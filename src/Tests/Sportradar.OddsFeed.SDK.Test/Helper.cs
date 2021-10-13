using System;

namespace Sportradar.OddsFeed.SDK.Test
{
    public static class Helper
    {
        /// <summary>
        /// Convert long epoch time to DateTime
        /// </summary>
        /// <param name="epochTime">The UNIX time</param>
        /// <returns>DateTime</returns>
        public static DateTime FromEpochTime(long epochTime)
        {
            var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            return epoch.AddMilliseconds(epochTime).ToLocalTime();
        }

        /// <summary>
        /// Convert DateTime to the epoch time (in seconds)
        /// </summary>
        /// <param name="date">The date</param>
        /// <returns>System.Int64</returns>
        public static long ToEpochTime(DateTime date)
        {
            var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            return Convert.ToInt64((date.ToUniversalTime() - epoch).TotalMilliseconds);
        }

        /// <summary>
        /// Return the fixed length of the string. If input to short it adds spaces, if too long, it is truncated
        /// </summary>
        /// <param name="value">¸Input string</param>
        /// <param name="length">Length of the returned string</param>
        /// <param name="postfix">If the spaces are added pre- or post- string</param>
        /// <returns>Fixed length string</returns>
        public static string FixedLength(this string value, int length, bool postfix = true)
        {
            var result = string.IsNullOrEmpty(value)
                ? string.Empty
                : value.Length >= length
                    ? value.Substring(0, length)
                    : value;
            var dif = length - result.Length;

            var space = string.Empty;
            for (var i = 0; i < dif; i++)
            {
                space += " ";
            }

            return postfix
                ? result + space
                : space + result;
        }
    }
}
