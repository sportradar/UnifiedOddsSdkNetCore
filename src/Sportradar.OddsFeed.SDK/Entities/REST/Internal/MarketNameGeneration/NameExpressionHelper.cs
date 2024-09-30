// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using System.Collections.Generic;
using System.Linq;
using Dawn;

namespace Sportradar.OddsFeed.SDK.Entities.Rest.Internal.MarketNameGeneration
{
    /// <summary>
    /// Contains various helper methods for parsing of name descriptors
    /// </summary>
    internal static class NameExpressionHelper
    {
        /// <summary>
        /// Lists supported operators
        /// </summary>
        internal static readonly string[] DefinedOperators = { "+", "-", "$", "!", "%" };

        /// <summary>
        /// Parses the market / outcome descriptor and returns a <see cref="IList{String}"/> containing name expressions
        /// </summary>
        /// <param name="descriptor">The name descriptor</param>
        /// <param name="descriptorFormat">When the call is completed it contains a provided descriptor where names are replaced with format placeholders e.g. {0}</param>
        /// <returns>a <see cref="IList{String}"/> containing name expressions</returns>
        /// <exception cref="FormatException">The descriptor could not be parsed due to incorrect format</exception>
        internal static IList<string> ParseDescriptor(string descriptor, out string descriptorFormat)
        {
            Guard.Argument(descriptor, nameof(descriptor)).NotNull().NotEmpty();

            var currentIndex = 0;
            var format = descriptor;
            var expressions = new List<string>();

            while (true)
            {
                var startIndex = descriptor.IndexOf("{", currentIndex, StringComparison.Ordinal);
                var endIndex = descriptor.IndexOf("}", currentIndex, StringComparison.Ordinal);

                if (startIndex < 0 && endIndex < 0)
                {
                    break;
                }

                if (startIndex < 0 || endIndex < 0 || endIndex <= startIndex)
                {
                    throw new FormatException("Format of the descriptor is incorrect. Each opening '{' must be closed by corresponding '}'");
                }

                var expression = descriptor.Substring(startIndex, endIndex - startIndex + 1);
                format = format.Replace(expression, "{" + expressions.Count + "}");
                expressions.Add(expression);
                currentIndex = endIndex == descriptor.Length
                                   ? endIndex
                                   : endIndex + 1;
            }

            descriptorFormat = format;

            return expressions;
        }

        /// <summary>
        /// Parses the expression and verifies its format
        /// </summary>
        /// <param name="expression">The name expression.</param>
        /// <param name="operator">When the call returns it specifies the operator parsed from the <c>expression</c></param>
        /// <param name="operand">When the call returns it specifies the operand parsed from the <c>expression</c></param>
        /// <exception cref="FormatException">The <c>expression</c> couldn't be parsed due to incorrect format</exception>
        internal static void ParseExpression(string expression, out string @operator, out string operand)
        {
            Guard.Argument(expression, nameof(expression)).NotNull().NotEmpty();

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
    }
}
