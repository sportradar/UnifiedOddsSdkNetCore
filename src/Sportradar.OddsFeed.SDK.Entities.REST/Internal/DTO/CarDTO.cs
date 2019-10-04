/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System.Diagnostics.Contracts;
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
            Contract.Requires(item != null);

            Name = item.name;
            Chassis = item.chassis;
            EngineName = item.engine_name;
        }
    }
}
