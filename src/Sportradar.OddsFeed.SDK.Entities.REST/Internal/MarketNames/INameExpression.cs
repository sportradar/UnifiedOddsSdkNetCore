/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Threading.Tasks;
using Sportradar.OddsFeed.SDK.Common.Exceptions;
using Sportradar.OddsFeed.SDK.Entities.REST.Contracts;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Internal.MarketNames
{
    /// <summary>
    /// Defines a contract implemented by classes used to generate market / selection names from their name formats / patterns
    /// </summary>
    [ContractClass(typeof(NameExpressionContract))]
    public interface INameExpression
    {
        /// <summary>
        /// Asynchronously builds a name of the associated instance
        /// </summary>
        /// <param name="culture">A <see cref="CultureInfo"/> specifying the language of the constructed name</param>
        /// <returns>A <see cref="Task{String}"/> representing the asynchronous operation</returns>
        /// <exception cref="NameExpressionException">Error occurred while evaluating name expression</exception>
        Task<string> BuildNameAsync(CultureInfo culture);
    }
}
