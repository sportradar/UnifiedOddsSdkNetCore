/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using Sportradar.OddsFeed.SDK.Messages.REST;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Internal.DTO
{
    /// <summary>
    /// A data-access-object representing a hole (used in golf course)
    /// </summary>
    internal class HoleDTO
    {
        internal int Number { get; }

        internal int Par { get; }

        internal HoleDTO(hole hole)
        {
            Number = hole.number;
            Par = hole.par;
        }
    }
}
