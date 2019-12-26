/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System.Collections.Generic;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Internal.MarketNames
{
    /// <summary>
    /// Defines a contract implemented by classes used to build <see cref="INameExpression"/> instances from string expressions
    /// </summary>
    public interface INameExpressionFactory
    {
        /// <summary>
        /// Builds and returns a <see cref="INameExpression"/> instance which can be used to generate name from the provided expression
        /// </summary>
        /// <param name="sportEvent">A <see cref="ISportEvent"/> instance representing associated sport @event</param>
        /// <param name="specifiers">A <see cref="IReadOnlyDictionary{String, String}"/> representing specifiers of the associated market</param>
        /// <param name="operator">A <see cref="string"/> specifying the operator for which to build the expression</param>
        /// <param name="operand">An operand for the built expression</param>
        /// <returns>The constructed <see cref="INameExpression"/> instance</returns>
        INameExpression BuildExpression(ISportEvent sportEvent, IReadOnlyDictionary<string, string> specifiers, string @operator, string operand);
    }
}