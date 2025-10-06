// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using System.Threading.Tasks;
using Dawn;
using Sportradar.OddsFeed.SDK.Entities.Rest.Caching.Exportable;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Dto;

namespace Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Caching.CI
{
    /// <summary>
    /// An implementation of Jersey cache item
    /// </summary>
    internal class JerseyCacheItem
    {
        public string BaseColor { get; }

        public string Number { get; }

        public string SleeveColor { get; }

        public string Type { get; }

        public bool? HorizontalStripes { get; }

        public bool? Split { get; }

        public bool? Squares { get; }

        public bool? Stripes { get; }

        public string StripesColor { get; }

        public string SplitColor { get; }

        public string ShirtType { get; }

        public string SleeveDetail { get; }

        public string SquareColor { get; }

        public string HorizontalStripesColor { get; }

        public JerseyCacheItem(JerseyDto item)
        {
            Guard.Argument(item, nameof(item)).NotNull();

            BaseColor = item.BaseColor;
            Number = item.Number;
            SleeveColor = item.SleeveColor;
            Type = item.Type;
            HorizontalStripes = item.HorizontalStripes;
            Split = item.Split;
            Squares = item.Squares;
            Stripes = item.Stripes;
            StripesColor = item.StripesColor;
            SplitColor = item.SplitColor;
            ShirtType = item.ShirtType;
            SleeveDetail = item.SleeveDetail;
            SquareColor = item.SquareColor;
            HorizontalStripesColor = item.HorizontalStripesColor;
        }

        public JerseyCacheItem(ExportableJersey exportable)
        {
            if (exportable == null)
            {
                throw new ArgumentNullException(nameof(exportable));
            }

            BaseColor = exportable.BaseColor;
            Number = exportable.Number;
            SleeveColor = exportable.SleeveColor;
            Type = exportable.Type;
            HorizontalStripes = exportable.HorizontalStripes;
            Split = exportable.Split;
            Squares = exportable.Squares;
            Stripes = exportable.Stripes;
            StripesColor = exportable.StripesColor;
            SplitColor = exportable.SplitColor;
            ShirtType = exportable.ShirtType;
            SleeveDetail = exportable.SleeveDetail;
            SquareColor = exportable.SquareColor;
            HorizontalStripesColor = exportable.HorizontalStripesColor;
        }

        /// <summary>
        /// Asynchronous export item's properties
        /// </summary>
        /// <returns>An <see cref="ExportableJersey"/> instance containing all relevant properties</returns>
        public Task<ExportableJersey> ExportAsync()
        {
            return Task.FromResult(new ExportableJersey
            {
                BaseColor = BaseColor,
                Number = Number,
                SleeveColor = SleeveColor,
                Type = Type,
                HorizontalStripes = HorizontalStripes,
                Split = Split,
                Squares = Squares,
                Stripes = Stripes,
                StripesColor = StripesColor,
                SplitColor = SplitColor,
                ShirtType = ShirtType,
                SleeveDetail = SleeveDetail,
                SquareColor = SquareColor,
                HorizontalStripesColor = HorizontalStripesColor
            });
        }
    }
}
