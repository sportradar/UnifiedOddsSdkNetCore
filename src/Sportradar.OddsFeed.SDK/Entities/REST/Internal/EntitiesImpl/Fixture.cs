/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Castle.Core.Internal;
using Dawn;
using Sportradar.OddsFeed.SDK.Entities.REST.Caching.Exportable;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching.CI;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.DTO;
using Sportradar.OddsFeed.SDK.Messages;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Internal.EntitiesImpl
{
    /// <summary>
    /// A implementation of <see cref="IFixture"/> used to return results to user
    /// </summary>
    internal class Fixture : EntityPrinter, IFixture
    {
        /// <summary>
        /// Gets a <see cref="DateTime"/> specifying when the fixture associated with the current <see cref="IFixture"/>
        /// is scheduled to start
        /// </summary>
        public DateTime? StartTime { get; }

        /// <summary>
        /// Gets a <see cref="DateTime"/> specifying the live time in case the fixture represented by current
        /// <see cref="IFixture"/> instance was re-schedule, or a null reference if the fixture was not re-scheduled
        /// </summary>
        public DateTime? NextLiveTime { get; }

        /// <summary>
        /// Gets a value indicating whether the start time of the fixture represented by current
        /// <see cref="IFixture" /> instance has been confirmed
        /// </summary>
        /// <value><c>true</c> if [start time confirmed]; otherwise, <c>false</c>.</value>
        public bool? StartTimeConfirmed { get; }

        /// <summary>
        /// Gets a value indicating whether the start time is to be determent
        /// </summary>
        /// <value><c>null</c> if [start time TBD] contains no value, <c>true</c> if [start time TBD]; otherwise, <c>false</c>.</value>
        public bool? StartTimeTBD { get; }

        /// <summary>
        /// When sport event is postponed this field indicates with which event it is replaced
        /// </summary>
        /// <value>The <see cref="URN"/> this event is replaced by</value>
        public URN ReplacedBy { get; }

        /// <summary>
        /// Gets a <see cref="IReadOnlyDictionary{String, String}" /> containing additional information about the
        /// fixture represented by current <see cref="IFixture" /> instance
        /// </summary>
        /// <value>The extra information.</value>
        public IReadOnlyDictionary<string, string> ExtraInfo { get; }

        /// <summary>
        /// Gets a <see cref="IEnumerable{ITvChannel}" /> representing TV channels covering the sport event
        /// represented by the current <see cref="IFixture" /> instance
        /// </summary>
        /// <value>The TV channels.</value>
        public IEnumerable<ITvChannel> TvChannels { get; }

        /// <summary>
        /// Gets a <see cref="ICoverageInfo" /> instance specifying what coverage is available for the sport event
        /// associated with current instance
        /// </summary>
        /// <value>The coverage information.</value>
        public ICoverageInfo CoverageInfo { get; }

        /// <summary>
        /// Gets a <see cref="IProductInfo" /> instance providing sportradar related information about the sport event associated
        /// with the current instance.
        /// </summary>
        /// <value>The product information.</value>
        public IProductInfo ProductInfo { get; }

        /// <summary>
        /// Gets the reference ids
        /// </summary>
        public IReference References { get; }

        /// <summary>
        /// Gets the scheduled start time changes
        /// </summary>
        /// <value>The scheduled start time changes</value>
        public IEnumerable<IScheduledStartTimeChange> ScheduledStartTimeChanges { get; }

        /// <summary>
        /// Gets a id of the parent stage associated with the current instance
        /// </summary>
        public URN ParentStageId { get; }

        /// <summary>
        /// Gets the list specifying the additional parent ids associated with the current instance
        /// </summary>
        public IEnumerable<URN> AdditionalParentsIds { get; }

        internal Fixture(FixtureDTO fixtureDto)
        {
            Guard.Argument(fixtureDto, nameof(fixtureDto)).NotNull();

            StartTime = fixtureDto.StartTime;
            NextLiveTime = fixtureDto.NextLiveTime;
            StartTimeConfirmed = fixtureDto.StartTimeConfirmed;
            StartTimeTBD = fixtureDto.StartTimeTbd;
            ReplacedBy = fixtureDto.ReplacedBy;
            ExtraInfo = fixtureDto.ExtraInfo;
            TvChannels = new List<ITvChannel>();
            if (!fixtureDto.TvChannels.IsNullOrEmpty())
            {
                TvChannels = fixtureDto.TvChannels.Select(tvChannelDTO => new TvChannel(tvChannelDTO.Name, tvChannelDTO.StartTime, tvChannelDTO.StreamUrl)).ToList();
            }
            if (fixtureDto.Coverage != null)
            {
                CoverageInfo = new CoverageInfo(fixtureDto.Coverage.Level, fixtureDto.Coverage.IsLive, fixtureDto.Coverage.Includes, fixtureDto.Coverage.CoveredFrom);
            }
            if (fixtureDto.ProductInfo != null)
            {
                ProductInfo = new ProductInfo(fixtureDto.ProductInfo);
            }
            if (fixtureDto.ReferenceIds != null)
            {
                References = new Reference(new ReferenceIdCI(fixtureDto.ReferenceIds));
            }
            ScheduledStartTimeChanges = new List<IScheduledStartTimeChange>();
            if (fixtureDto.ScheduledStartTimeChanges != null)
            {
                ScheduledStartTimeChanges = fixtureDto.ScheduledStartTimeChanges.Select(s => new ScheduledStartTimeChange(s));
            }
            if (fixtureDto.ParentStage != null)
            {
                ParentStageId = fixtureDto.ParentStage.Id;
            }
            AdditionalParentsIds = new List<URN>();
            if (!fixtureDto.AdditionalParents.IsNullOrEmpty())
            {
                AdditionalParentsIds = fixtureDto.AdditionalParents.Select(s => s.Id);
            }
        }

        internal Fixture(ExportableFixtureCI exportable)
        {
            if (exportable == null)
            {
                throw new ArgumentNullException(nameof(exportable));
            }

            StartTime = exportable.StartTime;
            NextLiveTime = exportable.NextLiveTime;
            StartTimeConfirmed = exportable.StartTimeConfirmed;
            StartTimeTBD = exportable.StartTimeTBD;
            ReplacedBy = exportable.ReplacedBy != null ? URN.Parse(exportable.ReplacedBy) : null;
            ExtraInfo = exportable.ExtraInfo.IsNullOrEmpty() ? new Dictionary<string, string>() : new Dictionary<string, string>(exportable.ExtraInfo);
            TvChannels = exportable.TvChannels.IsNullOrEmpty() ? new List<TvChannel>() : exportable.TvChannels?.Select(t => new TvChannel(t)).ToList();
            CoverageInfo = exportable.CoverageInfo != null ? new CoverageInfo(exportable.CoverageInfo) : null;
            ProductInfo = exportable.ProductInfo != null ? new ProductInfo(exportable.ProductInfo) : null;
            References = exportable.References != null ? new Reference(new ReferenceIdCI(exportable.References)) : null;
            ScheduledStartTimeChanges = exportable.ScheduledStartTimeChanges.IsNullOrEmpty() ? new List<ScheduledStartTimeChange>() : exportable.ScheduledStartTimeChanges?.Select(s => new ScheduledStartTimeChange(s)).ToList();
            ParentStageId = string.IsNullOrEmpty(exportable.ParentStageId) ? null : URN.Parse(exportable.ParentStageId);
            AdditionalParentsIds = exportable.AdditionalParentsIds.IsNullOrEmpty() ? new List<URN>() : exportable.AdditionalParentsIds.Select(URN.Parse).ToList();
        }

        /// <summary>
        /// Constructs and returns a <see cref="string"/> containing the id of the current instance
        /// </summary>
        /// <returns>A <see cref="string"/> containing the id of the current instance.</returns>
        protected override string PrintI()
        {
            return $"StartTime={StartTime}, StartTimeConfirmed={StartTimeConfirmed}";
        }

        /// <summary>
        /// Constructs and returns a <see cref="string"/> containing compacted representation of the current instance
        /// </summary>
        /// <returns>A <see cref="string"/> containing compacted representation of the current instance.</returns>
        protected override string PrintC()
        {
            var info = ExtraInfo.IsNullOrEmpty() ? string.Empty : string.Join(", ", ExtraInfo.Keys.Select(k => $"{k}={ExtraInfo[k]}"));
            return $"StartTime={StartTime}, StartTimeConfirmed={StartTimeConfirmed}, StartTimeTBD={StartTimeTBD}, ReplacedBy={ReplacedBy}, ExtraInfo=[{info}]";
        }

        /// <summary>
        /// Constructs and return a <see cref="string"/> containing details of the current instance
        /// </summary>
        /// <returns>A <see cref="string"/> containing details of the current instance.</returns>
        protected override string PrintF()
        {
            var tv = TvChannels.IsNullOrEmpty() ? string.Empty : string.Join(", ", TvChannels.Select(k => ((TvChannel)k).ToString("f")));
            var res = PrintC();
            res += $", TvChannels=[{tv}], CoverageInfo=[{((CoverageInfo)CoverageInfo)?.ToString("f")}], ProductInfo=[{((ProductInfo)ProductInfo)?.ToString("f")}]";
            return res;
        }

        /// <summary>
        /// Constructs and returns a string containing a JSON representation of the current instance
        /// </summary>
        /// <returns>a <see cref="string" /> containing a JSON representation of the current instance.</returns>
        protected override string PrintJ()
        {
            return PrintJ(GetType(), this);
        }

        /// <summary>
        /// Asynchronous export item's properties
        /// </summary>
        /// <returns>An <see cref="ExportableCI"/> instance containing all relevant properties</returns>
        public async Task<ExportableFixtureCI> ExportAsync()
        {
            var scheduledTasks = ScheduledStartTimeChanges?.Select(async s => await ((ScheduledStartTimeChange)s).ExportAsync().ConfigureAwait(false));
            var channelTasks = TvChannels?.Select(async c => await ((TvChannel)c).ExportAsync().ConfigureAwait(false));
            return new ExportableFixtureCI
            {
                ExtraInfo = ExtraInfo?.ToDictionary(i => i.Key, i => i.Value),
                CoverageInfo = CoverageInfo != null ? await ((CoverageInfo)CoverageInfo).ExportAsync().ConfigureAwait(false) : null,
                ProductInfo = ProductInfo != null ? await ((ProductInfo)ProductInfo).ExportAsync().ConfigureAwait(false) : null,
                ReplacedBy = ReplacedBy?.ToString(),
                References = References?.References?.ToDictionary(r => r.Key, r => r.Value),
                NextLiveTime = NextLiveTime,
                StartTimeConfirmed = StartTimeConfirmed,
                ScheduledStartTimeChanges = scheduledTasks != null ? await Task.WhenAll(scheduledTasks) : null,
                StartTime = StartTime,
                TvChannels = channelTasks != null ? await Task.WhenAll(channelTasks) : null,
                StartTimeTBD = StartTimeTBD,
                ParentStageId = ParentStageId?.ToString(),
                AdditionalParentsIds = AdditionalParentsIds?.Select(s => s.ToString()).ToList()
            };
        }
    }
}
