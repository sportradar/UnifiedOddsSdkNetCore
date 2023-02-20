/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Xml.Serialization;

namespace Sportradar.OddsFeed.SDK.Messages.Feed
{
    /// <summary>
    /// The base class for feed market
    /// </summary>
    public abstract class FeedMarket
    {
        /// <summary>
        /// Gets or sets a <see cref="IReadOnlyDictionary{String, String}"/> representing parsed specifiers
        /// </summary>
        [XmlIgnore]
        public IReadOnlyDictionary<string, string> Specifiers { get; set; }

        /// <summary>
        /// Gets or sets a indicating whether the validation of the market has failed and should not be mapped to exposed entity
        /// </summary>
        public bool ValidationFailed { get; set; }

        /// <summary>
        /// Gets the specifiers string
        /// </summary>
        [XmlIgnore]
        public abstract string SpecifierString { get; }
    }

    [SuppressMessage("Style", "IDE1006: Naming rule violation", Justification = "FeedMarket defaults")]
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public partial class market : FeedMarket
    {
        /// <summary>
        /// Gets or sets a <see cref="IReadOnlyDictionary{String, String}"/> representing parsed specifiers
        /// </summary>
        public override string SpecifierString => specifiers;
    }

    [SuppressMessage("Style", "IDE1006: Naming rule violation", Justification = "FeedMarket defaults")]
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public partial class betSettlementMarket : FeedMarket
    {
        /// <summary>
        /// Gets or sets a <see cref="IReadOnlyDictionary{String, String}"/> representing parsed specifiers
        /// </summary>
        public override string SpecifierString => specifiers;
    }

    [SuppressMessage("Style", "IDE1006: Naming rule violation", Justification = "FeedMarket defaults")]
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public partial class oddsChangeMarket : FeedMarket
    {
        /// <summary>
        /// Gets or sets a <see cref="IReadOnlyDictionary{String, String}"/> representing parsed specifiers
        /// </summary>
        public override string SpecifierString => specifiers;
    }
}
