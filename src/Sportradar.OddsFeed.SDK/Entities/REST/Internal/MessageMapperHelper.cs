/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using System.Collections.Generic;
using System.Globalization;
using Microsoft.Extensions.Logging;
using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Entities.REST.Enums;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Internal
{
    /// <summary>
    /// Defines static methods used when parsing feed messages
    /// </summary>
    internal static class MessageMapperHelper
    {
        /// <summary>
        /// Returns a <see cref="VoidFactor"/> member based on information received from the feed
        /// </summary>
        /// <param name="specified">Value indicating whether the void factor field was specified in the feed message</param>
        /// <param name="value">Value of the void factor specified in the feed message</param>
        /// <returns></returns>
        public static VoidFactor GetVoidFactor(bool specified, double value)
        {
            if (!specified)
            {
                return VoidFactor.Zero;
            }

            if (value < 0.1)
            {
                return VoidFactor.Zero;
            }
            if (value > 0.4 && value < 0.6)
            {
                return VoidFactor.Half;
            }
            if (value > 0.9)
            {
                return VoidFactor.One;
            }

            throw new ArgumentException($"Value {value} is not a valid void factor", nameof(value));
        }

        /// <summary>
        /// Determines whether the provided value is member of the specified enumeration
        /// </summary>
        /// <typeparam name="TEnum">The enumeration whose members are to be checked</typeparam>
        /// <param name="value">The value to check.</param>
        /// <returns>True if the provided value is member of the specified enumeration. Otherwise false.</returns>
        public static bool IsEnumMember<TEnum>(object value) where TEnum : struct
        {
            var enumType = typeof(TEnum);
            return !enumType.IsEnum
                ? throw new InvalidOperationException($"Type specified by generic parameter T must be an enum. Provided type was:{typeof(TEnum).FullName}")
                : Enum.IsDefined(enumType, value);
        }

        /// <summary>
        /// Converts the provided <c>value</c> to the member of the specified enum
        /// </summary>
        /// <typeparam name="T">The type of enum to which to convert the <c>value</c></typeparam>
        /// <param name="value">The value to be converted</param>
        /// <returns>The member of the specified enum</returns>
        public static T GetEnumValue<T>(int value) where T : struct, IConvertible
        {
            var type = typeof(T);
            if (!type.IsEnum)
            {
                throw new ArgumentException("T must be an enum");
            }
            if (!Enum.IsDefined(type, value))
            {
                throw new ArgumentException($"Value:{value} is not a member of enum{type.Name}", nameof(value));
            }
            return (T)(object)value;
        }

        /// <summary>
        /// Converts the provided int <c>value</c> to the member of the specified enum, or returns <c>defaultValue</c> if value of <c>specified</c> is false
        /// </summary>
        /// <typeparam name="T">The type of enum to which to convert the <c>value</c></typeparam>
        /// <param name="specified">Value indicating whether the value field was specified in the feed message</param>
        /// <param name="value">The value in the feed message</param>
        /// <param name="defaultValue">A structure member to be returned if <c>specified</c> is false</param>
        /// <returns>The <c>value</c> converted to enum structure member</returns>
        public static T GetEnumValue<T>(bool specified, int value, T defaultValue) where T : struct, IConvertible
        {
            return !specified
                       ? defaultValue
                       : GetEnumValue<T>(value);
        }

        /// <summary>
        /// Converts the provided int <c>value</c> to the member of the specified enum, or returns <c>defaultValue</c>
        /// if value of <c>specified</c> is false
        /// </summary>
        /// <typeparam name="T">The type of enum to which to convert the <c>value</c></typeparam>
        /// <param name="specified">Value indicating whether the value field was specified in the feed message</param>
        /// <param name="value">The value in the feed message</param>
        /// <param name="unknownValue">A structure member to be returned if value is not known, but specified</param>
        /// <param name="naValue">A structure member to be returned if value is not specified</param>
        /// <returns>The <c>value</c> converted to enum structure member</returns>
        public static T GetEnumValue<T>(bool specified, int value, T unknownValue, T naValue) where T : struct, IConvertible
        {
            return !specified
                ? naValue
                : GetEnumValue(value, unknownValue);
        }

        /// <summary>
        /// Converts the provided int <c>value</c> to the member of the specified enum, or returns <c>defaultValue</c>
        /// </summary>
        /// <typeparam name="T">The type of enum to which to convert the <c>value</c></typeparam>
        /// <param name="value">The value in the feed message</param>
        /// <param name="defaultValue">A structure member to be returned if unknown <c>value</c></param>
        /// <returns>The <c>value</c> converted to enum structure member</returns>
        public static T GetEnumValue<T>(int value, T defaultValue) where T : struct, IConvertible
        {
            try
            {
                return GetEnumValue<T>(value);
            }
            catch
            {
                var log = SdkLoggerFactory.GetLogger(typeof(MessageMapperHelper));
                log.LogError($"Enum value [{value}] not available for enum {typeof(T)}.");
                // ignored
            }
            return defaultValue;
        }

        /// <summary>
        /// Converts the provided string <c>value</c> (enum value name) to the member of the specified enum
        /// </summary>
        /// <typeparam name="T">The type of enum to which to convert the <c>value</c></typeparam>
        /// <param name="value">The value name to be converted</param>
        /// <returns>The member of the specified enum</returns>
        public static T GetEnumValue<T>(string value) where T : struct, IConvertible
        {
            var type = typeof(T);
            if (!type.IsEnum)
            {
                throw new ArgumentException("T must be an enum");
            }
            if (string.IsNullOrEmpty(value))
            {
                throw new ArgumentException($"Empty value is not a member of enum {type.Name}", nameof(value));
            }
            var trimmedValue = value.Replace(" ", string.Empty).Trim();
            var enumValues = Enum.GetValues(type);
            foreach (int v in enumValues)
            {
                var enumChoice = (T)(object)v;
                if (string.Equals(enumChoice.ToString(CultureInfo.InvariantCulture), value, StringComparison.InvariantCultureIgnoreCase)
                    || string.Equals(enumChoice.ToString(CultureInfo.InvariantCulture), trimmedValue, StringComparison.InvariantCultureIgnoreCase))
                {
                    return enumChoice;
                }
            }
            throw new ArgumentException($"Value:{value} is not a member of enum {type.Name}", nameof(value));
        }

        /// <summary>
        /// Converts the provided string <c>value</c> (enum value name) to the member of the specified enum
        /// </summary>
        /// <typeparam name="T">The type of enum to which to convert the <c>value</c></typeparam>
        /// <param name="value">The value name to be converted</param>
        /// <param name="defaultValue">A structure member to be returned if <c>value</c> is not member of enum</param>
        /// <returns>The member of the specified enum</returns>
        public static T GetEnumValue<T>(string value, T defaultValue) where T : struct, IConvertible
        {
            var type = typeof(T);
            if (!type.IsEnum)
            {
                throw new ArgumentException("T must be an enum");
            }
            if (string.IsNullOrEmpty(value))
            {
                return defaultValue;
            }
            var trimmedValue = value.Replace(" ", string.Empty).Trim();
            var enumValues = Enum.GetValues(type);
            foreach (int v in enumValues)
            {
                var enumChoice = (T)(object)v;
                if (string.Equals(enumChoice.ToString(CultureInfo.InvariantCulture), value, StringComparison.InvariantCultureIgnoreCase)
                    || string.Equals(enumChoice.ToString(CultureInfo.InvariantCulture), trimmedValue, StringComparison.InvariantCultureIgnoreCase))
                {
                    return enumChoice;
                }
            }
            return defaultValue;
        }

        /// <summary>
        /// Converts the provided string <c>value</c> (enum value name) to the member of the specified enum
        /// </summary>
        /// <typeparam name="T">The type of enum to which to convert the <c>value</c></typeparam>
        /// <param name="value">The value name to be converted</param>
        /// <param name="defaultValue">A struct member to be returned if <c>value</c> is not member of enum</param>
        /// <param name="predefinedPairs">The predefined pairs of string value to enum</param>
        /// <returns>The member of the specified enum</returns>
        public static T GetEnumValue<T>(string value, T defaultValue, IDictionary<string, T> predefinedPairs) where T : struct, IConvertible
        {
            if (predefinedPairs != null)
            {
                if (predefinedPairs.ContainsKey(value))
                {
                    return predefinedPairs[value];
                }
            }

            return GetEnumValue(value, defaultValue);
        }
    }
}
