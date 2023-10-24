using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Sportradar.OddsFeed.SDK.Api.Internal.Caching;
using Sportradar.OddsFeed.SDK.Api.Internal.Config;
using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Common.Extensions;
using Sportradar.OddsFeed.SDK.Common.Internal;
using Sportradar.OddsFeed.SDK.Common.Internal.Extensions;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Dto;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Enums;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Mapping;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.MarketNames;
using Sportradar.OddsFeed.SDK.Messages.Rest;
using Sportradar.OddsFeed.SDK.Tests.Common;
using Xunit;
using Xunit.Abstractions;

namespace Sportradar.OddsFeed.SDK.Tests.Entities.Rest.Markets;

public class MarketCacheProviderTests
{
    private readonly ITestOutputHelper _outputHelper;
    private readonly IMarketDescriptionsCache _invariantMarketDescriptionCache;
    private readonly IVariantDescriptionsCache _variantDescriptionListCache;
    private readonly IMarketDescriptionCache _variantMarketDescriptionCache;
    private readonly IMarketCacheProvider _marketCacheProvider;

    private readonly TestDataRouterManager _dataRouterManager;
    private readonly ICacheStore<string> _invariantMarketDescriptionMemoryCache;
    private readonly ICacheStore<string> _variantMarketDescriptionListMemoryCache;
    private readonly ICacheStore<string> _variantMarketDescriptionMemoryCache;
    private readonly IReadOnlyList<CultureInfo> _cultures;
    private readonly CacheManager _cacheManager;

    public MarketCacheProviderTests(ITestOutputHelper outputHelper)
    {
        _outputHelper = outputHelper;
        _cultures = TestData.Cultures.ToList();

        var testCacheStoreManager = new TestCacheStoreManager();
        _invariantMarketDescriptionMemoryCache = testCacheStoreManager.ServiceProvider.GetSdkCacheStore<string>(UofSdkBootstrap.CacheStoreNameForInvariantMarketDescriptionsCache);
        _variantMarketDescriptionListMemoryCache = testCacheStoreManager.ServiceProvider.GetSdkCacheStore<string>(UofSdkBootstrap.CacheStoreNameForVariantDescriptionListCache);
        _variantMarketDescriptionMemoryCache = testCacheStoreManager.ServiceProvider.GetSdkCacheStore<string>(UofSdkBootstrap.CacheStoreNameForVariantMarketDescriptionCache);

        _cacheManager = new CacheManager();
        _dataRouterManager = new TestDataRouterManager(_cacheManager, outputHelper);

        IMappingValidatorFactory mappingValidatorFactory = new MappingValidatorFactory();

        var timerVdl = new TestTimer(true);
        var timerIdl = new TestTimer(true);
        _invariantMarketDescriptionCache = new InvariantMarketDescriptionCache(_invariantMarketDescriptionMemoryCache, _dataRouterManager, mappingValidatorFactory, timerIdl, _cultures, _cacheManager);
        _variantDescriptionListCache = new VariantDescriptionListCache(_variantMarketDescriptionListMemoryCache, _dataRouterManager, mappingValidatorFactory, timerVdl, _cultures, _cacheManager);
        _variantMarketDescriptionCache = new VariantMarketDescriptionCache(_variantMarketDescriptionMemoryCache, _dataRouterManager, mappingValidatorFactory, _cacheManager);

        _marketCacheProvider = new MarketCacheProvider(_invariantMarketDescriptionCache, _variantMarketDescriptionCache, _variantDescriptionListCache);
    }

    [Fact]
    public void InitialCallsDone()
    {
        Assert.Equal(TestData.InvariantListCacheCount, _invariantMarketDescriptionMemoryCache.Count());
        Assert.True(_variantMarketDescriptionListMemoryCache.Count() > 0);
        Assert.Equal(0, _variantMarketDescriptionMemoryCache.Count());

        Assert.NotNull(_invariantMarketDescriptionCache);
        Assert.NotNull(_variantDescriptionListCache);
        Assert.NotNull(_variantMarketDescriptionCache);

        Assert.Equal(_cultures.Count * 2, _dataRouterManager.TotalRestCalls);
        Assert.Equal(_cultures.Count, _dataRouterManager.GetCallCount("GetMarketDescriptionsAsync"));
        Assert.Equal(_cultures.Count, _dataRouterManager.GetCallCount("GetVariantDescriptionsAsync"));
        Assert.Equal(0, _dataRouterManager.GetCallCount("GetVariantMarketDescriptionAsync"));

        Assert.Equal(TestData.InvariantListCacheCount, _invariantMarketDescriptionMemoryCache.Count());
        Assert.Equal(TestData.VariantListCacheCount, _variantMarketDescriptionListMemoryCache.Count());
    }

