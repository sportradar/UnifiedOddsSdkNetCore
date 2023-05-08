/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Threading.Tasks;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.MarketNames;
using Xunit;

namespace Sportradar.OddsFeed.SDK.Tests.Entities.REST.Markets
{
    public class OrdinalNameExpressionTests
    {
        private readonly IOperandFactory _operandFactory = new OperandFactory();

        [Fact]
        public async Task OrdinalNameExpression()
        {
            var specifiers = new Dictionary<string, string> { { "reply_nr", "1" } };
            var expression = new OrdinalNameExpression(_operandFactory.BuildOperand(new ReadOnlyDictionary<string, string>(specifiers), "reply_nr"));
            var result = await expression.BuildNameAsync(new CultureInfo("en"));

            Assert.Equal("1st", result);
        }
    }
}
