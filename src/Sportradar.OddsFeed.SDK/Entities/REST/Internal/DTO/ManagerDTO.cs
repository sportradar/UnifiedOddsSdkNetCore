// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using Sportradar.OddsFeed.SDK.Messages.Rest;

namespace Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Dto
{
    /// <summary>
    /// A data-transfer-object for manager
    /// </summary>
    internal class ManagerDto : SportEntityDto
    {
        public string Nationality { get; }

        public string CountryCode { get; }

        public ManagerDto(manager item)
            : base(item.id, item.name)
        {
            Nationality = item.nationality;
            CountryCode = item.country_code;
        }
    }
}