    [Fact]
    public async Task GetInvariantMarketDescription()
    {
        var marketDescription = await _marketCacheProvider.GetMarketDescriptionAsync(282, null, _cultures, true);

        Assert.Equal(_cultures.Count * 2, _dataRouterManager.TotalRestCalls);
        Assert.Equal(_cultures.Count, _dataRouterManager.GetCallCount("GetMarketDescriptionsAsync"));
        Assert.Equal(_cultures.Count, _dataRouterManager.GetCallCount("GetVariantDescriptionsAsync"));
        Assert.Equal(0, _dataRouterManager.GetCallCount("GetVariantMarketDescriptionAsync"));

        Assert.NotNull(marketDescription);
        Assert.Equal(282, marketDescription.Id);
        Assert.Single(marketDescription.Specifiers);
        Assert.Equal(2, marketDescription.Outcomes.Count());
        Assert.Single(marketDescription.Mappings);
        Assert.NotEqual(marketDescription.GetName(_cultures[0]), marketDescription.GetName(_cultures[1]));
        Assert.NotEqual(marketDescription.GetName(_cultures[0]), marketDescription.GetName(_cultures[2]));
        Assert.NotEqual(marketDescription.GetName(_cultures[1]), marketDescription.GetName(_cultures[2]));
    }

    [Fact]
    public async Task GetNonExistingInvariantMarketDescription()
    {
        var marketDescription = await _marketCacheProvider.GetMarketDescriptionAsync(2820, null, _cultures, true);

        Assert.Equal(_cultures.Count * 2, _dataRouterManager.TotalRestCalls);
        Assert.Equal(_cultures.Count, _dataRouterManager.GetCallCount("GetMarketDescriptionsAsync"));
        Assert.Equal(_cultures.Count, _dataRouterManager.GetCallCount("GetVariantDescriptionsAsync"));
        Assert.Equal(0, _dataRouterManager.GetCallCount("GetVariantMarketDescriptionAsync"));

        Assert.Null(marketDescription);
    }

    [Fact]
    public async Task GetVariantMarketDescriptionFromList()
    {
        var specifiers = new Dictionary<string, string> { { "variant", "sr:correct_score:bestof:12" } };
        var marketDescription = await _marketCacheProvider.GetMarketDescriptionAsync(374, specifiers, _cultures, true);

        Assert.Equal(_cultures.Count * 2, _dataRouterManager.TotalRestCalls);
        Assert.Equal(_cultures.Count, _dataRouterManager.GetCallCount("GetMarketDescriptionsAsync"));
        Assert.Equal(_cultures.Count, _dataRouterManager.GetCallCount("GetVariantDescriptionsAsync"));
        Assert.Equal(0, _dataRouterManager.GetCallCount("GetVariantMarketDescriptionAsync"));

        Assert.NotNull(marketDescription);
        Assert.Equal(374, marketDescription.Id);
        Assert.Equal(2, marketDescription.Specifiers.Count());
        Assert.Equal(13, marketDescription.Outcomes.Count());
        Assert.Single(marketDescription.Mappings);
        Assert.NotEqual(marketDescription.GetName(_cultures[0]), marketDescription.GetName(_cultures[1]));
        Assert.NotEqual(marketDescription.GetName(_cultures[0]), marketDescription.GetName(_cultures[2]));
        Assert.NotEqual(marketDescription.GetName(_cultures[1]), marketDescription.GetName(_cultures[2]));
    }

