/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System.Threading.Tasks;
using Sportradar.OddsFeed.SDK.Common.Exceptions;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Internal.MarketNames
{
    /// <summary>
    /// Defines a contract implemented by classes representing mathematical operands
    /// </summary>
    public interface IOperand
    {
        /// <summary>
        /// Gets the value of the operand as a <see cref="int"/>
        /// </summary>
        /// <returns>A <see cref="Task{Int32}"/> containing the value of the operand as a <see cref="int"/></returns>
        /// <exception cref="NameExpressionException">Error occurred while evaluating value of the operand</exception>
        Task<int> GetIntValue();

        /// <summary>
        /// Gets the value of the operand as a <see cref="decimal"/>
        /// </summary>
        /// <returns>A <see cref="Task{Int32}"/> containing the value of the operand as a <see cref="int"/></returns>
        /// <exception cref="NameExpressionException">Error occurred while evaluating value of the operand</exception>
        Task<decimal> GetDecimalValue();

        /// <summary>
        /// Gets the value of the operand as a <see cref="string"/>
        /// </summary>
        /// <returns>A <see cref="Task{String}"/> containing the value of the operand as a <see cref="string"/></returns>
        /// <exception cref="NameExpressionException">Error occurred while evaluating value of the operand</exception>
        Task<string> GetStringValue();

    }
}