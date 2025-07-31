// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using Shouldly;
using Sportradar.OddsFeed.SDK.Common.Exceptions;
using Sportradar.OddsFeed.SDK.Entities;
using Sportradar.OddsFeed.SDK.Tests.Common.Builders;
using Sportradar.OddsFeed.SDK.Tests.Common.Dsl.Api.Markets;
using Sportradar.OddsFeed.SDK.Tests.Common.Dsl.Api.SportEventBlock;
using Sportradar.OddsFeed.SDK.Tests.Common.Dsl.Feed;
using Xunit;
using Xunit.Abstractions;

namespace Sportradar.OddsFeed.SDK.Tests.Entities.OutMarket;

[SuppressMessage("Performance", "CA1859:Use concrete types when possible for improved performance")]
public class MarketWithErrorNameTemplateTests : MarketNameSetup
{
    public MarketWithErrorNameTemplateTests(ITestOutputHelper testOutputHelper)
        : base(testOutputHelper)
    {
    }

    [Fact]
    public async Task MarketNameTemplateHasWrongSpecifierPlaceholderBeginningThenThrows()
    {
        const string specifiers = "inningnr=5|runnr=3";
        const string expectedMarketNameTemplate = "When will the {{!runnr}";

        var market = GetMarket739WithNewMarketNameTemplate(expectedMarketNameTemplate, specifiers);

        var exception = await Should.ThrowAsync<NameGenerationException>(() => market.GetNameAsync(DefaultLanguage));
        exception.ShouldNotBeNull();
        exception.Message.ShouldBe("Error occurred while evaluating the name expression");
        exception.InnerException.ShouldNotBeNull();
        exception.InnerException.ShouldBeOfType<NameExpressionException>();
        exception.InnerException.Message.ShouldBe("Market specifiers do not contain a specifier with key={!runnr");
    }

    [Fact]
    public async Task MarketNameTemplateHasMissingSpecifierPlaceholderBeginningThenThrows()
    {
        const string specifiers = "inningnr=5|runnr=3";
        const string expectedMarketNameTemplate = "When will the !runnr}";

        var market = GetMarket739WithNewMarketNameTemplate(expectedMarketNameTemplate, specifiers);

        var exception = await Should.ThrowAsync<NameGenerationException>(() => market.GetNameAsync(DefaultLanguage));
        exception.ShouldNotBeNull();
        exception.Message.ShouldBe("The name description parsing failed");
        exception.InnerException.ShouldNotBeNull();
        exception.InnerException.ShouldBeOfType<FormatException>();
        exception.InnerException.Message.ShouldBe("Format of the descriptor is incorrect. Each opening '{' must be closed by corresponding '}'");
    }

    [Fact]
    public async Task MarketNameTemplateHasMissingSpecifierPlaceholderEndThenThrows()
    {
        const string specifiers = "inningnr=5|runnr=3";
        const string expectedMarketNameTemplate = "{!runnr";

        var market = GetMarket739WithNewMarketNameTemplate(expectedMarketNameTemplate, specifiers);

        var exception = await Should.ThrowAsync<NameGenerationException>(() => market.GetNameAsync(DefaultLanguage));
        exception.ShouldNotBeNull();
        exception.Message.ShouldBe("The name description parsing failed");
        exception.InnerException.ShouldNotBeNull();
        exception.InnerException.ShouldBeOfType<FormatException>();
        exception.InnerException.Message.ShouldBe("Format of the descriptor is incorrect. Each opening '{' must be closed by corresponding '}'");
    }

