/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/

using System.Collections.Generic;
using System.Linq;
using Sportradar.OddsFeed.SDK.Entities;
using Sportradar.OddsFeed.SDK.Entities.REST;
using Sportradar.OddsFeed.SDK.Entities.REST.Enums;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal;
using Sportradar.OddsFeed.SDK.Messages;
using Sportradar.OddsFeed.SDK.Messages.Feed;
using Sportradar.OddsFeed.SDK.Tests.Common;
using Xunit;
using Xunit.Abstractions;

namespace Sportradar.OddsFeed.SDK.Tests.Entities
{
    public class MapBetSettlementTests : MapEntityTestBase
    {
        private bet_settlement _record;

        private IBetSettlement<ICompetition> _entity;

        /// <inheritdoc />
        public MapBetSettlementTests(ITestOutputHelper outputHelper)
            : base(outputHelper)
        {
        }

        [Fact]
        public void BetSettlementIsMapped()
        {
            if (_entity == null)
            {
                LoadEntityFromFile();
            }

            Assert.NotNull(_entity);
        }

        [Fact]
        public void TestBetSettlementMapping()
        {
            LoadEntityFromFile(true);
            TestEntityValues(_entity, _record);
        }

        private void LoadEntityFromFile(bool load = false)
        {
            if (_entity == null || load)
            {
                var stream = FileHelper.OpenFile(TestData.FeedXmlPath, "bet_settlement.xml");
                _record = Deserializer.Deserialize<bet_settlement>(stream);
                TestData.FillMessageTimestamp(_record);
                _record.SportId = URN.Parse("sr:sport:1000");
                _entity = Mapper.MapBetSettlement<ICompetition>(_record, new[] { TestData.Culture }, null);
            }
        }

        private void TestEntityValues(IBetSettlement<ICompetition> entity, bet_settlement record)
        {
            Assert.Equal(entity.Event.Id.ToString(), record.event_id);
            var rProduct = TestProducerManager.Create().Get(record.product);
            Assert.Equal(entity.Producer, rProduct);
            Assert.Equal(entity.RequestId, record.request_idSpecified
                                                                ? (long?)record.request_id
                                                                : null);
            Assert.Equal(entity.Timestamps.Created, record.timestamp);

            TestMarketOutcomes(entity.Markets.ToList(), record.outcomes);
        }

        private void TestMarketOutcomes(IReadOnlyList<IMarketWithSettlement> markets, betSettlementMarket[] bets)
        {
            foreach (var b in bets)
            {
                var m = FindMarketWithSettlementByIdAndSpecifier(markets, b.id, b.specifiers);

                Assert.Equal(m.Id, b.id);
                var marketSpecifiersMatch = CompareMarketSpecifiers(m.Specifiers, b.specifiers);
                var marketOutcomeSettlementMatch = CompareMarketOutcomeSettlement(m.OutcomeSettlements.ToList(), b.Items);
                Assert.Equal(m.Specifiers, marketSpecifiersMatch
                                                            ? m.Specifiers
                                                            : null);
                Assert.Equal(m.OutcomeSettlements, marketOutcomeSettlementMatch
                                                            ? m.OutcomeSettlements
                                                            : null);
            }
        }

        private static IMarketWithSettlement FindMarketWithSettlementByIdAndSpecifier(IReadOnlyList<IMarketWithSettlement> markets, long recordId, string recordSpecifiers)
        {
            return markets.FirstOrDefault(m => m.Id == recordId && CompareMarketSpecifiers(m.Specifiers, recordSpecifiers));
        }

        private static bool CompareMarketSpecifiers(IReadOnlyDictionary<string, string> marketSpecifiers, string recordSpecifiers)
        {
            if (marketSpecifiers.Count == 0 && string.IsNullOrWhiteSpace(recordSpecifiers))
            {
                return true;
            }

            var result = true;

            var recordsSplit = recordSpecifiers.Split('|');

            foreach (var s in recordsSplit)
            {
                var recSplit = s.Split('=');

                result = marketSpecifiers.TryGetValue(recSplit[0], out var marketValue);

                if (!result || marketValue != recSplit[1])
                {
                    result = false;
                    break;
                }
            }

            return result;
        }

        private static bool CompareMarketOutcomeSettlement(IReadOnlyList<IOutcomeSettlement> marketOutcomeSettlement, betSettlementMarketOutcome[] betOutcomes)
        {
            var mos = marketOutcomeSettlement.ToList();
            if (mos.Count == 0 && betOutcomes.Length == 0)
            {
                return true;
            }

            foreach (var b in betOutcomes)
            {
                foreach (var m in mos)
                {
                    if (b.id == m.Id)
                    {
                        Assert.Equal(m.Id, b.id);
                        Assert.Equal(m.DeadHeatFactor, b.dead_heat_factorSpecified
                                                                ? (double?)b.dead_heat_factor
                                                                : null);
#pragma warning disable CS0618
                        Assert.Equal(m.Result, b.result == 1);
#pragma warning restore CS0618

                        var voidFactor = (VoidFactor?)MessageMapperHelper.GetVoidFactor(b.void_factorSpecified, b.void_factor);
                        Assert.Equal(m.VoidFactor, voidFactor);

                        break;
                    }
                }
            }

            return true;
        }
    }
}
