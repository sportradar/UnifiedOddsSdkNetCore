/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using Sportradar.OddsFeed.SDK.Messages;
using Sportradar.OddsFeed.SDK.Messages.REST;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Internal.DTO
{
    /// <summary>
    /// A data-transfer-object for competitor result
    /// </summary>
    internal class CompetitorResultDTO
    {
        public string Type { get; }

        public string Value { get; }

        public string Specifiers { get; }

        internal CompetitorResultDTO(stageResultCompetitorResult result)
        {
            if (result == null)
            {
                throw new ArgumentNullException(nameof(result));
            }

            Type = result.type;
            Value = result.value;
            Specifiers = result.specifiers;
        }
    }
}