    [Fact]
    public async Task GetVariantMarketDescriptionFromListMultipleMappings()
    {
        var specifiers = new Dictionary<string, string> { { "variant", "sr:decided_by_extra_points:bestof:5" } };
        var marketDescription = await _marketCacheProvider.GetMarketDescriptionAsync(239, specifiers, _cultures, true);

        Assert.Equal(_cultures.Count * 2, _dataRouterManager.TotalRestCalls);
        Assert.Equal(_cultures.Count, _dataRouterManager.GetCallCount("GetMarketDescriptionsAsync"));
        Assert.Equal(_cultures.Count, _dataRouterManager.GetCallCount("GetVariantDescriptionsAsync"));
        Assert.Equal(0, _dataRouterManager.GetCallCount("GetVariantMarketDescriptionAsync"));

        Assert.NotNull(marketDescription);
        Assert.Equal(239, marketDescription.Id);
        Assert.Single(marketDescription.Specifiers);
        Assert.Contains("variant", marketDescription.Specifiers.Select(s => s.Name));
        Assert.Equal(6, marketDescription.Outcomes.Count());
        Assert.Equal(4, marketDescription.Mappings.Count());
        Assert.NotEqual(marketDescription.GetName(_cultures[0]), marketDescription.GetName(_cultures[1]));
        Assert.NotEqual(marketDescription.GetName(_cultures[0]), marketDescription.GetName(_cultures[2]));
        Assert.NotEqual(marketDescription.GetName(_cultures[1]), marketDescription.GetName(_cultures[2]));
    }

    [Fact]
    public async Task GetVariantMarketDescriptionFromSinglePreOutcomeText()
    {
        const int marketId = 534;
        const string variantSpecifier = "pre:markettext:168883";
        _dataRouterManager.UriReplacements.Add(new Tuple<string, string>($"{marketId}_{variantSpecifier}", $"variant_market_description_{marketId}_culture.xml"));
        Assert.Equal(0, _variantMarketDescriptionMemoryCache.Count());
        var specifiers = new Dictionary<string, string> { { "variant", variantSpecifier } };
        var marketDescription = await _marketCacheProvider.GetMarketDescriptionAsync(marketId, specifiers, _cultures, true);

        Assert.Equal(_cultures.Count * 3, _dataRouterManager.TotalRestCalls);
        Assert.Equal(_cultures.Count, _dataRouterManager.GetCallCount("GetMarketDescriptionsAsync"));
        Assert.Equal(_cultures.Count, _dataRouterManager.GetCallCount("GetVariantDescriptionsAsync"));
        Assert.Equal(_cultures.Count, _dataRouterManager.GetCallCount("GetVariantMarketDescriptionAsync"));
        Assert.Equal(1, _variantMarketDescriptionMemoryCache.Count());

        Assert.NotNull(marketDescription);
        Assert.Equal(marketId, marketDescription.Id);
        Assert.Single(marketDescription.Specifiers);
        Assert.Contains("variant", marketDescription.Specifiers.Select(s => s.Name));
        Assert.Equal(9, marketDescription.Outcomes.Count());
        Assert.Single(marketDescription.Mappings);
        Assert.NotEqual(marketDescription.GetName(_cultures[0]), marketDescription.GetName(_cultures[1]));
        Assert.NotEqual(marketDescription.GetName(_cultures[0]), marketDescription.GetName(_cultures[2]));
        Assert.NotEqual(marketDescription.GetName(_cultures[1]), marketDescription.GetName(_cultures[2]));
    }

    [Fact]
    public async Task GetVariantMarketDescriptionFromSinglePlayerProps()
    {
        const int marketId = 768;
        const string variantSpecifier = "pre:playerprops:35432179:608000";
        _dataRouterManager.UriReplacements.Add(new Tuple<string, string>($"{marketId}_{variantSpecifier}", $"variant_market_description_{marketId}_culture.xml"));
        Assert.Equal(0, _variantMarketDescriptionMemoryCache.Count());
        var specifiers = new Dictionary<string, string> { { "variant", variantSpecifier } };
        var marketDescription = await _marketCacheProvider.GetMarketDescriptionAsync(marketId, specifiers, _cultures, true);

        Assert.Equal(_cultures.Count * 3, _dataRouterManager.TotalRestCalls);
        Assert.Equal(_cultures.Count, _dataRouterManager.GetCallCount("GetMarketDescriptionsAsync"));
        Assert.Equal(_cultures.Count, _dataRouterManager.GetCallCount("GetVariantDescriptionsAsync"));
        Assert.Equal(_cultures.Count, _dataRouterManager.GetCallCount("GetVariantMarketDescriptionAsync"));
        Assert.Equal(1, _variantMarketDescriptionMemoryCache.Count());

        Assert.NotNull(marketDescription);
        Assert.Equal(marketId, marketDescription.Id);
        Assert.Single(marketDescription.Specifiers);
        Assert.Contains("variant", marketDescription.Specifiers.Select(s => s.Name));
        Assert.Equal(18, marketDescription.Outcomes.Count());
        Assert.Single(marketDescription.Mappings);
        Assert.NotEqual(marketDescription.GetName(_cultures[0]), marketDescription.GetName(_cultures[1]));
        Assert.NotEqual(marketDescription.GetName(_cultures[0]), marketDescription.GetName(_cultures[2]));
        Assert.NotEqual(marketDescription.GetName(_cultures[1]), marketDescription.GetName(_cultures[2]));
    }

