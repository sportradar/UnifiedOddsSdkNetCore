// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
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
        _ = await Assert.ThrowsAsync<ArgumentNullException>(() => _profileCache.GetPlayerProfileAsync(null, _cultures, true));
    }

    [Fact]
    public async Task GetPlayerProfileWhenNullLanguagesThenThrow()
    {
        _ = await Assert.ThrowsAsync<ArgumentNullException>(() => _profileCache.GetPlayerProfileAsync(CreatePlayerUrn(1), null, true));
    }

    [Fact]
    public async Task GetPlayerProfileWhenEmptyLanguagesThenThrow()
    {
        _ = await Assert.ThrowsAsync<ArgumentException>(() => _profileCache.GetPlayerProfileAsync(CreatePlayerUrn(1), Array.Empty<CultureInfo>(), true));
    }

    [Fact]
    public async Task GetPlayerProfileWhenFetchedThenIsCached()
    {
        var player = await _profileCache.GetPlayerProfileAsync(CreatePlayerUrn(1), _cultures, true);

        Assert.NotNull(player);
        Assert.Equal(1, _profileMemoryCache.Count());
    }

    [Fact]
    public async Task GetPlayerProfileWhenFetchedThenInvokesPlayerProfileEndpoint()
    {
        Assert.Equal(0, _dataRouterManager.TotalRestCalls);
        Assert.Equal(0, _profileMemoryCache.Count());

        await _profileCache.GetPlayerProfileAsync(CreatePlayerUrn(1), _cultures, true);

        Assert.Equal(_cultures.Count, _dataRouterManager.TotalRestCalls);
        Assert.Equal(_cultures.Count, _dataRouterManager.GetCallCount(TestDataRouterManager.EndpointPlayerProfile));
    }

    [Fact]
    public async Task GetPlayerProfileWhenFetchedThenReturnValidPlayer()
    {
        var player = await _profileCache.GetPlayerProfileAsync(CreatePlayerUrn(1), _cultures, true);

        Assert.NotNull(player);
        ValidatePlayer1(player, TestData.Culture);
    }

    [Fact]
    public async Task GetPlayerProfileWhenFetchedTwiceThenInvokesPlayerProfileEndpointJustOnce()
    {
        Assert.Equal(0, _dataRouterManager.TotalRestCalls);
        Assert.Equal(0, _profileMemoryCache.Count());

        await _profileCache.GetPlayerProfileAsync(CreatePlayerUrn(1), _cultures, true);
        await _profileCache.GetPlayerProfileAsync(CreatePlayerUrn(1), _cultures, true);

        Assert.Equal(_cultures.Count, _dataRouterManager.TotalRestCalls);
        Assert.Equal(_cultures.Count, _dataRouterManager.GetCallCount(TestDataRouterManager.EndpointPlayerProfile));
    }

    [Fact]
    public async Task GetPlayerProfileWhenFetchMissingIsFalseAndPlayerIsCachedThenReturnExisting()
    {
        await _profileCache.GetPlayerProfileAsync(CreatePlayerUrn(1), FilterLanguages(), false);
        _dataRouterManager.ResetMethodCall();

        var player = await _profileCache.GetPlayerProfileAsync(CreatePlayerUrn(1), _cultures, false);

        Assert.NotNull(player);
        Assert.Single(player.Names);
    }

    [Fact]
    public async Task GetPlayerProfileWhenFetchMissingIsFalseAndPlayerIsCachedThenNoApiCallIsMade()
    {
        await _profileCache.GetPlayerProfileAsync(CreatePlayerUrn(1), FilterLanguages(), false);
        _dataRouterManager.ResetMethodCall();

        await _profileCache.GetPlayerProfileAsync(CreatePlayerUrn(1), _cultures, false);

        Assert.Equal(0, _dataRouterManager.TotalRestCalls);
        Assert.Equal(0, _dataRouterManager.GetCallCount(TestDataRouterManager.EndpointPlayerProfile));
    }

    [Fact]
    public async Task GetPlayerProfileWhenFetchMissingIsFalseAndPlayerIsNotCachedThenApiCallsAreMade()
    {
        await _profileCache.GetPlayerProfileAsync(CreatePlayerUrn(1), _cultures, false);

        Assert.Equal(_cultures.Count, _dataRouterManager.TotalRestCalls);
        Assert.Equal(_cultures.Count, _dataRouterManager.GetCallCount(TestDataRouterManager.EndpointPlayerProfile));
    }

    [Fact]
    public async Task GetPlayerProfileWhenFetchMissingIsFalseAndPlayerIsNotCachedThenAllIsFetchedAndReturned()
    {
        var player = await _profileCache.GetPlayerProfileAsync(CreatePlayerUrn(1), _cultures, false);

        Assert.NotNull(player);
        Assert.Equal(3, player.Names.Count);
    }

    [Fact]
    public async Task GetPlayerProfileWhenAlreadyCached1LanguageThenOnlyMissingLanguagesAreCalled()
    {
        await _profileCache.GetPlayerProfileAsync(CreatePlayerUrn(1), FilterLanguages(), true);
        _dataRouterManager.ResetMethodCall();

        await _profileCache.GetPlayerProfileAsync(CreatePlayerUrn(1), _cultures, true);

        Assert.Equal(_cultures.Count - 1, _dataRouterManager.TotalRestCalls);
        Assert.Equal(_cultures.Count - 1, _dataRouterManager.GetCallCount(TestDataRouterManager.EndpointPlayerProfile));
    }

    [Fact]
    public async Task GetPlayerProfileWhenPreloadedFromCompetitorThenNoPlayerProfileEndpointIsCalled()
    {
        await _profileCache.GetCompetitorProfileAsync(CreateCompetitorUrn(1), _cultures, true);
        _dataRouterManager.ResetMethodCall();

        await _profileCache.GetPlayerProfileAsync(CreatePlayerUrn(30401), _cultures, true);

        Assert.Equal(0, _dataRouterManager.TotalRestCalls);
        Assert.Equal(0, _dataRouterManager.GetCallCount(TestDataRouterManager.EndpointPlayerProfile));
    }

    [Fact]
    public async Task GetPlayerProfileWhenPreloadedFromCompetitorThenPlayerIsReturnedFromCache()
    {
        await _profileCache.GetCompetitorProfileAsync(CreateCompetitorUrn(1), _cultures, true);

        var player = await _profileCache.GetPlayerProfileAsync(CreatePlayerUrn(30401), _cultures, true);

        Assert.NotNull(player);
        Assert.Equal("Smithies, Alex", player.GetName(TestData.Culture));
    }

    [Fact]
    public async Task GetPlayerProfileWhenPartiallyPreloadedFromCompetitorThenMissingCompetitorProfileEndpointIsCalled()
    {
        await _profileCache.GetCompetitorProfileAsync(CreateCompetitorUrn(1), FilterLanguages(), true);
        _dataRouterManager.ResetMethodCall();

        await _profileCache.GetPlayerProfileAsync(CreatePlayerUrn(30401), _cultures, true);

        Assert.Equal(_cultures.Count - 1, _dataRouterManager.TotalRestCalls);
        Assert.Equal(_cultures.Count - 1, _dataRouterManager.GetCallCount(TestDataRouterManager.EndpointCompetitor));
        Assert.Equal(0, _dataRouterManager.GetCallCount(TestDataRouterManager.EndpointPlayerProfile));
    }

    [Fact]
    public async Task GetPlayerProfileWhenPartiallyPreloadedFromCompetitorThenFullPlayerIsReturnedFromCache()
    {
        await _profileCache.GetCompetitorProfileAsync(CreateCompetitorUrn(1), FilterLanguages(), true);

        var player = await _profileCache.GetPlayerProfileAsync(CreatePlayerUrn(30401), _cultures, true);

        Assert.NotNull(player);
        Assert.Equal(3, player.Names.Count);
        Assert.Equal("Smithies, Alex", player.GetName(TestData.Culture));
    }

    [Fact]
    public async Task GetPlayerProfileWhenAssociatedToCompetitorButCompetitorCacheItemIsMissingThenCompetitorProfileForMissingLanguagesIsCalled()
    {
        await _profileCache.GetCompetitorProfileAsync(CreateCompetitorUrn(1), FilterLanguages(), true);
        _profileCache.CacheDeleteItem(CreateCompetitorUrn(1), CacheItemType.Competitor);
        _dataRouterManager.ResetMethodCall();

        await _profileCache.GetPlayerProfileAsync(CreatePlayerUrn(30401), _cultures, true);

        Assert.Equal(_cultures.Count - 1, _dataRouterManager.TotalRestCalls);
        Assert.Equal(_cultures.Count - 1, _dataRouterManager.GetCallCount(TestDataRouterManager.EndpointCompetitor));
        Assert.Equal(0, _dataRouterManager.GetCallCount(TestDataRouterManager.EndpointPlayerProfile));
    }

    [Fact]
    public async Task GetPlayerProfileWhenAssociatedToCompetitorButCompetitorCacheItemIsMissingThenAllLanguagesAreReturned()
    {
        await _profileCache.GetCompetitorProfileAsync(CreateCompetitorUrn(1), FilterLanguages(), true);
        _profileCache.CacheDeleteItem(CreateCompetitorUrn(1), CacheItemType.Competitor);
        _dataRouterManager.ResetMethodCall();

        var player = await _profileCache.GetPlayerProfileAsync(CreatePlayerUrn(30401), _cultures, true);

        Assert.NotNull(player);
        Assert.Equal(3, player.Names.Count);
    }

    [Fact]
    public async Task GetPlayerProfileWhenApiCallThrowsMappingExceptionThenReturnCacheNotFound()
    {
        _dataRouterManager.UriExceptions.Add(new Tuple<string, Exception>("player.xml", ExceptionHelper.GetMappingException()));

        var resultException = await Assert.ThrowsAsync<CacheItemNotFoundException>(() => _profileCache.GetPlayerProfileAsync(CreatePlayerUrn(1), FilterLanguages(), true));

        Assert.NotNull(resultException);
        Assert.True(resultException.Message.Contains("sr:player:1", StringComparison.InvariantCultureIgnoreCase));
    }

    [Fact]
    public async Task GetPlayerProfileWhenApiCallThrowsDeserializationExceptionThenReturnCacheNotFound()
    {
        _dataRouterManager.UriExceptions.Add(new Tuple<string, Exception>("player.xml", ExceptionHelper.GetDeserializationException()));

        var resultException = await Assert.ThrowsAsync<CacheItemNotFoundException>(() => _profileCache.GetPlayerProfileAsync(CreatePlayerUrn(1), FilterLanguages(), true));

        Assert.NotNull(resultException);
        Assert.True(resultException.Message.Contains("sr:player:1", StringComparison.InvariantCultureIgnoreCase));
    }

    [Fact]
    public async Task GetPlayerProfileWhenApiCallThrowsUnhandledExceptionThenReturnOriginalException()
    {
        _dataRouterManager.UriExceptions.Add(new Tuple<string, Exception>("player.xml", ExceptionHelper.GetInvalidException()));

        var resultException = await Assert.ThrowsAsync<InvalidOperationException>(() => _profileCache.GetPlayerProfileAsync(CreatePlayerUrn(1), FilterLanguages(), true));

        Assert.NotNull(resultException);
    }

    //TODO: if competitor request fails it should still try player profile
    [Fact]
    public async Task GetPlayerProfileWhenPartiallyPreloadedFromCompetitorAndNewCompetitorRequestThrowsThenThrows()
    {
        await _profileCache.GetCompetitorProfileAsync(CreateCompetitorUrn(1), FilterLanguages(), true);
        _dataRouterManager.UriExceptions.Add(new Tuple<string, Exception>("competitor.xml", ExceptionHelper.GetMappingException()));

        var resultException = await Assert.ThrowsAsync<CacheItemNotFoundException>(() => _profileCache.GetPlayerProfileAsync(CreatePlayerUrn(30401), _cultures, true));

        Assert.NotNull(resultException);
    }

    [Fact]
    public async Task GetPlayerProfileWhenCallingForUnknownPlayerIdThenReturnNull()
    {
        var player = await _profileCache.GetPlayerProfileAsync(CreatePlayerUrn(1234), FilterLanguages(), true);

        Assert.Null(player);
    }

    [Fact]
    public async Task GetCompetitorProfileWhenNullPlayerIdThenThrow()
    {
        _ = await Assert.ThrowsAsync<ArgumentNullException>(() => _profileCache.GetCompetitorProfileAsync(null, _cultures, true));
    }

    [Fact]
    public async Task GetCompetitorProfileWhenNullLanguagesThenThrow()
    {
        _ = await Assert.ThrowsAsync<ArgumentNullException>(() => _profileCache.GetCompetitorProfileAsync(CreateCompetitorUrn(1), null, true));
    }

    [Fact]
    public async Task GetCompetitorProfileWhenEmptyLanguagesThenThrow()
    {
        _ = await Assert.ThrowsAsync<ArgumentException>(() => _profileCache.GetCompetitorProfileAsync(CreateCompetitorUrn(1), Array.Empty<CultureInfo>(), true));
    }

    [Fact]
    public async Task GetCompetitorProfileWhenFetchedThenIsCached()
    {
        var competitor = await _profileCache.GetCompetitorProfileAsync(CreateCompetitorUrn(1), _cultures, true);

        Assert.NotNull(competitor);
        Assert.Equal(35, _profileMemoryCache.Count());
    }

    [Fact]
    public async Task GetCompetitorProfileWhenFetchedThenInvokesCompetitorProfileEndpoint()
    {
        Assert.Equal(0, _dataRouterManager.TotalRestCalls);
        Assert.Equal(0, _profileMemoryCache.Count());

        await _profileCache.GetCompetitorProfileAsync(CreateCompetitorUrn(1), _cultures, true);

        Assert.Equal(_cultures.Count, _dataRouterManager.TotalRestCalls);
        Assert.Equal(_cultures.Count, _dataRouterManager.GetCallCount(TestDataRouterManager.EndpointCompetitor));
    }

    [Fact]
    public async Task GetCompetitorProfileWhenFetchedThenReturnValidCompetitor()
    {
        var competitorCacheItem = await _profileCache.GetCompetitorProfileAsync(CreateCompetitorUrn(1), _cultures, true);

        Assert.NotNull(competitorCacheItem);
        Assert.Equal(3, competitorCacheItem.Names.Count);
        Assert.Equal("Queens Park Rangers", competitorCacheItem.GetName(_cultures[0]));
        Assert.Equal(34, competitorCacheItem.AssociatedPlayerIds.Count());
    }

    [Fact]
    public async Task GetCompetitorProfileWhenFetchedTwiceThenInvokesPlayerProfileEndpointJustOnce()
    {
        await _profileCache.GetCompetitorProfileAsync(CreateCompetitorUrn(1), _cultures, true);
        await _profileCache.GetCompetitorProfileAsync(CreateCompetitorUrn(1), _cultures, true);

        Assert.Equal(_cultures.Count, _dataRouterManager.TotalRestCalls);
        Assert.Equal(_cultures.Count, _dataRouterManager.GetCallCount(TestDataRouterManager.EndpointCompetitor));
    }

    [Fact]
    public async Task GetCompetitorProfileWhenFetchMissingIsFalseAndCompetitorIsCachedThenReturnExisting()
    {
        await _profileCache.GetCompetitorProfileAsync(CreateCompetitorUrn(1), FilterLanguages(), false);
        _dataRouterManager.ResetMethodCall();

        var competitor = await _profileCache.GetCompetitorProfileAsync(CreateCompetitorUrn(1), _cultures, false);

        Assert.NotNull(competitor);
        Assert.Single(competitor.Names);
    }

    [Fact]
    public async Task GetCompetitorProfileWhenFetchMissingIsFalseAndCompetitorIsCachedThenNoApiCallIsMade()
    {
        await _profileCache.GetCompetitorProfileAsync(CreateCompetitorUrn(1), FilterLanguages(), false);
        _dataRouterManager.ResetMethodCall();

        await _profileCache.GetCompetitorProfileAsync(CreateCompetitorUrn(1), _cultures, false);

        Assert.Equal(0, _dataRouterManager.TotalRestCalls);
        Assert.Equal(0, _dataRouterManager.GetCallCount(TestDataRouterManager.EndpointCompetitor));
    }

    [Fact]
    public async Task GetCompetitorProfileWhenFetchMissingIsFalseAndCompetitorIsNotCachedThenApiCallAreMade()
    {
        await _profileCache.GetCompetitorProfileAsync(CreateCompetitorUrn(1), _cultures, false);

        Assert.Equal(_cultures.Count, _dataRouterManager.TotalRestCalls);
        Assert.Equal(_cultures.Count, _dataRouterManager.GetCallCount(TestDataRouterManager.EndpointCompetitor));
    }

    [Fact]
    public async Task GetCompetitorProfileWhenFetchMissingIsFalseAndCompetitorIsNotCachedThenAllIsFetchedAndReturned()
    {
        var competitor = await _profileCache.GetCompetitorProfileAsync(CreateCompetitorUrn(1), _cultures, false);

        Assert.NotNull(competitor);
        Assert.Equal(3, competitor.Names.Count());
    }

    [Fact]
    public async Task GetCompetitorProfileWhenAlreadyCachedOneLanguageThenOnlyMissingLanguagesAreFetched()
    {
        await _profileCache.GetCompetitorProfileAsync(CreateCompetitorUrn(1), FilterLanguages(), true);
        _dataRouterManager.ResetMethodCall();

        await _profileCache.GetCompetitorProfileAsync(CreateCompetitorUrn(1), _cultures, true);

        Assert.Equal(_cultures.Count - 1, _dataRouterManager.TotalRestCalls);
        Assert.Equal(_cultures.Count - 1, _dataRouterManager.GetCallCount(TestDataRouterManager.EndpointCompetitor));
    }

    [Fact]
    public async Task GetCompetitorProfileWhenCallingForDifferentIdsThenNewApiCallsAreMade()
    {
        var competitorNames1 = await _profileCache.GetCompetitorProfileAsync(CreateCompetitorUrn(1), _cultures, true);
        var competitorNames2 = await _profileCache.GetCompetitorProfileAsync(CreateCompetitorUrn(2), _cultures, true);

        Assert.NotEqual(competitorNames1.Id, competitorNames2.Id);
        Assert.Equal(_cultures.Count * 2, _dataRouterManager.TotalRestCalls);
        Assert.Equal(_cultures.Count * 2, _dataRouterManager.GetCallCount(TestDataRouterManager.EndpointCompetitor));
    }

    [Fact]
    public async Task GetCompetitorProfileWhenPreloadedFromMatchThenCompetitorProfileCallsAreStillMade()
    {
        await _dataRouterManager.GetSportEventSummaryAsync(_defaultMatchId, _cultures[0], null);
        await _dataRouterManager.GetSportEventSummaryAsync(_defaultMatchId, _cultures[1], null);
        await _dataRouterManager.GetSportEventSummaryAsync(_defaultMatchId, _cultures[2], null);
        _dataRouterManager.ResetMethodCall();

        var competitor = await _profileCache.GetCompetitorProfileAsync(CreateCompetitorUrn(1), _cultures, true);

        Assert.NotNull(competitor);
        Assert.Equal(_cultures.Count, _dataRouterManager.TotalRestCalls);
        Assert.Equal(_cultures.Count, _dataRouterManager.GetCallCount(TestDataRouterManager.EndpointCompetitor));
    }

    [Fact]
    public async Task GetCompetitorProfileWhenPartiallyPreloadedFromMatchThenCompetitorCallsIsCorrect()
    {
        await _dataRouterManager.GetSportEventSummaryAsync(_defaultMatchId, _cultures[0], null);
        _dataRouterManager.ResetMethodCall();

        await _profileCache.GetCompetitorProfileAsync(CreateCompetitorUrn(1), _cultures, true);

        Assert.Equal(_cultures.Count, _dataRouterManager.TotalRestCalls);
        Assert.Equal(0, _dataRouterManager.GetCallCount(TestDataRouterManager.EndpointSportEventSummary));
        Assert.Equal(_cultures.Count, _dataRouterManager.GetCallCount(TestDataRouterManager.EndpointCompetitor));
    }

    [Fact]
    public async Task GetCompetitorProfileWhenPartiallyPreloadedFromMatchThenCompetitorIsReturned()
    {
        await _dataRouterManager.GetSportEventSummaryAsync(_defaultMatchId, _cultures[0], null);
        _dataRouterManager.ResetMethodCall();

        var competitor = await _profileCache.GetCompetitorProfileAsync(CreateCompetitorUrn(1), _cultures, true);

        Assert.NotNull(competitor);
        Assert.Equal(3, competitor.Names.Count);
    }

    [Fact]
    public async Task GetCompetitorProfileWhenApiCallThrowsMappingExceptionThenReturnCacheNotFound()
    {
        _dataRouterManager.UriExceptions.Add(new Tuple<string, Exception>("competitor.xml", ExceptionHelper.GetMappingException()));

        var resultException = await Assert.ThrowsAsync<CacheItemNotFoundException>(() => _profileCache.GetCompetitorProfileAsync(CreateCompetitorUrn(1), FilterLanguages(), true));

        Assert.NotNull(resultException);
        Assert.True(resultException.Message.Contains("sr:competitor:1", StringComparison.InvariantCultureIgnoreCase));
    }

    [Fact]
    public async Task GetCompetitorProfileWhenApiCallThrowsDeserializationExceptionThenReturnCacheNotFound()
    {
        _dataRouterManager.UriExceptions.Add(new Tuple<string, Exception>("competitor.xml", ExceptionHelper.GetDeserializationException()));

        var resultException = await Assert.ThrowsAsync<CacheItemNotFoundException>(() => _profileCache.GetCompetitorProfileAsync(CreateCompetitorUrn(1), FilterLanguages(), true));

        Assert.NotNull(resultException);
        Assert.True(resultException.Message.Contains("sr:competitor:1", StringComparison.InvariantCultureIgnoreCase));
    }

    [Fact]
    public async Task GetCompetitorProfileWhenApiCallThrowsUnhandledExceptionThenReturnOriginalException()
    {
        _dataRouterManager.UriExceptions.Add(new Tuple<string, Exception>("competitor.xml", ExceptionHelper.GetInvalidException()));

        var resultException = await Assert.ThrowsAsync<InvalidOperationException>(() => _profileCache.GetCompetitorProfileAsync(CreateCompetitorUrn(1), FilterLanguages(), true));

        Assert.NotNull(resultException);
    }

    [Fact]
    public async Task GetPlayerNamesWhenNotCachedThenReturnFetchedValue()
    {
        var playerNames = await _profileCache.GetPlayerNamesAsync(CreatePlayerUrn(1), _cultures, true);

        Assert.NotNull(playerNames);
        Assert.NotEqual(string.Empty, playerNames[_cultures[0]]);
        Assert.NotEqual(string.Empty, playerNames[_cultures[1]]);
        Assert.NotEqual(string.Empty, playerNames[_cultures[2]]);
    }

    [Fact]
    public async Task GetPlayerNamesWhenNotCachedThenApiCallAreMade()
    {
        await _profileCache.GetPlayerNamesAsync(CreatePlayerUrn(1), _cultures, true);

        Assert.Equal(_cultures.Count, _dataRouterManager.TotalRestCalls);
        Assert.Equal(_cultures.Count, _dataRouterManager.GetCallCount(TestDataRouterManager.EndpointPlayerProfile));
    }

    [Fact]
    public async Task GetPlayerNamesWhenNotCachedThenApiIsCalledAndCached()
    {
        await _profileCache.GetPlayerNamesAsync(CreatePlayerUrn(1), _cultures, true);

        Assert.Equal(1, _profileMemoryCache.Count());
    }

    [Fact]
    public async Task GetPlayerNamesWhenMissingPlayerNameForAllCulturesAndSetNotToFetchThenNoCallIsMade()
    {
        await _profileCache.GetPlayerNamesAsync(CreatePlayerUrn(1), _cultures, false);

        Assert.Equal(0, _dataRouterManager.TotalRestCalls);
        Assert.Equal(0, _dataRouterManager.GetCallCount(TestDataRouterManager.EndpointPlayerProfile));
    }

    [Fact]
    public async Task GetPlayerNamesWhenMissingPlayerNameForAllCulturesAndSetNotToFetchThenNamesAreReturned()
    {
        var playerNames = await _profileCache.GetPlayerNamesAsync(CreatePlayerUrn(1), _cultures, false);

        Assert.NotNull(playerNames);
        Assert.Equal(_cultures.Count, playerNames.Count);
        Assert.True(playerNames.All(a => string.IsNullOrEmpty(a.Value)));
    }

    [Fact]
    public async Task GetPlayerNamesWhenMissingPlayerNameAndSetNotToFetchIfMissingThenDoesNotFetchesMissingPlayerProfile()
    {
        await _profileCache.GetPlayerProfileAsync(CreatePlayerUrn(1), FilterLanguages(), true);
        _dataRouterManager.ResetMethodCall();

        await _profileCache.GetPlayerNamesAsync(CreatePlayerUrn(1), _cultures, false);

        Assert.Equal(0, _dataRouterManager.TotalRestCalls);
        Assert.Equal(0, _dataRouterManager.GetCallCount(TestDataRouterManager.EndpointPlayerProfile));
    }

    [Fact]
    public async Task GetPlayerNamesWhenMissingPlayerNameAndSetNotToFetchIfMissingThenNamesAreReturnedWithEmptyValues()
    {
        await _profileCache.GetPlayerProfileAsync(CreatePlayerUrn(1), FilterLanguages(), true);
        _dataRouterManager.ResetMethodCall();

        var playerNames = await _profileCache.GetPlayerNamesAsync(CreatePlayerUrn(1), _cultures, false);

        Assert.NotNull(playerNames);
        Assert.Equal(3, playerNames.Count);
        Assert.NotEqual(string.Empty, playerNames[_cultures[0]]);
        Assert.Equal(string.Empty, playerNames[_cultures.Skip(1).First()]);
        Assert.Equal(string.Empty, playerNames[_cultures.Skip(2).First()]);
    }

    [Fact]
    public async Task GetPlayerNamesWhenPartiallyPopulatedFromCompetitorWhenRequestedThenFetchesMissingCompetitorProfile()
    {
        await _profileCache.GetCompetitorProfileAsync(CreateCompetitorUrn(1), FilterLanguages(), true);
        _dataRouterManager.ResetMethodCall();

        await _profileCache.GetPlayerNamesAsync(CreatePlayerUrn(1), _cultures, true);

        Assert.Equal(_cultures.Count - 1, _dataRouterManager.TotalRestCalls);
        Assert.Equal(_cultures.Count - 1, _dataRouterManager.GetCallCount(TestDataRouterManager.EndpointCompetitor));
        Assert.Equal(0, _dataRouterManager.GetCallCount(TestDataRouterManager.EndpointPlayerProfile));
    }

    [Fact]
    public async Task GetPlayerNamesWhenPartiallyPopulatedFromCompetitorWhenRequestedThenReturnsAllNames()
    {
        await _profileCache.GetCompetitorProfileAsync(CreateCompetitorUrn(1), FilterLanguages(), true);

        var playerNames = await _profileCache.GetPlayerNamesAsync(CreatePlayerUrn(1), _cultures, true);

        Assert.NotNull(playerNames);
        Assert.Equal(_cultures.Count, playerNames.Count);
        Assert.NotEqual(string.Empty, playerNames[_cultures[0]]);
        Assert.NotEqual(string.Empty, playerNames[_cultures.Skip(1).First()]);
        Assert.NotEqual(string.Empty, playerNames[_cultures.Skip(2).First()]);
    }

    [Fact]
    public async Task GetPlayerNamesWhenPrePopulatedFromCompetitorAndSetNotToFetchIfMissingThenDoesNotFetchesMissingProfile()
    {
        await _profileCache.GetCompetitorProfileAsync(CreateCompetitorUrn(1), FilterLanguages(), true);
        _dataRouterManager.ResetMethodCall();

        await _profileCache.GetPlayerNamesAsync(CreatePlayerUrn(1), _cultures, false);

        Assert.Equal(0, _dataRouterManager.TotalRestCalls);
        Assert.Equal(0, _dataRouterManager.GetCallCount(TestDataRouterManager.EndpointCompetitor));
        Assert.Equal(0, _dataRouterManager.GetCallCount(TestDataRouterManager.EndpointPlayerProfile));
    }

    [Fact]
    public async Task GetPlayerNamesWhenPrePopulatedFromCompetitorAndSetNotToFetchIfMissingThenReturnsAlsoEmptyNames()
    {
        await _profileCache.GetCompetitorProfileAsync(CreateCompetitorUrn(1), FilterLanguages(), true);

        var playerNames = await _profileCache.GetPlayerNamesAsync(CreatePlayerUrn(1), _cultures, false);

        Assert.NotNull(playerNames);
        Assert.NotEqual(string.Empty, playerNames[_cultures.First()]);
        Assert.Equal(string.Empty, playerNames[_cultures.Skip(1).First()]);
        Assert.Equal(string.Empty, playerNames[_cultures.Skip(2).First()]);
    }

    [Fact]
    public async Task GetPlayerNamesWhenFullyLoadedThenNoApiIsDone()
    {
        await _profileCache.GetPlayerNamesAsync(CreatePlayerUrn(1), _cultures, true);
        _dataRouterManager.ResetMethodCall();

        await _profileCache.GetPlayerNamesAsync(CreatePlayerUrn(1), _cultures, true);

        Assert.Equal(0, _dataRouterManager.TotalRestCalls);
        Assert.Equal(0, _dataRouterManager.GetCallCount(TestDataRouterManager.EndpointCompetitor));
        Assert.Equal(0, _dataRouterManager.GetCallCount(TestDataRouterManager.EndpointPlayerProfile));
    }

    [Fact]
    public async Task GetPlayerNamesWhenApiCallThrowsMappingExceptionThenReturnCacheNotFound()
    {
        _dataRouterManager.UriExceptions.Add(new Tuple<string, Exception>("player.xml", ExceptionHelper.GetMappingException()));

        var resultException = await Assert.ThrowsAsync<CacheItemNotFoundException>(() => _profileCache.GetPlayerNamesAsync(CreatePlayerUrn(1), FilterLanguages(), true));

        Assert.NotNull(resultException);
        Assert.True(resultException.Message.Contains("sr:player:1", StringComparison.InvariantCultureIgnoreCase));
    }

    [Fact]
    public async Task GetPlayerNamesWhenApiCallThrowsDeserializationExceptionThenReturnCacheNotFound()
    {
        _dataRouterManager.UriExceptions.Add(new Tuple<string, Exception>("player.xml", ExceptionHelper.GetDeserializationException()));

        var resultException = await Assert.ThrowsAsync<CacheItemNotFoundException>(() => _profileCache.GetPlayerNamesAsync(CreatePlayerUrn(1), FilterLanguages(), true));

        Assert.NotNull(resultException);
        Assert.True(resultException.Message.Contains("sr:player:1", StringComparison.InvariantCultureIgnoreCase));
    }

    [Fact]
    public async Task GetPlayerNamesWhenApiCallThrowsUnhandledExceptionThenReturnOriginalException()
    {
        _dataRouterManager.UriExceptions.Add(new Tuple<string, Exception>("player.xml", ExceptionHelper.GetInvalidException()));

        var resultException = await Assert.ThrowsAsync<InvalidOperationException>(() => _profileCache.GetPlayerNamesAsync(CreatePlayerUrn(1), FilterLanguages(), true));

        Assert.NotNull(resultException);
    }

    [Fact]
    public async Task GetCompetitorNamesWhenNotCachedThenReturnFetchedValue()
    {
        var competitorNames = await _profileCache.GetCompetitorNamesAsync(CreateCompetitorUrn(1), _cultures, true);

        Assert.NotNull(competitorNames);
        Assert.NotEqual(string.Empty, competitorNames[_cultures.First()]);
        Assert.NotEqual(string.Empty, competitorNames[_cultures.Skip(1).First()]);
        Assert.NotEqual(string.Empty, competitorNames[_cultures.Skip(2).First()]);
    }

    [Fact]
    public async Task GetCompetitorNamesWhenNotCachedThenMakeApiCalls()
    {
        await _profileCache.GetCompetitorNamesAsync(CreateCompetitorUrn(1), _cultures, true);

        Assert.Equal(_cultures.Count, _dataRouterManager.TotalRestCalls);
        Assert.Equal(_cultures.Count, _dataRouterManager.GetCallCount(TestDataRouterManager.EndpointCompetitor));
    }

    [Fact]
    public async Task GetCompetitorNamesWhenNotCachedThenCompetitorIsCached()
    {
        await _profileCache.GetCompetitorNamesAsync(CreateCompetitorUrn(1), _cultures, true);

        Assert.Equal(1, _profileMemoryCache.GetKeys().Count(c => c.Contains("competitor", StringComparison.InvariantCultureIgnoreCase)));
    }

    [Fact]
    public async Task GetCompetitorNamesWhenNotCachedThenAssociatedPlayersAreCached()
    {
        await _profileCache.GetCompetitorNamesAsync(CreateCompetitorUrn(1), _cultures, true);

        Assert.Equal(34, _profileMemoryCache.GetKeys().Count(c => c.Contains("player", StringComparison.InvariantCultureIgnoreCase)));
    }

    [Fact]
    public async Task GetCompetitorNamesWhenNotCachedAndSetNotToFetchThenReturnNamesAsEmpty()
    {
        var competitorNames = await _profileCache.GetCompetitorNamesAsync(CreateCompetitorUrn(1), _cultures, false);

        Assert.NotNull(competitorNames);
        Assert.Equal(_cultures.Count, competitorNames.Count);
        Assert.True(competitorNames.All(a => string.IsNullOrEmpty(a.Value)));
    }

    [Fact]
    public async Task GetCompetitorNamesWhenNotCachedAndSetNotToFetchThenNoCallsMade()
    {
        await _profileCache.GetCompetitorNamesAsync(CreateCompetitorUrn(1), _cultures, false);

        Assert.Equal(0, _dataRouterManager.TotalRestCalls);
        Assert.Equal(0, _dataRouterManager.GetCallCount(TestDataRouterManager.EndpointCompetitor));
    }

    [Fact]
    public async Task GetCompetitorNamesWhenMissingNameAndSetNotToFetchThenMissingValuesAreEmpty()
    {
        await _profileCache.GetCompetitorNamesAsync(CreateCompetitorUrn(1), FilterLanguages(), true);

        var competitorNames = await _profileCache.GetCompetitorNamesAsync(CreateCompetitorUrn(1), _cultures, false);

        Assert.NotNull(competitorNames);
        Assert.NotEqual(string.Empty, competitorNames[_cultures.First()]);
        Assert.Equal(string.Empty, competitorNames[_cultures.Skip(1).First()]);
        Assert.Equal(string.Empty, competitorNames[_cultures.Skip(2).First()]);
    }

    [Fact]
    public async Task GetCompetitorNamesWhenMissingNameAndSetNotToFetchThenNoApiCallMade()
    {
        await _profileCache.GetCompetitorNamesAsync(CreateCompetitorUrn(1), FilterLanguages(), true);
        _dataRouterManager.ResetMethodCall();

        await _profileCache.GetCompetitorNamesAsync(CreateCompetitorUrn(1), _cultures, false);

        Assert.Equal(0, _dataRouterManager.TotalRestCalls);
        Assert.Equal(0, _dataRouterManager.GetCallCount(TestDataRouterManager.EndpointCompetitor));
    }

    [Fact]
    public async Task GetCompetitorNamesWhenPartiallyPopulatedThenMissingValuesAreRetrieved()
    {
        await _profileCache.GetCompetitorNamesAsync(CreateCompetitorUrn(1), FilterLanguages(), true);

        var competitorNames = await _profileCache.GetCompetitorNamesAsync(CreateCompetitorUrn(1), _cultures, true);

        Assert.NotNull(competitorNames);
        Assert.Equal(_cultures.Count, competitorNames.Count);
        Assert.True(competitorNames.All(a => !string.IsNullOrEmpty(a.Value)));
    }

    [Fact]
    public async Task GetCompetitorNamesWhenPartiallyPopulatedThenApiCallsAreMade()
    {
        await _profileCache.GetCompetitorNamesAsync(CreateCompetitorUrn(1), FilterLanguages(), true);
        _dataRouterManager.ResetMethodCall();

        await _profileCache.GetCompetitorNamesAsync(CreateCompetitorUrn(1), _cultures, true);

        Assert.Equal(2, _dataRouterManager.TotalRestCalls);
        Assert.Equal(2, _dataRouterManager.GetCallCount(TestDataRouterManager.EndpointCompetitor));
    }

    [Fact]
    public async Task GetCompetitorNamesWhenPopulatedFromMatchThenMissingCompetitorNameFetchesMissingSummaryEndpoint()
    {
        PrepareSportEventCacheMockForMatchSummaryFetch(_defaultMatchId);
        await _dataRouterManager.GetSportEventSummaryAsync(_defaultMatchId, _cultures[0], null);
        _dataRouterManager.ResetMethodCall();

        await _profileCache.GetCompetitorNamesAsync(CreateCompetitorUrn(1), _cultures, true);

        Assert.Equal(2, _dataRouterManager.TotalRestCalls);
        Assert.Equal(2, _dataRouterManager.GetCallCount(TestDataRouterManager.EndpointSportEventSummary));
    }

    [Fact]
    public async Task GetCompetitorNamesWhenPopulatedFromMatchThenAllNamesHasValue()
    {
        PrepareSportEventCacheMockForMatchSummaryFetch(_defaultMatchId);
        await _dataRouterManager.GetSportEventSummaryAsync(_defaultMatchId, _cultures[0], null);

        var competitorNames = await _profileCache.GetCompetitorNamesAsync(CreateCompetitorUrn(1), _cultures, true);

        Assert.NotNull(competitorNames);
        Assert.Equal(_cultures.Count, competitorNames.Count);
        Assert.True(competitorNames.All(a => !string.IsNullOrEmpty(a.Value)));
    }

    [Fact]
    public async Task GetCompetitorNamesWhenPopulatedFromMatchAndSetNotToFetchIfMissingThenMissingCompetitorNameDoesNotFetch()
    {
        PrepareSportEventCacheMockForMatchSummaryFetch(_defaultMatchId);
        await _dataRouterManager.GetSportEventSummaryAsync(_defaultMatchId, _cultures[0], null);
        _dataRouterManager.ResetMethodCall();

        await _profileCache.GetCompetitorNamesAsync(CreateCompetitorUrn(1), _cultures, false);

        Assert.Equal(0, _dataRouterManager.TotalRestCalls);
        Assert.Equal(0, _dataRouterManager.GetCallCount(TestDataRouterManager.EndpointSportEventSummary));
    }

    [Fact]
    public async Task GetCompetitorNamesWhenPopulatedFromMatchAndSetNotToFetchIfMissingThenNamesHasEmptyValues()
    {
        PrepareSportEventCacheMockForMatchSummaryFetch(_defaultMatchId);
        await _dataRouterManager.GetSportEventSummaryAsync(_defaultMatchId, _cultures[0], null);

        var competitorNames = await _profileCache.GetCompetitorNamesAsync(CreateCompetitorUrn(1), _cultures, false);

        Assert.NotNull(competitorNames);
        Assert.Equal(_cultures.Count, competitorNames.Count);
        Assert.NotEqual(string.Empty, competitorNames[_cultures.First()]);
        Assert.Equal(string.Empty, competitorNames[_cultures.Skip(1).First()]);
        Assert.Equal(string.Empty, competitorNames[_cultures.Skip(2).First()]);
    }

    [Fact]
    public async Task GetCompetitorNamesWhenFullyPopulatedFromMatchThenNoAdditionalApiCall()
    {
        await _dataRouterManager.GetSportEventSummaryAsync(_defaultMatchId, _cultures[0], null);
        await _dataRouterManager.GetSportEventSummaryAsync(_defaultMatchId, _cultures[1], null);
        await _dataRouterManager.GetSportEventSummaryAsync(_defaultMatchId, _cultures[2], null);
        _dataRouterManager.ResetMethodCall();

        await _profileCache.GetCompetitorNamesAsync(CreateCompetitorUrn(1), _cultures, true);

        Assert.Equal(0, _dataRouterManager.TotalRestCalls);
        Assert.Equal(0, _dataRouterManager.GetCallCount(TestDataRouterManager.EndpointSportEventSummary));
    }

    [Fact]
    public async Task GetCompetitorNamesWhenFullyPopulatedFromMatchThenAllNamesReturned()
    {
        await _dataRouterManager.GetSportEventSummaryAsync(_defaultMatchId, _cultures[0], null);
        await _dataRouterManager.GetSportEventSummaryAsync(_defaultMatchId, _cultures[1], null);
        await _dataRouterManager.GetSportEventSummaryAsync(_defaultMatchId, _cultures[2], null);
        _dataRouterManager.ResetMethodCall();

        var competitorNames = await _profileCache.GetCompetitorNamesAsync(CreateCompetitorUrn(1), _cultures, true);

        Assert.NotNull(competitorNames);
        Assert.Equal(_cultures.Count, competitorNames.Count);
        Assert.True(competitorNames.All(a => !string.IsNullOrEmpty(a.Value)));
    }

    [Fact]
    public async Task GetCompetitorNamesWhenApiCallThrowsMappingExceptionThenReturnCacheNotFound()
    {
        _dataRouterManager.UriExceptions.Add(new Tuple<string, Exception>("competitor.xml", ExceptionHelper.GetMappingException()));

        var resultException = await Assert.ThrowsAsync<CacheItemNotFoundException>(() => _profileCache.GetCompetitorNamesAsync(CreateCompetitorUrn(1), FilterLanguages(), true));

        Assert.NotNull(resultException);
        Assert.True(resultException.Message.Contains("sr:competitor:1", StringComparison.InvariantCultureIgnoreCase));
    }

    [Fact]
    public async Task GetCompetitorNamesWhenApiCallThrowsDeserializationExceptionThenReturnCacheNotFound()
    {
        _dataRouterManager.UriExceptions.Add(new Tuple<string, Exception>("competitor.xml", ExceptionHelper.GetDeserializationException()));

        var resultException = await Assert.ThrowsAsync<CacheItemNotFoundException>(() => _profileCache.GetCompetitorNamesAsync(CreateCompetitorUrn(1), FilterLanguages(), true));

        Assert.NotNull(resultException);
        Assert.True(resultException.Message.Contains("sr:competitor:1", StringComparison.InvariantCultureIgnoreCase));
    }

    [Fact]
    public async Task GetCompetitorNamesWhenApiCallThrowsUnhandledExceptionThenReturnOriginalException()
    {
        _dataRouterManager.UriExceptions.Add(new Tuple<string, Exception>("competitor.xml", ExceptionHelper.GetInvalidException()));

        var resultException = await Assert.ThrowsAsync<InvalidOperationException>(() => _profileCache.GetCompetitorNamesAsync(CreateCompetitorUrn(1), FilterLanguages(), true));

        Assert.NotNull(resultException);
    }

    [Fact]
    public async Task GetCompetitorNamesWhenPartiallyPopulatedFromMatchAndSummaryThrowsThenCompetitorProfileIsFetched()
    {
        await _dataRouterManager.GetSportEventSummaryAsync(_defaultMatchId, _cultures[0], null);
        _dataRouterManager.ResetMethodCall();

        await _profileCache.GetCompetitorNamesAsync(CreateCompetitorUrn(1), _cultures, true);

        Assert.Equal(2, _dataRouterManager.TotalRestCalls);
        Assert.Equal(2, _dataRouterManager.GetCallCount(TestDataRouterManager.EndpointCompetitor));
    }

    [Fact]
    public async Task GetCompetitorNamesWhenPartiallyPopulatedFromMatchAndSummaryThrowsThenNamesAreReturned()
    {
        await _dataRouterManager.GetSportEventSummaryAsync(_defaultMatchId, _cultures[0], null);

        var competitorNames = await _profileCache.GetCompetitorNamesAsync(CreateCompetitorUrn(1), _cultures, true);

        Assert.NotNull(competitorNames);
        Assert.Equal(_cultures.Count, competitorNames.Count);
        Assert.True(competitorNames.All(a => !string.IsNullOrEmpty(a.Value)));
    }

    [Fact]
    public async Task GetPlayerNameWhenFullyLoadedThenNoApiIsDone()
    {
        await _profileCache.GetPlayerNamesAsync(CreatePlayerUrn(1), _cultures, true);
        _dataRouterManager.ResetMethodCall();

        await _profileCache.GetPlayerNameAsync(CreatePlayerUrn(1), _cultures[0], true);

        Assert.Equal(0, _dataRouterManager.TotalRestCalls);
        Assert.Equal(0, _dataRouterManager.GetCallCount(TestDataRouterManager.EndpointCompetitor));
        Assert.Equal(0, _dataRouterManager.GetCallCount(TestDataRouterManager.EndpointPlayerProfile));
    }

    [Fact]
    public async Task GetPlayerNameWhenNotCachedThenApiCallAreMade()
    {
        await _profileCache.GetPlayerNameAsync(CreatePlayerUrn(1), _cultures[0], true);

        Assert.Equal(1, _dataRouterManager.TotalRestCalls);
        Assert.Equal(1, _dataRouterManager.GetCallCount(TestDataRouterManager.EndpointPlayerProfile));
    }

    [Fact]
    public async Task GetPlayerNameWhenNotCachedThenApiIsCalledAndCached()
    {
        await _profileCache.GetPlayerNameAsync(CreatePlayerUrn(1), _cultures[0], true);

        Assert.Equal(1, _profileMemoryCache.Count());
    }

    [Fact]
    public async Task GetPlayerNameWhenMissingPlayerNameAndSetNotToFetchThenNoCallIsMade()
    {
        await _profileCache.GetPlayerNameAsync(CreatePlayerUrn(1), _cultures[0], false);

        Assert.Equal(0, _dataRouterManager.TotalRestCalls);
        Assert.Equal(0, _dataRouterManager.GetCallCount(TestDataRouterManager.EndpointPlayerProfile));
    }

    [Fact]
    public async Task GetPlayerNameWhenMissingPlayerNameAndSetNotToFetchThenEmptyIsReturned()
    {
        var name = await _profileCache.GetPlayerNameAsync(CreatePlayerUrn(1), _cultures[0], false);

        Assert.Empty(name);
    }

    [Fact]
    public async Task GetPlayerNameWhenFullyLoadedThenNameIsReturned()
    {
        await _profileCache.GetPlayerNamesAsync(CreatePlayerUrn(1), _cultures, true);
        _dataRouterManager.ResetMethodCall();

        var name = await _profileCache.GetPlayerNameAsync(CreatePlayerUrn(1), _cultures[0], true);

        Assert.NotNull(name);
        Assert.False(string.IsNullOrEmpty(name));
    }

    [Fact]
    public async Task GetPlayerNameWhenApiCallThrowsMappingExceptionThenReturnCacheNotFound()
    {
        _dataRouterManager.UriExceptions.Add(new Tuple<string, Exception>("player.xml", ExceptionHelper.GetMappingException()));

        var resultException = await Assert.ThrowsAsync<CacheItemNotFoundException>(() => _profileCache.GetPlayerNameAsync(CreatePlayerUrn(1), _cultures[0], true));

        Assert.NotNull(resultException);
        Assert.True(resultException.Message.Contains("sr:player:1", StringComparison.InvariantCultureIgnoreCase));
    }

    [Fact]
    public async Task GetPlayerNameWhenApiCallThrowsDeserializationExceptionThenReturnCacheNotFound()
    {
        _dataRouterManager.UriExceptions.Add(new Tuple<string, Exception>("player.xml", ExceptionHelper.GetDeserializationException()));

        var resultException = await Assert.ThrowsAsync<CacheItemNotFoundException>(() => _profileCache.GetPlayerNameAsync(CreatePlayerUrn(1), _cultures[0], true));

        Assert.NotNull(resultException);
        Assert.True(resultException.Message.Contains("sr:player:1", StringComparison.InvariantCultureIgnoreCase));
    }

    [Fact]
    public async Task GetPlayerNameWhenApiCallThrowsUnhandledExceptionThenReturnOriginalException()
    {
        _dataRouterManager.UriExceptions.Add(new Tuple<string, Exception>("player.xml", ExceptionHelper.GetInvalidException()));

        var resultException = await Assert.ThrowsAsync<InvalidOperationException>(() => _profileCache.GetPlayerNameAsync(CreatePlayerUrn(1), _cultures[0], true));

        Assert.NotNull(resultException);
    }

    [Fact]
    public async Task GetCompetitorNameWhenFullyLoadedThenNoApiIsDone()
    {
        await _profileCache.GetCompetitorNamesAsync(CreateCompetitorUrn(1), _cultures, true);
        _dataRouterManager.ResetMethodCall();

        await _profileCache.GetCompetitorNameAsync(CreateCompetitorUrn(1), _cultures[0], true);

        Assert.Equal(0, _dataRouterManager.TotalRestCalls);
        Assert.Equal(0, _dataRouterManager.GetCallCount(TestDataRouterManager.EndpointCompetitor));
        Assert.Equal(0, _dataRouterManager.GetCallCount(TestDataRouterManager.EndpointCompetitor));
    }

    [Fact]
    public async Task GetCompetitorNameWhenNotCachedThenApiCallAreMade()
    {
        await _profileCache.GetCompetitorNameAsync(CreateCompetitorUrn(1), _cultures[0], true);

        Assert.Equal(1, _dataRouterManager.TotalRestCalls);
        Assert.Equal(1, _dataRouterManager.GetCallCount(TestDataRouterManager.EndpointCompetitor));
    }

    [Fact]
    public async Task GetCompetitorNameWhenNotCachedThenApiIsCalledAndCached()
    {
        await _profileCache.GetCompetitorNameAsync(CreateCompetitorUrn(1), _cultures[0], true);

        Assert.Equal(35, _profileMemoryCache.Count());
    }

    [Fact]
    public async Task GetCompetitorNameWhenMissingCompetitorNameAndSetNotToFetchThenNoCallIsMade()
    {
        await _profileCache.GetCompetitorNameAsync(CreateCompetitorUrn(1), _cultures[0], false);

        Assert.Equal(0, _dataRouterManager.TotalRestCalls);
        Assert.Equal(0, _dataRouterManager.GetCallCount(TestDataRouterManager.EndpointCompetitor));
    }

    [Fact]
    public async Task GetCompetitorNameWhenMissingCompetitorNameAndSetNotToFetchThenEmptyIsReturned()
    {
        var name = await _profileCache.GetCompetitorNameAsync(CreateCompetitorUrn(1), _cultures[0], false);

        Assert.Empty(name);
    }

    [Fact]
    public async Task GetCompetitorNameWhenFullyLoadedThenNameIsReturned()
    {
        await _profileCache.GetCompetitorNamesAsync(CreateCompetitorUrn(1), _cultures, true);
        _dataRouterManager.ResetMethodCall();

        var name = await _profileCache.GetCompetitorNameAsync(CreateCompetitorUrn(1), _cultures[0], true);

        Assert.NotNull(name);
        Assert.False(string.IsNullOrEmpty(name));
    }

    [Fact]
    public async Task GetCompetitorNameWhenApiCallThrowsMappingExceptionThenReturnCacheNotFound()
    {
        _dataRouterManager.UriExceptions.Add(new Tuple<string, Exception>("competitor.xml", ExceptionHelper.GetMappingException()));

        var resultException = await Assert.ThrowsAsync<CacheItemNotFoundException>(() => _profileCache.GetCompetitorNameAsync(CreateCompetitorUrn(1), _cultures[0], true));

        Assert.NotNull(resultException);
        Assert.True(resultException.Message.Contains("sr:competitor:1", StringComparison.InvariantCultureIgnoreCase));
    }

    [Fact]
    public async Task GetCompetitorNameWhenApiCallThrowsDeserializationExceptionThenReturnCacheNotFound()
    {
        _dataRouterManager.UriExceptions.Add(new Tuple<string, Exception>("competitor.xml", ExceptionHelper.GetDeserializationException()));

        var resultException = await Assert.ThrowsAsync<CacheItemNotFoundException>(() => _profileCache.GetCompetitorNameAsync(CreateCompetitorUrn(1), _cultures[0], true));

        Assert.NotNull(resultException);
        Assert.True(resultException.Message.Contains("sr:competitor:1", StringComparison.InvariantCultureIgnoreCase));
    }

    [Fact]
    public async Task GetCompetitorNameWhenApiCallThrowsUnhandledExceptionThenReturnOriginalException()
    {
        _dataRouterManager.UriExceptions.Add(new Tuple<string, Exception>("competitor.xml", ExceptionHelper.GetInvalidException()));

        var resultException = await Assert.ThrowsAsync<InvalidOperationException>(() => _profileCache.GetCompetitorNameAsync(CreateCompetitorUrn(1), _cultures[0], true));

        Assert.NotNull(resultException);
    }
}
