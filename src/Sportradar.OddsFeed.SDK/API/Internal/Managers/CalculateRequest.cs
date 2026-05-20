// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System.Collections.Generic;
using Sportradar.OddsFeed.SDK.Entities.Rest.CustomBet;

namespace Sportradar.OddsFeed.SDK.Api.Internal.Managers
{
    /// <summary>
    /// Represents a single item in a calculate request — either a single AND selection or an OR group.
    /// </summary>
    internal abstract class CalculateRequestItem
    {
        /// <summary>
        /// Returns true if this item is an OR group (contains multiple alternative selections).
        /// </summary>
        public abstract bool IsOrGroup { get; }

        /// <summary>
        /// The selections contained in this item. For an AND leg this is a single element; for an OR group it is multiple.
        /// </summary>
        public abstract IReadOnlyList<ISelection> Selections { get; }
    }

    /// <summary>
    /// A single AND selection leg.
    /// </summary>
    internal sealed class AndSelectionItem : CalculateRequestItem
    {
        private readonly ISelection _selection;

        public AndSelectionItem(ISelection selection)
        {
            _selection = selection;
        }

        public override bool IsOrGroup => false;

        public override IReadOnlyList<ISelection> Selections => new[] { _selection };
    }

    /// <summary>
    /// An OR group leg — any one of the contained selections satisfies this leg.
    /// </summary>
    internal sealed class OrSelectionGroupItem : CalculateRequestItem
    {
        private readonly IReadOnlyList<ISelection> _selections;

        public OrSelectionGroupItem(IReadOnlyList<ISelection> selections)
        {
            _selections = selections;
        }

        public override bool IsOrGroup => true;

        public override IReadOnlyList<ISelection> Selections => _selections;
    }

    /// <summary>
    /// Internal representation of a calculate probability request. Holds an ordered list of legs
    /// (AND selections and OR groups) as built by <see cref="CalculateRequestBuilder"/>.
    /// </summary>
    internal sealed class CalculateRequest
    {
        /// <summary>
        /// The ordered list of legs in this request.
        /// </summary>
        public IReadOnlyList<CalculateRequestItem> Items { get; }

        public CalculateRequest(IReadOnlyList<CalculateRequestItem> items)
        {
            Items = items;
        }
    }
}
