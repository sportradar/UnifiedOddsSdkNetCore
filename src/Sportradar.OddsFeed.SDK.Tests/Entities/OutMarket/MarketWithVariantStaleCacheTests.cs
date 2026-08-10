// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using Moq;
using Shouldly;
using Sportradar.OddsFeed.SDK.Common.Exceptions;
using Sportradar.OddsFeed.SDK.Messages.Rest;
using Sportradar.OddsFeed.SDK.Tests.Common.Builders;
using Sportradar.OddsFeed.SDK.Tests.Common.Builders.Markets;
using Sportradar.OddsFeed.SDK.Tests.Common.Dsl.Api.Markets;
using Xunit;
using Xunit.Abstractions;

namespace Sportradar.OddsFeed.SDK.Tests.Entities.OutMarket;

[SuppressMessage("Performance", "CA1859:Use concrete types when possible for improved performance")]
public class MarketWithVariantStaleCacheTests : MarketNameSetup
{
    private const string VariantValue = "pre:playerprops:35432179:608000";
    private const string Specifiers = "variant=" + VariantValue;
    private const string MissingOutcomeId = "pre:playerprops:35432179:608000:11";
    private const string AlwaysMissingOutcomeId = "pre:playerprops:35432179:608000:99";

    public MarketWithVariantStaleCacheTests(ITestOutputHelper testOutputHelper)
        : base(testOutputHelper)
    {
    }

    [Fact]
    public async Task WhenVariantMarketDescriptorHasNullOutcomesAndThrowStrategy_ThenReloadsAndReturnsOutcomeName()
    {
        var noOutcomesMd = NullifyOutcomes(MdForVariantSingle.GetPlayerPropsMarket768());
        var freshVariantMd = MdForVariantSingle.GetPlayerPropsMarket768();
        SetupInvariantMarketListEndpoint(MdForInvariantList.GetPlayerPropsMarket768(), DefaultLanguage);
        SetupSingleVariantMarketEndpointSequence(DefaultLanguage, noOutcomesMd, freshVariantMd);

        var marketOddsChange = MarketWithOddsBuilder.Create()
                                                    .WithMarketId(768)
                                                    .WithOutcome(MissingOutcomeId, 1.5)
                                                    .WithOutcome("pre:playerprops:35432179:608000:22", 1.1)
                                                    .WithSpecifiers(Specifiers)
                                                    .Build();
        var market = MarketFactory.GetMarketWithOdds(AnySportEventMock.Object, marketOddsChange, 1, UrnCreate.SportId(1), [DefaultLanguage]);
        var outcome = market.OutcomeOdds.First(o => o.Id == MissingOutcomeId);

        var outcomeName = await outcome.GetNameAsync(DefaultLanguage);

        outcomeName.ShouldBe("Kyle Anderson 11+");
        VerifySingleVariantProviderWasCalled(Times.Exactly(2));
    }

    [Fact]
    public async Task WhenVariantMarketDescriptorHasEmptyOutcomesAndThrowStrategy_ThenReloadsAndReturnsOutcomeName()
    {
        var noOutcomesMd = EmptyOutcomes(MdForVariantSingle.GetPlayerPropsMarket768());
        var freshVariantMd = MdForVariantSingle.GetPlayerPropsMarket768();
        SetupInvariantMarketListEndpoint(MdForInvariantList.GetPlayerPropsMarket768(), DefaultLanguage);
        SetupSingleVariantMarketEndpointSequence(DefaultLanguage, noOutcomesMd, freshVariantMd);

        var marketOddsChange = MarketWithOddsBuilder.Create()
                                                    .WithMarketId(768)
                                                    .WithOutcome(MissingOutcomeId, 1.5)
                                                    .WithOutcome("pre:playerprops:35432179:608000:22", 1.1)
                                                    .WithSpecifiers(Specifiers)
                                                    .Build();
        var market = MarketFactory.GetMarketWithOdds(AnySportEventMock.Object, marketOddsChange, 1, UrnCreate.SportId(1), [DefaultLanguage]);
        var outcome = market.OutcomeOdds.First(o => o.Id == MissingOutcomeId);

        var outcomeName = await outcome.GetNameAsync(DefaultLanguage);

        outcomeName.ShouldBe("Kyle Anderson 11+");
        VerifySingleVariantProviderWasCalled(Times.Exactly(2));
    }

