// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using Shouldly;
using Sportradar.OddsFeed.SDK.Messages.Feed;
using Sportradar.OddsFeed.SDK.Tests.Common;
using Sportradar.OddsFeed.SDK.Tests.Common.Dsl.Api.Markets;
using Sportradar.OddsFeed.SDK.Tests.Common.Dsl.Api.SportEventBlock;
using Sportradar.OddsFeed.SDK.Tests.Common.Dsl.Feed;
using Xunit;
using Xunit.Abstractions;

namespace Sportradar.OddsFeed.SDK.Tests.Entities.OutMarket;

[SuppressMessage("Performance", "CA1859:Use concrete types when possible for improved performance")]
public class MarketWithOddsNameForCompetitorMarketsTests : MarketNameSetup
{
    // ReSharper disable once ConvertToPrimaryConstructor
    public MarketWithOddsNameForCompetitorMarketsTests(ITestOutputHelper testOutputHelper)
        : base(testOutputHelper)
    {
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task MarketNameIsGeneratedWithMatchCompetitor(bool useNonSrCompetitorId)
    {
        const string specifiers = "total=1.5";
        const string expectedMarketNameTemplate = "Innings 1 to 5th top - {$competitor1} total";
        const string expectedMarketName = "Innings 1 to 5th top - Liverpool FC total";
        const string outcomeId = "13";
        const string expectedOutcomeNameTemplate = "under {total}";
        const string expectedOutcomeName = "under 1.5";

        var apiMarketDescription = MdForInvariantList.GetMarket282WithCompetitorInMarketName();
        var competitorProfile = Soccer.GetHomeCompetitorProfileEndpoint();
        var matchEndpoint = Soccer.Summary();

        if (useNonSrCompetitorId)
        {
            competitorProfile.competitor.id = "bg:competitor:44";
            matchEndpoint = ReplaceMatchCompetitorPrefixFromSrToBg(matchEndpoint);
        }

        SetupInvariantMarketListEndpoint(apiMarketDescription, DefaultLanguage);
        SetupCompetitionWithCompetitorIds(matchEndpoint);
        SetupProfileCacheToReturnCompetitorNameWithoutFetching(competitorProfile, DefaultLanguage);

        var marketOddsChange = CreateMarketWithOdds282WithSpecifiers(specifiers);

        var market = MarketFactory.GetMarketWithOdds(CompetitionMock.Object, marketOddsChange, TestConsts.AnyProducerId, TestConsts.AnySportId, [DefaultLanguage]);
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

    [Theory]
    [InlineData("sr:competitor:44")]
    [InlineData("bg:competitor:44")]
    public async Task OutcomeNameIsGeneratedWithCompetitorReferenceInSpecifier(string specifierCompetitor)
    {
        var specifiers = $"competitor={specifierCompetitor}|total=1.5";
        const string expectedMarketNameTemplate = "{%competitor} total pass completions (incl. overtime)";
        const string expectedMarketName = "Liverpool FC total pass completions (incl. overtime)";
        const string outcomeId = "13";
        const string expectedOutcomeNameTemplate = "{%competitor} ({+total})";
        const string expectedOutcomeName = "Liverpool FC (+1.5)";

        var apiMarketDescription = MdForInvariantList.GetMarket282WithCompetitorInMarketName();
        apiMarketDescription.name = expectedMarketNameTemplate;
        var apiOutcome = apiMarketDescription.outcomes.First(o => o.id == outcomeId);
        apiOutcome.name = expectedOutcomeNameTemplate;
        var competitorProfile = Soccer.GetHomeCompetitorProfileEndpoint();
        competitorProfile.competitor.id = specifierCompetitor;

        SetupInvariantMarketListEndpoint(apiMarketDescription, DefaultLanguage);
        SetupProfileCacheToReturnCompetitorNameWithFetching(competitorProfile, DefaultLanguage);

        var marketOddsChange = CreateMarketWithOdds282WithSpecifiers(specifiers);

        var market = MarketFactory.GetMarketWithOdds(AnySportEventMock.Object, marketOddsChange, TestConsts.AnyProducerId, TestConsts.AnySportId, [DefaultLanguage]);
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

    private static oddsChangeMarket CreateMarketWithOdds282WithSpecifiers(string specifiers)
    {
        return MarketWithOddsBuilder.Create()
            .WithMarketId(282)
            .WithOutcome("13", 1.5)
            .WithOutcome("12", 2.5)
            .WithSpecifiers(specifiers)
            .Build();
    }
}
