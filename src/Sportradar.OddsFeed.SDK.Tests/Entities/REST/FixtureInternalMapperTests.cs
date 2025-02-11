﻿// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System.Collections.Generic;
using System.Linq;
using Sportradar.OddsFeed.SDK.Common.Internal;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Dto;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Mapping;
using Sportradar.OddsFeed.SDK.Messages.Rest;
using Sportradar.OddsFeed.SDK.Tests.Common;
using Xunit;

namespace Sportradar.OddsFeed.SDK.Tests.Entities.Rest;

public class FixtureInternalMapperTests
{
    private const string InputXml = "fixtures_en.xml";

    private readonly IDeserializer<fixturesEndpoint> _deserializer = new Deserializer<fixturesEndpoint>();
    private readonly fixturesEndpoint _record;
    private readonly FixtureDto _entity;

    public FixtureInternalMapperTests()
    {
        _record = _deserializer.Deserialize(FileHelper.OpenFile(TestData.RestXmlPath, InputXml));
        _entity = new FixtureMapperFactory().CreateMapper(_record).Map();
    }

    [Fact]
    public void MappedRecordIsNotNull()
    {
        Assert.NotNull(_entity);
        Assert.Equal(2, _entity.Competitors.ToList().Count);
    }

    [Fact]
    public void Mapping()
    {
        Assert.Equal(_entity.Id.ToString(), _record.fixture.id);

        Assert.Equal(_entity.Scheduled, _record.fixture.scheduledSpecified ? _record.fixture.scheduled : null);
        Assert.Equal(_entity.StartTime, _record.fixture.start_time);
        Assert.Equal(_entity.NextLiveTime, SdkInfo.ParseDate(_record.fixture.next_live_time));
        Assert.Equal(_entity.StartTimeTbd, _record.fixture.start_time_tbdSpecified ? _record.fixture.start_time_tbd : null);
        Assert.Equal(_entity.StartTimeConfirmed, _record.fixture.start_time_confirmed);

        Assert.Equal(_entity.Season.Id.ToString(), _record.fixture.season.id);
        Assert.Equal(_entity.Season.Name, _record.fixture.season.name);

        TestCompetitors(_entity.Competitors.ToList(), _record.fixture.competitors);

        MappingExtraInfo();

        MappingProductInfo();

        MappingTvChannels();

        MappingTournament();

        MappingTournamentRound();

        MappingVenue();
    }

    [Fact]
    public void MappingTournament()
    {
        Assert.Equal(_entity.Tournament.Id.ToString(), _record.fixture.tournament.id);
        Assert.Equal(_entity.Tournament.Name, _record.fixture.tournament.name);

        Assert.Equal(_entity.Tournament.Sport.Id.ToString(), _record.fixture.tournament.sport.id);
        Assert.Equal(_entity.Tournament.Sport.Name, _record.fixture.tournament.sport.name);

        Assert.Equal(_entity.Tournament.Category.Id.ToString(), _record.fixture.tournament.category.id);
        Assert.Equal(_entity.Tournament.Category.Name, _record.fixture.tournament.category.name);
    }

    [Fact]
    public void MappingTournamentRound()
    {
        Assert.Equal(_entity.Round.Name, _record.fixture.tournament_round.name);
        Assert.Equal(_entity.Round.Type, _record.fixture.tournament_round.type);
        Assert.Equal(_entity.Round.CupRoundMatchNumber, _record.fixture.tournament_round.cup_round_match_numberSpecified ? _record.fixture.tournament_round.cup_round_match_number : null);
        Assert.Equal(_entity.Round.CupRoundMatches, _record.fixture.tournament_round.cup_round_matchesSpecified ? _record.fixture.tournament_round.cup_round_matches : null);
        Assert.Equal(_entity.Round.Number, _record.fixture.tournament_round.numberSpecified ? _record.fixture.tournament_round.number : null);
    }

    [Fact]
    public void MappingVenue()
    {
        if (_record.fixture.venue == null)
        {
            return;
        }

        Assert.Equal(_entity.Venue.Id.ToString(), _record.fixture.venue.id);
        Assert.Equal(_entity.Venue.Name, _record.fixture.venue.name);
        Assert.Equal(_entity.Venue.City, _record.fixture.venue.city_name);
        Assert.Equal(_entity.Venue.Coordinates, _record.fixture.venue.map_coordinates);
        Assert.Equal(_entity.Venue.Country, _record.fixture.venue.country_name);
        Assert.Equal(_entity.Venue.State, _record.fixture.venue.state);
        Assert.Equal(_entity.Venue.Capacity, _record.fixture.venue.capacitySpecified ? _record.fixture.venue.capacity : null);
    }

    [Fact]
    public void MappingTvChannels()
    {
        if (_record.fixture.tv_channels == null)
        {
            return;
        }

        foreach (var i in _record.fixture.tv_channels)
        {
            var j = _entity.TvChannels.FirstOrDefault(x => x.Name == i.name);
            Assert.NotNull(j);
            Assert.Equal(j.Name, i.name);
            Assert.Equal(j.StartTime, i.start_timeSpecified ? i.start_time : null);
        }
    }

    [Fact]
    public void MappingProductInfo()
    {
        if (_record.fixture.product_info == null)
        {
            return;
        }

        if (_record.fixture.product_info.links != null)
        {
            foreach (var i in _record.fixture.product_info.links)
            {
                var j = _entity.ProductInfo.ProductInfoLinks.FirstOrDefault(x => x.Name == i.name);
                Assert.NotNull(j);
                Assert.Equal(j.Reference, i.@ref);
            }
        }
        if (_record.fixture.product_info.streaming != null)
        {
            foreach (var i in _record.fixture.product_info.streaming)
            {
                var j = _entity.ProductInfo.StreamingChannels.FirstOrDefault(x => x.Id == i.id);
                Assert.NotNull(j);
                Assert.Equal(j.Name, i.name);
            }
        }
    }

    [Fact]
    public void MappingExtraInfo()
    {
        Assert.Equal(_entity.ExtraInfo.Count, _record.fixture.extra_info.Length);
        foreach (var ei in _record.fixture.extra_info)
        {
            var extraInfoValue = _entity.ExtraInfo[ei.key];
            Assert.Equal(extraInfoValue, ei.value);
        }
    }

    private static void TestCompetitors(IReadOnlyCollection<TeamCompetitorDto> competitors, IReadOnlyCollection<teamCompetitor> comps)
    {
        Assert.Equal(competitors.Count, comps.Count);
        foreach (var c in comps)
        {
            var m = competitors.FirstOrDefault(x => x.Id.ToString() == c.id);
            Assert.NotNull(m);
            Assert.Equal(m.Id.ToString(), c.id);
            Assert.Equal(m.Qualifier, c.qualifier);
            Assert.Equal(m.Name, c.name);
            Assert.Equal(m.Abbreviation, c.abbreviation);
            Assert.Equal(m.CountryName, c.country);
            Assert.Equal(m.State, c.state);
            if (c.virtualSpecified)
            {
                Assert.Equal(m.IsVirtual, c.@virtual);
            }
            else
            {
                Assert.False(m.IsVirtual);
            }

            if (c.divisionSpecified)
            {
                Assert.NotNull(m.Division);
                Assert.Equal(m.Division.Id, c.division);
                Assert.Equal(m.Division.Name, c.division_name);
            }
            else
            {
                Assert.Null(m.Division);
            }
        }
    }
}
