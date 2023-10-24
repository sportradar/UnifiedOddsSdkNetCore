/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using Dawn;
using Sportradar.OddsFeed.SDK.Messages.Rest;

namespace Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Dto
{
    /// <summary>
    /// A data-transfer-object for manager
    /// </summary>
    internal class CarDto
    {
        public string Name { get; }

        public string Chassis { get; }

        public string EngineName { get; }

        public CarDto(car item)
        {
            Guard.Argument(item, nameof(item)).NotNull();

            Name = item.name;
            Chassis = item.chassis;
            EngineName = item.engine_name;
        }
    }
}
