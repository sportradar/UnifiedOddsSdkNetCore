// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using Sportradar.OddsFeed.SDK.Common;

namespace Sportradar.OddsFeed.SDK.Tests.Common.Builders;

public class UrnCreate
{
    public static Urn SportId(int id)
    {
        return Urn.Parse($"sr:sport:{id}");
    }

    public static Urn CategoryId(int id)
    {
        return Urn.Parse($"sr:category:{id}");
    }

    public static Urn TournamentId(int id)
    {
        return Urn.Parse($"sr:tournament:{id}");
    }

    public static Urn SimpleTournamentId(int id)
    {
        return Urn.Parse($"sr:simple_tournament:{id}");
    }

    public static Urn SeasonId(int id)
    {
        return Urn.Parse($"sr:season:{id}");
    }

    public static Urn MatchId(int id)
    {
        return Urn.Parse($"sr:match:{id}");
    }

    public static Urn StageId(int id)
    {
        return Urn.Parse($"sr:stage:{id}");
    }

    public static Urn CompetitorId(int id)
    {
        return Urn.Parse($"sr:competitor:{id}");
    }

    public static Urn PlayerId(int id)
    {
        return Urn.Parse($"sr:player:{id}");
    }

    public static Urn VenueId(int id)
    {
        return Urn.Parse($"sr:venue:{id}");
    }

    public static Urn GroupId(int id)
    {
        return Urn.Parse($"sr:group:{id}");
    }
}