    [Fact]
    public async Task GetVariantMarketDescriptionFromSingleUnsupportedCompetitorProps()
    {
        const int marketId = 1768;
        const string variantSpecifier = "pre:competitorprops:35432179:608000";
        await LoadNewInvariantMarketDescriptionAsync(768, marketId, 1, _cultures).ConfigureAwait(false);
        _dataRouterManager.UriReplacements.Add(new Tuple<string, string>($"{marketId}_{variantSpecifier}", $"variant_market_description_{marketId}_cp_culture.xml"));
        Assert.Equal(TestData.InvariantListCacheCount + 1, _invariantMarketDescriptionMemoryCache.Count());
        Assert.Equal(0, _variantMarketDescriptionMemoryCache.Count());
        var specifiers = new Dictionary<string, string> { { "variant", variantSpecifier } };
        var marketDescription = await _marketCacheProvider.GetMarketDescriptionAsync(marketId, specifiers, _cultures, true);

        Assert.Equal(_cultures.Count * 3, _dataRouterManager.TotalRestCalls);
        Assert.Equal(_cultures.Count, _dataRouterManager.GetCallCount("GetMarketDescriptionsAsync"));
        Assert.Equal(_cultures.Count, _dataRouterManager.GetCallCount("GetVariantDescriptionsAsync"));
        Assert.Equal(_cultures.Count, _dataRouterManager.GetCallCount("GetVariantMarketDescriptionAsync"));
        Assert.Equal(1, _variantMarketDescriptionMemoryCache.Count());

        Assert.NotNull(marketDescription);
        Assert.Equal(marketId, marketDescription.Id);
        Assert.Single(marketDescription.Specifiers);
        Assert.Contains("variant", marketDescription.Specifiers.Select(s => s.Name));
        Assert.Equal(18, marketDescription.Outcomes.Count());
        Assert.Single(marketDescription.Mappings);
        Assert.NotEqual(marketDescription.GetName(_cultures[0]), marketDescription.GetName(_cultures[1]));
        Assert.NotEqual(marketDescription.GetName(_cultures[0]), marketDescription.GetName(_cultures[2]));
        Assert.NotEqual(marketDescription.GetName(_cultures[1]), marketDescription.GetName(_cultures[2]));
    }

    [Fact]
    public async Task GetMarketDescriptionForOutComeTypePlayer()
    {
        const int marketId = 679;
        const string specifiers = "maxovers=20|type=live|inningnr=1";
        Assert.Equal(0, _variantMarketDescriptionMemoryCache.Count());
        var marketDescription = await _marketCacheProvider.GetMarketDescriptionAsync(marketId, new ReadOnlyDictionary<string, string>(SdkInfo.SpecifiersStringToDictionary(specifiers)), _cultures, true);

        Assert.Equal(_cultures.Count * 2, _dataRouterManager.TotalRestCalls);
        Assert.Equal(_cultures.Count, _dataRouterManager.GetCallCount("GetMarketDescriptionsAsync"));
        Assert.Equal(_cultures.Count, _dataRouterManager.GetCallCount("GetVariantDescriptionsAsync"));
        Assert.Equal(0, _dataRouterManager.GetCallCount("GetVariantMarketDescriptionAsync"));
        Assert.Equal(0, _variantMarketDescriptionMemoryCache.Count());

        Assert.NotNull(marketDescription);
        Assert.Equal(marketId, marketDescription.Id);
        Assert.Equal(2, marketDescription.Specifiers.Count());
        Assert.True(marketDescription.Outcomes.IsNullOrEmpty());
        Assert.Single(marketDescription.Mappings);
        Assert.NotEqual(marketDescription.GetName(_cultures[0]), marketDescription.GetName(_cultures[1]));
        Assert.NotEqual(marketDescription.GetName(_cultures[0]), marketDescription.GetName(_cultures[2]));
        Assert.NotEqual(marketDescription.GetName(_cultures[1]), marketDescription.GetName(_cultures[2]));
    }

