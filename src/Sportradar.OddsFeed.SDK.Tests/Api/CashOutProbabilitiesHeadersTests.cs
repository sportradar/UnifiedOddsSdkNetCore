// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System.Collections.Generic;
using System.Threading.Tasks;
using Shouldly;
using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Entities;
using Sportradar.OddsFeed.SDK.Entities.Rest;
using Sportradar.OddsFeed.SDK.Messages.Rest;
using Sportradar.OddsFeed.SDK.Tests.Common.Builders.CashOut;
using Sportradar.OddsFeed.SDK.Tests.Common.Builders.Extensions;
using Xunit;
using static Sportradar.OddsFeed.SDK.Tests.Common.Helpers.DataFetcherMockHelper;

namespace Sportradar.OddsFeed.SDK.Tests.Api;

public class CashOutProbabilitiesHeadersTests
{
    private static readonly Urn AnyEventId = new("sr", "match", 12345);

    [Fact]
    public async Task CashOutProbabilitiesMessageHeadersAreAlwaysEmpty()
    {
        var headers = new Dictionary<string, string>
        {
            { "Product", "ODDS_THIRD_PARTIES_PROVIDER|nvenue" },
            { "NULL_HEADER", null },
            { "custom_header", 5.ToString() }
        };

        var cashoutMessage = CreateCashoutMessage(headers);
        var fetcher = GetDataFetcherProvidingCashout(cashoutMessage);

        var provider = CashOutProbabilitiesProviderBuilder.Create()
                                                              .AddDefaultDependencies()
                                                              .WithConfigurationWithLanguage()
                                                              .WithCashOutFetcher(fetcher)
                                                              .Build();

        var result = await provider.GetCashOutProbabilitiesAsync<ISportEvent>(AnyEventId);

        var messageV2 = (IMessageV2)result;
        messageV2.ShouldNotBeNull();
        messageV2.MessageHeaders.ShouldBeEmpty();
    }

    private static cashout CreateCashoutMessage(IReadOnlyDictionary<string, string> headers)
    {
        return new cashout
        {
            event_id = AnyEventId.ToString(),
            product = 1,
            timestamp = 1234567890,
            MessageHeaders = headers
        };
    }
}
