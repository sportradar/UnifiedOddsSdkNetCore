// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Dto;
using Sportradar.OddsFeed.SDK.Messages.Rest;
using Xunit;

namespace Sportradar.OddsFeed.SDK.Tests.Entities.CacheItems.InMarkets;

public partial class MarketDescriptionDtoSubClassTests
{
    [Fact]
    public void ConstructOutcomeDescriptionFromDescOutcomesOutcomeIsValid()
    {
        var apiDescOutcomes = new desc_outcomesOutcome { id = "some-id", description = "some-description", name = "some-name" };

        var outcomeDescriptionDto = new OutcomeDescriptionDto(apiDescOutcomes);

        Assert.NotNull(outcomeDescriptionDto);
        Assert.Equal(apiDescOutcomes.id, outcomeDescriptionDto.Id);
        Assert.Equal(apiDescOutcomes.name, outcomeDescriptionDto.Name);
        Assert.Equal(apiDescOutcomes.description, outcomeDescriptionDto.Description);
    }

    [Fact]
    public void ConstructOutcomeDescriptionWhenNullDescOutcomesOutcomeThenThrows()
    {
        Assert.Throws<ArgumentNullException>(() => new OutcomeDescriptionDto((desc_outcomesOutcome)null));
    }

    [Fact]
    public void ConstructOutcomeDescriptionWhenDescOutcomesOutcomeMissingIdThenThrows()
    {
        var apiDescOutcomes = new desc_outcomesOutcome { description = "some-description", name = "some-name" };

        Assert.Throws<ArgumentNullException>(() => new OutcomeDescriptionDto(apiDescOutcomes));
    }

    [Fact]
    public void ConstructOutcomeDescriptionWhenDescOutcomesOutcomeHasEmptyIdThenThrows()
    {
        var apiDescOutcomes = new desc_outcomesOutcome { id = string.Empty, description = "some-description", name = "some-name" };

        Assert.Throws<ArgumentException>(() => new OutcomeDescriptionDto(apiDescOutcomes));
    }

    [Fact]
    public void ConstructOutcomeDescriptionWhenDescOutcomesOutcomeIsMissingDescription()
    {
        var apiDescOutcomes = new desc_outcomesOutcome { id = "some-id", name = "some-name" };

        var outcomeDescriptionDto = new OutcomeDescriptionDto(apiDescOutcomes);

        Assert.NotNull(outcomeDescriptionDto);
        Assert.Null(outcomeDescriptionDto.Description);
    }

    [Fact]
    public void ConstructOutcomeDescriptionWhenDescOutcomesOutcomeHasEmptyDescriptionThenThrows()
    {
        var apiDescOutcomes = new desc_outcomesOutcome { id = "some-id", description = string.Empty, name = "some-name" };

        var outcomeDescriptionDto = new OutcomeDescriptionDto(apiDescOutcomes);

        Assert.NotNull(outcomeDescriptionDto);
        Assert.Empty(outcomeDescriptionDto.Description);
    }

    [Fact]
    public void ConstructOutcomeDescriptionWhenDescOutcomesOutcomeHasMissingName()
    {
        var apiDescOutcomes = new desc_outcomesOutcome { id = "some-id", description = "some-description" };

        var outcomeDescriptionDto = new OutcomeDescriptionDto(apiDescOutcomes);

        Assert.NotNull(outcomeDescriptionDto);
        Assert.Null(outcomeDescriptionDto.Name);
    }

    [Fact]
    public void ConstructOutcomeDescriptionWhenDescOutcomesOutcomeHasEmptyName()
    {
        var apiDescOutcomes = new desc_outcomesOutcome { id = "some-id", description = "some-description", name = string.Empty };

        var outcomeDescriptionDto = new OutcomeDescriptionDto(apiDescOutcomes);

        Assert.NotNull(outcomeDescriptionDto);
        Assert.Empty(outcomeDescriptionDto.Name);
    }

