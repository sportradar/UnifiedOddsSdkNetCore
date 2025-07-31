// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using Moq;
using Shouldly;
using Sportradar.OddsFeed.SDK.Tests.Common.Builders;
using Sportradar.OddsFeed.SDK.Tests.Common.Dsl.Api.Markets;
using Sportradar.OddsFeed.SDK.Tests.Common.Dsl.Feed;
using Xunit;
using Xunit.Abstractions;

namespace Sportradar.OddsFeed.SDK.Tests.Entities.OutMarket;

[SuppressMessage("Performance", "CA1859:Use concrete types when possible for improved performance")]
public class MarketWithOddsNameTests : MarketNameSetup
{
    public MarketWithOddsNameTests(ITestOutputHelper testOutputHelper)
        : base(testOutputHelper)
    {
    }

    [Theory]
    [InlineData(null, "When will the 2nd run be scored (incl. extra innings)")]
    [InlineData("When will the goal be scored", "When will the goal be scored")]
    [InlineData("When will the {runnr} run be scored", "When will the 2 run be scored")]
    [InlineData("When will the {!runnr} run be scored", "When will the 2nd run be scored")]
    [InlineData("When will the {!inningnr} run be scored", "When will the 5th run be scored")]
    [InlineData("When will the {+runnr} run be scored", "When will the +2 run be scored")]
    [InlineData("When will the {-runnr} run be scored", "When will the -2 run be scored")]
    [InlineData("When will the {!runnr} run in {inningnr} be scored", "When will the 2nd run in 5 be scored")]
    [InlineData("When will the {(runnr+1)} run be scored", "When will the 3 run be scored")]
    [InlineData("When will the {(runnr-1)} run be scored", "When will the 1 run be scored")]
    [InlineData("When will the {!(runnr+1)} run be scored", "When will the 3rd run be scored")]
    [InlineData("When will the {!(runnr-1)} run be scored", "When will the 1st run be scored")]
    [InlineData("When will the {(1+runnr)} run be scored", "When will the 3 run be scored")]
    [InlineData("When will the {(4-runnr)} run be scored", "When will the 2 run be scored")]
    [InlineData("When will the {!(1+runnr)} run be scored", "When will the 3rd run be scored")]
    [InlineData("When will the {!(5-runnr)} run be scored", "When will the 3rd run be scored")]
    public async Task WhenMarketNameIsRequestedThenReturnsCorrectName(string marketNameTemplate, string expectedMarketName)
    {
        const string specifiers = "inningnr=5|runnr=2";
        var apiMarketDescription = MdForInvariantList.GetOrdinalMarket739();
        SetupDataInMarketDescription(apiMarketDescription, marketNameTemplate, null);
        SetupInvariantMarketListEndpoint(apiMarketDescription, DefaultLanguage);

        var apiMarket = MarketWithOddsBuilder.Create()
                                             .WithMarketId(739)
                                             .WithOutcome("1826", 1.5)
                                             .WithOutcome("1828", 2.5)
                                             .WithOutcome("1829", 3.5)
                                             .WithOutcome("1830", 4.5)
                                             .WithSpecifiers(specifiers)
                                             .Build();

        var market = MarketFactory.GetMarketWithOdds(AnySportEventMock.Object, apiMarket, 1, UrnCreate.SportId(1), [DefaultLanguage]);

        var marketName = await market.GetNameAsync(DefaultLanguage);

        marketName.ShouldBe(expectedMarketName);
    }

