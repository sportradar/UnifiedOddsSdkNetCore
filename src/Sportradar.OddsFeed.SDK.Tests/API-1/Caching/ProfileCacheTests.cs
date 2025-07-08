// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Sportradar.OddsFeed.SDK.Common.Exceptions;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Enums;
using Sportradar.OddsFeed.SDK.Tests.Common;
using Xunit;
using Xunit.Abstractions;

namespace Sportradar.OddsFeed.SDK.Tests.Api.Caching;

public class ProfileCacheTests : ProfileCacheSetup
{
    public ProfileCacheTests(ITestOutputHelper outputHelper)
        : base(outputHelper)
    {
    }

    [Fact]
    public async Task GetPlayerProfileWhenNullPlayerIdThenThrow()
    {
        var act = async () => await _profileCache.GetPlayerProfileAsync(null, _cultures, true);
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task GetPlayerProfileWhenNullLanguagesThenThrow()
    {
        var act = async () => await _profileCache.GetPlayerProfileAsync(CreatePlayerUrn(1), null, true);
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task GetPlayerProfileWhenEmptyLanguagesThenThrow()
    {
        var act = async () => await _profileCache.GetPlayerProfileAsync(CreatePlayerUrn(1), Array.Empty<CultureInfo>(), true);
        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task GetPlayerProfileWhenFetchedThenIsCached()
    {
        var player = await _profileCache.GetPlayerProfileAsync(CreatePlayerUrn(1), _cultures, true);

        player.Should().NotBeNull();
        _profileMemoryCache.Count().Should().Be(1);
    }

    [Fact]
    public async Task GetPlayerProfileWhenFetchedThenInvokesPlayerProfileEndpoint()
    {
        _dataRouterManager.TotalRestCalls.Should().Be(0);
        _profileMemoryCache.Count().Should().Be(0);

        await _profileCache.GetPlayerProfileAsync(CreatePlayerUrn(1), _cultures, true);

        _dataRouterManager.TotalRestCalls.Should().Be(_cultures.Count);
        _dataRouterManager.GetCallCount(TestDataRouterManager.EndpointPlayerProfile).Should().Be(_cultures.Count);
    }

    [Fact]
    public async Task GetPlayerProfileWhenFetchedThenReturnValidPlayer()
    {
        var player = await _profileCache.GetPlayerProfileAsync(CreatePlayerUrn(1), _cultures, true);

        player.Should().NotBeNull();
        ValidatePlayer1(player, TestData.Culture);
    }

    [Fact]
    public async Task GetPlayerProfileWhenFetchedTwiceThenInvokesPlayerProfileEndpointJustOnce()
    {
        _dataRouterManager.TotalRestCalls.Should().Be(0);
        _profileMemoryCache.Count().Should().Be(0);

        await _profileCache.GetPlayerProfileAsync(CreatePlayerUrn(1), _cultures, true);
        await _profileCache.GetPlayerProfileAsync(CreatePlayerUrn(1), _cultures, true);

        _dataRouterManager.TotalRestCalls.Should().Be(_cultures.Count);
        _dataRouterManager.GetCallCount(TestDataRouterManager.EndpointPlayerProfile).Should().Be(_cultures.Count);
    }

    [Fact]
    public async Task GetPlayerProfileWhenFetchMissingIsFalseAndPlayerIsCachedThenReturnExisting()
    {
        await _profileCache.GetPlayerProfileAsync(CreatePlayerUrn(1), FilterLanguages(), false);
        _dataRouterManager.ResetMethodCall();

        var player = await _profileCache.GetPlayerProfileAsync(CreatePlayerUrn(1), _cultures, false);

        player.Should().NotBeNull();
        player.Names.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetPlayerProfileWhenFetchMissingIsFalseAndPlayerIsCachedThenNoApiCallIsMade()
    {
        await _profileCache.GetPlayerProfileAsync(CreatePlayerUrn(1), FilterLanguages(), false);
        _dataRouterManager.ResetMethodCall();

        await _profileCache.GetPlayerProfileAsync(CreatePlayerUrn(1), _cultures, false);

        _dataRouterManager.TotalRestCalls.Should().Be(0);
        _dataRouterManager.GetCallCount(TestDataRouterManager.EndpointPlayerProfile).Should().Be(0);
    }

    [Fact]
    public async Task GetPlayerProfileWhenFetchMissingIsFalseAndPlayerIsNotCachedThenApiCallsAreMade()
    {
        await _profileCache.GetPlayerProfileAsync(CreatePlayerUrn(1), _cultures, false);

        _dataRouterManager.TotalRestCalls.Should().Be(_cultures.Count);
        _dataRouterManager.GetCallCount(TestDataRouterManager.EndpointPlayerProfile).Should().Be(_cultures.Count);
    }

    [Fact]
    public async Task GetPlayerProfileWhenFetchMissingIsFalseAndPlayerIsNotCachedThenAllIsFetchedAndReturned()
    {
        var player = await _profileCache.GetPlayerProfileAsync(CreatePlayerUrn(1), _cultures, false);

        player.Should().NotBeNull();
        player.Names.Should().HaveCount(3);
    }

    [Fact]
    public async Task GetPlayerProfileWhenAlreadyCached1LanguageThenOnlyMissingLanguagesAreCalled()
    {
        await _profileCache.GetPlayerProfileAsync(CreatePlayerUrn(1), FilterLanguages(), true);
        _dataRouterManager.ResetMethodCall();

        await _profileCache.GetPlayerProfileAsync(CreatePlayerUrn(1), _cultures, true);

        _dataRouterManager.TotalRestCalls.Should().Be(_cultures.Count - 1);
        _dataRouterManager.GetCallCount(TestDataRouterManager.EndpointPlayerProfile).Should().Be(_cultures.Count - 1);
    }

    [Fact]
    public async Task GetPlayerProfileWhenPreloadedFromCompetitorThenNoPlayerProfileEndpointIsCalled()
    {
        await _profileCache.GetCompetitorProfileAsync(CreateCompetitorUrn(1), _cultures, true);
        _dataRouterManager.ResetMethodCall();

        await _profileCache.GetPlayerProfileAsync(CreatePlayerUrn(30401), _cultures, true);

        _dataRouterManager.TotalRestCalls.Should().Be(0);
        _dataRouterManager.GetCallCount(TestDataRouterManager.EndpointPlayerProfile).Should().Be(0);
    }

    [Fact]
    public async Task GetPlayerProfileWhenPreloadedFromCompetitorThenPlayerIsReturnedFromCache()
    {
        await _profileCache.GetCompetitorProfileAsync(CreateCompetitorUrn(1), _cultures, true);

        var player = await _profileCache.GetPlayerProfileAsync(CreatePlayerUrn(30401), _cultures, true);

        player.Should().NotBeNull();
        player.GetName(TestData.Culture).Should().Be("Smithies, Alex");
    }

    [Fact]
    public async Task GetPlayerProfileWhenPartiallyPreloadedFromCompetitorThenMissingCompetitorProfileEndpointIsCalled()
    {
        await _profileCache.GetCompetitorProfileAsync(CreateCompetitorUrn(1), FilterLanguages(), true);
        _dataRouterManager.ResetMethodCall();

        await _profileCache.GetPlayerProfileAsync(CreatePlayerUrn(30401), _cultures, true);

        _dataRouterManager.TotalRestCalls.Should().Be(_cultures.Count - 1);
        _dataRouterManager.GetCallCount(TestDataRouterManager.EndpointCompetitor).Should().Be(_cultures.Count - 1);
        _dataRouterManager.GetCallCount(TestDataRouterManager.EndpointPlayerProfile).Should().Be(0);
    }

    [Fact]
    public async Task GetPlayerProfileWhenPartiallyPreloadedFromCompetitorThenFullPlayerIsReturnedFromCache()
    {
        await _profileCache.GetCompetitorProfileAsync(CreateCompetitorUrn(1), FilterLanguages(), true);

        var player = await _profileCache.GetPlayerProfileAsync(CreatePlayerUrn(30401), _cultures, true);

        player.Should().NotBeNull();
        player.Names.Should().HaveCount(3);
        player.GetName(TestData.Culture).Should().Be("Smithies, Alex");
    }

    [Fact]
    public async Task GetPlayerProfileWhenAssociatedToCompetitorButCompetitorCacheItemIsMissingThenCompetitorProfileForMissingLanguagesIsCalled()
    {
        await _profileCache.GetCompetitorProfileAsync(CreateCompetitorUrn(1), FilterLanguages(), true);
        _profileCache.CacheDeleteItem(CreateCompetitorUrn(1), CacheItemType.Competitor);
        _dataRouterManager.ResetMethodCall();

        await _profileCache.GetPlayerProfileAsync(CreatePlayerUrn(30401), _cultures, true);

        _dataRouterManager.TotalRestCalls.Should().Be(_cultures.Count - 1);
        _dataRouterManager.GetCallCount(TestDataRouterManager.EndpointCompetitor).Should().Be(_cultures.Count - 1);
        _dataRouterManager.GetCallCount(TestDataRouterManager.EndpointPlayerProfile).Should().Be(0);
    }

    [Fact]
    public async Task GetPlayerProfileWhenAssociatedToCompetitorButCompetitorCacheItemIsMissingThenAllLanguagesAreReturned()
    {
        await _profileCache.GetCompetitorProfileAsync(CreateCompetitorUrn(1), FilterLanguages(), true);
        _profileCache.CacheDeleteItem(CreateCompetitorUrn(1), CacheItemType.Competitor);
        _dataRouterManager.ResetMethodCall();

        var player = await _profileCache.GetPlayerProfileAsync(CreatePlayerUrn(30401), _cultures, true);

        player.Should().NotBeNull();
        player.Names.Should().HaveCount(3);
    }

    [Fact]
    public async Task GetPlayerProfileWhenApiCallThrowsMappingExceptionThenReturnCacheNotFound()
    {
        _dataRouterManager.UriExceptions.Add(new Tuple<string, Exception>("player.xml", ExceptionHelper.GetMappingException()));

        var act = () => _profileCache.GetPlayerProfileAsync(CreatePlayerUrn(1), FilterLanguages(), true);
        var exceptionAssertion = await act.Should().ThrowAsync<CacheItemNotFoundException>();
        exceptionAssertion.Which.Message.Should().Contain("sr:player:1");
    }

    [Fact]
    public async Task GetPlayerProfileWhenApiCallThrowsDeserializationExceptionThenReturnCacheNotFound()
    {
        _dataRouterManager.UriExceptions.Add(new Tuple<string, Exception>("player.xml", ExceptionHelper.GetDeserializationException()));

        var act = () => _profileCache.GetPlayerProfileAsync(CreatePlayerUrn(1), FilterLanguages(), true);
        var resultException = await act
                                   .Should()
                                   .ThrowAsync<CacheItemNotFoundException>();

        resultException.Which.Message.Should().Contain("sr:player:1");
    }

    [Fact]
    public async Task GetPlayerProfileWhenApiCallThrowsUnhandledExceptionThenReturnOriginalException()
    {
        _dataRouterManager.UriExceptions.Add(new Tuple<string, Exception>("player.xml", ExceptionHelper.GetInvalidException()));

        var act = () => _profileCache.GetPlayerProfileAsync(CreatePlayerUrn(1), FilterLanguages(), true);
        var resultException = await act
                                   .Should()
                                   .ThrowAsync<InvalidOperationException>();

        resultException.NotBeNull();
    }

    [Fact]
    public async Task GetPlayerProfileWhenPartiallyPreloadedFromCompetitorAndNewCompetitorRequestThrowsThenThrows()
    {
        await _profileCache.GetCompetitorProfileAsync(CreateCompetitorUrn(1), FilterLanguages(), true);
        _dataRouterManager.UriExceptions.Add(new Tuple<string, Exception>("competitor.xml", ExceptionHelper.GetMappingException()));

        var action = () => _profileCache.GetPlayerProfileAsync(CreatePlayerUrn(30401), _cultures, true);
        var resultException = await action
                                   .Should()
                                   .ThrowAsync<CacheItemNotFoundException>();

        resultException.NotBeNull();
    }

    [Fact]
    public async Task GetPlayerProfileWhenCallingForUnknownPlayerIdThenReturnNull()
    {
        var player = await _profileCache.GetPlayerProfileAsync(CreatePlayerUrn(1234), FilterLanguages(), true);

        player.Should().BeNull();
    }

    [Fact]
    public async Task GetCompetitorProfileWhenNullPlayerIdThenThrow()
    {
        var act = async () => await _profileCache.GetCompetitorProfileAsync(null, _cultures, true);
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task GetCompetitorProfileWhenNullLanguagesThenThrow()
    {
        var act = async () => await _profileCache.GetCompetitorProfileAsync(CreateCompetitorUrn(1), null, true);
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task GetCompetitorProfileWhenEmptyLanguagesThenThrow()
    {
        var act = async () => await _profileCache.GetCompetitorProfileAsync(CreateCompetitorUrn(1), Array.Empty<CultureInfo>(), true);
        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task GetCompetitorProfileWhenFetchedThenIsCached()
    {
        var competitor = await _profileCache.GetCompetitorProfileAsync(CreateCompetitorUrn(1), _cultures, true);

        competitor.Should().NotBeNull();
        _profileMemoryCache.Count().Should().Be(35);
    }

    [Fact]
    public async Task GetCompetitorProfileWhenFetchedThenInvokesCompetitorProfileEndpoint()
    {
        _dataRouterManager.TotalRestCalls.Should().Be(0);
        _profileMemoryCache.Count().Should().Be(0);

        await _profileCache.GetCompetitorProfileAsync(CreateCompetitorUrn(1), _cultures, true);

        _dataRouterManager.TotalRestCalls.Should().Be(_cultures.Count);
        _dataRouterManager.GetCallCount(TestDataRouterManager.EndpointCompetitor).Should().Be(_cultures.Count);
    }

    [Fact]
    public async Task GetCompetitorProfileWhenFetchedThenReturnValidCompetitor()
    {
        var competitorCacheItem = await _profileCache.GetCompetitorProfileAsync(CreateCompetitorUrn(1), _cultures, true);

        competitorCacheItem.Should().NotBeNull();
        competitorCacheItem.Names.Should().HaveCount(3);
        competitorCacheItem.GetName(_cultures[0]).Should().Be("Queens Park Rangers");
        competitorCacheItem.AssociatedPlayerIds.Should().HaveCount(34);
    }

    [Fact]
    public async Task GetCompetitorProfileWhenFetchedTwiceThenInvokesPlayerProfileEndpointJustOnce()
    {
        await _profileCache.GetCompetitorProfileAsync(CreateCompetitorUrn(1), _cultures, true);
        await _profileCache.GetCompetitorProfileAsync(CreateCompetitorUrn(1), _cultures, true);

        _dataRouterManager.TotalRestCalls.Should().Be(_cultures.Count);
        _dataRouterManager.GetCallCount(TestDataRouterManager.EndpointCompetitor).Should().Be(_cultures.Count);
    }

    [Fact]
    public async Task GetCompetitorProfileWhenFetchMissingIsFalseAndCompetitorIsCachedThenReturnExisting()
    {
        await _profileCache.GetCompetitorProfileAsync(CreateCompetitorUrn(1), FilterLanguages(), false);
        _dataRouterManager.ResetMethodCall();

        var competitor = await _profileCache.GetCompetitorProfileAsync(CreateCompetitorUrn(1), _cultures, false);

        competitor.Should().NotBeNull();
        competitor.Names.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetCompetitorProfileWhenFetchMissingIsFalseAndCompetitorIsCachedThenNoApiCallIsMade()
    {
        await _profileCache.GetCompetitorProfileAsync(CreateCompetitorUrn(1), FilterLanguages(), false);
        _dataRouterManager.ResetMethodCall();

        await _profileCache.GetCompetitorProfileAsync(CreateCompetitorUrn(1), _cultures, false);

        _dataRouterManager.TotalRestCalls.Should().Be(0);
        _dataRouterManager.GetCallCount(TestDataRouterManager.EndpointCompetitor).Should().Be(0);
    }

    [Fact]
    public async Task GetCompetitorProfileWhenFetchMissingIsFalseAndCompetitorIsNotCachedThenApiCallAreMade()
    {
        await _profileCache.GetCompetitorProfileAsync(CreateCompetitorUrn(1), _cultures, false);

        _dataRouterManager.TotalRestCalls.Should().Be(_cultures.Count);
        _dataRouterManager.GetCallCount(TestDataRouterManager.EndpointCompetitor).Should().Be(_cultures.Count);
    }

    [Fact]
    public async Task GetCompetitorProfileWhenFetchMissingIsFalseAndCompetitorIsNotCachedThenAllIsFetchedAndReturned()
    {
        var competitor = await _profileCache.GetCompetitorProfileAsync(CreateCompetitorUrn(1), _cultures, false);

        competitor.Should().NotBeNull();
        competitor.Names.Should().HaveCount(3);
    }

    [Fact]
    public async Task GetCompetitorProfileWhenAlreadyCachedOneLanguageThenOnlyMissingLanguagesAreFetched()
    {
        await _profileCache.GetCompetitorProfileAsync(CreateCompetitorUrn(1), FilterLanguages(), true);
        _dataRouterManager.ResetMethodCall();

        await _profileCache.GetCompetitorProfileAsync(CreateCompetitorUrn(1), _cultures, true);

        _dataRouterManager.TotalRestCalls.Should().Be(_cultures.Count - 1);
        _dataRouterManager.GetCallCount(TestDataRouterManager.EndpointCompetitor).Should().Be(_cultures.Count - 1);
    }

    [Fact]
    public async Task GetCompetitorProfileWhenCallingForDifferentIdsThenNewApiCallsAreMade()
    {
        var competitorNames1 = await _profileCache.GetCompetitorProfileAsync(CreateCompetitorUrn(1), _cultures, true);
        var competitorNames2 = await _profileCache.GetCompetitorProfileAsync(CreateCompetitorUrn(2), _cultures, true);

        competitorNames1.Id.Should().NotBe(competitorNames2.Id);
        _dataRouterManager.TotalRestCalls.Should().Be(_cultures.Count * 2);
        _dataRouterManager.GetCallCount(TestDataRouterManager.EndpointCompetitor).Should().Be(_cultures.Count * 2);
    }

    [Fact]
    public async Task GetCompetitorProfileWhenPreloadedFromMatchThenCompetitorProfileCallsAreStillMade()
    {
        await _dataRouterManager.GetSportEventSummaryAsync(_defaultMatchId, _cultures[0], null);
        await _dataRouterManager.GetSportEventSummaryAsync(_defaultMatchId, _cultures[1], null);
        await _dataRouterManager.GetSportEventSummaryAsync(_defaultMatchId, _cultures[2], null);
        _dataRouterManager.ResetMethodCall();

        var competitor = await _profileCache.GetCompetitorProfileAsync(CreateCompetitorUrn(1), _cultures, true);

        competitor.Should().NotBeNull();
        _dataRouterManager.TotalRestCalls.Should().Be(_cultures.Count);
        _dataRouterManager.GetCallCount(TestDataRouterManager.EndpointCompetitor).Should().Be(_cultures.Count);
    }

    [Fact]
    public async Task GetCompetitorProfileWhenPartiallyPreloadedFromMatchThenCompetitorCallsIsCorrect()
    {
        await _dataRouterManager.GetSportEventSummaryAsync(_defaultMatchId, _cultures[0], null);
        _dataRouterManager.ResetMethodCall();

        await _profileCache.GetCompetitorProfileAsync(CreateCompetitorUrn(1), _cultures, true);

        _dataRouterManager.TotalRestCalls.Should().Be(_cultures.Count);
        _dataRouterManager.GetCallCount(TestDataRouterManager.EndpointSportEventSummary).Should().Be(0);
        _dataRouterManager.GetCallCount(TestDataRouterManager.EndpointCompetitor).Should().Be(_cultures.Count);
    }

    [Fact]
    public async Task GetCompetitorProfileWhenPartiallyPreloadedFromMatchThenCompetitorIsReturned()
    {
        await _dataRouterManager.GetSportEventSummaryAsync(_defaultMatchId, _cultures[0], null);
        _dataRouterManager.ResetMethodCall();

        var competitor = await _profileCache.GetCompetitorProfileAsync(CreateCompetitorUrn(1), _cultures, true);

        competitor.Should().NotBeNull();
        competitor.Names.Should().HaveCount(3);
    }

    [Fact]
    public async Task GetCompetitorProfileWhenApiCallThrowsMappingExceptionThenReturnCacheNotFound()
    {
        _dataRouterManager.UriExceptions.Add(new Tuple<string, Exception>("competitor.xml", ExceptionHelper.GetMappingException()));

        var action = () => _profileCache.GetCompetitorProfileAsync(CreateCompetitorUrn(1), FilterLanguages(), true);
        var resultException = await action
                                   .Should()
                                   .ThrowAsync<CacheItemNotFoundException>();

        resultException.Which.Message.Should().Contain("sr:competitor:1");
    }

    [Fact]
    public async Task GetCompetitorProfileWhenApiCallThrowsDeserializationExceptionThenReturnCacheNotFound()
    {
        _dataRouterManager.UriExceptions.Add(new Tuple<string, Exception>("competitor.xml", ExceptionHelper.GetDeserializationException()));

        var action = () => _profileCache.GetCompetitorProfileAsync(CreateCompetitorUrn(1), FilterLanguages(), true);
        var resultException = await action
                                   .Should()
                                   .ThrowAsync<CacheItemNotFoundException>();

        resultException.Which.Message.Should().Contain("sr:competitor:1");
    }

    [Fact]
    public async Task GetCompetitorProfileWhenApiCallThrowsUnhandledExceptionThenReturnOriginalException()
    {
        _dataRouterManager.UriExceptions.Add(new Tuple<string, Exception>("competitor.xml", ExceptionHelper.GetInvalidException()));

        var action = () => _profileCache.GetCompetitorProfileAsync(CreateCompetitorUrn(1), FilterLanguages(), true);
        var resultException = await action
                                   .Should()
                                   .ThrowAsync<InvalidOperationException>();

        resultException.NotBeNull();
    }

    [Fact]
    public async Task GetPlayerNamesWhenNotCachedThenReturnFetchedValue()
    {
        var playerNames = await _profileCache.GetPlayerNamesAsync(CreatePlayerUrn(1), _cultures, true);

        playerNames.Should().NotBeNull();
        playerNames[_cultures[0]].Should().NotBeEmpty();
        playerNames[_cultures[1]].Should().NotBeEmpty();
        playerNames[_cultures[2]].Should().NotBeEmpty();
    }

    [Fact]
    public async Task GetPlayerNamesWhenNotCachedThenApiCallAreMade()
    {
        await _profileCache.GetPlayerNamesAsync(CreatePlayerUrn(1), _cultures, true);

        _dataRouterManager.TotalRestCalls.Should().Be(_cultures.Count);
        _dataRouterManager.GetCallCount(TestDataRouterManager.EndpointPlayerProfile).Should().Be(_cultures.Count);
    }

    [Fact]
    public async Task GetPlayerNamesWhenNotCachedThenApiIsCalledAndCached()
    {
        await _profileCache.GetPlayerNamesAsync(CreatePlayerUrn(1), _cultures, true);

        _profileMemoryCache.Count().Should().Be(1);
    }

    [Fact]
    public async Task GetPlayerNamesWhenMissingPlayerNameForAllCulturesAndSetNotToFetchThenNoCallIsMade()
    {
        await _profileCache.GetPlayerNamesAsync(CreatePlayerUrn(1), _cultures, false);

        _dataRouterManager.TotalRestCalls.Should().Be(0);
        _dataRouterManager.GetCallCount(TestDataRouterManager.EndpointPlayerProfile).Should().Be(0);
    }

    [Fact]
    public async Task GetPlayerNamesWhenMissingPlayerNameForAllCulturesAndSetNotToFetchThenNamesAreReturned()
    {
        var playerNames = await _profileCache.GetPlayerNamesAsync(CreatePlayerUrn(1), _cultures, false);

        playerNames.Should().NotBeNull();
        playerNames.Should().HaveCount(_cultures.Count);
        playerNames.Should().AllSatisfy(a => a.Value.Should().BeNullOrEmpty());
    }

    [Fact]
    public async Task GetPlayerNamesWhenMissingPlayerNameAndSetNotToFetchIfMissingThenDoesNotFetchesMissingPlayerProfile()
    {
        await _profileCache.GetPlayerProfileAsync(CreatePlayerUrn(1), FilterLanguages(), true);
        _dataRouterManager.ResetMethodCall();

        await _profileCache.GetPlayerNamesAsync(CreatePlayerUrn(1), _cultures, false);

        _dataRouterManager.TotalRestCalls.Should().Be(0);
        _dataRouterManager.GetCallCount(TestDataRouterManager.EndpointPlayerProfile).Should().Be(0);
    }

    [Fact]
    public async Task GetPlayerNamesWhenMissingPlayerNameAndSetNotToFetchIfMissingThenNamesAreReturnedWithEmptyValues()
    {
        await _profileCache.GetPlayerProfileAsync(CreatePlayerUrn(1), FilterLanguages(), true);
        _dataRouterManager.ResetMethodCall();

        var playerNames = await _profileCache.GetPlayerNamesAsync(CreatePlayerUrn(1), _cultures, false);

        playerNames.Should().NotBeNull();
        playerNames.Should().HaveCount(3);
        playerNames[_cultures[0]].Should().NotBeEmpty();
        playerNames[_cultures.Skip(1).First()].Should().BeEmpty();
        playerNames[_cultures.Skip(2).First()].Should().BeEmpty();
    }

    [Fact]
    public async Task GetPlayerNamesWhenPartiallyPopulatedFromCompetitorWhenRequestedThenFetchesMissingCompetitorProfile()
    {
        await _profileCache.GetCompetitorProfileAsync(CreateCompetitorUrn(1), FilterLanguages(), true);
        _dataRouterManager.ResetMethodCall();

        await _profileCache.GetPlayerNamesAsync(CreatePlayerUrn(1), _cultures, true);

        _dataRouterManager.TotalRestCalls.Should().Be(_cultures.Count - 1);
        _dataRouterManager.GetCallCount(TestDataRouterManager.EndpointCompetitor).Should().Be(_cultures.Count - 1);
        _dataRouterManager.GetCallCount(TestDataRouterManager.EndpointPlayerProfile).Should().Be(0);
    }

    [Fact]
    public async Task GetPlayerNamesWhenPartiallyPopulatedFromCompetitorWhenRequestedThenReturnsAllNames()
    {
        await _profileCache.GetCompetitorProfileAsync(CreateCompetitorUrn(1), FilterLanguages(), true);

        var playerNames = await _profileCache.GetPlayerNamesAsync(CreatePlayerUrn(1), _cultures, true);

        playerNames.Should().NotBeNull();
        playerNames.Count.Should().Be(_cultures.Count);
        playerNames[_cultures[0]].Should().NotBeEmpty();
        playerNames[_cultures.Skip(1).First()].Should().NotBeEmpty();
        playerNames[_cultures.Skip(2).First()].Should().NotBeEmpty();
    }

    [Fact]
    public async Task GetPlayerNamesWhenPrePopulatedFromCompetitorAndSetNotToFetchIfMissingThenDoesNotFetchesMissingProfile()
    {
        await _profileCache.GetCompetitorProfileAsync(CreateCompetitorUrn(1), FilterLanguages(), true);
        _dataRouterManager.ResetMethodCall();

        await _profileCache.GetPlayerNamesAsync(CreatePlayerUrn(1), _cultures, false);

        _dataRouterManager.TotalRestCalls.Should().Be(0);
        _dataRouterManager.GetCallCount(TestDataRouterManager.EndpointCompetitor).Should().Be(0);
        _dataRouterManager.GetCallCount(TestDataRouterManager.EndpointPlayerProfile).Should().Be(0);
    }

    [Fact]
    public async Task GetPlayerNamesWhenPrePopulatedFromCompetitorAndSetNotToFetchIfMissingThenReturnsAlsoEmptyNames()
    {
        await _profileCache.GetCompetitorProfileAsync(CreateCompetitorUrn(1), FilterLanguages(), true);

        var playerNames = await _profileCache.GetPlayerNamesAsync(CreatePlayerUrn(1), _cultures, false);

        playerNames.Should().NotBeNull();
        playerNames[_cultures[0]].Should().NotBeEmpty();
        playerNames[_cultures.Skip(1).First()].Should().BeEmpty();
        playerNames[_cultures.Skip(2).First()].Should().BeEmpty();
    }

    [Fact]
    public async Task GetPlayerNamesWhenFullyLoadedThenNoApiIsDone()
    {
        await _profileCache.GetPlayerNamesAsync(CreatePlayerUrn(1), _cultures, true);
        _dataRouterManager.ResetMethodCall();

        await _profileCache.GetPlayerNamesAsync(CreatePlayerUrn(1), _cultures, true);

        _dataRouterManager.TotalRestCalls.Should().Be(0);
        _dataRouterManager.GetCallCount(TestDataRouterManager.EndpointCompetitor).Should().Be(0);
        _dataRouterManager.GetCallCount(TestDataRouterManager.EndpointPlayerProfile).Should().Be(0);
    }

    [Fact]
    public async Task GetPlayerNamesWhenApiCallThrowsMappingExceptionThenReturnCacheNotFound()
    {
        _dataRouterManager.UriExceptions.Add(new Tuple<string, Exception>("player.xml", ExceptionHelper.GetMappingException()));

        var action = () => _profileCache.GetPlayerNamesAsync(CreatePlayerUrn(1), FilterLanguages(), true);
        var resultException = await action
                                   .Should()
                                   .ThrowAsync<CacheItemNotFoundException>();

        resultException.Which.Message.Should().Contain("sr:player:1");
    }

    [Fact]
    public async Task GetPlayerNamesWhenApiCallThrowsDeserializationExceptionThenReturnCacheNotFound()
    {
        _dataRouterManager.UriExceptions.Add(new Tuple<string, Exception>("player.xml", ExceptionHelper.GetDeserializationException()));

        var action = () => _profileCache.GetPlayerNamesAsync(CreatePlayerUrn(1), FilterLanguages(), true);
        var resultException = await action
                                   .Should()
                                   .ThrowAsync<CacheItemNotFoundException>();

        resultException.Which.Message.Should().Contain("sr:player:1");
    }

    [Fact]
    public async Task GetPlayerNamesWhenApiCallThrowsUnhandledExceptionThenReturnOriginalException()
    {
        _dataRouterManager.UriExceptions.Add(new Tuple<string, Exception>("player.xml", ExceptionHelper.GetInvalidException()));

        var action = () => _profileCache.GetPlayerNamesAsync(CreatePlayerUrn(1), FilterLanguages(), true);
        var resultException = await action
                                   .Should()
                                   .ThrowAsync<InvalidOperationException>();

        resultException.NotBeNull();
    }

    [Fact]
    public async Task GetCompetitorNamesWhenNotCachedThenReturnFetchedValue()
    {
        var competitorNames = await _profileCache.GetCompetitorNamesAsync(CreateCompetitorUrn(1), _cultures, true);

        competitorNames.Should().NotBeNull();
        competitorNames[_cultures[0]].Should().NotBeEmpty();
        competitorNames[_cultures.Skip(1).First()].Should().NotBeEmpty();
        competitorNames[_cultures.Skip(2).First()].Should().NotBeEmpty();
    }

    [Fact]
    public async Task GetCompetitorNamesWhenNotCachedThenMakeApiCalls()
    {
        await _profileCache.GetCompetitorNamesAsync(CreateCompetitorUrn(1), _cultures, true);

        _dataRouterManager.TotalRestCalls.Should().Be(_cultures.Count);
        _dataRouterManager.GetCallCount(TestDataRouterManager.EndpointCompetitor).Should().Be(_cultures.Count);
    }

    [Fact]
    public async Task GetCompetitorNamesWhenNotCachedThenCompetitorIsCached()
    {
        await _profileCache.GetCompetitorNamesAsync(CreateCompetitorUrn(1), _cultures, true);

        _profileMemoryCache.GetKeys().Count(c => c.Contains("competitor", StringComparison.InvariantCultureIgnoreCase)).Should().Be(1);
    }

    [Fact]
    public async Task GetCompetitorNamesWhenNotCachedThenAssociatedPlayersAreCached()
    {
        await _profileCache.GetCompetitorNamesAsync(CreateCompetitorUrn(1), _cultures, true);

        _profileMemoryCache.GetKeys().Count(c => c.Contains("player", StringComparison.InvariantCultureIgnoreCase)).Should().Be(34);
    }

    [Fact]
    public async Task GetCompetitorNamesWhenNotCachedAndSetNotToFetchThenReturnNamesAsEmpty()
    {
        var competitorNames = await _profileCache.GetCompetitorNamesAsync(CreateCompetitorUrn(1), _cultures, false);

        competitorNames.Should().NotBeNull();
        competitorNames.Count.Should().Be(_cultures.Count);

        competitorNames.Should().AllSatisfy(a => a.Value.Should().BeNullOrEmpty());
    }

    [Fact]
    public async Task GetCompetitorNamesWhenNotCachedAndSetNotToFetchThenNoCallsMade()
    {
        await _profileCache.GetCompetitorNamesAsync(CreateCompetitorUrn(1), _cultures, false);

        _dataRouterManager.TotalRestCalls.Should().Be(0);
        _dataRouterManager.GetCallCount(TestDataRouterManager.EndpointCompetitor).Should().Be(0);
    }

    [Fact]
    public async Task GetCompetitorNamesWhenMissingNameAndSetNotToFetchThenMissingValuesAreEmpty()
    {
        await _profileCache.GetCompetitorNamesAsync(CreateCompetitorUrn(1), FilterLanguages(), true);

        var competitorNames = await _profileCache.GetCompetitorNamesAsync(CreateCompetitorUrn(1), _cultures, false);

        competitorNames.Should().NotBeNull();
        competitorNames[_cultures[0]].Should().NotBeEmpty();
        competitorNames[_cultures.Skip(1).First()].Should().BeEmpty();
        competitorNames[_cultures.Skip(2).First()].Should().BeEmpty();
    }

    [Fact]
    public async Task GetCompetitorNamesWhenMissingNameAndSetNotToFetchThenNoApiCallMade()
    {
        await _profileCache.GetCompetitorNamesAsync(CreateCompetitorUrn(1), FilterLanguages(), true);
        _dataRouterManager.ResetMethodCall();

        await _profileCache.GetCompetitorNamesAsync(CreateCompetitorUrn(1), _cultures, false);

        _dataRouterManager.TotalRestCalls.Should().Be(0);
        _dataRouterManager.GetCallCount(TestDataRouterManager.EndpointCompetitor).Should().Be(0);
    }

    [Fact]
    public async Task GetCompetitorNamesWhenPartiallyPopulatedThenMissingValuesAreRetrieved()
    {
        await _profileCache.GetCompetitorNamesAsync(CreateCompetitorUrn(1), FilterLanguages(), true);

        var competitorNames = await _profileCache.GetCompetitorNamesAsync(CreateCompetitorUrn(1), _cultures, true);

        competitorNames.Should().NotBeNull();
        competitorNames.Count.Should().Be(_cultures.Count);
        competitorNames.All(a => !string.IsNullOrEmpty(a.Value)).Should().BeTrue();
    }

    [Fact]
    public async Task GetCompetitorNamesWhenPartiallyPopulatedThenApiCallsAreMade()
    {
        await _profileCache.GetCompetitorNamesAsync(CreateCompetitorUrn(1), FilterLanguages(), true);
        _dataRouterManager.ResetMethodCall();

        await _profileCache.GetCompetitorNamesAsync(CreateCompetitorUrn(1), _cultures, true);

        _dataRouterManager.TotalRestCalls.Should().Be(2);
        _dataRouterManager.GetCallCount(TestDataRouterManager.EndpointCompetitor).Should().Be(2);
    }

    [Fact]
    public async Task GetCompetitorNamesWhenPopulatedFromMatchThenMissingCompetitorNameFetchesMissingSummaryEndpoint()
    {
        PrepareSportEventCacheMockForMatchSummaryFetch(_defaultMatchId);
        await _dataRouterManager.GetSportEventSummaryAsync(_defaultMatchId, _cultures[0], null);
        _dataRouterManager.ResetMethodCall();

        await _profileCache.GetCompetitorNamesAsync(CreateCompetitorUrn(1), _cultures, true);

        _dataRouterManager.TotalRestCalls.Should().Be(2);
        _dataRouterManager.GetCallCount(TestDataRouterManager.EndpointSportEventSummary).Should().Be(2);
    }

    [Fact]
    public async Task GetCompetitorNamesWhenPopulatedFromMatchThenAllNamesHasValue()
    {
        PrepareSportEventCacheMockForMatchSummaryFetch(_defaultMatchId);
        await _dataRouterManager.GetSportEventSummaryAsync(_defaultMatchId, _cultures[0], null);

        var competitorNames = await _profileCache.GetCompetitorNamesAsync(CreateCompetitorUrn(1), _cultures, true);

        competitorNames.Should().NotBeNull();
        competitorNames.Count.Should().Be(_cultures.Count);
        competitorNames.All(a => !string.IsNullOrEmpty(a.Value)).Should().BeTrue();
    }

    [Fact]
    public async Task GetCompetitorNamesWhenPopulatedFromMatchAndSetNotToFetchIfMissingThenMissingCompetitorNameDoesNotFetch()
    {
        PrepareSportEventCacheMockForMatchSummaryFetch(_defaultMatchId);
        await _dataRouterManager.GetSportEventSummaryAsync(_defaultMatchId, _cultures[0], null);
        _dataRouterManager.ResetMethodCall();

        await _profileCache.GetCompetitorNamesAsync(CreateCompetitorUrn(1), _cultures, false);

        _dataRouterManager.TotalRestCalls.Should().Be(0);
        _dataRouterManager.GetCallCount(TestDataRouterManager.EndpointSportEventSummary).Should().Be(0);
    }

    [Fact]
    public async Task GetCompetitorNamesWhenPopulatedFromMatchAndSetNotToFetchIfMissingThenNamesHasEmptyValues()
    {
        PrepareSportEventCacheMockForMatchSummaryFetch(_defaultMatchId);
        await _dataRouterManager.GetSportEventSummaryAsync(_defaultMatchId, _cultures[0], null);

        var competitorNames = await _profileCache.GetCompetitorNamesAsync(CreateCompetitorUrn(1), _cultures, false);

        competitorNames.Should().NotBeNull();
        competitorNames.Count.Should().Be(_cultures.Count);
        competitorNames[_cultures[0]].Should().NotBeEmpty();
        competitorNames[_cultures.Skip(1).First()].Should().BeEmpty();
        competitorNames[_cultures.Skip(2).First()].Should().BeEmpty();
    }

    [Fact]
    public async Task GetCompetitorNamesWhenFullyPopulatedFromMatchThenNoAdditionalApiCall()
    {
        await _dataRouterManager.GetSportEventSummaryAsync(_defaultMatchId, _cultures[0], null);
        await _dataRouterManager.GetSportEventSummaryAsync(_defaultMatchId, _cultures[1], null);
        await _dataRouterManager.GetSportEventSummaryAsync(_defaultMatchId, _cultures[2], null);
        _dataRouterManager.ResetMethodCall();

        await _profileCache.GetCompetitorNamesAsync(CreateCompetitorUrn(1), _cultures, true);

        _dataRouterManager.TotalRestCalls.Should().Be(0);
        _dataRouterManager.GetCallCount(TestDataRouterManager.EndpointSportEventSummary).Should().Be(0);
        _dataRouterManager.GetCallCount(TestDataRouterManager.EndpointCompetitor).Should().Be(0);
    }

    [Fact]
    public async Task GetCompetitorNamesWhenFullyPopulatedFromMatchThenAllNamesReturned()
    {
        await _dataRouterManager.GetSportEventSummaryAsync(_defaultMatchId, _cultures[0], null);
        await _dataRouterManager.GetSportEventSummaryAsync(_defaultMatchId, _cultures[1], null);
        await _dataRouterManager.GetSportEventSummaryAsync(_defaultMatchId, _cultures[2], null);
        _dataRouterManager.ResetMethodCall();

        var competitorNames = await _profileCache.GetCompetitorNamesAsync(CreateCompetitorUrn(1), _cultures, true);

        competitorNames.Should().NotBeNull();
        competitorNames.Count.Should().Be(_cultures.Count);
        competitorNames.Should().AllSatisfy(a => a.Value.Should().NotBeNullOrEmpty());
    }

    [Fact]
    public async Task GetCompetitorNamesWhenApiCallThrowsMappingExceptionThenReturnCacheNotFound()
    {
        _dataRouterManager.UriExceptions.Add(new Tuple<string, Exception>("competitor.xml", ExceptionHelper.GetMappingException()));

        var action = () => _profileCache.GetCompetitorNamesAsync(CreateCompetitorUrn(1), FilterLanguages(), true);
        var resultException = await action
                                   .Should()
                                   .ThrowAsync<CacheItemNotFoundException>();

        resultException.Which.Message.Should().Contain("sr:competitor:1");
    }

    [Fact]
    public async Task GetCompetitorNamesWhenApiCallThrowsDeserializationExceptionThenReturnCacheNotFound()
    {
        _dataRouterManager.UriExceptions.Add(new Tuple<string, Exception>("competitor.xml", ExceptionHelper.GetDeserializationException()));

        var action = () => _profileCache.GetCompetitorNamesAsync(CreateCompetitorUrn(1), FilterLanguages(), true);
        var resultException = await action
                                   .Should()
                                   .ThrowAsync<CacheItemNotFoundException>();

        resultException.Which.Message.Should().Contain("sr:competitor:1");
    }

    [Fact]
    public async Task GetCompetitorNamesWhenApiCallThrowsUnhandledExceptionThenReturnOriginalException()
    {
        _dataRouterManager.UriExceptions.Add(new Tuple<string, Exception>("competitor.xml", ExceptionHelper.GetInvalidException()));

        var action = () => _profileCache.GetCompetitorNamesAsync(CreateCompetitorUrn(1), FilterLanguages(), true);
        var resultException = await action
                                   .Should()
                                   .ThrowAsync<InvalidOperationException>();

        resultException.NotBeNull();
    }

    [Fact]
    public async Task GetCompetitorNamesWhenPartiallyPopulatedFromMatchAndSummaryThrowsThenCompetitorProfileIsFetched()
    {
        await _dataRouterManager.GetSportEventSummaryAsync(_defaultMatchId, _cultures[0], null);
        _dataRouterManager.ResetMethodCall();

        await _profileCache.GetCompetitorNamesAsync(CreateCompetitorUrn(1), _cultures, true);

        _dataRouterManager.TotalRestCalls.Should().Be(2);
        _dataRouterManager.GetCallCount(TestDataRouterManager.EndpointCompetitor).Should().Be(2);
    }

    [Fact]
    public async Task GetCompetitorNamesWhenPartiallyPopulatedFromMatchAndSummaryThrowsThenNamesAreReturned()
    {
        await _dataRouterManager.GetSportEventSummaryAsync(_defaultMatchId, _cultures[0], null);

        var competitorNames = await _profileCache.GetCompetitorNamesAsync(CreateCompetitorUrn(1), _cultures, true);

        competitorNames.Should().NotBeNull();
        competitorNames.Count.Should().Be(_cultures.Count);
        competitorNames.All(a => !string.IsNullOrEmpty(a.Value)).Should().BeTrue();
    }

    [Fact]
    public async Task GetPlayerNameWhenFullyLoadedThenNoApiIsDone()
    {
        await _profileCache.GetPlayerNamesAsync(CreatePlayerUrn(1), _cultures, true);
        _dataRouterManager.ResetMethodCall();

        await _profileCache.GetPlayerNameAsync(CreatePlayerUrn(1), _cultures[0], true);

        _dataRouterManager.TotalRestCalls.Should().Be(0);
        _dataRouterManager.GetCallCount(TestDataRouterManager.EndpointCompetitor).Should().Be(0);
        _dataRouterManager.GetCallCount(TestDataRouterManager.EndpointPlayerProfile).Should().Be(0);
    }

    [Fact]
    public async Task GetPlayerNameWhenNotCachedThenApiCallAreMade()
    {
        await _profileCache.GetPlayerNameAsync(CreatePlayerUrn(1), _cultures[0], true);

        _dataRouterManager.TotalRestCalls.Should().Be(1);
        _dataRouterManager.GetCallCount(TestDataRouterManager.EndpointPlayerProfile).Should().Be(1);
    }

    [Fact]
    public async Task GetPlayerNameWhenNotCachedThenApiIsCalledAndCached()
    {
        await _profileCache.GetPlayerNameAsync(CreatePlayerUrn(1), _cultures[0], true);

        _profileMemoryCache.Count().Should().Be(1);
    }

    [Fact]
    public async Task GetPlayerNameWhenMissingPlayerNameAndSetNotToFetchThenNoCallIsMade()
    {
        await _profileCache.GetPlayerNameAsync(CreatePlayerUrn(1), _cultures[0], false);

        _dataRouterManager.TotalRestCalls.Should().Be(0);
        _dataRouterManager.GetCallCount(TestDataRouterManager.EndpointPlayerProfile).Should().Be(0);
    }

    [Fact]
    public async Task GetPlayerNameWhenMissingPlayerNameAndSetNotToFetchThenEmptyIsReturned()
    {
        var name = await _profileCache.GetPlayerNameAsync(CreatePlayerUrn(1), _cultures[0], false);

        name.Should().BeEmpty();
    }

    [Fact]
    public async Task GetPlayerNameWhenFullyLoadedThenNameIsReturned()
    {
        await _profileCache.GetPlayerNamesAsync(CreatePlayerUrn(1), _cultures, true);
        _dataRouterManager.ResetMethodCall();

        var name = await _profileCache.GetPlayerNameAsync(CreatePlayerUrn(1), _cultures[0], true);

        name.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task GetPlayerNameWhenApiCallThrowsMappingExceptionThenReturnCacheNotFound()
    {
        _dataRouterManager.UriExceptions.Add(new Tuple<string, Exception>("player.xml", ExceptionHelper.GetMappingException()));

        var action = () => _profileCache.GetPlayerNameAsync(CreatePlayerUrn(1), _cultures[0], true);
        var resultException = await action
                                   .Should()
                                   .ThrowAsync<CacheItemNotFoundException>();

        resultException.Which.Message.Should().Contain("sr:player:1");
    }

    [Fact]
    public async Task GetPlayerNameWhenApiCallThrowsDeserializationExceptionThenReturnCacheNotFound()
    {
        _dataRouterManager.UriExceptions.Add(new Tuple<string, Exception>("player.xml", ExceptionHelper.GetDeserializationException()));

        var action = () => _profileCache.GetPlayerNameAsync(CreatePlayerUrn(1), _cultures[0], true);
        var resultException = await action
                                   .Should()
                                   .ThrowAsync<CacheItemNotFoundException>();

        resultException.Which.Message.Should().Contain("sr:player:1");
    }

    [Fact]
    public async Task GetPlayerNameWhenApiCallThrowsUnhandledExceptionThenReturnOriginalException()
    {
        _dataRouterManager.UriExceptions.Add(new Tuple<string, Exception>("player.xml", ExceptionHelper.GetInvalidException()));

        var action = () => _profileCache.GetPlayerNameAsync(CreatePlayerUrn(1), _cultures[0], true);
        var resultException = await action
                                   .Should()
                                   .ThrowAsync<InvalidOperationException>();

        resultException.NotBeNull();
    }

    [Fact]
    public async Task GetCompetitorNameWhenFullyLoadedThenNoApiIsDone()
    {
        await _profileCache.GetCompetitorNamesAsync(CreateCompetitorUrn(1), _cultures, true);
        _dataRouterManager.ResetMethodCall();

        await _profileCache.GetCompetitorNameAsync(CreateCompetitorUrn(1), _cultures[0], true);

        _dataRouterManager.TotalRestCalls.Should().Be(0);
        _dataRouterManager.GetCallCount(TestDataRouterManager.EndpointCompetitor).Should().Be(0);
    }

    [Fact]
    public async Task GetCompetitorNameWhenNotCachedThenApiCallAreMade()
    {
        await _profileCache.GetCompetitorNameAsync(CreateCompetitorUrn(1), _cultures[0], true);

        _dataRouterManager.TotalRestCalls.Should().Be(1);
        _dataRouterManager.GetCallCount(TestDataRouterManager.EndpointCompetitor).Should().Be(1);
    }

    [Fact]
    public async Task GetCompetitorNameWhenNotCachedThenApiIsCalledAndCached()
    {
        await _profileCache.GetCompetitorNameAsync(CreateCompetitorUrn(1), _cultures[0], true);

        _profileMemoryCache.Count().Should().Be(35);
    }

    [Fact]
    public async Task GetCompetitorNameWhenMissingCompetitorNameAndSetNotToFetchThenNoCallIsMade()
    {
        await _profileCache.GetCompetitorNameAsync(CreateCompetitorUrn(1), _cultures[0], false);

        _dataRouterManager.TotalRestCalls.Should().Be(0);
        _dataRouterManager.GetCallCount(TestDataRouterManager.EndpointCompetitor).Should().Be(0);
    }

    [Fact]
    public async Task GetCompetitorNameWhenMissingCompetitorNameAndSetNotToFetchThenEmptyIsReturned()
    {
        var name = await _profileCache.GetCompetitorNameAsync(CreateCompetitorUrn(1), _cultures[0], false);

        name.Should().BeEmpty();
    }

    [Fact]
    public async Task GetCompetitorNameWhenFullyLoadedThenNameIsReturned()
    {
        await _profileCache.GetCompetitorNamesAsync(CreateCompetitorUrn(1), _cultures, true);
        _dataRouterManager.ResetMethodCall();

        var name = await _profileCache.GetCompetitorNameAsync(CreateCompetitorUrn(1), _cultures[0], true);

        name.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task GetCompetitorNameWhenApiCallThrowsMappingExceptionThenReturnCacheNotFound()
    {
        _dataRouterManager.UriExceptions.Add(new Tuple<string, Exception>("competitor.xml", ExceptionHelper.GetMappingException()));

        var action = () => _profileCache.GetCompetitorNameAsync(CreateCompetitorUrn(1), _cultures[0], true);
        var resultException = await action
                                   .Should()
                                   .ThrowAsync<CacheItemNotFoundException>();

        resultException.Which.Message.Should().Contain("sr:competitor:1");
    }

    [Fact]
    public async Task GetCompetitorNameWhenApiCallThrowsDeserializationExceptionThenReturnCacheNotFound()
    {
        _dataRouterManager.UriExceptions.Add(new Tuple<string, Exception>("competitor.xml", ExceptionHelper.GetDeserializationException()));

        var action = () => _profileCache.GetCompetitorNameAsync(CreateCompetitorUrn(1), _cultures[0], true);
        var resultException = await action
                                   .Should()
                                   .ThrowAsync<CacheItemNotFoundException>();

        resultException.Which.Message.Should().Contain("sr:competitor:1");
    }

    [Fact]
    public async Task GetCompetitorNameWhenApiCallThrowsUnhandledExceptionThenReturnOriginalException()
    {
        _dataRouterManager.UriExceptions.Add(new Tuple<string, Exception>("competitor.xml", ExceptionHelper.GetInvalidException()));

        var action = () => _profileCache.GetCompetitorNameAsync(CreateCompetitorUrn(1), _cultures[0], true);
        var resultException = await action
                                   .Should()
                                   .ThrowAsync<InvalidOperationException>();

        resultException.NotBeNull();
    }
}
