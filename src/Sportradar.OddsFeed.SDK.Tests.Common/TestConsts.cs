// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Tests.Common.Builders;

namespace Sportradar.OddsFeed.SDK.Tests.Common;

public static class TestConsts
{
    public const int AnyNodeId = 1;
    public const int AnyBookmakerId = 1;
    public const int AnyProducerId = 1;
    public const string AnyAccessToken = "token";
    public const string AnyVirtualHost = "/virtualhost";
    public const string AnyApiHost = "custom_api_host";
    public const string AnyRabbitHost = "custom_mq_host";

    public static readonly Urn AnyMatchId = UrnCreate.MatchId(1);
    public static readonly Urn AnyStageId = UrnCreate.StageId(2);
    public static readonly Urn AnySportId = UrnCreate.SportId(123);
    public static readonly Urn AnyCategoryId = UrnCreate.CategoryId(22);
    public static readonly Urn AnyTournamentId = UrnCreate.TournamentId(11);
    public static readonly Urn AnySimpleTournamentId = UrnCreate.SimpleTournamentId(111);
    public static readonly Urn AnySeasonId = UrnCreate.SeasonId(333);

    public static readonly CultureInfo CultureEn = new("en");
    public static readonly CultureInfo CultureDe = new("de");
    public static readonly CultureInfo CultureHu = new("hu");
    public static readonly CultureInfo CultureNl = new("nl");

    public static IReadOnlyCollection<CultureInfo> Cultures => Cultures3;
    public static IReadOnlyCollection<CultureInfo> Cultures1 => new Collection<CultureInfo> { CultureEn };
    public static readonly IReadOnlyCollection<CultureInfo> Cultures2 = new Collection<CultureInfo>([CultureEn, CultureDe]);
    public static readonly IReadOnlyCollection<CultureInfo> Cultures3 = new Collection<CultureInfo>([CultureEn, CultureDe, CultureHu]);
    public static readonly IReadOnlyCollection<CultureInfo> Cultures4 = new Collection<CultureInfo>([CultureEn, CultureDe, CultureHu, CultureNl]);
}
