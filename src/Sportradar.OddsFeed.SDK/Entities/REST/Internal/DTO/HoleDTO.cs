// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using Sportradar.OddsFeed.SDK.Messages.Rest;

namespace Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Dto
{
    /// <summary>
    /// A data-access-object representing a hole (used in golf course)
    /// </summary>
    internal class HoleDto
    {
        internal int Number { get; }

        internal int Par { get; }

        internal HoleDto(hole hole)
        {
            Number = hole.number;
            Par = hole.par;
        }
    }
}
