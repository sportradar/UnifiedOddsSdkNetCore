/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using System.Threading.Tasks;
using Dawn;
using Sportradar.OddsFeed.SDK.Entities.Rest.Caching.Exportable;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Dto;

namespace Sportradar.OddsFeed.SDK.Entities.Rest.Internal.EntitiesImpl
{
    internal class ScheduledStartTimeChange : IScheduledStartTimeChange
    {
        /// <summary>
        /// Gets the old time
        /// </summary>
        /// <value>The old time</value>
        public DateTime OldTime { get; }

        /// <summary>
        /// Gets the new time
        /// </summary>
        /// <value>The new time</value>
        public DateTime NewTime { get; }

        /// <summary>
        /// Gets the changed at
        /// </summary>
        /// <value>The changed at</value>
        public DateTime ChangedAt { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ScheduledStartTimeChangeDto"/> class
        /// </summary>
        /// <param name="dto">The time change</param>
        public ScheduledStartTimeChange(ScheduledStartTimeChangeDto dto)
        {
            Guard.Argument(dto, nameof(dto)).NotNull();

            OldTime = dto.OldTime;
            NewTime = dto.NewTime;
            ChangedAt = dto.ChangedAt;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ScheduledStartTimeChangeDto"/> class
        /// </summary>
        /// <param name="exportable">The exportable</param>
        public ScheduledStartTimeChange(ExportableScheduledStartTimeChange exportable)
        {
            if (exportable == null)
            {
                throw new ArgumentNullException(nameof(exportable));
            }

            OldTime = exportable.OldTime;
            NewTime = exportable.NewTime;
            ChangedAt = exportable.ChangedAt;
        }

        /// <summary>
        /// Asynchronous export item's properties
        /// </summary>
        /// <returns>An <see cref="ExportableBase"/> instance containing all relevant properties</returns>
        public Task<ExportableScheduledStartTimeChange> ExportAsync()
        {
            return Task.FromResult(new ExportableScheduledStartTimeChange
            {
                ChangedAt = ChangedAt,
                NewTime = NewTime,
                OldTime = OldTime
            });
        }
    }
}
