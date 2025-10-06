// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System.Linq;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Dto;
using Sportradar.OddsFeed.SDK.Messages.Rest;
using Sportradar.OddsFeed.SDK.Tests.Common;
using Sportradar.OddsFeed.SDK.Tests.Common.Dsl.Api.Markets;
using Xunit;

namespace Sportradar.OddsFeed.SDK.Tests;

public class ManualTests
{
    /// <summary>
    /// Always comment out when done testing
    /// </summary>
    [Fact]
    public void ManualTest()
    {
        // CheckIfSimpleTournamentCanBeConsumed();
        //CheckIfInvalidInvariantMarketListIsConsumed();
        CheckIfInvalidVariantMarketListIsConsumed();
        // Dummy(null);
    }

    [Fact]
    public void Manual2Test()
    {
        var apiData = MarketDescriptionEndpoint.GetDefaultVariantList();

        var dtoData = apiData.variant.Select(s => new VariantDescriptionDto(s));

        Assert.NotNull(dtoData);
        Assert.NotEmpty(dtoData);
    }

    // private void CheckIfInvalidInvariantMarketListIsConsumed()
    // {
    //     var xmlStream = FileHelper.GetResource("customformanualtest.xml");
    //     var deserializer = new Deserializer<market_descriptions>();
    //     var apiData = deserializer.Deserialize(xmlStream);
    //
    //     var dtoData = apiData.market.Select(s => new MarketDescriptionDto(s));
    //
    //     Assert.NotNull(dtoData);
    //     Assert.NotEmpty(dtoData);
    // }

    private void CheckIfInvalidVariantMarketListIsConsumed()
    {
        var xmlStream = FileHelper.GetResource("customformanualtest.xml");
        var deserializer = new Deserializer<variant_descriptions>();
        var apiData = deserializer.Deserialize(xmlStream);

        var dtoData = apiData.variant.Select(s => new VariantDescriptionDto(s));

        Assert.NotNull(dtoData);
        Assert.NotEmpty(dtoData);
    }
    //
    // private void CheckIfSimpleTournamentCanBeConsumed()
    // {
    //     var xmlStream = FileHelper.GetResource("customformanualtest.xml");
    //     var deserializer = new Deserializer<tournamentInfoEndpoint>();
    //     var apiTournamentInfoEndpoint = deserializer.Deserialize(xmlStream);
    //     var duplicates = apiTournamentInfoEndpoint.competitors.Where(s => s.reference_ids.Length == 1);
    //     var dtoTournamentInfo = new TournamentInfoDto(apiTournamentInfoEndpoint);
    //     //var dtoTournamentInfo = new TournamentInfoDto(duplicates);
    //
    //     Assert.NotNull(dtoTournamentInfo);
    // }
    //
    // private async Task Dummy(IMatch match)
    // {
    //     var matchStatus = await match.GetStatusAsync();
    //
    //     _ = matchStatus.HomeScore;
    //
    //     _ = matchStatus.AwayScore;
    //
    //     _ = (int)matchStatus.GetPropertyValue("CurrentServer");
    // }
}
