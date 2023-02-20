using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Runtime.Caching;
using System.Threading.Tasks;
using Castle.Core.Internal;
using FluentAssertions;
using Moq;
using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Common.Exceptions;
using Sportradar.OddsFeed.SDK.Common.Internal;
using Sportradar.OddsFeed.SDK.Entities.REST;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching.Profiles;
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
    public class NameProviderFactoryTests
    {
        private readonly ITestOutputHelper _outputHelper;
        private readonly IMarketDescriptionCache _invariantMarketDescriptionCache;
        private readonly IVariantDescriptionCache _variantDescriptionListCache;
        private readonly IMarketDescriptionCache _variantMarketDescriptionCache;
        private readonly IProfileCache _profileCache;
        private readonly IMarketCacheProvider _marketCacheProvider;

        private readonly TestDataRouterManager _dataRouterManager;
        private readonly MemoryCache _invariantMarketDescriptionMemoryCache;
        private readonly MemoryCache _variantMarketDescriptionListMemoryCache;
        private readonly MemoryCache _variantMarketDescriptionMemoryCache;
        private readonly MemoryCache _profileMemoryCache;
        private readonly IReadOnlyList<CultureInfo> _cultures;
        private readonly CacheManager _cacheManager;
        private readonly INameProviderFactory _nameProviderFactory;
        private readonly IMatch _match;

        public NameProviderFactoryTests(ITestOutputHelper outputHelper)
        {
            _outputHelper = outputHelper;
            _cultures = TestData.Cultures.ToList();
            _invariantMarketDescriptionMemoryCache = new MemoryCache("invariantMarketDescriptionCache");
            _variantMarketDescriptionListMemoryCache = new MemoryCache("variantDescriptionCache");
            _variantMarketDescriptionMemoryCache = new MemoryCache("variantMarketDescriptionCache");
            _profileMemoryCache = new MemoryCache("profileCache");

            _cacheManager = new CacheManager();
            _dataRouterManager = new TestDataRouterManager(_cacheManager, _outputHelper);
            MessageFactorySdk.SetOutputHelper(_outputHelper);

            IMappingValidatorFactory mappingValidatorFactory = new MappingValidatorFactory();

            var timerVdl = new TestTimer(true);
            var timerIdl = new TestTimer(true);
            _invariantMarketDescriptionCache = new InvariantMarketDescriptionCache(_invariantMarketDescriptionMemoryCache, _dataRouterManager, mappingValidatorFactory, timerIdl, _cultures, _cacheManager);
            _variantDescriptionListCache = new VariantDescriptionListCache(_variantMarketDescriptionListMemoryCache, _dataRouterManager, mappingValidatorFactory, timerVdl, _cultures, _cacheManager);
            _variantMarketDescriptionCache = new VariantMarketDescriptionCache(_variantMarketDescriptionMemoryCache, _dataRouterManager, mappingValidatorFactory, _cacheManager);

            _marketCacheProvider = new MarketCacheProvider(_invariantMarketDescriptionCache, _variantMarketDescriptionCache, _variantDescriptionListCache);

            _profileCache = new ProfileCache(_profileMemoryCache, _dataRouterManager, _cacheManager);

            _nameProviderFactory = new NameProviderFactory(_marketCacheProvider,
                                                           _profileCache,
                                                           new NameExpressionFactory(new OperandFactory(), _profileCache),
                                                           ExceptionHandlingStrategy.THROW);
            var matchMock = new Mock<IMatch>();
            var homeTeam = MessageFactorySdk.GetTeamCompetitor(1);
            var awayTeam = MessageFactorySdk.GetTeamCompetitor(2);
            var competitors = new List<ICompetitor> { homeTeam, awayTeam };
            matchMock.Setup(s => s.GetHomeCompetitorAsync()).ReturnsAsync(homeTeam);
            matchMock.Setup(s => s.GetAwayCompetitorAsync()).ReturnsAsync(awayTeam);
            matchMock.Setup(s => s.GetCompetitorsAsync()).ReturnsAsync(competitors);
            _match = matchMock.Object;
        }

        [Fact]
        public void InitialCallsDoneTest()
        {
            Assert.NotNull(_outputHelper);
            Assert.NotEmpty(_invariantMarketDescriptionMemoryCache);
            Assert.NotEmpty(_variantMarketDescriptionListMemoryCache);
            Assert.Empty(_variantMarketDescriptionMemoryCache);
            Assert.Empty(_profileMemoryCache);

            Assert.NotNull(_invariantMarketDescriptionCache);
            Assert.NotNull(_variantDescriptionListCache);
            Assert.NotNull(_variantMarketDescriptionCache);
            Assert.NotNull(_profileCache);

            Assert.NotNull(_marketCacheProvider);
            Assert.NotNull(_nameProviderFactory);

            Assert.Equal(_cultures.Count, _dataRouterManager.GetCallCount("GetMarketDescriptionsAsync"));
            Assert.Equal(_cultures.Count, _dataRouterManager.GetCallCount("GetVariantDescriptionsAsync"));
            Assert.Equal(0, _dataRouterManager.GetCallCount("GetVariantMarketDescriptionAsync"));
            Assert.Equal(0, _dataRouterManager.GetCallCount("GetSportEventSummaryAsync"));
            Assert.Equal(0, _dataRouterManager.GetCallCount("GetPlayerProfileAsync"));
            Assert.Equal(0, _dataRouterManager.GetCallCount("GetCompetitorAsync"));

            Assert.Equal(TestData.InvariantListCacheCount, _invariantMarketDescriptionMemoryCache.GetCount());
            Assert.Equal(TestData.VariantListCacheCount, _variantMarketDescriptionListMemoryCache.GetCount());

            Assert.NotNull(_match);
            Assert.Equal(1, _match.GetHomeCompetitorAsync().Result.Id.Id);
            Assert.Equal(2, _match.GetAwayCompetitorAsync().Result.Id.Id);
        }

        [Fact]
        public async Task GetNameFromInvariantMarketDescriptionTest()
        {
            var specifiers = new ReadOnlyDictionary<string, string>(SdkInfo.SpecifiersStringToDictionary("milestone=1|maxovers=3"));
            var nameProvider = _nameProviderFactory.BuildNameProvider(_match, 701, specifiers);
            var name0 = await nameProvider.GetMarketNameAsync(_cultures[0]);
            var name1 = await nameProvider.GetMarketNameAsync(_cultures[1]);
            var name2 = await nameProvider.GetMarketNameAsync(_cultures[2]);

            Assert.NotNull(name0);
            Assert.NotNull(name1);
            Assert.NotNull(name2);
            Assert.NotEqual(name0, name1);
            Assert.NotEqual(name0, name2);
            Assert.NotEqual(name1, name2);

            var outcome1 = await nameProvider.GetOutcomeNameAsync("74", _cultures[0]);
            Assert.NotNull(outcome1);
            Assert.Equal("yes", outcome1);
            var outcome2 = await nameProvider.GetOutcomeNameAsync("76", _cultures[0]);
            Assert.NotNull(outcome2);
            Assert.Equal("no", outcome2);

            outcome1 = await nameProvider.GetOutcomeNameAsync("74", _cultures[1]);
            Assert.NotNull(outcome1);
            Assert.Equal("ja", outcome1);
            outcome2 = await nameProvider.GetOutcomeNameAsync("76", _cultures[1]);
            Assert.NotNull(outcome2);
            Assert.Equal("nein", outcome2);

            Assert.Equal(_cultures.Count, _dataRouterManager.GetCallCount("GetMarketDescriptionsAsync"));
            Assert.Equal(_cultures.Count, _dataRouterManager.GetCallCount("GetVariantDescriptionsAsync"));
            Assert.Equal(0, _dataRouterManager.GetCallCount("GetVariantMarketDescriptionAsync"));
            Assert.Equal(0, _dataRouterManager.GetCallCount("GetSportEventSummaryAsync"));
            Assert.Equal(0, _dataRouterManager.GetCallCount("GetPlayerProfileAsync"));
            Assert.Equal(0, _dataRouterManager.GetCallCount("GetCompetitorAsync"));
        }

        [Fact]
        public void GetMarketNameFromNonExistingInvariantMarketDescriptionTest()
        {
            var nameProvider = _nameProviderFactory.BuildNameProvider(_match, 2820, null);
            string name0 = null;
            Action action = () => name0 = nameProvider.GetMarketNameAsync(_cultures[0]).Result;
            action.Should().Throw<NameGenerationException>();
            Assert.Null(name0);
        }

        [Fact]
        public async Task GetUnExistingOutcomeNameFromExistingInvariantMarketDescriptionTest()
        {
            var specifiers = new ReadOnlyDictionary<string, string>(SdkInfo.SpecifiersStringToDictionary("milestone=4|maxovers=2"));
            var nameProvider = _nameProviderFactory.BuildNameProvider(_match, 701, specifiers);
            var name0 = await nameProvider.GetMarketNameAsync(_cultures[0]);
            Assert.NotNull(name0);

            var outcome1 = await nameProvider.GetOutcomeNameAsync("74", _cultures[0]);
            Assert.NotNull(outcome1);
            Assert.Equal("yes", outcome1);

            Action action = () => outcome1 = nameProvider.GetOutcomeNameAsync("75", _cultures[0]).Result;
            action.Should().Throw<NameGenerationException>();
        }

        [Fact]
        public async Task GetMarketNamesFromVariantMarketDescriptionFromListTest()
        {
            var specifiers = new ReadOnlyDictionary<string, string>(SdkInfo.SpecifiersStringToDictionary("setnr=1|variant=sr:correct_score:bestof:12"));
            var nameProvider = _nameProviderFactory.BuildNameProvider(_match, 374, specifiers);

            var name0 = await nameProvider.GetMarketNameAsync(_cultures[0]);
            var name1 = await nameProvider.GetMarketNameAsync(_cultures[1]);
            var name2 = await nameProvider.GetMarketNameAsync(_cultures[2]);

            Assert.NotNull(name0);
            Assert.NotNull(name1);
            Assert.NotNull(name2);
            Assert.NotEqual(name0, name1);
            Assert.NotEqual(name0, name2);
            Assert.NotEqual(name1, name2);

            var outcome1 = await nameProvider.GetOutcomeNameAsync("sr:correct_score:bestof:12:192", _cultures[0]);
            Assert.NotNull(outcome1);
            Assert.Equal("7:0", outcome1);
            var outcome2 = await nameProvider.GetOutcomeNameAsync("sr:correct_score:bestof:12:200", _cultures[0]);
            Assert.NotNull(outcome2);
            Assert.Equal("4:7", outcome2);

            Assert.Equal(_cultures.Count, _dataRouterManager.GetCallCount("GetMarketDescriptionsAsync"));
            Assert.Equal(_cultures.Count, _dataRouterManager.GetCallCount("GetVariantDescriptionsAsync"));
            Assert.Equal(0, _dataRouterManager.GetCallCount("GetVariantMarketDescriptionAsync"));
            Assert.Equal(0, _dataRouterManager.GetCallCount("GetSportEventSummaryAsync"));
            Assert.Equal(0, _dataRouterManager.GetCallCount("GetPlayerProfileAsync"));
            Assert.Equal(0, _dataRouterManager.GetCallCount("GetCompetitorAsync"));
        }

        [Fact]
        public async Task GetMarketNamesFromVariantMarketDescriptionFromListMultipleMappingsTest()
        {
            var specifiers = new ReadOnlyDictionary<string, string>(SdkInfo.SpecifiersStringToDictionary("variant=sr:decided_by_extra_points:bestof:5"));
            var nameProvider = _nameProviderFactory.BuildNameProvider(_match, 239, specifiers);

            var name0 = await nameProvider.GetMarketNameAsync(_cultures[0]);
            var name1 = await nameProvider.GetMarketNameAsync(_cultures[1]);
            var name2 = await nameProvider.GetMarketNameAsync(_cultures[2]);

            Assert.NotNull(name0);
            Assert.NotNull(name1);
            Assert.NotNull(name2);
            Assert.NotEqual(name0, name1);
            Assert.NotEqual(name0, name2);
            Assert.NotEqual(name1, name2);

            var outcome1 = await nameProvider.GetOutcomeNameAsync("sr:decided_by_extra_points:bestof:5:53", _cultures[0]);
            Assert.NotNull(outcome1);
            Assert.Equal("0", outcome1);
            var outcome2 = await nameProvider.GetOutcomeNameAsync("sr:decided_by_extra_points:bestof:5:58", _cultures[0]);
            Assert.NotNull(outcome2);
            Assert.Equal("5", outcome2);

            Assert.Equal(_cultures.Count, _dataRouterManager.GetCallCount("GetMarketDescriptionsAsync"));
            Assert.Equal(_cultures.Count, _dataRouterManager.GetCallCount("GetVariantDescriptionsAsync"));
            Assert.Equal(0, _dataRouterManager.GetCallCount("GetVariantMarketDescriptionAsync"));
            Assert.Equal(0, _dataRouterManager.GetCallCount("GetSportEventSummaryAsync"));
            Assert.Equal(0, _dataRouterManager.GetCallCount("GetPlayerProfileAsync"));
            Assert.Equal(0, _dataRouterManager.GetCallCount("GetCompetitorAsync"));
        }

        [Fact]
        public async Task GetMarketNamesFromVariantMarketDescriptionFromSinglePreOutcomeTextTest()
        {
            const int marketId = 534;
            const string variantSpecifier = "pre:markettext:168883";
            _dataRouterManager.UriReplacements.Add(new Tuple<string, string>($"{marketId}_{variantSpecifier}", $"variant_market_description_{marketId}_culture.xml"));

            var specifiers = new ReadOnlyDictionary<string, string>(SdkInfo.SpecifiersStringToDictionary($"variant={variantSpecifier}"));
            var nameProvider = _nameProviderFactory.BuildNameProvider(_match, marketId, specifiers);

            Assert.Empty(_variantMarketDescriptionMemoryCache);

            var name0 = await nameProvider.GetMarketNameAsync(_cultures[0]);
            var name1 = await nameProvider.GetMarketNameAsync(_cultures[1]);
            var name2 = await nameProvider.GetMarketNameAsync(_cultures[2]);

            Assert.Single(_variantMarketDescriptionMemoryCache);

            Assert.NotNull(name0);
            Assert.NotNull(name1);
            Assert.NotNull(name2);
            Assert.NotEqual(name0, name1);
            Assert.NotEqual(name0, name2);
            Assert.NotEqual(name1, name2);

            var outcome1 = await nameProvider.GetOutcomeNameAsync("pre:outcometext:108719", _cultures[0]);
            Assert.NotNull(outcome1);
            Assert.Equal("7 Points", outcome1);
            var outcome2 = await nameProvider.GetOutcomeNameAsync("pre:outcometext:108726", _cultures[0]);
            Assert.NotNull(outcome2);
            Assert.Equal("1 Point", outcome2);

            Assert.Equal(_cultures.Count, _dataRouterManager.GetCallCount("GetMarketDescriptionsAsync"));
            Assert.Equal(_cultures.Count, _dataRouterManager.GetCallCount("GetVariantDescriptionsAsync"));
            Assert.Equal(_cultures.Count, _dataRouterManager.GetCallCount("GetVariantMarketDescriptionAsync"));
            Assert.Equal(0, _dataRouterManager.GetCallCount("GetSportEventSummaryAsync"));
            Assert.Equal(0, _dataRouterManager.GetCallCount("GetPlayerProfileAsync"));
            Assert.Equal(0, _dataRouterManager.GetCallCount("GetCompetitorAsync"));
        }

        [Fact]
        public async Task GetMarketNamesForVariantMarketDescriptionFromSinglePlayerPropsTest()
        {
            const int marketId = 768;
            const string variantSpecifier = "pre:playerprops:35432179:608000";
            _dataRouterManager.UriReplacements.Add(new Tuple<string, string>($"{marketId}_{variantSpecifier}", $"variant_market_description_{marketId}_culture.xml"));

            var specifiers = new ReadOnlyDictionary<string, string>(SdkInfo.SpecifiersStringToDictionary($"variant={variantSpecifier}"));
            var nameProvider = _nameProviderFactory.BuildNameProvider(_match, marketId, specifiers);

            Assert.Empty(_variantMarketDescriptionMemoryCache);

            var name0 = await nameProvider.GetMarketNameAsync(_cultures[0]);
            var name1 = await nameProvider.GetMarketNameAsync(_cultures[1]);
            var name2 = await nameProvider.GetMarketNameAsync(_cultures[2]);

            Assert.Single(_variantMarketDescriptionMemoryCache);

            Assert.NotNull(name0);
            Assert.NotNull(name1);
            Assert.NotNull(name2);
            Assert.NotEqual(name0, name1);
            Assert.NotEqual(name0, name2);
            Assert.NotEqual(name1, name2);

            var outcome1 = await nameProvider.GetOutcomeNameAsync("pre:playerprops:35432179:608000:10", _cultures[0]);
            Assert.NotNull(outcome1);
            Assert.Equal("Kyle Anderson 10+", outcome1);
            var outcome2 = await nameProvider.GetOutcomeNameAsync("pre:playerprops:35432179:608000:20", _cultures[0]);
            Assert.NotNull(outcome2);
            Assert.Equal("Kyle Anderson 20+", outcome2);

            Assert.Equal(_cultures.Count, _dataRouterManager.GetCallCount("GetMarketDescriptionsAsync"));
            Assert.Equal(_cultures.Count, _dataRouterManager.GetCallCount("GetVariantDescriptionsAsync"));
            Assert.Equal(_cultures.Count, _dataRouterManager.GetCallCount("GetVariantMarketDescriptionAsync"));
            Assert.Equal(0, _dataRouterManager.GetCallCount("GetSportEventSummaryAsync"));
            Assert.Equal(0, _dataRouterManager.GetCallCount("GetPlayerProfileAsync"));
            Assert.Equal(0, _dataRouterManager.GetCallCount("GetCompetitorAsync"));
        }

        [Fact]
        public async Task GetMarketNamesForVariantMarketDescriptionFromSingleUnsupportedCompetitorPropsTest()
        {
            const int marketId = 1768;
            const string variantSpecifier = "pre:competitorprops:35432179:608000";
            await LoadNewInvariantMarketDescriptionAsync(768, marketId, 1, _cultures).ConfigureAwait(false);
            _dataRouterManager.UriReplacements.Add(new Tuple<string, string>($"{marketId}_{variantSpecifier}", $"variant_market_description_{marketId}_cp_culture.xml"));
            Assert.Equal(TestData.InvariantListCacheCount + 1, _invariantMarketDescriptionMemoryCache.GetCount());
            Assert.Empty(_variantMarketDescriptionMemoryCache);

            var specifiers = new ReadOnlyDictionary<string, string>(SdkInfo.SpecifiersStringToDictionary($"variant={variantSpecifier}"));
            var nameProvider = _nameProviderFactory.BuildNameProvider(_match, marketId, specifiers);

            Assert.Empty(_variantMarketDescriptionMemoryCache);

            var name0 = await nameProvider.GetMarketNameAsync(_cultures[0]);
            var name1 = await nameProvider.GetMarketNameAsync(_cultures[1]);
            var name2 = await nameProvider.GetMarketNameAsync(_cultures[2]);

            Assert.Single(_variantMarketDescriptionMemoryCache);

            Assert.NotNull(name0);
            Assert.NotNull(name1);
            Assert.NotNull(name2);
            Assert.NotEqual(name0, name1);
            Assert.NotEqual(name0, name2);
            Assert.NotEqual(name1, name2);

            var outcome1 = await nameProvider.GetOutcomeNameAsync("pre:competitorprops:35432179:608000:10", _cultures[0]);
            Assert.NotNull(outcome1);
            Assert.Equal("Kyle Anderson 10+", outcome1);
            var outcome2 = await nameProvider.GetOutcomeNameAsync("pre:competitorprops:35432179:608000:20", _cultures[0]);
            Assert.NotNull(outcome2);
            Assert.Equal("Kyle Anderson 20+", outcome2);

            Assert.Equal(_cultures.Count, _dataRouterManager.GetCallCount("GetMarketDescriptionsAsync"));
            Assert.Equal(_cultures.Count, _dataRouterManager.GetCallCount("GetVariantDescriptionsAsync"));
            Assert.Equal(_cultures.Count, _dataRouterManager.GetCallCount("GetVariantMarketDescriptionAsync"));
            Assert.Equal(0, _dataRouterManager.GetCallCount("GetSportEventSummaryAsync"));
            Assert.Equal(0, _dataRouterManager.GetCallCount("GetPlayerProfileAsync"));
            Assert.Equal(0, _dataRouterManager.GetCallCount("GetCompetitorAsync"));
        }

        [Fact]
        public async Task GetMarketNamesForMarketDescriptionForOutcomeTypePlayerTest()
        {
            const int marketId = 679;
            var specifiers = new ReadOnlyDictionary<string, string>(SdkInfo.SpecifiersStringToDictionary("maxovers=20|type=live|inningnr=1"));
            var nameProvider = _nameProviderFactory.BuildNameProvider(_match, marketId, specifiers);

            Assert.Empty(_variantMarketDescriptionMemoryCache);

            var name0 = await nameProvider.GetMarketNameAsync(_cultures[0]);
            var name1 = await nameProvider.GetMarketNameAsync(_cultures[1]);
            var name2 = await nameProvider.GetMarketNameAsync(_cultures[2]);

            Assert.Empty(_variantMarketDescriptionMemoryCache);

            Assert.NotNull(name0);
            Assert.NotNull(name1);
            Assert.NotNull(name2);
            Assert.NotEqual(name0, name1);
            Assert.NotEqual(name0, name2);
            Assert.NotEqual(name1, name2);

            var outcome1 = await nameProvider.GetOutcomeNameAsync("sr:player:36759", _cultures[0]);
            Assert.NotNull(outcome1);
            Assert.Equal("Main, Curtis", outcome1);
            var outcome2 = await nameProvider.GetOutcomeNameAsync("sr:player:1132695", _cultures[0]);
            Assert.NotNull(outcome2);
            Assert.Equal("Bowler, Josh", outcome2);

            Assert.Equal(_cultures.Count, _dataRouterManager.GetCallCount("GetMarketDescriptionsAsync"));
            Assert.Equal(_cultures.Count, _dataRouterManager.GetCallCount("GetVariantDescriptionsAsync"));
            Assert.Equal(0, _dataRouterManager.GetCallCount("GetVariantMarketDescriptionAsync"));
            Assert.Equal(0, _dataRouterManager.GetCallCount("GetSportEventSummaryAsync"));
            Assert.Equal(0, _dataRouterManager.GetCallCount("GetPlayerProfileAsync"));
            Assert.Equal(2, _dataRouterManager.GetCallCount("GetCompetitorAsync"));
        }

        [Fact]
        public async Task GetMarketNamesForMarketDescriptionForOutComeTypeCompetitorTest()
        {
            const int marketId = 1109;
            var specifiers = new ReadOnlyDictionary<string, string>(SdkInfo.SpecifiersStringToDictionary("lapnr=5"));
            var nameProvider = _nameProviderFactory.BuildNameProvider(_match, marketId, specifiers);

            Assert.Empty(_variantMarketDescriptionMemoryCache);

            var name0 = await nameProvider.GetMarketNameAsync(_cultures[0]);
            var name1 = await nameProvider.GetMarketNameAsync(_cultures[1]);
            var name2 = await nameProvider.GetMarketNameAsync(_cultures[2]);

            Assert.Empty(_variantMarketDescriptionMemoryCache);

            Assert.NotNull(name0);
            Assert.NotNull(name1);
            Assert.NotNull(name2);
            Assert.NotEqual(name0, name1);
            Assert.NotEqual(name0, name2);
            Assert.NotEqual(name1, name2);

            var outcome1 = await nameProvider.GetOutcomeNameAsync("sr:player:36759", _cultures[0]);
            Assert.NotNull(outcome1);
            Assert.Equal("Main, Curtis", outcome1);
            var outcome2 = await nameProvider.GetOutcomeNameAsync("sr:player:1132695", _cultures[0]);
            Assert.NotNull(outcome2);
            Assert.Equal("Bowler, Josh", outcome2);

            Assert.Equal(_cultures.Count, _dataRouterManager.GetCallCount("GetMarketDescriptionsAsync"));
            Assert.Equal(_cultures.Count, _dataRouterManager.GetCallCount("GetVariantDescriptionsAsync"));
            Assert.Equal(0, _dataRouterManager.GetCallCount("GetVariantMarketDescriptionAsync"));
            Assert.Equal(0, _dataRouterManager.GetCallCount("GetSportEventSummaryAsync"));
            Assert.Equal(0, _dataRouterManager.GetCallCount("GetPlayerProfileAsync"));
            Assert.Equal(2, _dataRouterManager.GetCallCount("GetCompetitorAsync"));
        }

        [Fact]
        public async Task GetMarketNamesForMarketDescriptionWithCompetitorInOutcomeTemplateTest()
        {
            const int marketId = 303;
            var specifiers = new ReadOnlyDictionary<string, string>(SdkInfo.SpecifiersStringToDictionary("quarternr=4|hcp=-3.5"));
            var nameProvider = _nameProviderFactory.BuildNameProvider(_match, marketId, specifiers);

            Assert.Empty(_variantMarketDescriptionMemoryCache);

            var name0 = await nameProvider.GetMarketNameAsync(_cultures[0]);
            var name1 = await nameProvider.GetMarketNameAsync(_cultures[1]);
            var name2 = await nameProvider.GetMarketNameAsync(_cultures[2]);

            Assert.Empty(_variantMarketDescriptionMemoryCache);

            Assert.NotNull(name0);
            Assert.NotNull(name1);
            Assert.NotNull(name2);
            Assert.NotEqual(name0, name1);
            Assert.NotEqual(name0, name2);
            Assert.NotEqual(name1, name2);

            var outcome1 = await nameProvider.GetOutcomeNameAsync("1714", _cultures[0]);
            Assert.NotNull(outcome1);
            Assert.Equal("Competitor 1 (-3.5)", outcome1);
            var outcome2 = await nameProvider.GetOutcomeNameAsync("1715", _cultures[0]);
            Assert.NotNull(outcome2);
            Assert.Equal("Competitor 2 (+3.5)", outcome2);

            Assert.Equal(_cultures.Count, _dataRouterManager.GetCallCount("GetMarketDescriptionsAsync"));
            Assert.Equal(_cultures.Count, _dataRouterManager.GetCallCount("GetVariantDescriptionsAsync"));
            Assert.Equal(0, _dataRouterManager.GetCallCount("GetVariantMarketDescriptionAsync"));
            Assert.Equal(0, _dataRouterManager.GetCallCount("GetSportEventSummaryAsync"));
            Assert.Equal(0, _dataRouterManager.GetCallCount("GetPlayerProfileAsync"));
            Assert.Equal(0, _dataRouterManager.GetCallCount("GetCompetitorAsync"));
        }

        [Fact]
        public async Task GetMarketNamesForMarketDescriptionWithScoreInNameTemplateTest()
        {
            const int marketId = 41;
            var specifiers = new ReadOnlyDictionary<string, string>(SdkInfo.SpecifiersStringToDictionary("score=2:0"));
            var nameProvider = _nameProviderFactory.BuildNameProvider(_match, marketId, specifiers);

            Assert.Empty(_variantMarketDescriptionMemoryCache);

            var name0 = await nameProvider.GetMarketNameAsync(_cultures[0]);
            var name1 = await nameProvider.GetMarketNameAsync(_cultures[1]);
            var name2 = await nameProvider.GetMarketNameAsync(_cultures[2]);

            Assert.Empty(_variantMarketDescriptionMemoryCache);

            Assert.NotNull(name0);
            Assert.NotNull(name1);
            Assert.NotNull(name2);
            Assert.NotEqual(name0, name1);
            Assert.NotEqual(name0, name2);
            Assert.NotEqual(name1, name2);

            var outcome1 = await nameProvider.GetOutcomeNameAsync("134", _cultures[0]);
            Assert.NotNull(outcome1);
            Assert.Equal("6:1", outcome1);
            var outcome2 = await nameProvider.GetOutcomeNameAsync("144", _cultures[0]);
            Assert.NotNull(outcome2);
            Assert.Equal("5:2", outcome2);

            Assert.Equal(_cultures.Count, _dataRouterManager.GetCallCount("GetMarketDescriptionsAsync"));
            Assert.Equal(_cultures.Count, _dataRouterManager.GetCallCount("GetVariantDescriptionsAsync"));
            Assert.Equal(0, _dataRouterManager.GetCallCount("GetVariantMarketDescriptionAsync"));
            Assert.Equal(0, _dataRouterManager.GetCallCount("GetSportEventSummaryAsync"));
            Assert.Equal(0, _dataRouterManager.GetCallCount("GetPlayerProfileAsync"));
            Assert.Equal(0, _dataRouterManager.GetCallCount("GetCompetitorAsync"));
        }

        [Fact]
        public void GetMarketNamesForVariantMarketDescriptionFromSinglePreOutcomeTextInvalidOutcomeTest()
        {
            const int marketId = 534;
            const string variantSpecifier = "pre:markettext:168883";
            var specifiers = new ReadOnlyDictionary<string, string>(SdkInfo.SpecifiersStringToDictionary($"variant={variantSpecifier}"));
            _dataRouterManager.UriReplacements.Add(new Tuple<string, string>($"{marketId}_{variantSpecifier}", $"variant_market_description_invalid_{marketId}_culture.xml"));

            var nameProvider = _nameProviderFactory.BuildNameProvider(_match, marketId, specifiers);

            Assert.Empty(_variantMarketDescriptionMemoryCache);

            string name0 = null;
            Action action = () => name0 = nameProvider.GetMarketNameAsync(_cultures[0]).Result;
            action.Should().Throw<NameGenerationException>();
            Assert.Null(name0);

            Assert.Empty(_variantMarketDescriptionMemoryCache);

            Assert.Equal(_cultures.Count, _dataRouterManager.GetCallCount("GetMarketDescriptionsAsync"));
            Assert.Equal(_cultures.Count, _dataRouterManager.GetCallCount("GetVariantDescriptionsAsync"));
            Assert.Equal(1, _dataRouterManager.GetCallCount("GetVariantMarketDescriptionAsync"));
            Assert.Equal(0, _dataRouterManager.GetCallCount("GetSportEventSummaryAsync"));
            Assert.Equal(0, _dataRouterManager.GetCallCount("GetPlayerProfileAsync"));
            Assert.Equal(0, _dataRouterManager.GetCallCount("GetCompetitorAsync"));
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
