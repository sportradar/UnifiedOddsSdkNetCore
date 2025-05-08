// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using System.Linq;
using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Messages.Rest;

namespace Sportradar.OddsFeed.SDK.Tests.Common.Dsl.Api.Endpoints;

public class SportCategoriesEndpoint
{
    private readonly sportCategoriesEndpoint _endpoint = new sportCategoriesEndpoint();

    public SportCategoriesEndpoint ForSport(Urn sportId, string name)
    {
        _endpoint.sport = new sport
        {
            id = sportId.ToString(),
            name = name
        };
        return this;
    }

    public SportCategoriesEndpoint AddCategory(Urn categoryId, string name, string countryCode)
    {
        _endpoint.categories ??= [];
        var list = _endpoint.categories.ToList();
        list.Add(new category
        {
            id = categoryId.ToString(),
            name = name,
            country_code = countryCode
        });
        _endpoint.categories = list.ToArray();
        return this;
    }

    public sportCategoriesEndpoint Build()
    {
        _endpoint.generated_at = DateTime.UtcNow;
        _endpoint.generated_atSpecified = true;
        return _endpoint;
    }
}
