/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using Dawn;
using Sportradar.OddsFeed.SDK.Messages.REST;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Internal.DTO
{
    /// <summary>
    /// A data-transfer-object for outcome description
    /// </summary>
    public class OutcomeDescriptionDTO
    {
        internal string Id { get; }

        internal string Name { get; }

        internal string Description { get; }

        internal OutcomeDescriptionDTO(desc_outcomesOutcome outcome)
        {
            Guard.Argument(outcome, nameof(outcome)).NotNull();
            Guard.Argument(outcome.id, nameof(outcome.id)).NotNull().NotEmpty();
            Guard.Argument(outcome.name, nameof(outcome.name)).NotNull().NotEmpty();

            Id = outcome.id;
            Name = outcome.name;
            Description = outcome.description;
        }

        internal OutcomeDescriptionDTO(desc_variant_outcomesOutcome outcome)
        {
            Guard.Argument(outcome, nameof(outcome)).NotNull();
            Guard.Argument(outcome.id, nameof(outcome.id)).NotNull().NotEmpty();
            Guard.Argument(outcome.name, nameof(outcome.name)).NotNull().NotEmpty();

            Id = outcome.id;
            Name = outcome.name;
            Description = null;
        }
    }
}