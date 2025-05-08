// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System.Collections.Generic;
using Sportradar.OddsFeed.SDK.Messages.Rest;

namespace Sportradar.OddsFeed.SDK.Tests.Common.Dsl.Api.SportEventBlock.Builders;

// <reference_ids>
//   <reference_id name="BetradarCtrl" value="144965313"/>
//   <reference_id name="betradar" value="8882"/>
// </reference_ids>
public class ReferencesBuilder
{
    private readonly List<referenceIdsReference_id> _refs = [];

    public ReferencesBuilder WithReference(string key, string value)
    {
        _refs.Add(new referenceIdsReference_id
        {
            name = key,
            value = value
        });
        return this;
    }

    public ReferencesBuilder WithBetradarCtrl(int value)
    {
        WithReference("BetradarCtrl", value.ToString());
        return this;
    }

    public ReferencesBuilder WithBetradar(int value)
    {
        WithReference("betradar", value.ToString());
        return this;
    }

    public ReferencesBuilder WithBetfair(int value)
    {
        WithReference("betfair", value.ToString());
        return this;
    }

    public ReferencesBuilder WithRotationNumber(int value)
    {
        WithReference("rotation_number", value.ToString());
        return this;
    }

    public ReferencesBuilder WithAams(int value)
    {
        WithReference("aams", value.ToString());
        return this;
    }

    public ReferencesBuilder WithLugas(string value)
    {
        WithReference("lugas", value);
        return this;
    }

    public referenceIdsReference_id[] BuildForSportEvent()
    {
        return _refs.ToArray();
    }

    public competitorReferenceIdsReference_id[] BuildForCompetitor()
    {
        return _refs.ConvertAll(r => new competitorReferenceIdsReference_id
        {
            name = r.name,
            value = r.value
        })
                    .ToArray();
    }
}
