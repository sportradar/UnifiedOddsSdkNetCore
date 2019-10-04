/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using System.Collections.Generic;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Internal.MarketNames
{
    /// <summary>
    /// A factory used to construct <see cref="IOperand"/> instances
    /// </summary>
    public interface IOperandFactory
    {
        /// <summary>
        /// Constructs and returns a <see cref="IOperand"/> instance
        /// </summary>
        /// <param name="specifiers">A <see cref="IReadOnlyDictionary{String,String}"/> representing specifiers for the market associated with the constructed operand.</param>
        /// <param name="operandExpression">A <see cref="string"/> representation of the operand.</param>
        /// <returns>The constructed <see cref="IOperand"/> instance.</returns>
        /// <exception cref="FormatException">The format of the <code>operandExpression</code> is not correct</exception>
        IOperand BuildOperand(IReadOnlyDictionary<string, string> specifiers, string operandExpression);
    }
}