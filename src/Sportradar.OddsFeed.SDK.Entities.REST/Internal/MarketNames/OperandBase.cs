/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Linq;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Internal.MarketNames
{
    /// <summary>
    /// Represents a base class for <see cref="IOperand"/> implementations which deal with market specifiers
    /// </summary>
    public abstract class SpecifierBasedOperator
    {
        /// <summary>
        /// Parses the value of the specified specifier
        /// </summary>
        /// <param name="specifierName">Name of the specifier to parse</param>
        /// <param name="specifiers">A <see cref="IReadOnlyDictionary{String, String}"/> containing market specifiers</param>
        /// <param name="specifierValue">When the call is completed it contains the value of the specified specifier</param>
        /// <exception cref="InvalidOperationException">The specified specifier does not exist or it's value is not string representation of int</exception>
        protected static void ParseSpecifier(string specifierName, IReadOnlyDictionary<string, string> specifiers, out int specifierValue)
        {
            Contract.Requires(!string.IsNullOrEmpty(specifierName));
            Contract.Requires(specifiers != null && specifiers.Any());

            string specifierValueString;

            if (!specifiers.TryGetValue(specifierName, out specifierValueString))
            {
                throw new InvalidOperationException($"Specifier with name {specifierName} does not exist");
            }

            if (!int.TryParse(specifierValueString, out specifierValue))
            {
                throw new InvalidOperationException($"Specifier[key={specifierName}, value={specifierValueString}] must be a string representation of a {typeof(int).FullName} type");
            }
        }

        /// <summary>
        /// Parses the value of the specified specifier
        /// </summary>
        /// <param name="specifierName">Name of the specifier to parse</param>
        /// <param name="specifiers">A <see cref="IReadOnlyDictionary{String, String}"/> containing market specifiers</param>
        /// <param name="specifierValue">When the call is completed it contains the value of the specified specifier</param>
        /// <exception cref="InvalidOperationException">The specified specifier does not exist or it's value is not string representation of decimal</exception>
        protected static void ParseSpecifier(string specifierName, IReadOnlyDictionary<string, string> specifiers, out decimal specifierValue)
        {
            Contract.Requires(!string.IsNullOrEmpty(specifierName));
            Contract.Requires(specifiers != null && specifiers.Any());

            string specifierValueString;

            if (!specifiers.TryGetValue(specifierName, out specifierValueString))
            {
                throw new InvalidOperationException($"Specifier with name {specifierName} does not exist");
            }

            if (!decimal.TryParse(specifierValueString, NumberStyles.Number, new CultureInfo("en").NumberFormat, out specifierValue))
            {
                throw new InvalidOperationException($"Specifier[key={specifierName}, value={specifierValueString}] must be a string representation of a {typeof(decimal).FullName} type");
            }
        }
    }
}