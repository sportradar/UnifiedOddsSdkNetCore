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
        /// Initializes a new instance of the <see cref="ProductInfo"/> class.
        /// </summary>
        /// <param name="isAutoTraded"></param>
        /// <param name="isInHostedStatistics">a value indicating whether the sport event associated with the current instance is available in hosted solutions</param>
        /// <param name="isInLiveCenterSoccer">a value indicating whether the sport event associated with the current instance is available in LiveCenterSoccer solution</param>
        /// <param name="isInLiveScore">a value indicating whether the sport event associated with the current instance is available in LiveScore solution</param>
        /// <param name="links">a <see cref="IEnumerable{IProductInfoLink}" /> representing links to the product represented by current instance</param>
        /// <param name="channels">a <see cref="IEnumerable{IStreamingChannel}" /> representing streaming channel associated with product</param>
        public ProductInfo(bool isAutoTraded, bool isInHostedStatistics, bool isInLiveCenterSoccer, bool isInLiveScore, IEnumerable<IProductInfoLink> links, IEnumerable<IStreamingChannel> channels)
        {
            _isAutoTraded = isAutoTraded;
            _isInHostedStatistics = isInHostedStatistics;
            _isInLiveCenterSoccer = isInLiveCenterSoccer;
            _isInLiveScore = isInLiveScore;

            if (links != null)
            {
                _links = links as IReadOnlyCollection<IProductInfoLink> ?? new ReadOnlyCollection<IProductInfoLink>(links.ToList());
            }
            if (channels != null)
            {
                _channels = channels as IReadOnlyCollection<IStreamingChannel> ?? new ReadOnlyCollection<IStreamingChannel>(channels.ToList());
            }
        }

        public ProductInfo(ProductInfoDTO dto)
            :this(dto.IsAutoTraded,
                dto.IsInHostedStatistics,
                dto.IsInLiveCenterSoccer,
                dto.IsInLiveScore,
                dto.ProductInfoLinks?.Select(s => new ProductInfoLink(s.Reference, s.Name)),
                dto.StreamingChannels?.Select(s => new StreamingChannel(s.Id, s.Name)))
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ProductInfo"/> class.
        /// </summary>
        /// <param name="exportable">A <see cref="ExportableProductInfoCI" /> representing the current item</param>
        public ProductInfo(ExportableProductInfoCI exportable)
        {
            if (exportable == null)
                throw new ArgumentNullException(nameof(exportable));

            _isAutoTraded = exportable.IsAutoTraded;
            _isInHostedStatistics = exportable.IsInHostedStatistics;
            _isInLiveCenterSoccer = exportable.IsInLiveCenterSoccer;
            _isInLiveScore = exportable.IsInLiveScore;
            _links = exportable.Links?.Select(l => new ProductInfoLink(l)).ToList();
            _channels = exportable.Channels?.Select(c => new StreamingChannel(c)).ToList();
        }

        /// <summary>
        /// TODO: Add comments
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

        protected override string PrintI()
        {
            return $"IsAutoTraded={_isAutoTraded}";
        }

        protected override string PrintC()
        {
            var l = _links == null ? string.Empty : string.Join("; ", _links.Select(k => ((ProductInfoLink)k).ToString("c")));
            var c = _channels == null ? string.Empty : string.Join("; ", _channels.Select(k => ((StreamingChannel)k).ToString("c")));
            return $"IsAutoTraded={_isAutoTraded}, IsInHostedStatistics={_isInHostedStatistics}, IsInLiveScore={_isInLiveScore}, IsInLiveCenterSoccer={_isInLiveCenterSoccer}, Links=[{l}], Channels=[{c}]";
        }

        protected override string PrintF()
        {
            var l = _links == null ? string.Empty : string.Join("; ", _links.Select(k => ((ProductInfoLink)k).ToString("f")));
            var c = _channels == null ? string.Empty : string.Join("; ", _channels.Select(k => ((StreamingChannel)k).ToString("f")));
            return $"IsAutoTraded={_isAutoTraded}, IsInHostedStatistics={_isInHostedStatistics}, IsInLiveScore={_isInLiveScore}, IsInLiveCenterSoccer={_isInLiveCenterSoccer}, Links=[{l}], Channels=[{c}]";
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
            var linkTasks = _links?.Select(async l => await ((ProductInfoLink) l).ExportAsync().ConfigureAwait(false));
            var channeltasks = _channels?.Select(async c => await ((StreamingChannel) c).ExportAsync().ConfigureAwait(false));

            return new ExportableProductInfoCI
            {
                IsInLiveCenterSoccer = _isInLiveCenterSoccer,
                IsAutoTraded = _isAutoTraded,
                IsInHostedStatistics = _isInHostedStatistics,
                Links = linkTasks != null ? await Task.WhenAll(linkTasks) : null,
                Channels = channeltasks != null ? await Task.WhenAll(channeltasks) : null,
                IsInLiveScore = _isInLiveScore
            };
        }
    }
}
