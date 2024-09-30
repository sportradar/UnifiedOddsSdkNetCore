// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System.Collections.Generic;
using System.Threading.Tasks;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.MarketNameGeneration;
using Sportradar.OddsFeed.SDK.Tests.Common;
using Xunit;

namespace Sportradar.OddsFeed.SDK.Tests.Entities.Rest.MarketNameGeneration;

public class OrdinalNameExpressionTests
{
    private readonly IOperandFactory _operandFactory = new OperandFactory();

    [Fact]
    public async Task OrdinalNameExpression()
    {
        var specifiers = new Dictionary<string, string> { { "reply_nr", "1" } };
        var expression = new OrdinalNameExpression(_operandFactory.BuildOperand(specifiers, "reply_nr"));

        var result = await expression.BuildNameAsync(TestData.Culture);

        Assert.Equal("1st", result);
    }
}
