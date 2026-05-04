// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Messages.Rest;
using Sportradar.OddsFeed.SDK.Tests.Common.Builders.CustomBet;

namespace Sportradar.OddsFeed.SDK.Tests.Common.Dsl.Api.CustomBet;

public class CustomBetEndpoint
{
    public static PreBuiltBetsType GetFullyPopulatedPrebuiltBetsType()
    {
        var result = PrebuiltBetsBuilder.Create()
                                        .WithEventId(Urn.Parse("sr:match:1000"))
                                        .WithRequestedRecommendations(2)
                                        .WithGeneratedAt("2024-01-01T00:00:00Z")
                                        .WithProvidedRecommendations(2)
                                        .AddRecommendation(() => PrebuiltBetsRecommendationBuilder.Create()
                                                                                                  .WithOdds(2.20)
                                                                                                  .WithProbability(0.45)
                                                                                                  .AddSelection(101, "12", "variant=sr:exact_goals:bestof:3")
                                                                                                  .AddSelection(102, "13", "variant=sr:exact_goals:bestof:5"))
                                        .AddRecommendation(() => PrebuiltBetsRecommendationBuilder.Create()
                                                                                                  .WithOdds(3.40)
                                                                                                  .WithProbability(0.30)
                                                                                                  .AddSelection(103, "14", "variant=sr:exact_goals:bestof:7"))
                                        .Build();

        result.@event[0].source = "default-source";
        return result;
    }

    public static AvailableSelectionsType GetAvailableSelectionsFor(Urn eventId)
    {
        return AvailableSelectionsBuilder.Create()
                                         .WithGeneratedAt("2022-08-29T11:32:59+00:00")
                                         .WithEventId(eventId)
                                         .AddMarket(10, null, "9", "10", "11")
                                         .AddMarket(19, "total=2.5", "12", "13")
                                         .AddMarket(41, "score=1:2", "128", "130", "132", "134", "136", "138", "140", "142", "144", "146", "148", "150", "152", "154", "156", "158", "160", "162", "164", "166", "110", "114", "116", "118", "120", "122", "124", "126")
                                         .AddMarket(18, "total=7.5", "12", "13")
                                         .AddMarket(18, "total=3.5", "12", "13")
                                         .AddMarket(20, "total=4.5", "12", "13")
                                         .AddMarket(7, "score=1:2", "1", "2", "3")
                                         .AddMarket(21, "variant=sr:exact_goals:5+", "sr:exact_goals:5+:1336", "sr:exact_goals:5+:1337", "sr:exact_goals:5+:1338", "sr:exact_goals:5+:1339", "sr:exact_goals:5+:1340", "sr:exact_goals:5+:1341")
                                         .AddMarket(879, null, "74", "76")
                                         .AddMarket(26, null, "70", "72")
                                         .AddMarket(19, "total=5.5", "12", "13")
                                         .AddMarket(19, "total=1.5", "12", "13")
                                         .AddMarket(18, "total=4.5", "12", "13")
                                         .AddMarket(20, "total=3.5", "12", "13")
                                         .AddMarket(14, "hcp=2:0", "1711", "1712", "1713")
                                         .AddMarket(1, null, "1", "2", "3")
                                         .AddMarket(20, "total=6.5", "12", "13")
                                         .AddMarket(19, "total=4.5", "12", "13")
                                         .AddMarket(18, "total=5.5", "12", "13")
                                         .AddMarket(20, "total=2.5", "12", "13")
                                         .AddMarket(45, null, "320", "322", "324", "274", "276", "278", "280", "282", "284", "286", "288", "290", "292", "294", "296", "298", "300", "302", "304", "306", "308", "310", "312", "314", "316", "318")
                                         .AddMarket(8, "goalnr=4", "6", "7", "8")
                                         .AddMarket(23, "variant=sr:exact_goals:3+", "sr:exact_goals:3+:88", "sr:exact_goals:3+:89", "sr:exact_goals:3+:90", "sr:exact_goals:3+:91")
                                         .AddMarket(14, "hcp=3:0", "1711", "1712", "1713")
                                         .AddMarket(21, "variant=sr:exact_goals:6+", "sr:exact_goals:6+:68", "sr:exact_goals:6+:69", "sr:exact_goals:6+:70", "sr:exact_goals:6+:71", "sr:exact_goals:6+:72", "sr:exact_goals:6+:73", "sr:exact_goals:6+:74")
                                         .AddMarket(880, null, "74", "76")
                                         .AddMarket(20, "total=5.5", "12", "13")
                                         .AddMarket(19, "total=3.5", "12", "13")
                                         .AddMarket(18, "total=6.5", "12", "13")
                                         .AddMarket(24, "variant=sr:exact_goals:3+", "sr:exact_goals:3+:88", "sr:exact_goals:3+:89", "sr:exact_goals:3+:90", "sr:exact_goals:3+:91")
                                         .AddMarket(14, "hcp=0:2", "1711", "1712", "1713")
                                         .AddMarket(14, "hcp=0:1", "1711", "1712", "1713")
                                         .AddMarket(881, null, "74", "76")
                                         .Build();
    }
}
