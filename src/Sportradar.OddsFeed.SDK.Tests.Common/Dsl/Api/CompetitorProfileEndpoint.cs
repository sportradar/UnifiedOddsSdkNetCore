// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Dto;
using Sportradar.OddsFeed.SDK.Messages.Rest;
using SR = Sportradar.OddsFeed.SDK.Tests.Common.StaticRandom;

namespace Sportradar.OddsFeed.SDK.Tests.Common.Dsl.Api;

[SuppressMessage("ReSharper", "UnusedMember.Global")]
public class CompetitorProfileEndpoint
{
    public competitorProfileEndpoint Raw { get; } = BuildCompetitorProfileEndpoint(1);
    internal CompetitorProfileDto Dto => new(Raw);

    public static CompetitorProfileEndpoint AsCompetitorProfile => new();
    public static ApiPlayerExtended AsPlayerExtended => new();
    public static SimpleTeamProfileEndpoint AsSimpleTeam => new();

    public CompetitorProfileEndpoint Id(int id)
    {
        Raw.competitor.id = "sr:competitor:" + id;
        Raw.competitor.name = "Competitor " + id;
        return this;
    }

    public CompetitorProfileEndpoint ClearPlayers()
    {
        Raw.competitor.players = null;
        return this;
    }

    public CompetitorProfileEndpoint AddCompetitorPlayer(int id)
    {
        if (Raw.competitor.players == null)
        {
            Raw.competitor.players = new[] { BuildPlayerCompetitor(id) };
            return this;
        }

        var existingPlayers = Raw.competitor.players.ToList();
        existingPlayers.Add(BuildPlayerCompetitor(id));
        Raw.competitor.players = existingPlayers.ToArray();
        return this;
    }

    public CompetitorProfileEndpoint AddCompetitorPlayers(int startId, int size)
    {
        for (var i = startId; i < startId + size; i++)
        {
            AddCompetitorPlayer(i);
        }

        return this;
    }

    public CompetitorProfileEndpoint Lang(CultureInfo culture)
    {
        Raw.competitor.name = Raw.competitor.name + " " + culture.TwoLetterISOLanguageName;
        foreach (var player in Raw.competitor.players)
        {
            player.name += " " + culture.TwoLetterISOLanguageName;
        }

        foreach (var player in Raw.players)
        {
            player.name += " " + culture.TwoLetterISOLanguageName;
        }

        return this;
    }

    private static competitorProfileEndpoint BuildCompetitorProfileEndpoint(int id = 0, int playerCount = 0, IDictionary<string, string> referenceIds = null)
    {
        if (playerCount == -1)
        {
            playerCount = SR.I(20);
        }

        var players = new List<playerExtended>();
        for (var j = 0; j < playerCount; j++)
        {
            players.Add(BuildPlayerExtended(j + 1));
        }

        var cUrn = id == 0 ? SR.Urn("competitor", 100000) : SR.Urn(id, "competitor");

        return new competitorProfileEndpoint { competitor = BuildTeamExtended((int)cUrn.Id, referenceIds), generated_at = DateTime.Now, generated_atSpecified = true, players = players.ToArray() };
    }

    private static simpleTeamProfileEndpoint BuildSimpleTeamProfileEndpoint(int id = 0, IDictionary<string, string> referenceIds = null)
    {
        return new simpleTeamProfileEndpoint { competitor = BuildTeam(id, referenceIds), generated_at = DateTime.Now, generated_atSpecified = true };
    }

    public static teamExtended BuildTeamExtended(int id = 0, IDictionary<string, string> referenceIds = null)
    {
        var teamUrn = id == 0 ? SR.Urn("competitor", 1000) : SR.Urn(id, "competitor");
        return new teamExtended
        {
            id = teamUrn.ToString(), name = "Team " + teamUrn.Id, abbreviation = "T" + teamUrn.Id, @virtual = true,
            virtualSpecified = false, country = SR.S1000, state = "PA", reference_ids = referenceIds?.Select(s => new competitorReferenceIdsReference_id { name = s.Key, value = s.Value })
                                                                                                            .ToArray(),
            sport = new sport { id = "sr:sport:1", name = "Soccer" }
        };
    }

    private static team BuildTeam(int id = 0, IDictionary<string, string> referenceIds = null)
    {
        return BuildTeamExtended(id, referenceIds);
    }

    public static playerCompetitor BuildPlayerCompetitor(int id = 0)
    {
        var playerUrn = id == 0 ? SR.Urn("player") : SR.Urn(id, "player");
        return new playerCompetitor { id = playerUrn.ToString(), name = "Player " + playerUrn.Id, abbreviation = "P" + playerUrn.Id, nationality = "nat " + SR.S1000 };
    }

