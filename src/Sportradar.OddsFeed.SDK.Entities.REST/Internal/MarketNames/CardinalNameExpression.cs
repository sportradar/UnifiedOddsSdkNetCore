/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Threading.Tasks;
using Sportradar.OddsFeed.SDK.Common.Exceptions;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Internal.MarketNames
{
    /// <summary>
    /// A <see cref="INameExpression"/> implementation supporting cardinal expressions (i.e. {period_nr})
    /// </summary>
    /// <seealso cref="INameExpression" />
    internal class CardinalNameExpression : INameExpression
    {
        /// <summary>
        /// A <see cref="IOperand"/> representing part of the name expression
        /// </summary>
        private readonly IOperand _operand;

        /// <summary>
        /// Initializes a new instance of the <see cref="CardinalNameExpression"/> class
        /// </summary>
        /// <param name="operand">A <see cref="IOperand"/> representing part of the name expression</param>
        internal CardinalNameExpression(IOperand operand)
        {
            Contract.Requires(operand != null);

            _operand = operand;
        }

        /// <summary>
        /// Defines object invariants as required by code contracts
        /// </summary>
        [ContractInvariantMethod]
        private void ObjectInvariant()
        {
            Contract.Invariant(_operand != null);
        }

        /// <summary>
        /// Asynchronously builds a name of the associated instance
        /// </summary>
        /// <param name="culture">A <see cref="CultureInfo" /> specifying the language of the constructed name</param>
        /// <returns>A <see cref="Task{String}" /> representing the asynchronous operation</returns>
        /// <exception cref="NameExpressionException">Market specifiers do not contain required specifier</exception>
        public Task<string> BuildNameAsync(CultureInfo culture)
        {
            return _operand.GetStringValue();
        }
    }
}