    [Fact]
    public async Task GetMarketDescriptionForOutComeTypeCompetitor()
    {
        const int marketId = 1109;
        const string specifiers = "lapnr=5";
        Assert.Equal(0, _variantMarketDescriptionMemoryCache.Count());
        var marketDescription = await _marketCacheProvider.GetMarketDescriptionAsync(marketId, new ReadOnlyDictionary<string, string>(SdkInfo.SpecifiersStringToDictionary(specifiers)), _cultures, true);

        Assert.Equal(_cultures.Count * 2, _dataRouterManager.TotalRestCalls);
        Assert.Equal(_cultures.Count, _dataRouterManager.GetCallCount("GetMarketDescriptionsAsync"));
        Assert.Equal(_cultures.Count, _dataRouterManager.GetCallCount("GetVariantDescriptionsAsync"));
        Assert.Equal(0, _dataRouterManager.GetCallCount("GetVariantMarketDescriptionAsync"));
        Assert.Equal(0, _variantMarketDescriptionMemoryCache.Count());

        Assert.NotNull(marketDescription);
        Assert.Equal(marketId, marketDescription.Id);
        Assert.Single(marketDescription.Specifiers);
        Assert.True(marketDescription.Outcomes.IsNullOrEmpty());
        Assert.Single(marketDescription.Mappings);
    }

    [Fact]
    public async Task GetMarketDescriptionWithCompetitorInOutcomeTemplate()
    {
        const int marketId = 303;
        const string specifiers = "quarternr=4|hcp=-3.5";
        Assert.Equal(0, _variantMarketDescriptionMemoryCache.Count());
        var marketDescription = await _marketCacheProvider.GetMarketDescriptionAsync(marketId, new ReadOnlyDictionary<string, string>(SdkInfo.SpecifiersStringToDictionary(specifiers)), _cultures, true);

        Assert.Equal(_cultures.Count * 2, _dataRouterManager.TotalRestCalls);
        Assert.Equal(_cultures.Count, _dataRouterManager.GetCallCount("GetMarketDescriptionsAsync"));
        Assert.Equal(_cultures.Count, _dataRouterManager.GetCallCount("GetVariantDescriptionsAsync"));
        Assert.Equal(0, _dataRouterManager.GetCallCount("GetVariantMarketDescriptionAsync"));
        Assert.Equal(0, _variantMarketDescriptionMemoryCache.Count());

        Assert.NotNull(marketDescription);
        Assert.Equal(marketId, marketDescription.Id);
        Assert.Equal(2, marketDescription.Specifiers.Count());
        Assert.Equal(2, marketDescription.Outcomes.Count());
        Assert.Equal(25, marketDescription.Mappings.Count());
        Assert.NotEqual(marketDescription.GetName(_cultures[0]), marketDescription.GetName(_cultures[1]));
        Assert.NotEqual(marketDescription.GetName(_cultures[0]), marketDescription.GetName(_cultures[2]));
        Assert.NotEqual(marketDescription.GetName(_cultures[1]), marketDescription.GetName(_cultures[2]));
    }

