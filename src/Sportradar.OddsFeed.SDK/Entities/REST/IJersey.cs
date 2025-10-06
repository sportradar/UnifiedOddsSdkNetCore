// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

namespace Sportradar.OddsFeed.SDK.Entities.Rest
{
    /// <summary>
    /// Defines a contract for classes implementing jersey
    /// </summary>
    public interface IJersey
    {
        /// <summary>
        /// Gets the base color of the jersey
        /// </summary>
        /// <value>The base color of the jersey</value>
        string BaseColor { get; }

        /// <summary>
        /// Gets the number of the jersey
        /// </summary>
        /// <value>The number of the jersey</value>
        string Number { get; }

        /// <summary>
        /// Gets the color of the sleeves
        /// </summary>
        /// <value>The color of the sleeves</value>
        string SleeveColor { get; }

        /// <summary>
        /// Gets the typ of the jersey
        /// </summary>
        /// <value>The type of the jersey</value>
        string Type { get; }

        /// <summary>
        /// Gets a value indicating whether jersey has horizontal stripes
        /// </summary>
        /// <value><c>null</c> if [horizontal stripes] contains no value, <c>true</c> if [horizontal stripes]; otherwise, <c>false</c>.</value>
        bool? HorizontalStripes { get; }

        /// <summary>
        /// Gets a value indicating whether this <see cref="IJersey"/> is split
        /// </summary>
        /// <value><c>null</c> if [split] contains no value, <c>true</c> if [split]; otherwise, <c>false</c>.</value>
        bool? Split { get; }

        /// <summary>
        /// Gets a value indicating whether this <see cref="IJersey"/> has squares
        /// </summary>
        /// <value><c>null</c> if [squares] contains no value, <c>true</c> if [squares]; otherwise, <c>false</c>.</value>
        bool? Squares { get; }

        /// <summary>
        /// Gets a value indicating whether this <see cref="IJersey"/> has stripes
        /// </summary>
        /// <value><c>null</c> if [stripes] contains no value, <c>true</c> if [stripes]; otherwise, <c>false</c>.</value>
        bool? Stripes { get; }

        /// <summary>
        /// Gets the stripes color of the jersey
        /// </summary>
        /// <value>The stripes color of the jersey</value>
        string StripesColor { get; }

        /// <summary>
        /// Gets the split color of the jersey
        /// </summary>
        /// <value>The split color of the jersey</value>
        string SplitColor { get; }

        /// <summary>
        /// Gets the shirt type of the jersey
        /// </summary>
        /// <value>The shirt type of the jersey</value>
        string ShirtType { get; }

        /// <summary>
        /// Gets the sleeve detail of the jersey
        /// </summary>
        /// <value>The sleeve detail of the jersey</value>
        string SleeveDetail { get; }

        /// <summary>
        /// Gets the square color of the jersey
        /// </summary>
        /// <value>The square color of the jersey</value>
        string SquareColor { get; }

        /// <summary>
        /// Gets the horizontal stripes color of the jersey
        /// </summary>
        /// <value>The horizontal stripes color of the jersey</value>
        string HorizontalStripesColor { get; }
    }
}
