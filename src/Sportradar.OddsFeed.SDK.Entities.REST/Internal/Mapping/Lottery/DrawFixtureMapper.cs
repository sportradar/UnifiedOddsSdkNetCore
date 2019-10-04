/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System.Diagnostics.Contracts;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.DTO.Lottery;
using Sportradar.OddsFeed.SDK.Messages.REST;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Internal.Mapping.Lottery
{
    /// <summary>
    /// A <see cref="ISingleTypeMapper{T}" /> used to map <see cref="draw_fixtures" /> instances to <see cref="DrawDTO" /> instances
    /// </summary>
    /// <seealso cref="ISingleTypeMapper{DrawDTO}" />
    internal class DrawFixtureMapper : ISingleTypeMapper<DrawDTO>
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
            Contract.Requires(drawFixture != null);

            _drawFixture = drawFixture;
        }

        public DrawDTO Map()
        {
            return new DrawDTO(_drawFixture);
        }
    }
}