    [Fact]
    public async Task MarketNameTemplateHasMissingSpecifierPlaceholderDoubleEndThenThrows()
    {
        const string specifiers = "inningnr=5|runnr=3";
        const string expectedMarketNameTemplate = "{!runnr}}";

        var market = GetMarket739WithNewMarketNameTemplate(expectedMarketNameTemplate, specifiers);

        var exception = await Should.ThrowAsync<NameGenerationException>(() => market.GetNameAsync(DefaultLanguage));
        exception.ShouldNotBeNull();
        exception.Message.ShouldBe("The name description parsing failed");
        exception.InnerException.ShouldNotBeNull();
        exception.InnerException.ShouldBeOfType<FormatException>();
        exception.InnerException.Message.ShouldBe("Format of the descriptor is incorrect. Each opening '{' must be closed by corresponding '}'");
    }
    [Fact]
    public async Task MarketNameTemplateHasMissingSpecifierPlaceholderDoubleThenThrows()
    {
        const string specifiers = "inningnr=5|runnr=3";
        const string expectedMarketNameTemplate = "{{!runnr}}";

        var market = GetMarket739WithNewMarketNameTemplate(expectedMarketNameTemplate, specifiers);

        var exception = await Should.ThrowAsync<NameGenerationException>(() => market.GetNameAsync(DefaultLanguage));
        exception.ShouldNotBeNull();
        exception.Message.ShouldBe("The name description parsing failed");
        exception.InnerException.ShouldNotBeNull();
        exception.InnerException.ShouldBeOfType<FormatException>();
        exception.InnerException.Message.ShouldBe("Format of the descriptor is incorrect. Each opening '{' must be closed by corresponding '}'");
    }

    [Fact]
    public async Task MarketNameTemplateHasMissingSpecifierBetweenPlaceholderThenThrows()
    {
        const string specifiers = "inningnr=5|runnr=3";
        const string expectedMarketNameTemplate = "When will the {}";

        var market = GetMarket739WithNewMarketNameTemplate(expectedMarketNameTemplate, specifiers);

        var exception = await Should.ThrowAsync<NameGenerationException>(() => market.GetNameAsync(DefaultLanguage));
        exception.ShouldNotBeNull();
        exception.Message.ShouldBe("The name description parsing failed");
        exception.InnerException.ShouldNotBeNull();
        exception.InnerException.ShouldBeOfType<FormatException>();
        exception.InnerException.Message.ShouldBe("Format of the 'expression' is not correct. Minimum required length is 3");
    }

    [Fact]
    public async Task OutcomeNameIsGeneratedForGoalScorerMarketWithOutcomesGeneratedForListOfAssociatedPlayersWithNonPlayerIdThenThrowsWrongId()
    {
        const string specifiers = "type=live";
        const string expectedMarketNameTemplate = "Anytime goalscorer";
        const string expectedMarketName = "Anytime goalscorer";
        const string outcomeId = "sr:player:151545,sr:competition_group:784655"; // Incorrect ID format
        const string expectedOutcomeNameTemplate = "sr:player:151545,sr:competition_group:784655";

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
        innerException.Message.ShouldBe("OutcomeId=sr:competition_group:784655 must contain ':player:' or ':competitor:'");

        marketName.ShouldBe(expectedMarketName);
        marketNameTemplate.ShouldBe(apiMarketDescription.name);
        marketNameTemplate.ShouldBe(expectedMarketNameTemplate);
        outcomeNameTemplate.ShouldBe(expectedOutcomeNameTemplate);
    }

    private IMarketWithOdds GetMarket739WithNewMarketNameTemplate(string newMarketNameTemplate, string specifiers)
    {
        var apiMarketDescription = MdForInvariantList.GetOrdinalMarket739();
        apiMarketDescription.name = newMarketNameTemplate;

        SetupInvariantMarketListEndpoint(apiMarketDescription, DefaultLanguage);

        var marketOddsChange = MarketWithOddsBuilder.Create()
                                                    .WithMarketId(739)
                                                    .WithOutcome("1826", 1.5)
                                                    .WithOutcome("1828", 2.5)
                                                    .WithSpecifiers(specifiers)
                                                    .Build();

        return MarketFactory.GetMarketWithOdds(AnySportEventMock.Object, marketOddsChange, 1, UrnCreate.SportId(1), [DefaultLanguage]);
    }
}
