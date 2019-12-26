/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using Dawn;
using Sportradar.OddsFeed.SDK.Messages.REST;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Internal.DTO
{
    /// <summary>
    /// A data-transfer-object for manager
    /// </summary>
    public class CarDTO
    {
        public string Name { get; }

        public string Chassis { get; }

        public string EngineName { get; }

        public CarDTO(car item)
        {
            Guard.Argument(item, nameof(item)).NotNull();

            Name = item.name;
            Chassis = item.chassis;
            EngineName = item.engine_name;
        }
    }
}