    [Fact]
    public async Task WhenVariantMarketDescriptorHasNullOutcomesAndOutcomeStillMissingAfterReload_ThenThrows()
    {
        var noOutcomesMd = NullifyOutcomes(MdForVariantSingle.GetPlayerPropsMarket768());
        SetupInvariantMarketListEndpoint(MdForInvariantList.GetPlayerPropsMarket768(), DefaultLanguage);
        SetupSingleVariantMarketEndpoint(noOutcomesMd, DefaultLanguage);

        var marketOddsChange = MarketWithOddsBuilder.Create()
                                                    .WithMarketId(768)
                                                    .WithOutcome(MissingOutcomeId, 1.5)
                                                    .WithOutcome("pre:playerprops:35432179:608000:22", 1.1)
                                                    .WithSpecifiers(Specifiers)
                                                    .Build();
        var market = MarketFactory.GetMarketWithOdds(AnySportEventMock.Object, marketOddsChange, 1, UrnCreate.SportId(1), [DefaultLanguage]);
        var outcome = market.OutcomeOdds.First(o => o.Id == MissingOutcomeId);

        await Should.ThrowAsync<NameGenerationException>(() => outcome.GetNameAsync(DefaultLanguage));

        VerifySingleVariantProviderWasCalled(Times.Exactly(2));
    }

    [Fact]
    public async Task WhenVariantMarketHasStaleCacheWithMissingOutcomeAndThrowStrategy_ThenReloadsAndReturnsOutcomeName()
    {
        var freshVariantMd = MdForVariantSingle.GetPlayerPropsMarket768();
        var staleVariantMd = WithoutOutcome(freshVariantMd, MissingOutcomeId);
        SetupInvariantMarketListEndpoint(MdForInvariantList.GetPlayerPropsMarket768(), DefaultLanguage);
        SetupSingleVariantMarketEndpointSequence(DefaultLanguage, staleVariantMd, freshVariantMd);

        var marketOddsChange = MarketWithOddsBuilder.Create()
                                                    .WithMarketId(768)
                                                    .WithOutcome(MissingOutcomeId, 1.5)
                                                    .WithOutcome("pre:playerprops:35432179:608000:22", 1.1)
                                                    .WithSpecifiers(Specifiers)
                                                    .Build();
        var market = MarketFactory.GetMarketWithOdds(AnySportEventMock.Object, marketOddsChange, 1, UrnCreate.SportId(1), [DefaultLanguage]);
        var outcome = market.OutcomeOdds.First(o => o.Id == MissingOutcomeId);

        var outcomeName = await outcome.GetNameAsync(DefaultLanguage);

        outcomeName.ShouldBe("Kyle Anderson 11+");
        VerifySingleVariantProviderWasCalled(Times.Exactly(2));
    }

    [Fact]
    public async Task WhenVariantMarketHasStaleCacheWithMissingOutcomeAndCatchStrategy_ThenReloadsAndReturnsOutcomeName()
    {
        var freshVariantMd = MdForVariantSingle.GetPlayerPropsMarket768();
        var staleVariantMd = WithoutOutcome(freshVariantMd, MissingOutcomeId);
        SetupInvariantMarketListEndpoint(MdForInvariantList.GetPlayerPropsMarket768(), DefaultLanguage);
        SetupSingleVariantMarketEndpointSequence(DefaultLanguage, staleVariantMd, freshVariantMd);

        var marketOddsChange = MarketWithOddsBuilder.Create()
                                                    .WithMarketId(768)
                                                    .WithOutcome(MissingOutcomeId, 1.5)
                                                    .WithOutcome("pre:playerprops:35432179:608000:22", 1.1)
                                                    .WithSpecifiers(Specifiers)
                                                    .Build();
        var market = MarketFactoryWithCatchStrategy.GetMarketWithOdds(AnySportEventMock.Object, marketOddsChange, 1, UrnCreate.SportId(1), [DefaultLanguage]);
        var outcome = market.OutcomeOdds.First(o => o.Id == MissingOutcomeId);

        var outcomeName = await outcome.GetNameAsync(DefaultLanguage);

        outcomeName.ShouldBe("Kyle Anderson 11+");
        VerifySingleVariantProviderWasCalled(Times.Exactly(2));
    }