    [Fact]
    public void ConstructOutcomeDescriptionWhenDescVariantOutcomesOutcomeIsValid()
    {
        var apiDescOutcomes = new desc_variant_outcomesOutcome { id = "some-id", name = "some-name" };

        var outcomeDescriptionDto = new OutcomeDescriptionDto(apiDescOutcomes);

        Assert.NotNull(outcomeDescriptionDto);
        Assert.Equal(apiDescOutcomes.id, outcomeDescriptionDto.Id);
        Assert.Equal(apiDescOutcomes.name, outcomeDescriptionDto.Name);
        Assert.Null(outcomeDescriptionDto.Description);
    }

    [Fact]
    public void ConstructOutcomeDescriptionWhenDescVariantOutcomesOutcomeIsNullThenThrows()
    {
        Assert.Throws<ArgumentNullException>(() => new OutcomeDescriptionDto((desc_variant_outcomesOutcome)null));
    }

    [Fact]
    public void ConstructOutcomeDescriptionWhenDescVariantOutcomesOutcomeIsMissingIdThenThrows()
    {
        var apiDescOutcomes = new desc_variant_outcomesOutcome { name = "some-name" };

        Assert.Throws<ArgumentNullException>(() => new OutcomeDescriptionDto(apiDescOutcomes));
    }

    [Fact]
    public void ConstructOutcomeDescriptionWhenDescVariantOutcomesOutcomeHasEmptyIdThenThrows()
    {
        var apiDescOutcomes = new desc_variant_outcomesOutcome { id = string.Empty, name = "some-name" };

        Assert.Throws<ArgumentException>(() => new OutcomeDescriptionDto(apiDescOutcomes));
    }

    [Fact]
    public void ConstructOutcomeDescriptionWhenDescVariantOutcomesOutcomeHasMissingNameThenSaveAsNull()
    {
        var apiDescOutcomes = new desc_variant_outcomesOutcome { id = "some-id" };

        var outcomeDescriptionDto = new OutcomeDescriptionDto(apiDescOutcomes);

        Assert.NotNull(outcomeDescriptionDto);
        Assert.Equal(apiDescOutcomes.id, outcomeDescriptionDto.Id);
        Assert.Null(outcomeDescriptionDto.Name);
    }

    [Fact]
    public void ConstructOutcomeDescriptionWhenDescVariantOutcomesOutcomeHasEmptyNameThenSaveAsEmpty()
    {
        var apiDescOutcomes = new desc_variant_outcomesOutcome { id = "some-id", name = string.Empty };

        var outcomeDescriptionDto = new OutcomeDescriptionDto(apiDescOutcomes);

        Assert.NotNull(outcomeDescriptionDto);
        Assert.Equal(apiDescOutcomes.id, outcomeDescriptionDto.Id);
        Assert.Empty(outcomeDescriptionDto.Name);
    }

    [Fact]
    public void ConstructSpecifierWhenValidData()
    {
        var apiSpecifier = new desc_specifiersSpecifier { name = "some-name", type = "some-type", description = "some-description" };

        var specifierDto = new SpecifierDto(apiSpecifier);

        Assert.NotNull(specifierDto);
        Assert.Equal(apiSpecifier.name, specifierDto.Name);
        Assert.Equal(apiSpecifier.type, specifierDto.Type);
        Assert.Equal(apiSpecifier.description, specifierDto.Description);
    }

    [Fact]
    public void ConstructSpecifierWhenNullDataThenThrows()
    {
        Assert.Throws<ArgumentNullException>(() => new SpecifierDto(null));
    }

    [Fact]
    public void ConstructSpecifierWhenMissingNameThenThrows()
    {
        var apiSpecifier = new desc_specifiersSpecifier { type = "some-type", description = "some-description" };

        Assert.Throws<ArgumentNullException>(() => new SpecifierDto(apiSpecifier));
    }

    [Fact]
    public void ConstructSpecifierWhenEmptyNameThenThrows()
    {
        var apiSpecifier = new desc_specifiersSpecifier { name = string.Empty, type = "some-type", description = "some-description" };

        Assert.Throws<ArgumentException>(() => new SpecifierDto(apiSpecifier));
    }

