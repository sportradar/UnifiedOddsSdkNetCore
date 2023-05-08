using System.Collections.Generic;
using System.Globalization;
using Sportradar.OddsFeed.SDK.Entities.REST;
using Sportradar.OddsFeed.SDK.Messages;

namespace Sportradar.OddsFeed.SDK.Tests.Common
{
    public class TestTeamCompetitor : TestCompetitor, ITeamCompetitor
    {
        public TestTeamCompetitor(URN id, string name, CultureInfo culture)
            : base(id, name, culture)
        {
        }

        public TestTeamCompetitor(URN id, IDictionary<CultureInfo, string> names)
            : base(id, names)
        {
        }

        /// <inheritdoc />
        public string Qualifier { get; }

        /// <inheritdoc />
        public int? Division { get; }
    }
}
