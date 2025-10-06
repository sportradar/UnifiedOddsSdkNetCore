// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Messages.Feed;
using Sportradar.OddsFeed.SDK.Tests.Common.Extensions;

namespace Sportradar.OddsFeed.SDK.Tests.Common.Dsl.Feed.Messages;

public static class PreconfiguredFeedMessages
{
    private static readonly long UnixEpoch = DateTimeOffset.UnixEpoch.ToUnixTimeMilliseconds();
    private static readonly Urn SoccerId = "sr:sport:1".ToUrn();
    private static readonly Urn VolleyballId = "sr:sport:23".ToUrn();
    private static readonly Urn TennisId = "sr:sport:5".ToUrn();

    public static rollback_bet_cancel GsApollonVsAeGravasBetCancelRollback()
    {
        return new rollback_bet_cancel
        {
            product = 1,
            event_id = "sr:match:10237855",
            timestamp = 1475958052704,
            request_id = 1,
            request_idSpecified = true,

            SportId = VolleyballId,
            SentAt = UnixEpoch,
            ReceivedAt = UnixEpoch,

            market =
                           [
                               new market { id = 312, specifiers = "setnr=4|pointnr=45" },
                               new market { id = 313, specifiers = "pointnr=1|setnr=2" }
                           ]
        };
    }

    public static bet_cancel GsApollonVsAeGravasBetCancel()
    {
        return new bet_cancel
        {
            product = 1,
            event_id = "sr:match:10237855",
            timestamp = 1475958052704,
            request_id = 1,
            request_idSpecified = true,

            market = new[]
                                    {
                                        new market { id = 312, specifiers = "setnr=4|pointnr=45", extended_specifiers = "info1=10|info2=word" },
                                        new market { id = 313, specifiers = "pointnr=1|setnr=2", extended_specifiers = "info1=4|info2=word1" },
                                        new market { id = 314 }
                                    },

            SentAt = UnixEpoch,
            ReceivedAt = UnixEpoch,
            SportId = VolleyballId
        };
    }

    public static rollback_bet_settlement GsApollonVsAeGravasBetSettlementRollback()
    {
        return new rollback_bet_settlement
        {
            product = 1,
            event_id = "sr:match:10237855",
            timestamp = 1475958052704L,
            request_id = 1,
            request_idSpecified = true,

            SportId = VolleyballId,
            SentAt = UnixEpoch,
            ReceivedAt = UnixEpoch,

            market = new[]
                                    {
                                        new market { id = 312, specifiers = "setnr=4|pointnr=45" },
                                        new market { id = 313, specifiers = "pointnr=1|setnr=2" }
                                    }
        };
    }

    public static alive AliveSubscribed()
    {
        return new alive
        {
            product = 1,
            timestamp = UnixEpoch,
            subscribed = 1,
            SentAt = UnixEpoch,
            ReceivedAt = UnixEpoch
        };
    }

    public static snapshot_complete SnapshotCompleteRequest()
    {
        return new snapshot_complete
        {
            product = 1,
            timestamp = UnixEpoch,
            request_id = 1,
            SentAt = UnixEpoch,
            ReceivedAt = UnixEpoch
        };
    }

    public static fixture_change Bk46VsFcHonkaFixtureChange()
    {
        return new fixture_change
        {
            product = 1,
            event_id = "sr:match:8816128",
            timestamp = UnixEpoch,
            start_time = UnixEpoch,
            change_type = 2,
            change_typeSpecified = true,
            SentAt = UnixEpoch,
            ReceivedAt = UnixEpoch,
            SportId = SoccerId
        };
    }

    public static bet_stop HaukarKaVsKeflavikBetStop()
    {
        return new bet_stop
        {
            product = 1,
            event_id = "sr:match:9578495",
            timestamp = UnixEpoch,
            groups = "all",
            SportId = SoccerId,
            SentAt = UnixEpoch,
            ReceivedAt = UnixEpoch
        };
    }

