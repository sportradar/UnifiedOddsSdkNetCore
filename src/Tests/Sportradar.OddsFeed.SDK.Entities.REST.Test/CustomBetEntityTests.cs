using Castle.Core.Internal;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sportradar.OddsFeed.SDK.API;
using Sportradar.OddsFeed.SDK.API.Internal;
using Sportradar.OddsFeed.SDK.Common.Internal;
using Sportradar.OddsFeed.SDK.Entities.REST.CustomBet;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching;
using Sportradar.OddsFeed.SDK.Messages;
using Sportradar.OddsFeed.SDK.Messages.REST;
using Sportradar.OddsFeed.SDK.Test.Shared;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Test
{
    [TestClass]
    public class CustomBetEntityTests
    {
        private IDataRouterManager _dataRouterManager;
        private ICustomBetSelectionBuilder _customBetSelectionBuilder;

        [TestInitialize]
        public void Init()
        {
            _dataRouterManager = new TestDataRouterManager(new CacheManager());
            _customBetSelectionBuilder = new CustomBetSelectionBuilder();
        }

        [TestMethod]
        public void AvailableSelectionsMapTest()
        {
            var availableSelectionsType = MessageFactoryRest.GetAvailableSelections(URN.Parse("sr:match:1000"));
            var resultAvailableSelections = MessageFactorySdk.GetAvailableSelections(availableSelectionsType);

            AvailableSelectionsCompare(availableSelectionsType, resultAvailableSelections);
        }

        [TestMethod]
        public void AvailableSelectionsEmptyMapTest()
        {
            var availableSelectionsType = MessageFactoryRest.GetAvailableSelections(URN.Parse("sr:match:1000"), 0);
            Assert.IsTrue(availableSelectionsType.@event.markets.IsNullOrEmpty());
            var resultAvailableSelections = MessageFactorySdk.GetAvailableSelections(availableSelectionsType);
            Assert.IsTrue(resultAvailableSelections.Markets.IsNullOrEmpty());

            AvailableSelectionsCompare(availableSelectionsType, resultAvailableSelections);
        }

        [TestMethod]
        public void CalculationEmptyMapTest()
        {
            var calculationResponseType = MessageFactoryRest.GetCalculationResponse(URN.Parse("sr:match:1000"), 0);
            var calculation = MessageFactorySdk.GetCalculation(calculationResponseType);

            Assert.IsNotNull(calculationResponseType);
            Assert.IsNotNull(calculation);
            Assert.IsTrue(calculationResponseType.available_selections.IsNullOrEmpty());
            Assert.IsTrue(calculation.AvailableSelections.IsNullOrEmpty());

            Assert.AreEqual(calculationResponseType.calculation.odds, calculation.Odds);
            Assert.AreEqual(calculationResponseType.calculation.probability, calculation.Probability);
            if (calculationResponseType.generated_at.IsNullOrEmpty())
            {
                Assert.IsNull(calculation.GeneratedAt);
            }
            else
            {
                Assert.AreEqual(SdkInfo.ParseDate(calculationResponseType.generated_at), calculation.GeneratedAt);
            }
        }

        [TestMethod]
        public void CalculationMapTest()
        {
            var calculationResponseType = MessageFactoryRest.GetCalculationResponse(URN.Parse("sr:match:1000"), 7);
            var calculation = MessageFactorySdk.GetCalculation(calculationResponseType);

            Assert.IsNotNull(calculationResponseType);
            Assert.IsNotNull(calculation);

            Assert.AreEqual(calculationResponseType.calculation.odds, calculation.Odds);
            Assert.AreEqual(calculationResponseType.calculation.probability, calculation.Probability);
            if (calculationResponseType.generated_at.IsNullOrEmpty())
            {
                Assert.IsNull(calculation.GeneratedAt);
            }
            else
            {
                Assert.AreEqual(SdkInfo.ParseDate(calculationResponseType.generated_at), calculation.GeneratedAt);
            }

            Assert.AreEqual(calculationResponseType.available_selections.Length, calculation.AvailableSelections.Count());
            for (var i = 0; i < calculationResponseType.available_selections.Length; i++)
            {
                var sourceAvailableSelection = calculationResponseType.available_selections[i];
                var resultAvailableSelection = calculation.AvailableSelections.ElementAt(i);

                Assert.AreEqual(URN.Parse(sourceAvailableSelection.id), resultAvailableSelection.Event);
                foreach (var sourceMarket in sourceAvailableSelection.markets)
                {
                    var resultMarket = resultAvailableSelection.Markets.First(f => f.Id == sourceMarket.id);
                    Assert.IsNotNull(resultMarket);
                    MarketCompare(sourceMarket, resultMarket);
                }
            }
            var marketCount = calculation.AvailableSelections.SelectMany(s => s.Markets).Count();
            var outcomeCount = calculation.AvailableSelections.SelectMany(s => s.Markets).SelectMany(o => o.Outcomes).Count();
            Trace.WriteLine($"Calculation has {marketCount} markets and {outcomeCount} outcomes.");
        }


        [TestMethod]
        public void CalculationFilterEmptyMapTest()
        {
            var calculationResponseType = MessageFactoryRest.GetFilteredCalculationResponse(URN.Parse("sr:match:1000"), 0);
            var calculation = MessageFactorySdk.GetCalculationFilter(calculationResponseType);

            Assert.IsNotNull(calculationResponseType);
            Assert.IsNotNull(calculation);
            Assert.IsTrue(calculationResponseType.available_selections.IsNullOrEmpty());
            Assert.IsTrue(calculation.AvailableSelections.IsNullOrEmpty());

            Assert.AreEqual(calculationResponseType.calculation.odds, calculation.Odds);
            Assert.AreEqual(calculationResponseType.calculation.probability, calculation.Probability);
            if (calculationResponseType.generated_at.IsNullOrEmpty())
            {
                Assert.IsNull(calculation.GeneratedAt);
            }
            else
            {
                Assert.AreEqual(SdkInfo.ParseDate(calculationResponseType.generated_at), calculation.GeneratedAt);
            }
            var marketCount = calculation.AvailableSelections.SelectMany(s => s.Markets).Count();
            var outcomeCount = calculation.AvailableSelections.SelectMany(s => s.Markets).SelectMany(o => o.Outcomes).Count();
            Trace.WriteLine($"Calculation has {marketCount} markets and {outcomeCount} outcomes.");
        }

        [TestMethod]
        public void CalculationFilterMapTest()
        {
            var calculationResponseType = MessageFactoryRest.GetFilteredCalculationResponse(URN.Parse("sr:match:1000"), 7);
            var calculation = MessageFactorySdk.GetCalculationFilter(calculationResponseType);

            Assert.IsNotNull(calculationResponseType);
            Assert.IsNotNull(calculation);

            Assert.AreEqual(calculationResponseType.calculation.odds, calculation.Odds);
            Assert.AreEqual(calculationResponseType.calculation.probability, calculation.Probability);
            if (calculationResponseType.generated_at.IsNullOrEmpty())
            {
                Assert.IsNull(calculation.GeneratedAt);
            }
            else
            {
                Assert.AreEqual(SdkInfo.ParseDate(calculationResponseType.generated_at), calculation.GeneratedAt);
            }

            Assert.AreEqual(calculationResponseType.available_selections.Length, calculation.AvailableSelections.Count());
            for (var i = 0; i < calculationResponseType.available_selections.Length; i++)
            {
                var sourceAvailableSelection = calculationResponseType.available_selections[i];
                var resultAvailableSelection = calculation.AvailableSelections.ElementAt(i);

                Assert.AreEqual(URN.Parse(sourceAvailableSelection.id), resultAvailableSelection.Event);
                foreach (var sourceMarket in sourceAvailableSelection.markets)
                {
                    var resultMarket = resultAvailableSelection.Markets.First(f => f.Id == sourceMarket.id);
                    Assert.IsNotNull(resultMarket);
                    MarketCompare(sourceMarket, resultMarket);
                }
            }
            var marketCount = calculation.AvailableSelections.SelectMany(s => s.Markets).Count();
            var outcomeCount = calculation.AvailableSelections.SelectMany(s => s.Markets).SelectMany(o => o.Outcomes).Count();
            Trace.WriteLine($"Calculation has {marketCount} markets and {outcomeCount} outcomes.");
        }

        [TestMethod]
        public void GetAvailableSelectionsTest()
        {
            var eventId = URN.Parse("sr:match:31561675");
            var resultAvailableSelections = _dataRouterManager.GetAvailableSelectionsAsync(eventId).Result;

            Assert.IsNotNull(resultAvailableSelections);
            Assert.AreEqual(eventId, resultAvailableSelections.Event);
            Assert.IsFalse(resultAvailableSelections.Markets.IsNullOrEmpty());
        }

        [TestMethod]
        public void GetCalculationTest()
        {
            var eventId = URN.Parse("sr:match:31561675");
            var availableSelections = _dataRouterManager.GetAvailableSelectionsAsync(eventId).Result;
            Assert.IsNotNull(availableSelections);

            var matchSelections = new List<ISelection>();
            var market = availableSelections.Markets.First();
            var selection = _customBetSelectionBuilder.SetEventId(eventId).SetMarketId(market.Id).SetOutcomeId(market.Outcomes.First()).SetSpecifiers(market.Specifiers).Build();
            matchSelections.Add(selection);
            market = availableSelections.Markets.Last();
            selection = _customBetSelectionBuilder.SetEventId(eventId).SetMarketId(market.Id).SetOutcomeId(market.Outcomes.First()).SetSpecifiers(market.Specifiers).Build();
            matchSelections.Add(selection);

            var calculation = _dataRouterManager.CalculateProbabilityAsync(matchSelections).Result;

            Assert.IsNotNull(calculation);
            Assert.AreEqual(eventId, calculation.AvailableSelections.First().Event);
            Assert.IsFalse(calculation.AvailableSelections.IsNullOrEmpty());
        }

        [TestMethod]
        public void GetCalculationFilterTest()
        {
            var eventId = URN.Parse("sr:match:31561675");
            var availableSelections = _dataRouterManager.GetAvailableSelectionsAsync(eventId).Result;
            Assert.IsNotNull(availableSelections);

            var matchSelections = new List<ISelection>();
            var market = availableSelections.Markets.First();
            var selection = _customBetSelectionBuilder.SetEventId(eventId).SetMarketId(market.Id).SetOutcomeId(market.Outcomes.First()).SetSpecifiers(market.Specifiers).Build();
            matchSelections.Add(selection);
            market = availableSelections.Markets.Last();
            selection = _customBetSelectionBuilder.SetEventId(eventId).SetMarketId(market.Id).SetOutcomeId(market.Outcomes.First()).SetSpecifiers(market.Specifiers).Build();
            matchSelections.Add(selection);

            var calculation = _dataRouterManager.CalculateProbabilityFilteredAsync(matchSelections).Result;

            Assert.IsNotNull(calculation);
            Assert.AreEqual(eventId, calculation.AvailableSelections.First().Event);
            Assert.IsFalse(calculation.AvailableSelections.IsNullOrEmpty());
        }

        private void AvailableSelectionsCompare(AvailableSelectionsType source, IAvailableSelections result)
        {
            Assert.IsNotNull(source);
            Assert.IsNotNull(result);

            Assert.IsNotNull(source.@event);
            Assert.IsFalse(string.IsNullOrEmpty(source.generated_at));

            Assert.AreEqual(source.@event.id, result.Event.ToString());

            if (source.@event.markets.IsNullOrEmpty())
            {
                Assert.IsTrue(result.Markets.IsNullOrEmpty());
                return;
            }

            Assert.AreEqual(source.@event.markets.Length, result.Markets.Count());
            foreach (var sourceMarket in source.@event.markets)
            {
                var resultMarket = result.Markets.First(f => f.Id == sourceMarket.id);
                Assert.IsNotNull(resultMarket);
                MarketCompare(sourceMarket, resultMarket);
            }
        }

        private void MarketCompare(MarketType source, CustomBet.IMarket result)
        {
            Assert.IsNotNull(source);
            Assert.IsNotNull(result);

            Assert.AreEqual(source.id, result.Id);
            Assert.AreEqual(source.specifiers, result.Specifiers);

            if (source.outcome.IsNullOrEmpty())
            {
                Assert.IsTrue(result.Outcomes.IsNullOrEmpty());
                return;
            }

            Assert.AreEqual(source.outcome.Length, result.Outcomes.Count());
            foreach (var outcomeType in source.outcome)
            {
                Assert.IsTrue(result.Outcomes.Contains(outcomeType.id));
            }
        }

        private void MarketCompare(FilteredMarketType source, IMarketFilter result)
        {
            Assert.IsNotNull(source);
            Assert.IsNotNull(result);

            Assert.AreEqual(source.id, result.Id);
            Assert.AreEqual(source.specifiers, result.Specifiers);

            if (source.outcome.IsNullOrEmpty())
            {
                Assert.IsTrue(result.Outcomes.IsNullOrEmpty());
                return;
            }

            Assert.AreEqual(source.outcome.Length, result.Outcomes.Count());
            foreach (var sourceOutcome in source.outcome)
            {
                var resultOutcome = result.Outcomes.First(f => f.Id == sourceOutcome.id);
                Assert.IsNotNull(resultOutcome);
                Assert.AreEqual(sourceOutcome.id, resultOutcome.Id);
                if (sourceOutcome.conflictSpecified)
                {
                    Assert.AreEqual(sourceOutcome.conflict, resultOutcome.IsConflict);
                }
                else
                {
                    Assert.IsNull(resultOutcome.IsConflict);
                }
            }
        }
    }
}
