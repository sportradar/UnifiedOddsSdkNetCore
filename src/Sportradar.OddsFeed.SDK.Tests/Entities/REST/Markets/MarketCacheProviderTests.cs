using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Runtime.Caching;
using System.Threading.Tasks;
using Castle.Core.Internal;
using Sportradar.OddsFeed.SDK.Common.Internal;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.DTO;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.Enums;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.Mapping;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.MarketNames;
using Sportradar.OddsFeed.SDK.Messages;
using Sportradar.OddsFeed.SDK.Messages.REST;
using Sportradar.OddsFeed.SDK.Tests.Common;
using Xunit;
using Xunit.Abstractions;

namespace Sportradar.OddsFeed.SDK.Tests.Entities.REST.Markets
{
    public class MarketCacheProviderTests
    {
        private readonly ITestOutputHelper _outputHelper;
        private readonly IMarketDescriptionCache _invariantMarketDescriptionCache;
        private readonly IVariantDescriptionCache _variantDescriptionListCache;
        private readonly IMarketDescriptionCache _variantMarketDescriptionCache;
        private readonly IMarketCacheProvider _marketCacheProvider;

        private readonly TestDataRouterManager _dataRouterManager;
        private readonly MemoryCache _invariantMarketDescriptionMemoryCache;
        private readonly MemoryCache _variantMarketDescriptionListMemoryCache;
        private readonly MemoryCache _variantMarketDescriptionMemoryCache;
        private readonly IReadOnlyList<CultureInfo> _cultures;
        private readonly CacheManager _cacheManager;

