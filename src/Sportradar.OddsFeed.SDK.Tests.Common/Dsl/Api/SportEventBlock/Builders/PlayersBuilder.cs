// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Messages.Rest;

namespace Sportradar.OddsFeed.SDK.Tests.Common.Dsl.Api.SportEventBlock.Builders;

// <player type="goalkeeper" date_of_birth="1997-03-04" nationality="England" country_code="ENG" height="188" weight="83" full_name="Frederick John Woodman" gender="male" id="sr:player:284417" name="Woodman, Freddie"/>
public class PlayerBuilder
{
    private string _id;
    private string _name;
    private string _type;
    private string _dateOfBirth;
    private string _nationality;
    private string _countryCode;
    private int _height;
    private int _weight;
    private string _fullName;
    private string _gender;
    private string _nickname;
    private int _jerseyNumber;

    public PlayerBuilder WithId(Urn id)
    {
        _id = id.ToString();
        return this;
    }

    public PlayerBuilder WithName(string name)
    {
        _name = name;
        return this;
    }

    public PlayerBuilder WithType(string type)
    {
        _type = type;
        return this;
    }

    public PlayerBuilder WithDateOfBirth(string dateOfBirth)
    {
        _dateOfBirth = dateOfBirth;
        return this;
    }

    public PlayerBuilder WithNationality(string nationality)
    {
        _nationality = nationality;
        return this;
    }

    public PlayerBuilder WithCountryCode(string countryCode)
    {
        _countryCode = countryCode;
        return this;
    }

    public PlayerBuilder WithHeight(int height)
    {
        _height = height;
        return this;
    }

    public PlayerBuilder WithWeight(int weight)
    {
        _weight = weight;
        return this;
    }

    public PlayerBuilder WithFullName(string fullName)
    {
        _fullName = fullName;
        return this;
    }

    public PlayerBuilder IsMale(bool isMale = true)
    {
        _gender = isMale ? "male" : "female";
        return this;
    }

    public PlayerBuilder WithNickname(string nickname)
    {
        _nickname = nickname;
        return this;
    }

    public PlayerBuilder WithJerseyNumber(int jerseyNumber)
    {
        _jerseyNumber = jerseyNumber;
        return this;
    }

    public player Build()
    {
        return new player
        {
            id = _id,
            name = _name
        };
    }

    public playerExtended BuildExtended()
    {
        return new playerExtended
        {
            id = _id,
            name = _name,
            type = _type,
            date_of_birth = _dateOfBirth,
            nationality = _nationality,
            country_code = _countryCode,
            height = _height,
            heightSpecified = _height > 0,
            weight = _weight,
            weightSpecified = _weight > 0,
            full_name = _fullName,
            gender = _gender,
            nickname = _nickname,
            jersey_number = _jerseyNumber,
            jersey_numberSpecified = _jerseyNumber > 0
        };
    }
}