    [Theory]
    [InlineData("1826", "other inning or no run", "other inning or no run")]
    [InlineData("1826", "{inningnr} inning", "5 inning")]
    [InlineData("1826", "{+inningnr} inning", "+5 inning")]
    [InlineData("1826", "{-inningnr} inning", "-5 inning")]
    [InlineData("1826", "{!inningnr} inning", "5th inning")]
    [InlineData("1826", "{!inningnr} inning and {runnr}", "5th inning and 2")]
    [InlineData("1826", "{(inningnr+1)} inning", "6 inning")]
    [InlineData("1826", "{(inningnr-1)} inning", "4 inning")]
    [InlineData("1826", "{(1+inningnr)} inning", "6 inning")]
    [InlineData("1826", "{(10-inningnr)} inning", "5 inning")]
    [InlineData("1826", "{!(inningnr+1)} inning", "6th inning")]
    [InlineData("1826", "{!(inningnr-1)} inning", "4th inning")]
    [InlineData("1826", "{!(1+inningnr)} inning", "6th inning")]
    [InlineData("1826", "{!(10-inningnr)} inning", "5th inning")]
    public async Task WhenOutcomeNameIsRequestedThenReturnsCorrectName(string outcomeId, string outcomeNameTemplate, string expectedOutcomeName)
    {
        const string specifiers = "inningnr=5|runnr=2";
        var newOutcomes = new Dictionary<string, string> { { outcomeId, outcomeNameTemplate } };
        var apiMarketDescription = MdForInvariantList.GetOrdinalMarket739();
        SetupDataInMarketDescription(apiMarketDescription, null, newOutcomes);
        SetupInvariantMarketListEndpoint(apiMarketDescription, DefaultLanguage);

        var apiMarket = MarketWithOddsBuilder.Create()
                                             .WithMarketId(739)
                                             .WithOutcome("1826", 1.8)
                                             .WithOutcome("1828", 2.8)
                                             .WithOutcome("1829", 3.8)
                                             .WithOutcome("1830", 4.8)
                                             .WithSpecifiers(specifiers)
                                             .Build();

        var market = MarketFactory.GetMarketWithOdds(AnySportEventMock.Object, apiMarket, 1, UrnCreate.SportId(1), [DefaultLanguage]);

        var outcome = market.OutcomeOdds.FirstOrDefault(o => o.Id == outcomeId);
        outcome.ShouldNotBeNull();

        var outcomeName = await outcome.GetNameAsync(DefaultLanguage);
        outcomeName.ShouldBe(expectedOutcomeName);
    }

    [Fact]
    public async Task MarketNameIsGeneratedFromInvariantDescriptionWithSpecifierValues()
    {
        const string specifiers = "inningnr=5|runnr=3";
        const string expectedMarketNameTemplate = "When will the {!runnr} run be scored (incl. extra innings)";
        const string expectedMarketName = "When will the 3rd run be scored (incl. extra innings)";
        const string outcomeId = "1828";
        const string expectedOutcomeNameTemplate = "{!(inningnr+1)} inning";
        const string expectedOutcomeName = "6th inning";

        var apiMarketDescription = MdForInvariantList.GetOrdinalMarket739();
        SetupInvariantMarketListEndpoint(apiMarketDescription, DefaultLanguage);

        var marketOddsChange = MarketWithOddsBuilder.Create()
            .WithMarketId(739)
            .WithOutcome("1826", 1.5)
            .WithOutcome("1828", 2.5)
            .WithOutcome("1829", 3.5)
            .WithOutcome("1830", 4.5)
            .WithSpecifiers(specifiers)
            .Build();

        var market = MarketFactory.GetMarketWithOdds(AnySportEventMock.Object, marketOddsChange, 1, UrnCreate.SportId(1), [DefaultLanguage]);
        var outcome = market.OutcomeOdds.First(o => o.Id == outcomeId);

        market.ShouldNotBeNull();
        outcome.ShouldNotBeNull();
        var marketName = await market.GetNameAsync(DefaultLanguage);
        var marketNameTemplate = market.MarketDefinition.GetNameTemplate(DefaultLanguage);
        var outcomeName = await outcome.GetNameAsync(DefaultLanguage);
        var outcomeNameTemplate = outcome.OutcomeDefinition.GetNameTemplate(DefaultLanguage);

        marketName.ShouldBe(expectedMarketName);
        marketNameTemplate.ShouldBe(apiMarketDescription.name);
        marketNameTemplate.ShouldBe(expectedMarketNameTemplate);
        outcomeName.ShouldBe(expectedOutcomeName);
        outcomeNameTemplate.ShouldBe(expectedOutcomeNameTemplate);
    }

