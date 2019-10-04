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
    /// Contains various helper methods for parsing of name descriptors
    /// </summary>
    internal static class NameExpressionHelper
    {
        /// <summary>
        /// Lists supported operators
        /// </summary>
        internal static readonly string[] DefinedOperators =
        {
            "+",
            "-",
            "$",
            "!",
            "%"
        };

        /// <summary>
        /// Parses the market / outcome descriptor and returns a <see cref="IList{String}"/> containing name expressions
        /// </summary>
        /// <param name="descriptor">The name descriptor</param>
        /// <param name="descriptorFormat">When the call is completed it contains a provided descriptor where names are replaced with format placeholders e.g. {0}</param>
        /// <returns>a <see cref="IList{String}"/> containing name expressions</returns>
        /// <exception cref="FormatException">The descriptor could not be parsed due to incorrect format</exception>
        internal static IList<string> ParseDescriptor(string descriptor, out string descriptorFormat)
        {
            Contract.Requires(!string.IsNullOrEmpty(descriptor));
            Contract.Ensures(!string.IsNullOrEmpty(Contract.ValueAtReturn(out descriptorFormat)));

            var currentIndex = 0;
            var format = descriptor;
            var expressions = new List<string>();
            do
            {
                var startIndex = descriptor.IndexOf("{", currentIndex, StringComparison.Ordinal);
                var endIndex = descriptor.IndexOf("}", currentIndex, StringComparison.Ordinal);

                if (startIndex < 0 && endIndex < 0)
                {
                    break;
                }

                if (startIndex < 0 || endIndex < 0 || endIndex <= startIndex)
                {
                    throw new FormatException(@"Format of the descriptor is incorrect. Each opening '{' must be closed by corresponding '}'");
                }

                var expression = descriptor.Substring(startIndex, endIndex - startIndex + 1);
                format = format.Replace(expression, "{" + expressions.Count + "}");
                expressions.Add(expression);
                currentIndex = endIndex == descriptor.Length
                    ? endIndex
                    : endIndex + 1;
            } while (true);

            descriptorFormat = format;

            Contract.Assume(!string.IsNullOrEmpty(descriptorFormat));
            return expressions.Any()
                ? expressions
                : null;
        }

        /// <summary>
        /// Parses the expression and verifies it's format
        /// </summary>
        /// <param name="expression">The name expression.</param>
        /// <param name="operator">When the call returns it specifies the operator parsed from the <code>expression</code></param>
        /// <param name="operand">When the call returns it specifies the operand parsed from the <code>expression</code></param>
        /// <exception cref="FormatException">The <code>expression</code> couldn't be parsed due to incorrect format</exception>
        internal static void ParseExpression(string expression, out string @operator, out string operand)
        {
            Contract.Requires(!string.IsNullOrEmpty(expression));
            Contract.Ensures(!string.IsNullOrEmpty(Contract.ValueAtReturn(out operand)));

            if (expression.Length < 3)
            {
                throw new FormatException("Format of the 'expression' is not correct. Minimum required length is 3");
            }
            if (expression.First() != '{')
            {
                throw new FormatException("Format of the 'expression' is not correct. It must start with char '{'");
            }
            if (expression.Last() != '}')
            {
                throw new FormatException("Format of the 'expression' is not correct. It must end with char '}");
            }

            @operator = expression.Substring(1, 1);
            if (Array.IndexOf(DefinedOperators, @operator) < 0)
            {
                @operator = null;
                operand = expression.Substring(1, expression.Length - 2);
            }
            else
            {
                operand = expression.Substring(2, expression.Length - 3);
            }
        }

        /// <summary>
        /// Parses the value of the specified specifier
        /// </summary>
        /// <param name="specifierName">Name of the specifier to parse</param>
        /// <param name="specifiers">A <see cref="IReadOnlyDictionary{String, String}"/> containing market specifiers</param>
        /// <param name="specifierValue">When the call is completed it contains the value of the specified specifier</param>
        /// <exception cref="InvalidOperationException">The specified specifier does not exist or it's value is not string representation of int</exception>
        internal static void ParseSpecifier(string specifierName, IReadOnlyDictionary<string, string> specifiers, out int specifierValue)
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
        internal static void ParseSpecifier(string specifierName, IReadOnlyDictionary<string, string> specifiers, out decimal specifierValue)
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