    [Fact]
    public async Task GetMarketDescriptionWithScoreInNameTemplate()
    {
        const int marketId = 41;
        const string specifiers = "score=2:0";
        Assert.Equal(0, _variantMarketDescriptionMemoryCache.Count());
        var marketDescription = await _marketCacheProvider.GetMarketDescriptionAsync(marketId, new ReadOnlyDictionary<string, string>(SdkInfo.SpecifiersStringToDictionary(specifiers)), _cultures, true);

        Assert.Equal(_cultures.Count * 2, _dataRouterManager.TotalRestCalls);
        Assert.Equal(_cultures.Count, _dataRouterManager.GetCallCount("GetMarketDescriptionsAsync"));
        Assert.Equal(_cultures.Count, _dataRouterManager.GetCallCount("GetVariantDescriptionsAsync"));
        Assert.Equal(0, _dataRouterManager.GetCallCount("GetVariantMarketDescriptionAsync"));
        Assert.Equal(0, _variantMarketDescriptionMemoryCache.Count());

        Assert.NotNull(marketDescription);
        Assert.Equal(marketId, marketDescription.Id);
        Assert.Single(marketDescription.Specifiers);
        Assert.Equal(28, marketDescription.Outcomes.Count());
        Assert.Equal(5, marketDescription.Mappings.Count());
        Assert.NotEqual(marketDescription.GetName(_cultures[0]), marketDescription.GetName(_cultures[1]));
        Assert.NotEqual(marketDescription.GetName(_cultures[0]), marketDescription.GetName(_cultures[2]));
        Assert.NotEqual(marketDescription.GetName(_cultures[1]), marketDescription.GetName(_cultures[2]));
    }

    [Fact]
    public async Task GetVariantMarketDescriptionFromSinglePreOutcomeTextInvalidOutcome()
    {
        const int marketId = 534;
        const string variantSpecifier = "pre:markettext:168883";
        _dataRouterManager.UriReplacements.Add(new Tuple<string, string>($"{marketId}_{variantSpecifier}", $"variant_market_description_invalid_{marketId}_culture.xml"));
        Assert.Equal(0, _variantMarketDescriptionMemoryCache.Count());
        var specifiers = new Dictionary<string, string> { { "variant", variantSpecifier } };
        var marketDescription = await _marketCacheProvider.GetMarketDescriptionAsync(marketId, specifiers, TestData.Cultures1, true);

        Assert.Equal((_cultures.Count * 2) + 1, _dataRouterManager.TotalRestCalls);
        Assert.Equal(_cultures.Count, _dataRouterManager.GetCallCount("GetMarketDescriptionsAsync"));
        Assert.Equal(_cultures.Count, _dataRouterManager.GetCallCount("GetVariantDescriptionsAsync"));
        Assert.Equal(TestData.Cultures1.Count, _dataRouterManager.GetCallCount("GetVariantMarketDescriptionAsync"));
        Assert.Equal(0, _variantMarketDescriptionMemoryCache.Count());

        Assert.Null(marketDescription);
    }

    private async Task LoadNewInvariantMarketDescriptionAsync(int existingId, int newId, int doAction, IReadOnlyCollection<CultureInfo> cultures)
    {
        foreach (var culture in cultures)
        {
            await LoadNewInvariantMarketDescriptionAsync(existingId, newId, doAction, culture).ConfigureAwait(false);
        }
    }

    private async Task LoadNewInvariantMarketDescriptionAsync(int existingId, int newId, int doAction, CultureInfo culture)
    {
        var restDeserializer = new Deserializer<market_descriptions>();
        var mapper = new MarketDescriptionsMapperFactory();

        var resourceName = $"invariant_market_descriptions_{culture.TwoLetterISOLanguageName}.xml";
        await using var stream = FileHelper.GetResource(resourceName);

        if (stream != null)
        {
            var result = mapper.CreateMapper(restDeserializer.Deserialize(stream)).Map();
            if (result?.Items.IsNullOrEmpty() == false)
            {
                var existingMd = result.Items.First(f => f.Id.Equals(existingId));
                existingMd.OverrideId(newId);
                switch (doAction)
                {
                    case 1:
                        {
                            // switch player_props to unknown competitor_props
                            var newGroups = existingMd.Groups.Where(s => !s.Contains("player_props")).ToList();
                            newGroups.Add("competitor_props");
                            existingMd.OverrideGroups(newGroups);
                            break;
                        }
                    default:
                        return;
                }

                var items = new EntityList<MarketDescriptionDto>(new List<MarketDescriptionDto> { existingMd });
                await _cacheManager.SaveDtoAsync(Urn.Parse("sr:markets:1"), items, culture, DtoType.MarketDescriptionList, null).ConfigureAwait(false);
            }
            else
            {
                _outputHelper.WriteLine($"No results found for {resourceName}");
            }
        }
    }
}
