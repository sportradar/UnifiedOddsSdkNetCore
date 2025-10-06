// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using Dawn;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Dto.Lottery;
using Sportradar.OddsFeed.SDK.Messages.Rest;

namespace Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Mapping.Lottery
{
    /// <summary>
    /// A <see cref="ISingleTypeMapper{T}" /> used to map <see cref="draw_fixtures" /> instances to <see cref="DrawDto" /> instances
    /// </summary>
    /// <seealso cref="ISingleTypeMapper{DrawDto}" />
    internal class DrawFixtureMapper : ISingleTypeMapper<DrawDto>
    {
        /// <summary>
        /// A <see cref="draw_fixture"/> containing rest data
        /// </summary>
        private readonly draw_fixtures _drawFixture;

        /// <summary>
        /// Initializes a new instance of the <see cref="DrawFixtureMapper"/> class
        /// </summary>
        /// <param name="drawFixture">A <see cref="draw_fixtures"/> containing lottery draw data</param>
        internal DrawFixtureMapper(draw_fixtures drawFixture)
        {
            Guard.Argument(drawFixture, nameof(drawFixture)).NotNull();

            _drawFixture = drawFixture;
        }

        public DrawDto Map()
        {
            return new DrawDto(_drawFixture);
        }
    }
}
