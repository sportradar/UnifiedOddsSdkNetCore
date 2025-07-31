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

    public static readonly Urn AnyMatchId = UrnCreate.MatchId(1);
    public static readonly Urn AnyStageId = UrnCreate.StageId(1);
    public static readonly Urn AnySportId = UrnCreate.SportId(1);
    public static readonly Urn AnyCategoryId = UrnCreate.CategoryId(1);
    public static readonly Urn AnyTournamentId = UrnCreate.TournamentId(1);
    public static readonly Urn AnySimpleTournamentId = UrnCreate.SimpleTournamentId(1);
    public static readonly Urn AnySeasonId = UrnCreate.SeasonId(1);

    public static readonly CultureInfo CultureEn = new CultureInfo("en");
    public static readonly CultureInfo CultureDe = new CultureInfo("de");
    public static readonly CultureInfo CultureHu = new CultureInfo("hu");
    public static readonly CultureInfo CultureNl = new CultureInfo("nl");

    public static IReadOnlyCollection<CultureInfo> Cultures => Cultures3;
    public static IReadOnlyCollection<CultureInfo> Cultures1 => new Collection<CultureInfo> { CultureEn };
    public static readonly IReadOnlyCollection<CultureInfo> Cultures3 = new Collection<CultureInfo>([CultureEn, CultureDe, CultureHu]);
    public static readonly IReadOnlyCollection<CultureInfo> Cultures4 = new Collection<CultureInfo>([CultureEn, CultureDe, CultureHu, CultureNl]);
}
