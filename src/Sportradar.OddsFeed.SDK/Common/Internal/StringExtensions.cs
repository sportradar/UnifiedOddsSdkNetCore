/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
// ReSharper disable MemberCanBePrivate.Global

namespace Sportradar.OddsFeed.SDK.Common.Internal
{
    /// <summary>
    /// String extensions
    /// </summary>
    public static class StringExtensions
    {
        /// <summary>
        /// Truncate the string to max length
        /// </summary>
        /// <param name="value">String to be truncated</param>
        /// <param name="maxLength">Max length of the returned string</param>
        /// <returns>Truncated string</returns>
        public static string Truncate(this string value, int maxLength)
        {
            return value?.Substring(0, Math.Min(value.Length, maxLength));
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
            var result = value.Truncate(length) ?? string.Empty;
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
