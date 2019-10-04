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
    public class ManagerDTO : SportEntityDTO
    {
        public string Nationality { get; }

        public string CountryCode { get; }

        public ManagerDTO(manager item)
            : base(item.id, item.name)
        {
            Contract.Requires(item != null);

            Nationality = item.nationality;
            CountryCode = item.country_code;
        }
    }
}
