/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Sportradar.OddsFeed.SDK.Entities.REST.Caching.Exportable;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.DTO;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Internal.EntitiesImpl
{
    /// <summary>
    /// Providing information about a specific product
    /// </summary>
    internal class ProductInfo : EntityPrinter, IProductInfo
    {
        /// <summary>
        /// The <see cref="IsAutoTraded"/> property backing field
        /// </summary>
        private readonly bool _isAutoTraded;

        /// <summary>
        /// The <see cref="IsInHostedStatistics"/> property backing field
        /// </summary>
        private readonly bool _isInHostedStatistics;

        /// <summary>
        /// The <see cref="IsInLiveCenterSoccer"/> property backing field
        /// </summary>
        private readonly bool _isInLiveCenterSoccer;

        /// <summary>
        /// The <see cref="IsInLiveMatchTracker"/> property backing field
        /// </summary>
        private readonly bool _isInLiveMatchTracker;

        /// <summary>
        /// The <see cref="IsInLiveScore"/> property backing field
        /// </summary>
        private readonly bool _isInLiveScore;

        /// <summary>
        /// The <see cref="Links"/> property backing field
        /// </summary>
        private readonly IReadOnlyCollection<IProductInfoLink> _links;

        /// <summary>
        /// The <see cref="Channels"/> property backing field
        /// </summary>
        private readonly IReadOnlyCollection<IStreamingChannel> _channels;

        /// <summary>
        /// Gets a value indicating whether the sport event is auto traded
        /// </summary>
        /// <value><c>true</c> if this instance is automatic traded; otherwise, <c>false</c>.</value>
        public bool IsAutoTraded => _isAutoTraded;

        /// <summary>
        /// Gets a value indicating whether the sport event associated with the current instance is available in hosted solutions
        /// </summary>
        public bool IsInHostedStatistics => _isInHostedStatistics;

        /// <summary>
        /// Gets a value indicating whether the sport event associated with the current instance is available in LiveCenterSoccer solution
        /// </summary>
        public bool IsInLiveCenterSoccer => _isInLiveCenterSoccer;

        /// <summary>
        /// Gets a value indicating whether the sport event associated with the current instance is available in LiveMatchTracker solution
        /// </summary>
        public bool IsInLiveMatchTracker => _isInLiveMatchTracker;

        /// <summary>
        /// Gets a value indicating whether the sport event associated with the current instance is available in LiveScore solution
        /// </summary>
        public bool IsInLiveScore => _isInLiveScore;

        /// <summary>
        /// Gets a <see cref="IEnumerable{IProductInfoLink}" /> representing links to the product represented by current instance
        /// </summary>
        public IEnumerable<IProductInfoLink> Links => _links;

        /// <summary>
        /// Gets a <see cref="IEnumerable{IStreamingChannel}" /> representing streaming channel associated with product
        /// </summary>
        public IEnumerable<IStreamingChannel> Channels => _channels;

        /// <summary>
        /// Initializes a new instance of the <see cref="ProductInfo"/> class
        /// </summary>
        /// <param name="dto">The <see cref="ProductInfoDTO"/> data</param>
        public ProductInfo(ProductInfoDTO dto)
        {
            if (dto == null)
            {
                return;
            }

            _isAutoTraded = dto.IsAutoTraded;
            _isInHostedStatistics = dto.IsInHostedStatistics;
            _isInLiveCenterSoccer = dto.IsInLiveCenterSoccer;
            _isInLiveMatchTracker = dto.IsInLiveMatchTracker;
            _isInLiveScore = dto.IsInLiveScore;

            if (dto.ProductInfoLinks != null)
            {
                var links = dto.ProductInfoLinks.Select(s => (IProductInfoLink)new ProductInfoLink(s.Reference, s.Name)).ToList();
                _links = new ReadOnlyCollection<IProductInfoLink>(links);
            }
            if (dto.StreamingChannels != null)
            {
                var channels = dto.StreamingChannels.Select(s => (IStreamingChannel)new StreamingChannel(s.Id, s.Name)).ToList();
                _channels = new ReadOnlyCollection<IStreamingChannel>(channels);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ProductInfo"/> class.
        /// </summary>
        /// <param name="exportable">A <see cref="ExportableProductInfoCI" /> representing the current item</param>
        public ProductInfo(ExportableProductInfoCI exportable)
        {
            if (exportable == null)
            {
                throw new ArgumentNullException(nameof(exportable));
            }

            _isAutoTraded = exportable.IsAutoTraded;
            _isInHostedStatistics = exportable.IsInHostedStatistics;
            _isInLiveCenterSoccer = exportable.IsInLiveCenterSoccer;
            _isInLiveMatchTracker = exportable.IsInLiveMatchTracker;
            _isInLiveScore = exportable.IsInLiveScore;
            _links = exportable.Links?.Select(l => new ProductInfoLink(l)).ToList();
            _channels = exportable.Channels?.Select(c => new StreamingChannel(c)).ToList();
        }

        protected override string PrintI()
        {
            return $"IsAutoTraded={_isAutoTraded}";
        }

        protected override string PrintC()
        {
            var l = _links == null ? string.Empty : string.Join("; ", _links.Select(k => ((ProductInfoLink)k).ToString("c")));
            var c = _channels == null ? string.Empty : string.Join("; ", _channels.Select(k => ((StreamingChannel)k).ToString("c")));
            return $"IsAutoTraded={_isAutoTraded}, IsInHostedStatistics={_isInHostedStatistics}, IsInLiveCenterSoccer={_isInLiveCenterSoccer}, IsInLiveMatchTracker={_isInLiveMatchTracker}, IsInLiveScore={_isInLiveScore}, Links=[{l}], Channels=[{c}]";
        }

        protected override string PrintF()
        {
            var l = _links == null ? string.Empty : string.Join("; ", _links.Select(k => ((ProductInfoLink)k).ToString("f")));
            var c = _channels == null ? string.Empty : string.Join("; ", _channels.Select(k => ((StreamingChannel)k).ToString("f")));
            return $"IsAutoTraded={_isAutoTraded}, IsInHostedStatistics={_isInHostedStatistics}, IsInLiveCenterSoccer={_isInLiveCenterSoccer}, IsInLiveMatchTracker={_isInLiveMatchTracker}, IsInLiveScore={_isInLiveScore}, Links=[{l}], Channels=[{c}]";
        }

        protected override string PrintJ()
        {
            return PrintJ(GetType(), this);
        }

        /// <summary>
        /// Asynchronous export item's properties
        /// </summary>
        /// <returns>An <see cref="ExportableCI"/> instance containing all relevant properties</returns>
        public async Task<ExportableProductInfoCI> ExportAsync()
        {
            var linkTasks = _links?.Select(async l => await ((ProductInfoLink)l).ExportAsync().ConfigureAwait(false));
            var channelTasks = _channels?.Select(async c => await ((StreamingChannel)c).ExportAsync().ConfigureAwait(false));

            return new ExportableProductInfoCI
            {
                IsAutoTraded = _isAutoTraded,
                IsInHostedStatistics = _isInHostedStatistics,
                IsInLiveCenterSoccer = _isInLiveCenterSoccer,
                IsInLiveMatchTracker = _isInLiveMatchTracker,
                IsInLiveScore = _isInLiveScore,
                Links = linkTasks != null ? await Task.WhenAll(linkTasks) : null,
                Channels = channelTasks != null ? await Task.WhenAll(channelTasks) : null
            };
        }
    }
}
