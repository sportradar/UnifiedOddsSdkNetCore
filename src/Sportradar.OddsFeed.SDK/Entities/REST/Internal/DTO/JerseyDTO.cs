// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using Dawn;
using Sportradar.OddsFeed.SDK.Messages.Rest;

namespace Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Dto
{
    /// <summary>
    /// A data-transfer-object for jersey
    /// </summary>
    internal class JerseyDto
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

        public JerseyDto(jersey item)
        {
            Guard.Argument(item, nameof(item)).NotNull();

            BaseColor = item.@base;
            Number = item.number;
            SleeveColor = item.sleeve;
            Type = item.type;
            HorizontalStripes = item.horizontal_stripesSpecified
                                    ? (bool?)item.horizontal_stripes
                                    : null;
            Split = item.splitSpecified
                        ? (bool?)item.split
                        : null;
            Squares = item.squaresSpecified
                          ? (bool?)item.squares
                          : null;
            Stripes = item.stripesSpecified
                          ? (bool?)item.stripes
                          : null;
            StripesColor = item.stripes_color;
            SplitColor = item.split_color;
            ShirtType = item.shirt_type;
            SleeveDetail = item.sleeve_detail;
            SquareColor = item.squares_color;
            HorizontalStripesColor = item.horizontal_stripes_color;
        }
    }
}
