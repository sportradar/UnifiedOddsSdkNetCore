/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using System.Collections.Generic;
using System.Globalization;
using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Entities.REST.Enums;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Internal
{
    /// <summary>
    /// Defines static methods used when parsing feed messages
    /// </summary>
    public static class MessageMapperHelper
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
            if (!enumType.IsEnum)
            {
                throw new InvalidOperationException($"Type specified by generic parameter T must be an enum. Provided type was:{typeof(TEnum).FullName}");
            }
            return Enum.IsDefined(enumType, value);
        }

        /// <summary>
        /// Converts the provided <code>value</code> to the member of the specified enum
        /// </summary>
        /// <typeparam name="T">The type of enum to which to convert the <code>value</code></typeparam>
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
        /// Converts the provided int <code>value</code> to the member of the specified enum, or returns <code>defaultValue</code>
        /// if value of <code>specified</code> is false
        /// </summary>
        /// <typeparam name="T">The type of enum to which to convert the <code>value</code></typeparam>
        /// <param name="specified">Value indicating whether the value field was specified in the feed message</param>
        /// <param name="value">The value in the feed message</param>
        /// <param name="defaultValue">A <see cref="T"/> member to be returned if <code>specified</code> is false</param>
        /// <returns>The <code>value</code> converted to enum <see cref="T"/> member</returns>
        public static T GetEnumValue<T>(bool specified, int value, T defaultValue) where T : struct, IConvertible
        {
            return !specified
                       ? defaultValue
                       : GetEnumValue<T>(value);
        }

        /// <summary>
        /// Converts the provided int <code>value</code> to the member of the specified enum, or returns <code>defaultValue</code>
        /// </summary>
        /// <typeparam name="T">The type of enum to which to convert the <code>value</code></typeparam>
        /// <param name="value">The value in the feed message</param>
        /// <param name="defaultValue">A <see cref="T"/> member to be returned if unknown <code>value</code></param>
        /// <returns>The <code>value</code> converted to enum <see cref="T"/> member</returns>
        public static T GetEnumValue<T>(int value, T defaultValue) where T : struct, IConvertible
        {
            try
            {
                return GetEnumValue<T>(value);
            }
            catch
            {
                var log = SdkLoggerFactory.GetLogger(typeof(MessageMapperHelper));
                log.Error($"Enum value [{value}] not available for enum {typeof(T)}.");
                // ignored
            }
            return defaultValue;
        }

        /// <summary>
        /// Converts the provided string <code>value</code> (enum value name) to the member of the specified enum
        /// </summary>
        /// <typeparam name="T">The type of enum to which to convert the <code>value</code></typeparam>
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
        /// Converts the provided string <code>value</code> (enum value name) to the member of the specified enum
        /// </summary>
        /// <typeparam name="T">The type of enum to which to convert the <code>value</code></typeparam>
        /// <param name="value">The value name to be converted</param>
        /// <param name="defaultValue">A <see cref="T"/> member to be returned if <code>value</code> is not member of enum</param>
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
        /// Converts the provided string <code>value</code> (enum value name) to the member of the specified enum
        /// </summary>
        /// <typeparam name="T">The type of enum to which to convert the <code>value</code></typeparam>
        /// <param name="value">The value name to be converted</param>
        /// <param name="defaultValue">A <see cref="T"/> member to be returned if <code>value</code> is not member of enum</param>
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
