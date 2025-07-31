// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System.Collections.Generic;
using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Messages.Rest;
using Sportradar.OddsFeed.SDK.Tests.Common.Extensions;

namespace Sportradar.OddsFeed.SDK.Tests.Common.Dsl.Api.SportEventBlock.Builders;

// <competitor qualifier="home" id="sr:competitor:3853" name="Adler Mannheim" abbreviation="MAN" short_name="Mannheim" country="Germany" country_code="DEU" gender="male">
//   <reference_ids>
//     <reference_id name="betradar" value="8882"/>
//   </reference_ids>
// </competitor>
public class CompetitorBuilder
{
    private Urn _id;
    private string _name;
    private string _abbreviation;
    private string _shortName;
    private string _country;
    private string _countryCode;
    private string _state;
    private bool? _isMaleGender;
    private bool? _isHomeQualifier;
    private bool? _isVirtual;
    private int? _divisionId;
    private string _divisionName;
    private string _ageGroup;
    private sport _sport;
    private category _category;
    private ReferencesBuilder _referencesBuilder;
    private List<playerCompetitor> _players = [];

    public CompetitorBuilder WithId(Urn id)
    {
        _id = id;
        return this;
    }

    public CompetitorBuilder WithId(string id)
    {
        _id = id.ToUrn();
        return this;
    }

    public CompetitorBuilder WithName(string name)
    {
        _name = name;
        return this;
    }

    public CompetitorBuilder WithAbbreviation(string abbreviation)
    {
        _abbreviation = abbreviation;
        return this;
    }

    public CompetitorBuilder WithShortName(string shortName)
    {
        _shortName = shortName;
        return this;
    }

    public CompetitorBuilder WithCountry(string country)
    {
        _country = country;
        return this;
    }

    public CompetitorBuilder WithCountryCode(string countryCode)
    {
        _countryCode = countryCode;
        return this;
    }

    public CompetitorBuilder WithState(string state)
    {
        _state = state;
        return this;
    }

    public CompetitorBuilder IsMale(bool isMale = true)
    {
        _isMaleGender = isMale;
        return this;
    }

    public CompetitorBuilder IsHome(bool isHome)
    {
        _isHomeQualifier = isHome;
        return this;
    }

    public CompetitorBuilder IsVirtual(bool isVirtual)
    {
        _isVirtual = isVirtual;
        return this;
    }

    public CompetitorBuilder WithDivision(int divisionId, string divisionName)
    {
        _divisionId = divisionId;
        _divisionName = divisionName;
        return this;
    }

    public CompetitorBuilder WithAgeGroup(string ageGroup)
    {
        _ageGroup = ageGroup;
        return this;
    }

    public CompetitorBuilder WithReferences(ReferencesBuilder referencesBuilder)
    {
        _referencesBuilder = referencesBuilder;
        return this;
    }

    public CompetitorBuilder WithSport(Urn sportId, string name)
    {
        _sport = new sport
        {
            id = sportId.ToString(),
            name = name
        };
        return this;
    }

    public CompetitorBuilder WithCategory(Urn categoryId, string name, string countryCode)
    {
        _category = new category
        {
            id = categoryId.ToString(),
            name = name,
            country_code = countryCode
        };
        return this;
    }

    public team Build()
    {
        var team = new team();
        UpdateTeamFields(team);

        return team;
    }

    public teamCompetitor BuildTeamCompetitor()
    {
        var team = new teamCompetitor();
        UpdateTeamFields(team);

        if (_isHomeQualifier != null)
        {
            team.qualifier = _isHomeQualifier.Value ? "home" : "away";
        }

        return team;
    }

    public teamExtended BuildTeamExtended()
    {
        var team = new teamExtended();
        UpdateTeamFields(team);
        team.sport = _sport;
        team.category = _category;

        return team;
    }

    private void UpdateTeamFields(team team)
    {
        team.id = _id.ToString();
        team.name = _name;
        team.abbreviation = _abbreviation;
        team.state = _state;
        team.short_name = _shortName;
        team.country = _country;
        team.country_code = _countryCode;
        team.age_group = _ageGroup;

        if (_isVirtual != null)
        {
            team.@virtual = _isVirtual.Value;
            team.virtualSpecified = true;
        }
        if (_isMaleGender != null)
        {
            team.gender = _isMaleGender.Value ? "male" : "female";
        }
        if (_divisionId != null)
        {
            team.division = _divisionId.Value;
            team.division_name = _divisionName;
            team.divisionSpecified = true;
        }
        if (_referencesBuilder != null)
        {
            team.reference_ids = _referencesBuilder.BuildForCompetitor();
        }
    }
}