    [Fact]
    public async Task WhenVariantMarketOutcomeIsStillMissingAfterReloadAndThrowStrategy_ThenThrowsAfterReload()
    {
        var freshVariantMd = MdForVariantSingle.GetPlayerPropsMarket768();
        var staleVariantMd = WithoutOutcome(freshVariantMd, AlwaysMissingOutcomeId);
        SetupInvariantMarketListEndpoint(MdForInvariantList.GetPlayerPropsMarket768(), DefaultLanguage);
        SetupSingleVariantMarketEndpoint(staleVariantMd, DefaultLanguage);

        var marketOddsChange = MarketWithOddsBuilder.Create()
                                                    .WithMarketId(768)
                                                    .WithOutcome(AlwaysMissingOutcomeId, 1.5)
                                                    .WithOutcome("pre:playerprops:35432179:608000:22", 1.1)
                                                    .WithSpecifiers(Specifiers)
                                                    .Build();
        var market = MarketFactory.GetMarketWithOdds(AnySportEventMock.Object, marketOddsChange, 1, UrnCreate.SportId(1), [DefaultLanguage]);
        var outcome = market.OutcomeOdds.First(o => o.Id == AlwaysMissingOutcomeId);

        await Should.ThrowAsync<NameGenerationException>(() => outcome.GetNameAsync(DefaultLanguage));

        VerifySingleVariantProviderWasCalled(Times.Exactly(2));
    }

    [Fact]
    public async Task WhenVariantMarketOutcomeIsStillMissingAfterReloadAndCatchStrategy_ThenReturnsNullAfterReload()
    {
        var freshVariantMd = MdForVariantSingle.GetPlayerPropsMarket768();
        var staleVariantMd = WithoutOutcome(freshVariantMd, AlwaysMissingOutcomeId);
        SetupInvariantMarketListEndpoint(MdForInvariantList.GetPlayerPropsMarket768(), DefaultLanguage);
        SetupSingleVariantMarketEndpoint(staleVariantMd, DefaultLanguage);

        var marketOddsChange = MarketWithOddsBuilder.Create()
                                                    .WithMarketId(768)
                                                    .WithOutcome(AlwaysMissingOutcomeId, 1.5)
                                                    .WithOutcome("pre:playerprops:35432179:608000:22", 1.1)
                                                    .WithSpecifiers(Specifiers)
                                                    .Build();
        var market = MarketFactoryWithCatchStrategy.GetMarketWithOdds(AnySportEventMock.Object, marketOddsChange, 1, UrnCreate.SportId(1), [DefaultLanguage]);
        var outcome = market.OutcomeOdds.First(o => o.Id == AlwaysMissingOutcomeId);

        var outcomeName = await outcome.GetNameAsync(DefaultLanguage);

        outcomeName.ShouldBeNull();
        VerifySingleVariantProviderWasCalled(Times.Exactly(2));
    }

    private static desc_market WithoutOutcome(desc_market source, string outcomeId)
    {
        return new desc_market
        {
            id = source.id,
            name = source.name,
            variant = source.variant,
            outcomes = source.outcomes?.Where(o => o.id != outcomeId).ToArray()
        };
    }

    private static desc_market NullifyOutcomes(desc_market source)
    {
        return new desc_market
        {
            id = source.id,
            name = source.name,
            variant = source.variant,
            outcomes = null
        };
    }

    private static desc_market EmptyOutcomes(desc_market source)
    {
        return new desc_market
        {
            id = source.id,
            name = source.name,
            variant = source.variant,
            outcomes = Array.Empty<desc_outcomesOutcome>()
        };
    }
}
