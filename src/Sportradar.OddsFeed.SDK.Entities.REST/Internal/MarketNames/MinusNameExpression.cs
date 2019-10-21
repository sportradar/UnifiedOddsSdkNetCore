/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using Dawn;
using System.Globalization;
using System.Threading.Tasks;
using Sportradar.OddsFeed.SDK.Common.Exceptions;
using Sportradar.OddsFeed.SDK.Common.Internal;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Internal.MarketNames
{
    /// <summary>
    /// A <see cref="INameExpression"/> implementation supporting '-' based expressions (i.e. {-reply_nr})
    /// </summary>
    /// <seealso cref="INameExpression" />
    internal class MinusNameExpression : INameExpression
    {
        /// <summary>
        /// A <see cref="IOperand"/> representing part of the name expression
        /// </summary>
        private readonly IOperand _operand;

        /// <summary>
        /// Initializes a new instance of the <see cref="MinusNameExpression"/> class.
        /// </summary>
        /// <param name="operand">A <see cref="IOperand"/> representing part of the name expression</param>
        internal MinusNameExpression(IOperand operand)
        {
            Guard.Argument(operand).NotNull();

            _operand = operand;
        }

        /// <summary>
        /// Asynchronously builds a name of the associated instance
        /// </summary>
        /// <param name="culture">A <see cref="CultureInfo" /> specifying the language of the constructed name</param>
        /// <returns>A <see cref="Task{String}" /> representing the asynchronous operation</returns>
        /// <exception cref="NameExpressionException">The specified specifier does not exist or it's value is not string representation of decimal</exception>
        public async Task<string> BuildNameAsync(CultureInfo culture)
        {
            var value = await _operand.GetDecimalValue().ConfigureAwait(false);
            var result = SdkInfo.DecimalToStringWithSign(value * -1);
            return result;
        }
    }
}