/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using Sportradar.OddsFeed.SDK.Messages.Rest;

namespace Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Dto
{
    /// <summary>
    /// A data-transfer-object for competitor result
    /// </summary>
    internal class CompetitorResultDto
    {
        public string Type { get; }

        public string Value { get; }

        public string Specifiers { get; }

        internal CompetitorResultDto(stageResultCompetitorResult result)
        {
            if (result == null)
            {
                throw new ArgumentNullException(nameof(result));
            }

            Type = result.type;
            Value = result.value;
            Specifiers = result.specifiers;
        }

        internal CompetitorResultDto(periodStatusCompetitorResult result)
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