    [Fact]
    public void ConstructSpecifierWhenMissingTypeThenThrows()
    {
        var apiSpecifier = new desc_specifiersSpecifier { name = "some-name", description = "some-description" };

        Assert.Throws<ArgumentNullException>(() => new SpecifierDto(apiSpecifier));
    }

    [Fact]
    public void ConstructSpecifierWhenEmptyTypeThenThrows()
    {
        var apiSpecifier = new desc_specifiersSpecifier { name = "some-name", type = string.Empty, description = "some-description" };

        Assert.Throws<ArgumentException>(() => new SpecifierDto(apiSpecifier));
    }

    [Fact]
    public void ConstructSpecifierWhenMissingDescription()
    {
        var apiSpecifier = new desc_specifiersSpecifier { name = "some-name", type = "some-type" };

        var specifierDto = new SpecifierDto(apiSpecifier);

        Assert.NotNull(specifierDto);
        Assert.Null(specifierDto.Description);
    }

    [Fact]
    public void ConstructSpecifierWhenEmptyDescription()
    {
        var apiSpecifier = new desc_specifiersSpecifier { name = "some-name", type = "some-type", description = string.Empty };

        var specifierDto = new SpecifierDto(apiSpecifier);

        Assert.NotNull(specifierDto);
        Assert.Empty(specifierDto.Description);
    }

    [Fact]
    public void ConstructMarketAttributeIsValid()
    {
        var apiMarketAttribute = new attributesAttribute { name = "some-name", description = "some-description" };

        var marketAttributeDto = new MarketAttributeDto(apiMarketAttribute);

        Assert.NotNull(marketAttributeDto);
        Assert.Equal(apiMarketAttribute.name, marketAttributeDto.Name);
        Assert.Equal(apiMarketAttribute.description, marketAttributeDto.Description);
    }

    [Fact]
    public void ConstructMarketAttributeWhenNullDataThenThrows()
    {
        Assert.Throws<ArgumentNullException>(() => new MarketAttributeDto(null));
    }

    [Fact]
    public void ConstructMarketAttributeWhenMissingName()
    {
        var apiMarketAttribute = new attributesAttribute { description = "some-description" };

        var marketAttributeDto = new MarketAttributeDto(apiMarketAttribute);

        Assert.NotNull(marketAttributeDto);
        Assert.Null(marketAttributeDto.Name);
        Assert.Equal(apiMarketAttribute.description, marketAttributeDto.Description);
    }

    [Fact]
    public void ConstructMarketAttributeWhenEmptyName()
    {
        var apiMarketAttribute = new attributesAttribute { name = string.Empty, description = "some-description" };

        var marketAttributeDto = new MarketAttributeDto(apiMarketAttribute);

        Assert.NotNull(marketAttributeDto);
        Assert.Empty(marketAttributeDto.Name);
        Assert.Equal(apiMarketAttribute.description, marketAttributeDto.Description);
    }

    [Fact]
    public void ConstructMarketAttributeWhenMissingDescription()
    {
        var apiMarketAttribute = new attributesAttribute { name = "some-name" };

        var marketAttributeDto = new MarketAttributeDto(apiMarketAttribute);

        Assert.NotNull(marketAttributeDto);
        Assert.Equal(apiMarketAttribute.name, marketAttributeDto.Name);
        Assert.Null(marketAttributeDto.Description);
    }

    [Fact]
    public void ConstructMarketAttributeWhenEmptyDescription()
    {
        var apiMarketAttribute = new attributesAttribute { name = "some-name", description = string.Empty };

        var marketAttributeDto = new MarketAttributeDto(apiMarketAttribute);

        Assert.NotNull(marketAttributeDto);
        Assert.Equal(apiMarketAttribute.name, marketAttributeDto.Name);
        Assert.Empty(marketAttributeDto.Description);
    }
}