    [Fact]
    public async Task MarketNameIsGeneratedFromVariantDescriptionWithSpecifierValues()
    {
        const string specifiers = "variant=sr:correct_score:bestof:12";
        const string expectedMarketNameTemplate = "Correct score";
        const string expectedMarketName = "Correct score";
        const string outcomeId = "sr:correct_score:bestof:12:196";
        const string expectedOutcomeNameTemplate = "7:4";
        const string expectedOutcomeName = "7:4";

        var apiMarketDescription = MdForInvariantList.GetMarket199ForVariantList();
        SetupInvariantMarketListEndpoint(apiMarketDescription, DefaultLanguage);
        SetupVariantMarketListEndpoint(MdForVariantList.GetCorrectScoreBestOf12(), DefaultLanguage);

        var marketOddsChange = MarketWithOddsBuilder.Create()
            .WithMarketId(199)
            .WithOutcome("sr:correct_score:bestof:12:192", 1.5)
            .WithOutcome("sr:correct_score:bestof:12:193", 2.5)
            .WithOutcome("sr:correct_score:bestof:12:196", 3.5)
            .WithOutcome("sr:correct_score:bestof:12:200", 4.5)
            .WithSpecifiers(specifiers)
            .Build();

        var market = MarketFactory.GetMarketWithOdds(AnySportEventMock.Object, marketOddsChange, 1, UrnCreate.SportId(1), [DefaultLanguage]);
        var outcome = market.OutcomeOdds.First(o => o.Id == outcomeId);

        market.ShouldNotBeNull();
        outcome.ShouldNotBeNull();
        var marketName = await market.GetNameAsync(DefaultLanguage);
        var marketNameTemplate = market.MarketDefinition.GetNameTemplate(DefaultLanguage);
        var outcomeName = await outcome.GetNameAsync(DefaultLanguage);
        var outcomeNameTemplate = outcome.OutcomeDefinition.GetNameTemplate(DefaultLanguage);

        marketName.ShouldBe(expectedMarketName);
        marketNameTemplate.ShouldBe(apiMarketDescription.name);
        marketNameTemplate.ShouldBe(expectedMarketNameTemplate);
        outcomeName.ShouldBe(expectedOutcomeName);
        outcomeNameTemplate.ShouldBe(expectedOutcomeNameTemplate);
    }

    [Fact]
    public async Task MarketNameIsGeneratedFromSingleVariantDescriptionWithSpecifierValues()
    {
        const string specifiers = "variant=pre:markettext:289339";
        const string expectedMarketNameTemplate = "Magical Kenya Open 2025 - Round 1 - 18 Hole Match Bet - H Li vs A Saddier";
        const string expectedMarketName = "Magical Kenya Open 2025 - Round 1 - 18 Hole Match Bet - H Li vs A Saddier";
        const string outcomeId = "pre:outcometext:6388892";
        const string expectedOutcomeNameTemplate = "Saddier, Adrien";
        const string expectedOutcomeName = "Saddier, Adrien";

        var apiMarketDescription = MdForInvariantList.GetPreOutcomeTextMarket535();
        SetupInvariantMarketListEndpoint(apiMarketDescription, DefaultLanguage);
        var singleApiMarketDescription = MdForVariantSingle.GetPreMarketTextMarket535();
        SetupSingleVariantMarketEndpoint(singleApiMarketDescription, DefaultLanguage);

        var marketOddsChange = MarketWithOddsBuilder.Create()
            .WithMarketId(535)
            .WithOutcome("pre:outcometext:6388892", 1.5)
            .WithOutcome("pre:outcometext:5725004", 2.5)
            .WithSpecifiers(specifiers)
            .Build();

        var market = MarketFactory.GetMarketWithOdds(AnySportEventMock.Object, marketOddsChange, 1, UrnCreate.SportId(1), [DefaultLanguage]);
        var outcome = market.OutcomeOdds.First(o => o.Id == outcomeId);

        market.ShouldNotBeNull();
        outcome.ShouldNotBeNull();
        var marketName = await market.GetNameAsync(DefaultLanguage);
        var marketNameTemplate = market.MarketDefinition.GetNameTemplate(DefaultLanguage);
        var outcomeName = await outcome.GetNameAsync(DefaultLanguage);
        var outcomeNameTemplate = outcome.OutcomeDefinition.GetNameTemplate(DefaultLanguage);

        marketName.ShouldBe(expectedMarketName);
        marketNameTemplate.ShouldBe(singleApiMarketDescription.name);
        marketNameTemplate.ShouldBe(expectedMarketNameTemplate);
        outcomeName.ShouldBe(expectedOutcomeName);
        outcomeNameTemplate.ShouldBe(expectedOutcomeNameTemplate);

        VerifySingleVariantProviderWasCalled(Times.Once());
    }
}
