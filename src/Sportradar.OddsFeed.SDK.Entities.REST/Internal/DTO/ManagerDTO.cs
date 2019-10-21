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
    public class ManagerDTO : SportEntityDTO
    {
        public string Nationality { get; }

        public string CountryCode { get; }

        public ManagerDTO(manager item)
            : base(item.id, item.name)
        {
            Guard.Argument(item).NotNull();

            Nationality = item.nationality;
            CountryCode = item.country_code;
        }
    }
}
