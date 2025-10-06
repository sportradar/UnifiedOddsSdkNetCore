// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Dawn;
using Sportradar.OddsFeed.SDK.Common.Internal;

namespace Sportradar.OddsFeed.SDK.Entities.Rest.Internal
{
    /// <summary>
    /// Defines static methods used when parsing feed messages
    /// </summary>
    internal static class FeedMapperHelper
    {
        /// <summary>
        /// Creates and returns a <see cref="IReadOnlyDictionary{TKey, TValue}"/> by splitting values provided by the <c>valueParts</c>
        /// </summary>
        /// <param name="values">The values to be split</param>
        /// <param name="separators">The separators to be used for splitting </param>
        /// <returns>The <see cref="IReadOnlyDictionary{TKey,TValue}"/> obtained by splitting the provided values</returns>
        private static IReadOnlyDictionary<string, string> CreateDictionary(string[] values, params string[] separators)
        {
            Guard.Argument(values, nameof(values)).NotNull();
            Guard.Argument(separators, nameof(separators)).NotNull();
            //TODO: Check why .NotEmpty throws in some cases
            if (values.Length == 0)
            {
                throw new ArgumentOutOfRangeException(nameof(values));
            }

            if (separators.Length == 0)
            {
                throw new ArgumentOutOfRangeException(nameof(separators));
            }

            var tuples = new List<Tuple<string, string>>(values.Length);
            foreach (var specifier in values)
            {
                var specifierArray = specifier.Split(separators, StringSplitOptions.RemoveEmptyEntries);
                if (specifierArray.Length != 2)
                {
                    throw new FormatException($"Format of the specifier {specifier} is not correct. It must have a key and value separated by = sign");
                }
                var key = specifierArray[0].Trim();
                var value = specifierArray[1].Trim();
                if (string.IsNullOrEmpty(key))
                {
                    throw new FormatException($"Format of the specifier {specifier} is not correct. The key must not be empty");
                }
                if (string.IsNullOrEmpty(value))
                {
                    throw new FormatException($"Format of the specifier {specifier} is not correct. The value must not be empty");
                }
                if (tuples.Any(t => t.Item1 == key))
                {
                    throw new ArgumentException($"Duplicated specifiers are not allowed. specifier={key}", nameof(values));
                }
                tuples.Add(new Tuple<string, string>(key, value));
            }
            return new ReadOnlyDictionary<string, string>(tuples.ToDictionary(t => t.Item1, t => t.Item2));
        }

        /// <summary>
        /// Creates a <see cref="IReadOnlyDictionary{TKey,TValue}"/> from a <see cref="string"/> containing string representation of validity attributes
        /// </summary>
        /// <param name="value">A <see cref="string"/> representation of the validity attributes</param>
        /// <returns>A <see cref="IDictionary{String, String}"/> containing validity attributes</returns>
        public static IReadOnlyDictionary<string, string> GetValidForAttributes(string value)
        {
            Guard.Argument(value, nameof(value)).NotNull().NotEmpty();

            var parts = value.Split(new[] { SdkInfo.SpecifiersDelimiter }, StringSplitOptions.None);
            return CreateDictionary(parts, "=", "~");
        }

        /// <summary>
        /// Creates a <see cref="IReadOnlyDictionary{TKey,TValue}"/> from a <see cref="string"/> containing market specifiers
        /// </summary>
        /// <param name="specifiers">A <see cref="string"/> containing market specifiers</param>
        /// <returns>A <see cref="IDictionary{String, String}"/> containing market specifiers</returns>
        public static IReadOnlyDictionary<string, string> GetSpecifiers(string specifiers)
        {
            Guard.Argument(specifiers, nameof(specifiers)).NotNull().NotEmpty();

            var splitSpecifiers = specifiers.Split(new[] { SdkInfo.SpecifiersDelimiter }, StringSplitOptions.None);
            return CreateDictionary(splitSpecifiers, "=");
        }
    }
}
