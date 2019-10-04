/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System.Diagnostics.Contracts;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching.CI;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Internal.EntitiesImpl
{
    public class Jersey : IJerseyV1
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

        public Jersey(JerseyCI item)
        {
            Contract.Requires(item != null);

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
        }
    }
}
