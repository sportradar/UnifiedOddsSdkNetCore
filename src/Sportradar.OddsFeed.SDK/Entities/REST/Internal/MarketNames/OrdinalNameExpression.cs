/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using Dawn;
using System.Globalization;
using System.Threading.Tasks;
using Humanizer;
using Sportradar.OddsFeed.SDK.Common.Exceptions;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Internal.MarketNames
{
    /// <summary>
    /// A <see cref="INameExpression"/> implementation supporting '!' based expressions (e.g. {!reply_nr})
    /// </summary>
    internal class OrdinalNameExpression : INameExpression
    {
        /// <summary>
        /// A <see cref="IOperand"/> representing part of the name expression
        /// </summary>
        private readonly IOperand _operand;

        /// <summary>
        /// Initializes a new instance of the <see cref="OrdinalNameExpression"/> class
        /// </summary>
        /// <param name="operand">A <see cref="IOperand"/> representing part of the name expression</param>
        internal OrdinalNameExpression(IOperand operand)
        {
            Guard.Argument(operand, nameof(operand)).NotNull();

            _operand = operand;
        }

        /// <summary>
        /// Asynchronously builds a name of the associated instance
        /// </summary>
        /// <param name="culture">A <see cref="CultureInfo" /> specifying the language of the constructed name</param>
        /// <returns>A <see cref="Task{String}" /> representing the asynchronous operation</returns>
        /// <exception cref="NameExpressionException">Error occurred while evaluating name expression</exception>
        public async Task<string> BuildNameAsync(CultureInfo culture)
        {
            var value = await _operand.GetIntValue().ConfigureAwait(false);
            return value.Ordinalize();
        }
    }
}