    public static bet_settlement MenaVsGonzalezBetSettlement()
    {
        var betSettlementBuilder = BetSettlementBuilder.CreateForSportEvent("sr:match:9583135")
                                                       .WithTimestamp(UnixEpoch)
                                                       .WithSportId(TennisId)
                                                       .WithProducerId(1)
                                                       .AddMarket(213, "setnr=3|gamenr=3", "setnr=3|gamenr=3")
                                                       .AddOutcome(70, 0)
                                                       .AddOutcome(72, 1)
                                                       .AddMarket(213, "setnr=3|gamenr=2")
                                                       .AddOutcome(70, 0)
                                                       .AddOutcome(72, 1)
                                                       .AddMarket(209, "setnr=3|gamenrX=2|gamenrY=3")
                                                       .AddOutcome(879, 1)
                                                       .AddOutcome(880, 0)
                                                       .AddOutcome(881, 0)
                                                       .AddMarket(190, "total=11.5")
                                                       .AddOutcome(13, 0)
                                                       .AddOutcome(12, 1)
                                                       .AddMarket(217, "setnr=3|gamenr=4|pointnr=2")
                                                       .AddOutcome(879, 1)
                                                       .AddOutcome(880, 0)
                                                       .AddOutcome(881, 0)
                                                       .AddMarket(212, "setnr=3-3|gamenr=3-3")
                                                       .AddOutcome(74, 0)
                                                       .AddOutcome(76, 1)
                                                       .AddMarket(212, "setnr=3-2|gamenr=3-2")
                                                       .AddOutcome(74, 0)
                                                       .AddOutcome(76, 1)
                                                       .AddMarket(209, "setnr=3|gamenrX=3|gamenrY=4")
                                                       .AddOutcome(879, 1)
                                                       .AddOutcome(880, 0)
                                                       .AddOutcome(881, 0)
                                                       .AddMarket(212, "setnr=3-4|gamenr=3-4")
                                                       .AddOutcome(74, 0)
                                                       .AddOutcome(76, 1)
                                                       .AddMarket(214, "setnr=3|gamenr=3")
                                                       .AddOutcome(886, 0)
                                                       .AddOutcome(887, 0)
                                                       .AddOutcome(888, 1)
                                                       .AddOutcome(889, 0)
                                                       .AddOutcome(890, 0)
                                                       .AddOutcome(891, 0)
                                                       .AddOutcome(892, 0)
                                                       .AddOutcome(893, 0)
                                                       .AddMarket(214, "setnr=3|gamenr=4")
                                                       .AddOutcome(886, 0)
                                                       .AddOutcome(887, 1)
                                                       .AddOutcome(888, 0)
                                                       .AddOutcome(889, 0)
                                                       .AddOutcome(890, 0)
                                                       .AddOutcome(891, 0)
                                                       .AddOutcome(892, 0)
                                                       .AddOutcome(893, 0)
                                                       .AddMarket(214, "setnr=3|gamenr=2")
                                                       .AddOutcome(886, 1)
                                                       .AddOutcome(887, 0)
                                                       .AddOutcome(888, 0)
                                                       .AddOutcome(889, 0)
                                                       .AddOutcome(890, 0)
                                                       .AddOutcome(891, 0)
                                                       .AddOutcome(892, 0)
                                                       .AddOutcome(893, 0)
                                                       .AddMarket(213, "setnr=3|gamenr=4")
                                                       .AddOutcome(70, 1)
                                                       .AddOutcome(72, 0)
                                                       .AddMarket(210, "setnr=3|gamenr=2")
                                                       .AddOutcome(844, 1)
                                                       .AddOutcome(845, 0)
                                                       .AddMarket(210, "setnr=3|gamenr=4")
                                                       .AddOutcome(844, 1)
                                                       .AddOutcome(845, 0)
                                                       .AddMarket(210, "setnr=3|gamenr=3")
                                                       .AddOutcome(844, 1)
                                                       .AddOutcome(845, 0)
                                                       .AddMarket(215, "setnr=3|gamenr=3|server=sr:competitor:66390")
                                                       .AddOutcome(894, 0)
                                                       .AddOutcome(895, 0)
                                                       .AddOutcome(896, 0)
                                                       .AddOutcome(897, 0)
                                                       .AddOutcome(898, 1)
                                                       .AddMarket(215, "setnr=3|gamenr=2|server=sr:competitor:66390")
                                                       .AddOutcome(894, 1)
                                                       .AddOutcome(895, 0)
                                                       .AddOutcome(896, 0)
                                                       .AddOutcome(897, 0)
                                                       .AddOutcome(898, 0)
                                                       .AddMarket(189, "total=19.5")
                                                       .AddOutcome(13, 0)
                                                       .AddOutcome(12, 1)
                                                       .AddMarket(215, "setnr=3|gamenr=4|server=sr:competitor:66390")
                                                       .AddOutcome(894, 0)
                                                       .AddOutcome(895, 1)
                                                       .AddOutcome(896, 0)
                                                       .AddOutcome(897, 0)
                                                       .AddOutcome(898, 0)
                                                       .AddMarket(208, "setnr=3|games=4")
                                                       .AddOutcome(844, 1)
                                                       .AddOutcome(845, 0)
                                                       .AddMarket(208, "setnr=3|games=3")
                                                       .AddOutcome(844, 1)
                                                       .AddOutcome(845, 0)
                                                       .AddMarket(218, "setnr=3|gamenr=2|pointnr=1")
                                                       .AddOutcome(844, 1)
                                                       .AddOutcome(845, 0)
                                                       .AddMarket(218, "setnr=3|gamenr=2|pointnr=2")
                                                       .AddOutcome(844, 1)
                                                       .AddOutcome(845, 0)
                                                       .AddMarket(218, "setnr=3|gamenr=3|pointnr=1")
                                                       .AddOutcome(844, 0)
                                                       .AddOutcome(845, 1)
                                                       .AddMarket(208, "setnr=3|games=2")
                                                       .AddOutcome(844, 1)
                                                       .AddOutcome(845, 0)
                                                       .AddMarket(218, "setnr=3|gamenr=2|pointnr=3")
                                                       .AddOutcome(844, 1)
                                                       .AddOutcome(845, 0)
                                                       .AddMarket(218, "setnr=3|gamenr=3|pointnr=2")
                                                       .AddOutcome(844, 1)
                                                       .AddOutcome(845, 0)
                                                       .AddMarket(218, "setnr=3|gamenr=4|pointnr=1")
                                                       .AddOutcome(844, 1)
                                                       .AddOutcome(845, 0)
                                                       .AddMarket(218, "setnr=3|gamenr=2|pointnr=4")
                                                       .AddOutcome(844, 1)
                                                       .AddOutcome(845, 0)
                                                       .AddMarket(218, "setnr=3|gamenr=3|pointnr=3")
                                                       .AddOutcome(844, 1)
                                                       .AddOutcome(845, 0)
                                                       .AddMarket(218, "setnr=3|gamenr=4|pointnr=2")
                                                       .AddOutcome(844, 1)
                                                       .AddOutcome(845, 0)
                                                       .AddMarket(218, "setnr=3|gamenr=3|pointnr=4")
                                                       .AddOutcome(844, 1)
                                                       .AddOutcome(845, 0)
                                                       .AddMarket(218, "setnr=3|gamenr=4|pointnr=3")
                                                       .AddOutcome(844, 1)
                                                       .AddOutcome(845, 0)
                                                       .AddMarket(218, "setnr=3|gamenr=3|pointnr=5")
                                                       .AddOutcome(844, 0)
                                                       .AddOutcome(845, 1)
                                                       .AddMarket(218, "setnr=3|gamenr=4|pointnr=4")
                                                       .AddOutcome(844, 0)
                                                       .AddOutcome(845, 1)
                                                       .AddMarket(216, "setnr=3|gamenr=3|pointnr=2")
                                                       .AddOutcome(844, 1)
                                                       .AddOutcome(845, 0)
                                                       .AddMarket(189, "total=21.5")
                                                       .AddOutcome(13, 0)
                                                       .AddOutcome(12, 1)
                                                       .AddMarket(216, "setnr=3|gamenr=4|pointnr=2")
                                                       .AddOutcome(844, 1)
                                                       .AddOutcome(845, 0)
                                                       .AddMarket(189, "total=20.5")
                                                       .AddOutcome(13, 0)
                                                       .AddOutcome(12, 1)
                                                       .AddMarket(216, "setnr=3|gamenr=2|pointnr=2")
                                                       .AddOutcome(844, 1)
                                                       .AddOutcome(845, 0)
                                                       .AddMarket(217, "setnr=3|gamenr=3|pointnr=2")
                                                       .AddOutcome(879, 0)
                                                       .AddOutcome(880, 1)
                                                       .AddOutcome(881, 0)
                                                       .AddMarket(217, "setnr=3|gamenr=2|pointnr=2")
                                                       .AddOutcome(879, 1)
                                                       .AddOutcome(880, 0)
                                                       .AddOutcome(881, 0)
                                                       .AddMarket(211, "setnr=3|gamenr=4")
                                                       .AddOutcome(882, 0)
                                                       .AddOutcome(883, 1)
                                                       .AddOutcome(884, 0)
                                                       .AddOutcome(885, 0)
                                                       .AddMarket(211, "setnr=3|gamenr=3")
                                                       .AddOutcome(882, 0)
                                                       .AddOutcome(883, 0)
                                                       .AddOutcome(884, 1)
                                                       .AddOutcome(885, 0)
                                                       .AddMarket(211, "setnr=3|gamenr=2")
                                                       .AddOutcome(882, 1)
                                                       .AddOutcome(883, 0)
                                                       .AddOutcome(884, 0)
                                                       .AddOutcome(885, 0);

        return betSettlementBuilder.Build();
    }

