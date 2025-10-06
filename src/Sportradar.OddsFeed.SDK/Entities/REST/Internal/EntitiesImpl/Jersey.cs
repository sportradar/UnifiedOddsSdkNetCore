// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using Dawn;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Caching.CI;

namespace Sportradar.OddsFeed.SDK.Entities.Rest.Internal.EntitiesImpl
{
    internal class Jersey : IJersey
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

        public Jersey(JerseyCacheItem item)
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
    }
}
