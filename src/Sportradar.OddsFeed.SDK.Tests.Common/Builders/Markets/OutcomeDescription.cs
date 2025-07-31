// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System.Collections.Generic;
using System.Globalization;
using Sportradar.OddsFeed.SDK.Entities.Rest.Market;
using Sportradar.OddsFeed.SDK.Messages.Rest;

namespace Sportradar.OddsFeed.SDK.Tests.Common.Builders.Markets;

internal class OutcomeDescription : IOutcomeDescription
{
    private readonly Dictionary<CultureInfo, string> _names;
    private readonly Dictionary<CultureInfo, string> _descriptions;

    public string Id
    {
        get;
    }

    public OutcomeDescription(desc_outcomesOutcome outcome, CultureInfo culture)
    {
        Id = outcome.id;
        _names = new Dictionary<CultureInfo, string>
                     {
                         { culture, outcome.name }
                     };
        _descriptions = new Dictionary<CultureInfo, string>
                            {
                                { culture, outcome.description }
                            };
    }

    public OutcomeDescription(string id,
                              Dictionary<CultureInfo, string> names,
                              Dictionary<CultureInfo, string> descriptions)
    {
        Id = id;
        _names = names;
        _descriptions = descriptions;
    }

    public string GetName(CultureInfo culture)
    {
        return _names.GetValueOrDefault(culture);
    }

    public string GetDescription(CultureInfo culture)
    {
        return _descriptions.GetValueOrDefault(culture);
    }
}