        public MarketCacheProviderTests(ITestOutputHelper outputHelper)
        {
            _outputHelper = outputHelper;
            _cultures = TestData.Cultures.ToList();
            _invariantMarketDescriptionMemoryCache = new MemoryCache("invariantMarketDescriptionCache");
            _variantMarketDescriptionListMemoryCache = new MemoryCache("variantDescriptionCache");
            _variantMarketDescriptionMemoryCache = new MemoryCache("variantMarketDescriptionCache");

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
        public void InitialCallsDoneTest()
        {
            Assert.NotEmpty(_invariantMarketDescriptionMemoryCache);
            Assert.NotEmpty(_variantMarketDescriptionListMemoryCache);
            Assert.Empty(_variantMarketDescriptionMemoryCache);

            Assert.NotNull(_invariantMarketDescriptionCache);
            Assert.NotNull(_variantDescriptionListCache);
            Assert.NotNull(_variantMarketDescriptionCache);

            Assert.Equal(_cultures.Count, _dataRouterManager.GetCallCount("GetMarketDescriptionsAsync"));
            Assert.Equal(_cultures.Count, _dataRouterManager.GetCallCount("GetVariantDescriptionsAsync"));
            Assert.Equal(0, _dataRouterManager.GetCallCount("GetVariantMarketDescriptionAsync"));

            Assert.Equal(TestData.InvariantListCacheCount, _invariantMarketDescriptionMemoryCache.GetCount());
            Assert.Equal(TestData.VariantListCacheCount, _variantMarketDescriptionListMemoryCache.GetCount());
        }

        [Fact]
        public async Task GetInvariantMarketDescriptionTest()
        {
            var marketDescription = await _marketCacheProvider.GetMarketDescriptionAsync(282, null, _cultures, true);

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
        public async Task GetNonExistingInvariantMarketDescriptionTest()
        {
            var marketDescription = await _marketCacheProvider.GetMarketDescriptionAsync(2820, null, _cultures, true);

            Assert.Equal(_cultures.Count, _dataRouterManager.GetCallCount("GetMarketDescriptionsAsync"));
            Assert.Equal(_cultures.Count, _dataRouterManager.GetCallCount("GetVariantDescriptionsAsync"));
            Assert.Equal(0, _dataRouterManager.GetCallCount("GetVariantMarketDescriptionAsync"));

            Assert.Null(marketDescription);
        }

        [Fact]
        public async Task GetVariantMarketDescriptionFromListTest()
        {
            var specifiers = new Dictionary<string, string> { { "variant", "sr:correct_score:bestof:12" } };
            var marketDescription = await _marketCacheProvider.GetMarketDescriptionAsync(374, specifiers, _cultures, true);

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
        public async Task GetVariantMarketDescriptionFromListMultipleMappingsTest()
        {
            var specifiers = new Dictionary<string, string> { { "variant", "sr:decided_by_extra_points:bestof:5" } };
            var marketDescription = await _marketCacheProvider.GetMarketDescriptionAsync(239, specifiers, _cultures, true);

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
        public async Task GetVariantMarketDescriptionFromSinglePreOutcomeTextTest()
        {
            const int marketId = 534;
            const string variantSpecifier = "pre:markettext:168883";
            _dataRouterManager.UriReplacements.Add(new Tuple<string, string>($"{marketId}_{variantSpecifier}", $"variant_market_description_{marketId}_culture.xml"));
            Assert.Empty(_variantMarketDescriptionMemoryCache);
            var specifiers = new Dictionary<string, string> { { "variant", variantSpecifier } };
            var marketDescription = await _marketCacheProvider.GetMarketDescriptionAsync(marketId, specifiers, _cultures, true);

            Assert.Equal(_cultures.Count, _dataRouterManager.GetCallCount("GetMarketDescriptionsAsync"));
            Assert.Equal(_cultures.Count, _dataRouterManager.GetCallCount("GetVariantDescriptionsAsync"));
            Assert.Equal(_cultures.Count, _dataRouterManager.GetCallCount("GetVariantMarketDescriptionAsync"));
            Assert.Single(_variantMarketDescriptionMemoryCache);

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
        public async Task GetVariantMarketDescriptionFromSinglePlayerPropsTest()
        {
            const int marketId = 768;
            const string variantSpecifier = "pre:playerprops:35432179:608000";
            _dataRouterManager.UriReplacements.Add(new Tuple<string, string>($"{marketId}_{variantSpecifier}", $"variant_market_description_{marketId}_culture.xml"));
            Assert.Empty(_variantMarketDescriptionMemoryCache);
            var specifiers = new Dictionary<string, string> { { "variant", variantSpecifier } };
            var marketDescription = await _marketCacheProvider.GetMarketDescriptionAsync(marketId, specifiers, _cultures, true);

            Assert.Equal(_cultures.Count, _dataRouterManager.GetCallCount("GetMarketDescriptionsAsync"));
            Assert.Equal(_cultures.Count, _dataRouterManager.GetCallCount("GetVariantDescriptionsAsync"));
            Assert.Equal(_cultures.Count, _dataRouterManager.GetCallCount("GetVariantMarketDescriptionAsync"));
            Assert.Single(_variantMarketDescriptionMemoryCache);

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
        public async Task GetVariantMarketDescriptionFromSingleUnsupportedCompetitorPropsTest()
        {
            const int marketId = 1768;
            const string variantSpecifier = "pre:competitorprops:35432179:608000";
            await LoadNewInvariantMarketDescriptionAsync(768, marketId, 1, _cultures).ConfigureAwait(false);
            _dataRouterManager.UriReplacements.Add(new Tuple<string, string>($"{marketId}_{variantSpecifier}", $"variant_market_description_{marketId}_cp_culture.xml"));
            Assert.Equal(TestData.InvariantListCacheCount + 1, _invariantMarketDescriptionMemoryCache.GetCount());
            Assert.Empty(_variantMarketDescriptionMemoryCache);
            var specifiers = new Dictionary<string, string> { { "variant", variantSpecifier } };
            var marketDescription = await _marketCacheProvider.GetMarketDescriptionAsync(marketId, specifiers, _cultures, true);

            Assert.Equal(_cultures.Count, _dataRouterManager.GetCallCount("GetMarketDescriptionsAsync"));
            Assert.Equal(_cultures.Count, _dataRouterManager.GetCallCount("GetVariantDescriptionsAsync"));
            Assert.Equal(_cultures.Count, _dataRouterManager.GetCallCount("GetVariantMarketDescriptionAsync"));
            Assert.Single(_variantMarketDescriptionMemoryCache);

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
        public async Task GetMarketDescriptionForOutComeTypePlayerTest()
        {
            const int marketId = 679;
            const string specifiers = "maxovers=20|type=live|inningnr=1";
            Assert.Empty(_variantMarketDescriptionMemoryCache);
            var marketDescription = await _marketCacheProvider.GetMarketDescriptionAsync(marketId, new ReadOnlyDictionary<string, string>(SdkInfo.SpecifiersStringToDictionary(specifiers)), _cultures, true);

            Assert.Equal(_cultures.Count, _dataRouterManager.GetCallCount("GetMarketDescriptionsAsync"));
            Assert.Equal(_cultures.Count, _dataRouterManager.GetCallCount("GetVariantDescriptionsAsync"));
            Assert.Equal(0, _dataRouterManager.GetCallCount("GetVariantMarketDescriptionAsync"));
            Assert.Empty(_variantMarketDescriptionMemoryCache);

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
        public async Task GetMarketDescriptionForOutComeTypeCompetitorTest()
        {
            const int marketId = 1109;
            const string specifiers = "lapnr=5";
            Assert.Empty(_variantMarketDescriptionMemoryCache);
            var marketDescription = await _marketCacheProvider.GetMarketDescriptionAsync(marketId, new ReadOnlyDictionary<string, string>(SdkInfo.SpecifiersStringToDictionary(specifiers)), _cultures, true);

            Assert.Equal(_cultures.Count, _dataRouterManager.GetCallCount("GetMarketDescriptionsAsync"));
            Assert.Equal(_cultures.Count, _dataRouterManager.GetCallCount("GetVariantDescriptionsAsync"));
            Assert.Equal(0, _dataRouterManager.GetCallCount("GetVariantMarketDescriptionAsync"));
            Assert.Empty(_variantMarketDescriptionMemoryCache);

            Assert.NotNull(marketDescription);
            Assert.Equal(marketId, marketDescription.Id);
            Assert.Single(marketDescription.Specifiers);
            Assert.True(marketDescription.Outcomes.IsNullOrEmpty());
            Assert.Single(marketDescription.Mappings);
        }

        [Fact]
        public async Task GetMarketDescriptionWithCompetitorInOutcomeTemplateTest()
        {
            const int marketId = 303;
            const string specifiers = "quarternr=4|hcp=-3.5";
            Assert.Empty(_variantMarketDescriptionMemoryCache);
            var marketDescription = await _marketCacheProvider.GetMarketDescriptionAsync(marketId, new ReadOnlyDictionary<string, string>(SdkInfo.SpecifiersStringToDictionary(specifiers)), _cultures, true);

            Assert.Equal(_cultures.Count, _dataRouterManager.GetCallCount("GetMarketDescriptionsAsync"));
            Assert.Equal(_cultures.Count, _dataRouterManager.GetCallCount("GetVariantDescriptionsAsync"));
            Assert.Equal(0, _dataRouterManager.GetCallCount("GetVariantMarketDescriptionAsync"));
            Assert.Empty(_variantMarketDescriptionMemoryCache);

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
        public async Task GetMarketDescriptionWithScoreInNameTemplateTest()
        {
            const int marketId = 41;
            const string specifiers = "score=2:0";
            Assert.Empty(_variantMarketDescriptionMemoryCache);
            var marketDescription = await _marketCacheProvider.GetMarketDescriptionAsync(marketId, new ReadOnlyDictionary<string, string>(SdkInfo.SpecifiersStringToDictionary(specifiers)), _cultures, true);

            Assert.Equal(_cultures.Count, _dataRouterManager.GetCallCount("GetMarketDescriptionsAsync"));
            Assert.Equal(_cultures.Count, _dataRouterManager.GetCallCount("GetVariantDescriptionsAsync"));
            Assert.Equal(0, _dataRouterManager.GetCallCount("GetVariantMarketDescriptionAsync"));
            Assert.Empty(_variantMarketDescriptionMemoryCache);

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
        public async Task GetVariantMarketDescriptionFromSinglePreOutcomeTextInvalidOutcomeTest()
        {
            const int marketId = 534;
            const string variantSpecifier = "pre:markettext:168883";
            _dataRouterManager.UriReplacements.Add(new Tuple<string, string>($"{marketId}_{variantSpecifier}", $"variant_market_description_invalid_{marketId}_culture.xml"));
            Assert.Empty(_variantMarketDescriptionMemoryCache);
            var specifiers = new Dictionary<string, string> { { "variant", variantSpecifier } };
            var marketDescription = await _marketCacheProvider.GetMarketDescriptionAsync(marketId, specifiers, TestData.Cultures1, true);

            Assert.Equal(_cultures.Count, _dataRouterManager.GetCallCount("GetMarketDescriptionsAsync"));
            Assert.Equal(_cultures.Count, _dataRouterManager.GetCallCount("GetVariantDescriptionsAsync"));
            Assert.Equal(TestData.Cultures1.Count, _dataRouterManager.GetCallCount("GetVariantMarketDescriptionAsync"));
            Assert.Empty(_variantMarketDescriptionMemoryCache);

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

                    var items = new EntityList<MarketDescriptionDTO>(new List<MarketDescriptionDTO> { existingMd });
                    await _cacheManager.SaveDtoAsync(URN.Parse("sr:markets:1"), items, culture, DtoType.MarketDescriptionList, null).ConfigureAwait(false);
                }
                else
                {
                    _outputHelper.WriteLine($"No results found for {resourceName}");
                }
            }
        }
    }
}
