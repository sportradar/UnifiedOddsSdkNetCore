// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using Dawn;
using Sportradar.OddsFeed.SDK.Messages.Rest;

namespace Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Dto
{
    /// <summary>
    /// A data-transfer-object for outcome description
    /// </summary>
    internal class OutcomeDescriptionDto
    {
        internal string Id { get; }

        internal string Name { get; }

        internal string Description { get; }

        internal OutcomeDescriptionDto(desc_outcomesOutcome outcome)
        {
            Guard.Argument(outcome, nameof(outcome)).NotNull();
            Guard.Argument(outcome.id, nameof(outcome.id)).NotNull().NotEmpty();

            Id = outcome.id;
            Name = outcome.name;
            Description = outcome.description;
        }

        internal OutcomeDescriptionDto(desc_variant_outcomesOutcome outcome)
        {
            Guard.Argument(outcome, nameof(outcome)).NotNull();
            Guard.Argument(outcome.id, nameof(outcome.id)).NotNull().NotEmpty();

            Id = outcome.id;
            Name = outcome.name;
            Description = null;
        }
    }
}
