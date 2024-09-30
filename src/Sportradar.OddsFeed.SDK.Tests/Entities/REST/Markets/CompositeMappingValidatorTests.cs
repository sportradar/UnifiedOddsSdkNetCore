// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using FluentAssertions;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal;
using Xunit;

namespace Sportradar.OddsFeed.SDK.Tests.Entities.Rest.Markets;

public class CompositeMappingValidatorTests
{
    private readonly IMappingValidatorFactory _factory = new MappingValidatorFactory();

    [Fact]
    public void TrueIsReturnedWhenAllValidatorsReturnTrue()
    {
        var validator = _factory.Build("setnr=1|gamenr=1|total~*.5");

        Assert.True(validator.Validate(FeedMapperHelper.GetValidForAttributes("setnr=1|gamenr=1|total=1.5")));
    }

    [Fact]
    public void FalseIsReturnedWhenAnyValidatorReturnsFalse()
    {
        var validator = _factory.Build("setnr=1|gamenr=1|total~*.5");

        Assert.False(validator.Validate(FeedMapperHelper.GetValidForAttributes("setnr=2|gamenr=1|total=1.5")));
        Assert.False(validator.Validate(FeedMapperHelper.GetValidForAttributes("setnr=1|gamenr=2|total=1.5")));
        Assert.False(validator.Validate(FeedMapperHelper.GetValidForAttributes("setnr=1|gamenr=1|total=1.75")));
    }

    [Fact]
    public void MissingSpecifierCausesException()
    {
        var validator = _factory.Build("setnr=1|total~*.75");

        Action action = () => validator.Validate(FeedMapperHelper.GetValidForAttributes("setnr=1"));
        action.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void SpecifierWithNonDecimalValueCausesException()
    {
        var validator = _factory.Build("setnr=1|total~*.75");

        Action action = () => validator.Validate(FeedMapperHelper.GetValidForAttributes("setnr=1|total=zero"));
        action.Should().Throw<InvalidOperationException>();
    }
}
