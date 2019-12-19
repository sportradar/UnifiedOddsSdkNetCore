/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/

using System;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Caching.Exportable
{
    /// <summary>
    /// Class used to export/import jersey item properties
    /// </summary>
    [Serializable]
    public class ExportableJerseyCI
    {
        /// <summary>
        /// Gets the <see cref="string"/> specifying the base color
        /// </summary>
        public string BaseColor { get; set; }

        /// <summary>
        /// Gets the <see cref="string"/> specifying the number
        /// </summary>
        public string Number { get; set; }

        /// <summary>
        /// Gets the <see cref="string"/> specifying the sleeve color
        /// </summary>
        public string SleeveColor { get; set; }

        /// <summary>
        /// Gets the <see cref="string"/> specifying the type
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// Gets the <see cref="bool"/> specifying the horizontal stripes
        /// </summary>
        public bool? HorizontalStripes { get; set; }

        /// <summary>
        /// Gets the <see cref="bool"/> specifying the split
        /// </summary>
        public bool? Split { get; set; }

        /// <summary>
        /// Gets the <see cref="bool"/> specifying the squares
        /// </summary>
        public bool? Squares { get; set; }

        /// <summary>
        /// Gets the <see cref="bool"/> specifying the stripes
        /// </summary>
        public bool? Stripes { get; set; }

        /// <summary>
        /// Gets the <see cref="string"/> specifying the stripes color
        /// </summary>
        public string StripesColor { get; set; }

        /// <summary>
        /// Gets the <see cref="string"/> specifying the split color
        /// </summary>
        public string SplitColor { get; set; }

        /// <summary>
        /// Gets the <see cref="string"/> specifying the shirt type
        /// </summary>
        public string ShirtType { get; set; }

        /// <summary>
        /// Gets the <see cref="string"/> specifying the sleeve detail
        /// </summary>
        public string SleeveDetail { get; set; }
    }
}