    public static playerExtended BuildPlayerExtended(int id = 0)
    {
        var newId = id == 0 ? SR.I1000 : id;
        return new playerExtended
        {
            id = SR.Urn(id, "player").ToString(),
            name = "Player " + newId,
            weight = SR.I(50, 150),
            weightSpecified = true,
            height = SR.I(100, 200),
            heightSpecified = true,
            jersey_number = SR.I(100),
            jersey_numberSpecified = true,
            nationality = "nat " + SR.S1000,
            type = SR.S100,
            date_of_birth = DateTime.Today.AddYears(-SR.I(20, 40)).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
            country_code = "ENG",
            full_name = $"Player{newId} Surname",
            gender = SR.B ? "male" : "female",
            nickname = SR.S10000P
        };
    }

    public static Collection<playerExtended> BuildPlayerExtendedList(int limit = 10)
    {
        var players = new Collection<playerExtended>();
        for (var i = 0; i < limit; i++)
        {
            players.Add(BuildPlayerExtended(i + 1));
        }
        return players;
    }

    public static teamCompetitor BuildTeamCompetitor(int id = 0, bool? isVirtual = null)
    {
        var references = new List<competitorReferenceIdsReference_id>();
        for (var j = 0; j < 5; j++)
        {
            var rc = BuildReferenceCompetitor();
            if (references.Find(i => i.name == rc.name) == null)
            {
                references.Add(rc);
            }
        }

        if (id == 0)
        {
            id = SR.I1000;
        }

        return new teamCompetitor
        {
            id = SR.Urn(id, "competitor").ToString(), name = "Competitor " + id, abbreviation = SR.S1000, @virtual = isVirtual ?? false,
            country = SR.S1000, virtualSpecified = isVirtual.HasValue, qualifier = SR.S1000, country_code = SR.S1000,
            reference_ids = references.ToArray(), divisionSpecified = true, division = SR.I100, state = "PA"
        };
    }

    public static ICollection<teamCompetitor> BuildTeamCompetitorList(int startId = 1, int size = 1)
    {
        var teamCompetitors = new List<teamCompetitor>();
        for (var i = startId; i < startId + size; i++)
        {
            teamCompetitors.Add(BuildTeamCompetitor(i));
        }

        return teamCompetitors;
    }

    private static competitorReferenceIdsReference_id BuildReferenceCompetitor()
    {
        return new competitorReferenceIdsReference_id { name = SR.S10000P, value = SR.S10000 };
    }

    public class ApiPlayerExtended
    {
        public playerExtended Raw { get; } = BuildPlayerExtended(1);
        internal PlayerProfileDto Dto => new(Raw, DateTime.Now);

        public ApiPlayerExtended Id(int id)
        {
            Raw.id = "sr:player:" + id;
            Raw.name = "Player " + id;
            return this;
        }

        public ApiPlayerExtended Lang(CultureInfo culture)
        {
            Raw.name = Raw.name + " " + culture.TwoLetterISOLanguageName;
            return this;
        }
    }

    public class SimpleTeamProfileEndpoint
    {
        public simpleTeamProfileEndpoint Raw { get; } = BuildSimpleTeamProfileEndpoint(1);
        internal SimpleTeamProfileDto Dto => new(Raw);

        public SimpleTeamProfileEndpoint Id(int id)
        {
            Raw.competitor.id = "sr:simpleteam:" + id;
            Raw.competitor.name = "SimpleTeam " + id;
            return this;
        }

        public SimpleTeamProfileEndpoint ClearPlayers()
        {
            Raw.competitor.players = null;
            return this;
        }

        public SimpleTeamProfileEndpoint AddCompetitorPlayer(int id)
        {
            if (Raw.competitor.players == null)
            {
                Raw.competitor.players = new[] { BuildPlayerCompetitor(id) };
                return this;
            }

            var existingPlayers = Raw.competitor.players.ToList();
            existingPlayers.Add(BuildPlayerCompetitor(id));
            Raw.competitor.players = existingPlayers.ToArray();
            return this;
        }

        public SimpleTeamProfileEndpoint AddCompetitorPlayers(int startId, int size)
        {
            for (var i = startId; i < startId + size; i++)
            {
                AddCompetitorPlayer(i);
            }

            return this;
        }

        public SimpleTeamProfileEndpoint Lang(CultureInfo culture)
        {
            Raw.competitor.name = Raw.competitor.name + " " + culture.TwoLetterISOLanguageName;
            foreach (var player in Raw.competitor.players)
            {
                player.name += " " + culture.TwoLetterISOLanguageName;
            }

            return this;
        }
    }
}
