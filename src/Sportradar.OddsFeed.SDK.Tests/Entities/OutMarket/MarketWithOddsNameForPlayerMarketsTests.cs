// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using Shouldly;
using Sportradar.OddsFeed.SDK.Common.Exceptions;
using Sportradar.OddsFeed.SDK.Tests.Common.Builders;
using Sportradar.OddsFeed.SDK.Tests.Common.Dsl.Api.Endpoints;
using Sportradar.OddsFeed.SDK.Tests.Common.Dsl.Api.Markets;
using Sportradar.OddsFeed.SDK.Tests.Common.Dsl.Api.SportEventBlock;
using Sportradar.OddsFeed.SDK.Tests.Common.Dsl.Feed;
using Xunit;
using Xunit.Abstractions;

namespace Sportradar.OddsFeed.SDK.Tests.Entities.OutMarket;

[SuppressMessage("Performance", "CA1859:Use concrete types when possible for improved performance")]
public class MarketWithOddsNameForPlayerMarketsTests : MarketNameSetup
{
    public MarketWithOddsNameForPlayerMarketsTests(ITestOutputHelper testOutputHelper)
        : base(testOutputHelper)
    {
    }

    [Fact]
    public async Task MarketNameIsGeneratedWithPlayerReferenceInSpecifier()
    {
        const string specifiers = "player=sr:player:151545|total=1.5";
        const string expectedMarketNameTemplate = "{%player} total pass completions (incl. overtime)";
        const string expectedMarketName = "Van Dijk, Virgil total pass completions (incl. overtime)";
        const string outcomeId = "13";
        const string expectedOutcomeNameTemplate = "under {total}";
        const string expectedOutcomeName = "under 1.5";

        var apiMarketDescription = MdForInvariantList.GetMarket915WithPlayerInNameFromSpecifier();
        SetupInvariantMarketListEndpoint(apiMarketDescription, DefaultLanguage);
        SetupProfileCacheToReturnPlayerNameWithFetching(Soccer.GetHomeCompetitorPlayerProfile151545(), DefaultLanguage);

        var marketOddsChange = MarketWithOddsBuilder.Create()
                                                    .WithMarketId(915)
                                                    .WithOutcome("13", 1.5)
                                                    .WithOutcome("12", 2.5)
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
    public async Task MarketNameIsGeneratedWithNonSrPlayerReferenceInSpecifier()
    {
        const string specifiers = "player=bg:player:151545|total=1.5";
        const string expectedMarketNameTemplate = "{%player} total pass completions (incl. overtime)";
        const string expectedMarketName = "Van Dijk, Virgil total pass completions (incl. overtime)";
        const string outcomeId = "13";
        const string expectedOutcomeNameTemplate = "under {total}";
        const string expectedOutcomeName = "under 1.5";

        var apiMarketDescription = MdForInvariantList.GetMarket915WithPlayerInNameFromSpecifier();
        SetupInvariantMarketListEndpoint(apiMarketDescription, DefaultLanguage);
        var playerProfile = Soccer.GetHomeCompetitorPlayerProfile151545();
        playerProfile.player.id = "bg:player:151545"; // Change the player ID to a non-sr format
        SetupProfileCacheToReturnPlayerNameWithFetching(playerProfile, DefaultLanguage);

        var marketOddsChange = MarketWithOddsBuilder.Create()
                                                    .WithMarketId(915)
                                                    .WithOutcome("13", 1.5)
                                                    .WithOutcome("12", 2.5)
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
    public async Task OutcomeNameIsGeneratedWithPlayerReferenceInSpecifier()
    {
        const string specifiers = "player1=sr:player:151545|player2=sr:player:784655|hcp=1.5";
        const string expectedMarketNameTemplate = "Batter head2head (handicap)";
        const string expectedMarketName = "Batter head2head (handicap)";
        const string outcomeId = "1816";
        const string expectedOutcomeNameTemplate = "{%player1} ({+hcp})";
        const string expectedOutcomeName = "Van Dijk, Virgil (+1.5)";

        var apiMarketDescription = MdForInvariantList.GetMarket717WithPlayerInOutcomeNameFromSpecifier();
        SetupInvariantMarketListEndpoint(apiMarketDescription, DefaultLanguage);
        SetupProfileCacheToReturnPlayerNameWithFetching(Soccer.GetHomeCompetitorPlayerProfile151545(), DefaultLanguage);

        var marketOddsChange = MarketWithOddsBuilder.Create()
                                                    .WithMarketId(717)
                                                    .WithOutcome("1816", 1.5)
                                                    .WithOutcome("1817", 2.5)
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
    public async Task OutcomeNameIsGeneratedWithNonSrPlayerReferenceInSpecifier()
    {
        const string specifiers = "player1=bg:player:151545|player2=sr:player:784655|hcp=1.5";
        const string expectedMarketNameTemplate = "Batter head2head (handicap)";
        const string expectedMarketName = "Batter head2head (handicap)";
        const string outcomeId = "1816";
        const string expectedOutcomeNameTemplate = "{%player1} ({+hcp})";
        const string expectedOutcomeName = "Van Dijk, Virgil (+1.5)";

        var apiMarketDescription = MdForInvariantList.GetMarket717WithPlayerInOutcomeNameFromSpecifier();
        SetupInvariantMarketListEndpoint(apiMarketDescription, DefaultLanguage);
        var playerProfile = Soccer.GetHomeCompetitorPlayerProfile151545();
        playerProfile.player.id = "bg:player:151545"; // Change the player ID to a non-sr format
        SetupProfileCacheToReturnPlayerNameWithFetching(playerProfile, DefaultLanguage);

        var marketOddsChange = MarketWithOddsBuilder.Create()
                                                    .WithMarketId(717)
                                                    .WithOutcome("1816", 1.5)
                                                    .WithOutcome("1817", 2.5)
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
    public async Task MarketNameIsGeneratedWithPlayerWithoutReferenceInSpecifierDefinition()
    {
        const string specifiers = "player1=sr:player:151545|player2=sr:player:784655|inningnr=1";
        const string expectedMarketNameTemplate = "{!inningnr} innings - {%player1} or {%player2} dismissal method";
        const string expectedMarketName = "1st innings - Van Dijk, Virgil or Ruiz, Fabian dismissal method";
        const string outcomeId = "1808";
        const string expectedOutcomeNameTemplate = "keeper catch";
        const string expectedOutcomeName = "keeper catch";

        var apiMarketDescription = MdForInvariantList.GetMarket1140WithPlayerInNameWithoutSpecifier();
        SetupInvariantMarketListEndpoint(apiMarketDescription, DefaultLanguage);
        SetupProfileCacheToReturnPlayerNameWithFetching(Soccer.GetHomeCompetitorPlayerProfile151545(), DefaultLanguage);
        SetupProfileCacheToReturnPlayerNameWithFetching(Soccer.GetAwayCompetitorPlayerProfile784655(), DefaultLanguage);

        var marketOddsChange = MarketWithOddsBuilder.Create()
                                                    .WithMarketId(1140)
                                                    .WithOutcome("1806", 1.1)
                                                    .WithOutcome("1807", 1.2)
                                                    .WithOutcome("1808", 1.5)
                                                    .WithOutcome("1809", 2.5)
                                                    .WithOutcome("1810", 3.5)
                                                    .WithOutcome("1811", 4.5)
                                                    .WithOutcome("1812", 5.5)
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
    public async Task PlayerPropsMarket()
    {
        const string specifiers = "variant=pre:playerprops:35432179:608000";
        const string expectedMarketNameTemplate = "Player points (incl. overtime)";
        const string expectedMarketName = "Player points (incl. overtime)";
        const string outcomeId = "pre:playerprops:35432179:608000:22";
        const string expectedOutcomeNameTemplate = "Kyle Anderson 22+";
        const string expectedOutcomeName = "Kyle Anderson 22+";

        SetupInvariantMarketListEndpoint(MdForInvariantList.GetPlayerPropsMarket768(), DefaultLanguage);
        SetupSingleVariantMarketEndpoint(MdForVariantSingle.GetPlayerPropsMarket768(), DefaultLanguage);
        SetupProfileCacheToReturnPlayerNameWithoutFetching(Soccer.GetHomeCompetitorPlayerProfile151545(), DefaultLanguage);
        SetupProfileCacheToReturnPlayerNameWithoutFetching(Soccer.GetAwayCompetitorPlayerProfile784655(), DefaultLanguage);

        var marketOddsChange = MarketWithOddsBuilder.Create()
                                                    .WithMarketId(768)
                                                    .WithOutcome("pre:playerprops:35432179:608000:22", 1.1)
                                                    .WithOutcome("pre:playerprops:35432179:608000:23", 1.2)
                                                    .WithOutcome("pre:playerprops:35432179:608000:24", 1.5)
                                                    .WithOutcome("pre:playerprops:35432179:608000:20", 15.5)
                                                    .WithOutcome("pre:playerprops:35432179:608000:21", 16.5)
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
        marketNameTemplate.ShouldBe(expectedMarketNameTemplate);
        outcomeName.ShouldBe(expectedOutcomeName);
        outcomeNameTemplate.ShouldBe(expectedOutcomeNameTemplate);
    }

    [Fact]
    public async Task OutcomeNameIsGeneratedForGoalScorerMarketWithOutcomesGeneratedFromAssociatedPlayers()
    {
        const string specifiers = "type=live";
        const string expectedMarketNameTemplate = "Anytime goalscorer";
        const string expectedMarketName = "Anytime goalscorer";
        const string outcomeId = "sr:player:151545";
        const string expectedOutcomeNameTemplate = "sr:player:151545";
        const string expectedOutcomeName = "Van Dijk, Virgil";

        var apiMarketDescription = MdForInvariantList.GetGoalScorerMarket40();
        SetupInvariantMarketListEndpoint(apiMarketDescription, DefaultLanguage);
        SetupCompetitionWithCompetitorIds(Soccer.Summary(), DefaultLanguage);
        SetupProfileCacheToReturnCompetitorNameWithFetching(Soccer.GetHomeCompetitorProfileEndpoint(), DefaultLanguage);
        SetupProfileCacheToReturnCompetitorNameWithFetching(Soccer.GetAwayCompetitorProfileEndpoint(), DefaultLanguage);
        SetupProfileCacheToReturnPlayerNameWithoutFetching(Soccer.GetHomeCompetitorPlayerProfile151545(), DefaultLanguage);
        SetupProfileCacheToReturnPlayerNameWithoutFetching(Soccer.GetAwayCompetitorPlayerProfile784655(), DefaultLanguage);

        var marketOddsChange = MarketWithOddsBuilder.Create()
                                                    .WithMarketId(40)
                                                    .WithOutcome("sr:player:151545", 1.1)
                                                    .WithOutcome("sr:player:784655", 1.2)
                                                    .WithOutcome("1716", 1.5)
                                                    .WithSpecifiers(specifiers)
                                                    .Build();

        var market = MarketFactory.GetMarketWithOdds(CompetitionMock.Object, marketOddsChange, 1, UrnCreate.SportId(1), [DefaultLanguage]);
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
    public async Task OutcomeNameIsGeneratedForGoalScorerMarketWithOutcomesGeneratedForListOfAssociatedPlayers()
    {
        const string specifiers = "type=live";
        const string expectedMarketNameTemplate = "Anytime goalscorer";
        const string expectedMarketName = "Anytime goalscorer";
        const string outcomeId = "sr:player:151545,sr:player:784655";
        const string expectedOutcomeNameTemplate = "sr:player:151545,sr:player:784655";
        const string expectedOutcomeName = "Van Dijk, Virgil,Ruiz, Fabian";

        var apiMarketDescription = MdForInvariantList.GetGoalScorerMarket40();
        SetupInvariantMarketListEndpoint(apiMarketDescription, DefaultLanguage);
        SetupCompetitionWithCompetitorIds(Soccer.Summary(), DefaultLanguage);
        SetupProfileCacheToReturnCompetitorNameWithFetching(Soccer.GetHomeCompetitorProfileEndpoint(), DefaultLanguage);
        SetupProfileCacheToReturnCompetitorNameWithFetching(Soccer.GetAwayCompetitorProfileEndpoint(), DefaultLanguage);
        SetupProfileCacheToReturnPlayerNameWithoutFetching(Soccer.GetHomeCompetitorPlayerProfile151545(), DefaultLanguage);
        SetupProfileCacheToReturnPlayerNameWithoutFetching(Soccer.GetAwayCompetitorPlayerProfile784655(), DefaultLanguage);

        var marketOddsChange = MarketWithOddsBuilder.Create()
                                                    .WithMarketId(40)
                                                    .WithOutcome("sr:player:151545,sr:player:784655", 1.1)
                                                    .WithOutcome("sr:player:784655", 1.2)
                                                    .WithOutcome("1716", 1.5)
                                                    .WithSpecifiers(specifiers)
                                                    .Build();

        var market = MarketFactory.GetMarketWithOdds(CompetitionMock.Object, marketOddsChange, 1, UrnCreate.SportId(1), [DefaultLanguage]);
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
    public async Task OutcomeNameIsGeneratedForGoalScorerMarketWithOutcomesGeneratedForListOfAssociatedPlayersWithInvalidIdThenThrowsWrongUrn()
    {
        const string specifiers = "type=live";
        const string expectedMarketNameTemplate = "Anytime goalscorer";
        const string expectedMarketName = "Anytime goalscorer";
        const string outcomeId = "sr:player:151545,anything:784655"; // Incorrect ID format
        const string expectedOutcomeNameTemplate = "sr:player:151545,anything:784655";

        var apiMarketDescription = MdForInvariantList.GetGoalScorerMarket40();
        SetupInvariantMarketListEndpoint(apiMarketDescription, DefaultLanguage);
        SetupCompetitionWithCompetitorIds(Soccer.Summary(), DefaultLanguage);
        SetupProfileCacheToReturnCompetitorNameWithFetching(Soccer.GetHomeCompetitorProfileEndpoint(), DefaultLanguage);
        SetupProfileCacheToReturnCompetitorNameWithFetching(Soccer.GetAwayCompetitorProfileEndpoint(), DefaultLanguage);
        SetupProfileCacheToReturnPlayerNameWithoutFetching(Soccer.GetHomeCompetitorPlayerProfile151545(), DefaultLanguage);
        SetupProfileCacheToReturnPlayerNameWithoutFetching(Soccer.GetAwayCompetitorPlayerProfile784655(), DefaultLanguage);

        var marketOddsChange = MarketWithOddsBuilder.Create()
                                                    .WithMarketId(40)
                                                    .WithOutcome(outcomeId, 1.1)
                                                    .WithOutcome("sr:player:784655", 1.2)
                                                    .WithOutcome("1716", 1.5)
                                                    .WithSpecifiers(specifiers)
                                                    .Build();

        var market = MarketFactory.GetMarketWithOdds(CompetitionMock.Object, marketOddsChange, 1, UrnCreate.SportId(1), [DefaultLanguage]);
        var outcome = market.OutcomeOdds.First(o => o.Id == outcomeId);

        market.ShouldNotBeNull();
        outcome.ShouldNotBeNull();
        var marketNameTemplate = market.MarketDefinition.GetNameTemplate(DefaultLanguage);
        var marketName = await market.GetNameAsync(DefaultLanguage);
        var outcomeNameTemplate = outcome.OutcomeDefinition.GetNameTemplate(DefaultLanguage);
        var exception = await Should.ThrowAsync<NameGenerationException>(() => _ = outcome.GetNameAsync(DefaultLanguage));

        exception.ShouldNotBeNull();
        exception.ShouldBeAssignableTo<NameGenerationException>();
        exception.Message.ShouldBe("Failed to generate outcome name from profile");
        exception.InnerException.ShouldNotBeNull();
        var innerException = exception.InnerException as NameExpressionException;
        innerException.ShouldNotBeNull();
        innerException.Message.ShouldBe("OutcomeId=anything:784655 is not a valid urn");

        marketName.ShouldBe(expectedMarketName);
        marketNameTemplate.ShouldBe(apiMarketDescription.name);
        marketNameTemplate.ShouldBe(expectedMarketNameTemplate);
        outcomeNameTemplate.ShouldBe(expectedOutcomeNameTemplate);
    }

    [Fact]
    public async Task MarketNameIsGeneratedForGoalScorerMarketWithOutcomesGeneratedFromAssociatedNonSrPlayers()
    {
        const string specifiers = "type=live";
        const string expectedMarketNameTemplate = "Anytime goalscorer";
        const string expectedMarketName = "Anytime goalscorer";
        const string outcomeId = "bg:player:151545";
        const string expectedOutcomeNameTemplate = "bg:player:151545";
        const string expectedOutcomeName = "Van Dijk, Virgil";

        var apiMarketDescription = MdForInvariantList.GetGoalScorerMarket40();
        SetupInvariantMarketListEndpoint(apiMarketDescription, DefaultLanguage);
        // SetupCompetitionWithCompetitorIds(Soccer.Summary(), DefaultLanguage);
        // var homeCompetitorProfile = Soccer.GetHomeCompetitorProfileEndpoint();
        // var awayCompetitorProfile = Soccer.GetAwayCompetitorProfileEndpoint();
        // SetupProfileCacheToReturnCompetitorName(ReplacePlayerPrefixFromSrToBg(homeCompetitorProfile), DefaultLanguage);
        // SetupProfileCacheToReturnCompetitorName(ReplacePlayerPrefixFromSrToBg(awayCompetitorProfile), DefaultLanguage);
        var playerProfile151545 = Soccer.GetHomeCompetitorPlayerProfile151545();
        playerProfile151545.player.id = "bg:player:151545"; // Change the player ID to a non-sr format
        SetupProfileCacheToReturnPlayerNameWithoutFetching(playerProfile151545, DefaultLanguage);
        SetupProfileCacheToReturnPlayerNameWithoutFetching(Soccer.GetAwayCompetitorPlayerProfile784655(), DefaultLanguage);

        var marketOddsChange = MarketWithOddsBuilder.Create()
                                                    .WithMarketId(40)
                                                    .WithOutcome("bg:player:151545", 1.1)
                                                    .WithOutcome("bg:player:784655", 1.2)
                                                    .WithOutcome("1716", 1.5)
                                                    .WithSpecifiers(specifiers)
                                                    .Build();

        var market = MarketFactory.GetMarketWithOdds(CompetitionMock.Object, marketOddsChange, 1, UrnCreate.SportId(1), [DefaultLanguage]);
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
    public async Task GetNameWhenCalledWithLastGoalscorerMarketReturnsNameFromMarketDescription()
    {
        const string specifiers = "type=live";
        const string expectedMarketNameTemplate = "Anytime goalscorer";
        const string expectedMarketName = "Anytime goalscorer";
        const string outcomeId = "sr:player:151545";
        const string expectedOutcomeNameTemplate = "sr:player:151545";
        const string expectedOutcomeName = "Van Dijk, Virgil";

        var apiMarketDescription = MdForInvariantList.GetGoalScorerMarket40();
        SetupInvariantMarketListEndpoint(apiMarketDescription, DefaultLanguage);
        SetupCompetitionWithCompetitorIds(Soccer.Summary(), DefaultLanguage);
        SetupProfileCacheToReturnCompetitorNameWithFetching(Soccer.GetHomeCompetitorProfileEndpoint(), DefaultLanguage);
        SetupProfileCacheToReturnCompetitorNameWithFetching(Soccer.GetAwayCompetitorProfileEndpoint(), DefaultLanguage);
        SetupProfileCacheToReturnPlayerNameWithoutFetching(Soccer.GetHomeCompetitorPlayerProfile151545(), DefaultLanguage);
        SetupProfileCacheToReturnPlayerNameWithoutFetching(Soccer.GetAwayCompetitorPlayerProfile784655(), DefaultLanguage);

        var marketOddsChange = MarketWithOddsBuilder.Create()
                                                    .WithMarketId(40)
                                                    .WithOutcome("sr:player:151545", 1.1)
                                                    .WithOutcome("sr:player:784655", 1.2)
                                                    .WithOutcome("1716", 1.5)
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
    public async Task GetNameWhenCalledWithPlayerToScoreMarketReturnsNameThatContainsPlayerName()
    {
        const string playerName = "Raphinha";
        const string specifiers = "player=sr:player:956430";
        const string expectedMarketNameTemplate = "{%player} to score (incl. overtime)";
        const string expectedMarketName = "Raphinha to score (incl. overtime)";
        const string outcomeId = "74";
        const string expectedOutcomeNameTemplate = "yes";
        const string expectedOutcomeName = "yes";

        var apiMarketDescription = MdForInvariantList.GetPlayerToScoreIncludingOvertimeMarket882();
        SetupInvariantMarketListEndpoint(apiMarketDescription, DefaultLanguage);
        SetupProfileCacheToReturnPlayerNameWithFetching(PlayerProfileEndpoint.Build(builder => builder.WithId(UrnCreate.PlayerId(956430)).WithName(playerName)), DefaultLanguage);

        var marketOddsChange = MarketWithOddsBuilder.Create()
                                                    .WithMarketId(882)
                                                    .WithOutcome("74", 1.1)
                                                    .WithOutcome("76", 1.2)
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
}