    public static odds_change AlNasrVsMuscatOddsChange()
    {
        const long timeStamp = 1487254396715L;

        var oc = OddsChangeBuilder.Create()
                                  .WithProduct(4)
                                  .ForEventId("sr:match:10927088".ToUrn())
                                  .WithTimestamp(timeStamp)
                                  .WithSportEventStatus(new sportEventStatus
                                  {
                                      status = 1,
                                      reporting = 1,
                                      match_status = 6,
                                      home_score = 0,
                                      away_score = 2,
                                      clock = new clockType()
                                      {
                                          match_time = "27:33"
                                      },
                                      period_scores = [new periodScoreType { away_score = 2, home_score = 0, match_status_code = 6, number = 1 }]
                                  })

                                  .AddMarket(m => m.WithMarketId(26)
                                                   .WithStatus(1)
                                                   .WithFavourite(1)
                                                   .WithOutcome("70", 1.85)
                                                   .WithOutcome("72", 1.75))
                                  .AddMarket(m => m.WithMarketId(10)
                                                   .WithStatus(1)
                                                   .WithFavourite(1)
                                                   .WithOutcome("9", 3.9)
                                                   .WithOutcome("10", 1.08))
                                  .AddMarket(m => m.WithMarketId(68)
                                                   .WithStatus(1)
                                                   .WithFavourite(1)
                                                   .WithSpecifiers("total=2.5")
                                                   .WithOutcome("13", 1.5)
                                                   .WithOutcome("12", 2.35))
                                  .AddMarket(m => m.WithMarketId(68)
                                                   .WithStatus(1)
                                                   .WithSpecifiers("total=3.5")
                                                   .WithOutcome("12", 6.5))
                                  .AddMarket(m => m.WithMarketId(68).WithStatus(-3).WithSpecifiers("total=1.5"))
                                  .AddMarket(m => m.WithMarketId(61)
                                                   .WithStatus(1)
                                                   .WithFavourite(1)
                                                   .WithSpecifiers("score=0:2")
                                                   .WithOutcome("1", 3.6)
                                                   .WithOutcome("2", 1.4)
                                                   .WithOutcome("3", 7.75))
                                  .AddMarket(m => m.WithMarketId(71)
                                                   .WithStatus(1)
                                                   .WithFavourite(1)
                                                   .WithSpecifiers("variant=sr:exact_goals:4+")
                                                   .WithOutcome("sr:exact_goals:4+:94", 1.5)
                                                   .WithOutcome("sr:exact_goals:4+:95", 3.0)
                                                   .WithOutcome("sr:exact_goals:4+:96", 10.5))
                                  .AddMarket(m => m.WithMarketId(19)
                                                   .WithStatus(1)
                                                   .WithFavourite(1)
                                                   .WithSpecifiers("total=1.5")
                                                   .WithOutcome("13", 1.4)
                                                   .WithOutcome("12", 2.6))
                                  .AddMarket(m => m.WithMarketId(73)
                                                   .WithStatus(1)
                                                   .WithFavourite(1)
                                                   .WithSpecifiers("variant=sr:exact_goals:3+")
                                                   .WithOutcome("sr:exact_goals:3+:90", 1.1)
                                                   .WithOutcome("sr:exact_goals:3+:91", 5.0))
                                  .AddMarket(m => m.WithMarketId(61).WithStatus(0).WithSpecifiers("score=0:0"))
                                  .AddMarket(m => m.WithMarketId(18)
                                                   .WithStatus(1)
                                                   .WithSpecifiers("total=4.5")
                                                   .WithOutcome("12", 3.1)
                                                   .WithOutcome("13", 1.25))
                                  .AddMarket(m => m.WithMarketId(61).WithStatus(0).WithSpecifiers("score=0:1"))
                                  .AddMarket(m => m.WithMarketId(18)
                                                   .WithStatus(1)
                                                   .WithFavourite(1)
                                                   .WithSpecifiers("total=3.5")
                                                   .WithOutcome("12", 1.7)
                                                   .WithOutcome("13", 1.9))
                                  .AddMarket(m => m.WithMarketId(18)
                                                   .WithStatus(1)
                                                   .WithSpecifiers("total=2.5")
                                                   .WithOutcome("12", 1.14)
                                                   .WithOutcome("13", 5.25))
                                  .AddMarket(m => m.WithMarketId(18).WithStatus(-3).WithSpecifiers("total=1.5"))
                                  .AddMarket(m => m.WithMarketId(20).WithStatus(-3).WithSpecifiers("total=0.5"))
                                  .AddMarket(m => m.WithMarketId(7).WithStatus(0).WithSpecifiers("score=0:1"))
                                  .AddMarket(m => m.WithMarketId(7).WithStatus(0).WithSpecifiers("score=0:0"))
                                  .AddMarket(m => m.WithMarketId(20)
                                                   .WithStatus(1)
                                                   .WithFavourite(1)
                                                   .WithSpecifiers("total=2.5")
                                                   .WithOutcome("13", 1.65)
                                                   .WithOutcome("12", 2.0))
                                  .AddMarket(m => m.WithMarketId(20).WithStatus(-3).WithSpecifiers("total=1.5"))
                                  .AddMarket(m => m.WithMarketId(7)
                                                   .WithStatus(1)
                                                   .WithFavourite(1)
                                                   .WithSpecifiers("score=0:2")
                                                   .WithOutcome("1", 1.8)
                                                   .WithOutcome("2", 2.85)
                                                   .WithOutcome("3", 4.75))
                                  .AddMarket(m => m.WithMarketId(24)
                                                   .WithStatus(1)
                                                   .WithFavourite(1)
                                                   .WithSpecifiers("variant=sr:exact_goals:3+")
                                                   .WithOutcome("sr:exact_goals:3+:90", 1.65)
                                                   .WithOutcome("sr:exact_goals:3+:91", 2.0))
                                  .AddMarket(m => m.WithMarketId(8).WithStatus(-3).WithSpecifiers("goalnr=1"))
                                  .AddMarket(m => m.WithMarketId(8).WithStatus(-3).WithSpecifiers("goalnr=2"))
                                  .AddMarket(m => m.WithMarketId(29)
                                                   .WithStatus(1)
                                                   .WithFavourite(1)
                                                   .WithOutcome("74", 1.35)
                                                   .WithOutcome("76", 2.95))
                                  .AddMarket(m => m.WithMarketId(8)
                                                   .WithStatus(1)
                                                   .WithFavourite(1)
                                                   .WithSpecifiers("goalnr=3")
                                                   .WithOutcome("6", 1.65)
                                                   .WithOutcome("7", 5.25)
                                                   .WithOutcome("8", 3.2))
                                  .AddMarket(m => m.WithMarketId(14).WithStatus(0).WithSpecifiers("hcp=0:2"))
                                  .AddMarket(m => m.WithMarketId(23)
                                                   .WithStatus(1)
                                                   .WithFavourite(1)
                                                   .WithSpecifiers("variant=sr:exact_goals:3+")
                                                   .WithOutcome("sr:exact_goals:3+:88", 2.95)
                                                   .WithOutcome("sr:exact_goals:3+:89", 2.5)
                                                   .WithOutcome("sr:exact_goals:3+:90", 4.2)
                                                   .WithOutcome("sr:exact_goals:3+:91", 7.5))
                                  .AddMarket(m => m.WithMarketId(14).WithStatus(0).WithSpecifiers("hcp=2:0"))
                                  .AddMarket(m => m.WithMarketId(14)
                                                   .WithStatus(1)
                                                   .WithSpecifiers("hcp=3:0")
                                                   .WithOutcome("1711", 1.16)
                                                   .WithOutcome("1712", 5.5)
                                                   .WithOutcome("1713", 16.25))
                                  .AddMarket(m => m.WithMarketId(14)
                                                   .WithStatus(1)
                                                   .WithSpecifiers("hcp=0:1")
                                                   .WithOutcome("1711", 24.0)
                                                   .WithOutcome("1712", 9.25))
                                  .AddMarket(m => m.WithMarketId(14)
                                                   .WithStatus(1)
                                                   .WithFavourite(1)
                                                   .WithSpecifiers("hcp=1:0")
                                                   .WithOutcome("1711", 3.9)
                                                   .WithOutcome("1712", 3.15)
                                                   .WithOutcome("1713", 1.85))
                                  .AddMarket(m => m.WithMarketId(11)
                                                   .WithStatus(1)
                                                   .WithFavourite(1)
                                                   .WithOutcome("4", 6.25)
                                                   .WithOutcome("5", 1.05))
                                  .AddMarket(m => m.WithMarketId(62)
                                                   .WithStatus(1)
                                                   .WithFavourite(1)
                                                   .WithSpecifiers("goalnr=3")
                                                   .WithOutcome("6", 3.45)
                                                   .WithOutcome("7", 1.5)
                                                   .WithOutcome("8", 6.75))
                                  .AddMarket(m => m.WithMarketId(62).WithStatus(-3).WithSpecifiers("goalnr=2"))
                                  .AddMarket(m => m.WithMarketId(37).WithStatus(0).WithSpecifiers("total=2.5"))
                                  .AddMarket(m => m.WithMarketId(21)
                                                   .WithStatus(1)
                                                   .WithFavourite(1)
                                                   .WithSpecifiers("variant=sr:exact_goals:6+")
                                                   .WithOutcome("sr:exact_goals:6+:70", 5.25)
                                                   .WithOutcome("sr:exact_goals:6+:71", 3.0)
                                                   .WithOutcome("sr:exact_goals:6+:72", 3.45)
                                                   .WithOutcome("sr:exact_goals:6+:73", 5.5)
                                                   .WithOutcome("sr:exact_goals:6+:74", 8.5))
                                  .AddMarket(m => m.WithMarketId(62).WithStatus(-3).WithSpecifiers("goalnr=1"))
                                  .AddMarket(m => m.WithMarketId(72)
                                                   .WithStatus(1)
                                                   .WithFavourite(1)
                                                   .WithSpecifiers("variant=sr:exact_goals:3+")
                                                   .WithOutcome("sr:exact_goals:3+:88", 1.3)
                                                   .WithOutcome("sr:exact_goals:3+:89", 3.6)
                                                   .WithOutcome("sr:exact_goals:3+:90", 22.0)
                                                   .WithOutcome("sr:exact_goals:3+:91", 50.0))
                                  .AddMarket(m => m.WithMarketId(1)
                                                   .WithStatus(1)
                                                   .WithFavourite(1)
                                                   .WithOutcome("1", 9.75)
                                                   .WithOutcome("2", 5.5)
                                                   .WithOutcome("3", 1.22))
                                  .AddMarket(m => m.WithMarketId(68).WithStatus(-3).WithSpecifiers("total=0.5"))
                                  .AddMarket(m => m.WithMarketId(60)
                                                   .WithStatus(1)
                                                   .WithFavourite(1)
                                                   .WithOutcome("1", 50.0)
                                                   .WithOutcome("2", 9.25))
                                  .Build();

        oc.SportId = SoccerId;
        oc.SentAt = UnixEpoch;
        oc.ReceivedAt = UnixEpoch;

        return oc;
    }
}
