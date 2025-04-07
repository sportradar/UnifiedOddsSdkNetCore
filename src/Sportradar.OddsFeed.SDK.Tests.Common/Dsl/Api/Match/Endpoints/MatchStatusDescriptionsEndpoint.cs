// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using Sportradar.OddsFeed.SDK.Messages.Rest;
using Sportradar.OddsFeed.SDK.Tests.Common.Dsl.Api.Match.Builders;

namespace Sportradar.OddsFeed.SDK.Tests.Common.Dsl.Api.Match.Endpoints;

public static class MatchStatusDescriptionsEndpoint
{
    public static match_status_descriptions GetMatchStatusDescriptionForStatus01En()
    {
        var matchStatusDescriptions = new MatchStatusDescriptionsBuilder()
                                     .WithResponseCode(response_code.OK)
                                     .AddMatchStatus(m => m
                                                         .WithId(0)
                                                         .WithDescription("Not started")
                                                         .WithSports(s => s.WithAll().Build()))
                                     .AddMatchStatus(m => m
                                                         .WithId(1)
                                                         .WithDescription("1st period")
                                                         .WithPeriodNumber(1)
                                                         .WithSports(s => s
                                                                         .AddSport(sp => sp.WithId("sr:sport:2"))
                                                                         .AddSport(sp => sp.WithId("sr:sport:4"))
                                                                         .AddSport(sp => sp.WithId("sr:sport:7"))
                                                                         .AddSport(sp => sp.WithId("sr:sport:13"))
                                                                         .AddSport(sp => sp.WithId("sr:sport:24"))
                                                                         .AddSport(sp => sp.WithId("sr:sport:32"))
                                                                         .AddSport(sp => sp.WithId("sr:sport:34"))
                                                                         .AddSport(sp => sp.WithId("sr:sport:131"))
                                                                         .AddSport(sp => sp.WithId("sr:sport:153"))
                                                                         .AddSport(sp => sp.WithId("sr:sport:157"))
                                                                         .AddSport(sp => sp.WithId("sr:sport:195")).Build()
                                                                    ))
                                     .Build();

        return matchStatusDescriptions;
    }

    public static match_status_descriptions GetMatchStatusDescriptionForStatus01De()
    {
        var matchStatusDescriptions = new MatchStatusDescriptionsBuilder()
                                     .WithResponseCode(response_code.OK)
                                     .AddMatchStatus(m => m
                                                         .WithId(0)
                                                         .WithDescription("Nicht gestartet")
                                                         .WithSports(s => s.WithAll().Build()))
                                     .AddMatchStatus(m => m
                                                         .WithId(1)
                                                         .WithDescription("1. Drittel")
                                                         .WithPeriodNumber(1)
                                                         .WithSports(s => s
                                                                         .AddSport(sp => sp.WithId("sr:sport:2"))
                                                                         .AddSport(sp => sp.WithId("sr:sport:4"))
                                                                         .AddSport(sp => sp.WithId("sr:sport:7"))
                                                                         .AddSport(sp => sp.WithId("sr:sport:13"))
                                                                         .AddSport(sp => sp.WithId("sr:sport:24"))
                                                                         .AddSport(sp => sp.WithId("sr:sport:32"))
                                                                         .AddSport(sp => sp.WithId("sr:sport:34"))
                                                                         .AddSport(sp => sp.WithId("sr:sport:131"))
                                                                         .AddSport(sp => sp.WithId("sr:sport:153"))
                                                                         .AddSport(sp => sp.WithId("sr:sport:157"))
                                                                         .AddSport(sp => sp.WithId("sr:sport:195"))
                                                                         .Build()
                                                                    ))
                                     .Build();

        return matchStatusDescriptions;
    }

    public static match_status_descriptions GetMatchStatusDescriptionForStatus01Hu()
    {
        var matchStatusDescriptions = new MatchStatusDescriptionsBuilder()
                                     .WithResponseCode(response_code.OK)
                                     .AddMatchStatus(m => m
                                                         .WithId(0)
                                                         .WithDescription("Nem kezdődött el")
                                                         .WithSports(s => s.WithAll().Build()))
                                     .AddMatchStatus(m => m
                                                         .WithId(1)
                                                         .WithDescription("1. harmad")
                                                         .WithPeriodNumber(1)
                                                         .WithSports(s => s
                                                                         .AddSport(sp => sp.WithId("sr:sport:2"))
                                                                         .AddSport(sp => sp.WithId("sr:sport:4"))
                                                                         .AddSport(sp => sp.WithId("sr:sport:7"))
                                                                         .AddSport(sp => sp.WithId("sr:sport:13"))
                                                                         .AddSport(sp => sp.WithId("sr:sport:24"))
                                                                         .AddSport(sp => sp.WithId("sr:sport:32"))
                                                                         .AddSport(sp => sp.WithId("sr:sport:34"))
                                                                         .AddSport(sp => sp.WithId("sr:sport:131"))
                                                                         .AddSport(sp => sp.WithId("sr:sport:153"))
                                                                         .AddSport(sp => sp.WithId("sr:sport:157"))
                                                                         .AddSport(sp => sp.WithId("sr:sport:195"))
                                                                         .Build()
                                                                    ))
                                     .Build();

        return matchStatusDescriptions;
    }

    public static match_status_descriptions GetMatchStatusDescriptionForStatus01Nl()
    {
        var matchStatusDescriptions = new MatchStatusDescriptionsBuilder()
                                     .WithResponseCode(response_code.OK)
                                     .AddMatchStatus(m => m
                                                         .WithId(0)
                                                         .WithDescription("Niet begonnen")
                                                         .WithSports(s => s.WithAll().Build()))
                                     .AddMatchStatus(m => m
                                                         .WithId(1)
                                                         .WithDescription("1e periode")
                                                         .WithPeriodNumber(1)
                                                         .WithSports(s => s
                                                                         .AddSport(sp => sp.WithId("sr:sport:2"))
                                                                         .AddSport(sp => sp.WithId("sr:sport:4"))
                                                                         .AddSport(sp => sp.WithId("sr:sport:7"))
                                                                         .AddSport(sp => sp.WithId("sr:sport:13"))
                                                                         .AddSport(sp => sp.WithId("sr:sport:24"))
                                                                         .AddSport(sp => sp.WithId("sr:sport:32"))
                                                                         .AddSport(sp => sp.WithId("sr:sport:34"))
                                                                         .AddSport(sp => sp.WithId("sr:sport:131"))
                                                                         .AddSport(sp => sp.WithId("sr:sport:153"))
                                                                         .AddSport(sp => sp.WithId("sr:sport:157"))
                                                                         .AddSport(sp => sp.WithId("sr:sport:195"))
                                                                         .Build()
                                                                    ))
                                     .Build();

        return matchStatusDescriptions;
    }
}
