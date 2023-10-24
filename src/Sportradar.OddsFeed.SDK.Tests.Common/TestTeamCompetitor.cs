using System.Collections.Generic;
using System.Globalization;
using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Entities.Rest;

namespace Sportradar.OddsFeed.SDK.Tests.Common;

public class TestTeamCompetitor : TestCompetitor, ITeamCompetitor
{
    public TestTeamCompetitor(Urn id, string name, CultureInfo culture)
        : base(id, name, culture)
    {
    }

    public TestTeamCompetitor(Urn id, IDictionary<CultureInfo, string> names)
        : base(id, names)
    {
    }

    /// <inheritdoc />
    public string Qualifier { get; }
}
