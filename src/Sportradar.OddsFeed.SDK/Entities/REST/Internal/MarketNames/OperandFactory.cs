/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using System.Collections.Generic;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Internal.MarketNames
{
    /// <summary>
    /// A default implementation of the <see cref="IOperandFactory"/> contract
    /// </summary>
    public class OperandFactory : IOperandFactory
    {
        /// <summary>
        /// Constructs and returns a <see cref="IOperand" /> instance
        /// </summary>
        /// <param name="specifiers">A <see cref="IReadOnlyDictionary{String,String}" /> representing specifiers for the market associated with the constructed operand.</param>
        /// <param name="operandExpression">A <see cref="string" /> representation of the operand.</param>
        /// <returns>The constructed <see cref="IOperand" /> instance.</returns>
        /// <exception cref="FormatException">The format of the <code>operandExpression</code> is not correct</exception>
        public IOperand BuildOperand(IReadOnlyDictionary<string, string> specifiers, string operandExpression)
        {
            if((operandExpression.StartsWith("(") && !operandExpression.EndsWith(")")) ||
                (!operandExpression.StartsWith("(") && operandExpression.EndsWith(")")))
            {
                throw new FormatException($"Format of the operand {operandExpression} is not correct. It contains un-closed parenthesis");
            }

            if (operandExpression.StartsWith("(") && operandExpression.EndsWith(")"))
            {
                //remove parenthesis
                operandExpression = operandExpression.Substring(1, operandExpression.Length - 2);
                SimpleMathOperation operation;
                string[] parts;
                if (operandExpression.Contains("+"))
                {
                    operation = SimpleMathOperation.ADD;
                    parts = operandExpression.Split(new[] { '+' }, StringSplitOptions.RemoveEmptyEntries);
                }
                else if (operandExpression.Contains("-"))
                {
                    operation = SimpleMathOperation.SUBTRACT;
                    parts = operandExpression.Split(new[] { '-' }, StringSplitOptions.RemoveEmptyEntries);
                }
                else
                {
                    throw new FormatException($"Format of operand {operandExpression} is not correct. It does not contain an operation identifier");
                }

                if (parts.Length != 2)
                {
                    throw new FormatException($"Format of operand {operandExpression} is not correct. It contains more than one operation identifier");
                }

                var staticValue = int.Parse(parts[1]);
                return new ExpressionOperand(specifiers, parts[0], operation, staticValue);
            }
            return new SimpleOperand(specifiers, operandExpression);
        }
    }
}