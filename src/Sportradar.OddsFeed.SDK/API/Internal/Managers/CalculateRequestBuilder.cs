// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using System.Collections.Generic;
using Sportradar.OddsFeed.SDK.Api.Managers;
using Sportradar.OddsFeed.SDK.Entities.Rest.CustomBet;

namespace Sportradar.OddsFeed.SDK.Api.Internal.Managers
{
    /// <summary>
    /// The run-time implementation of the <see cref="ICalculateRequestBuilder"/> interface.
    /// </summary>
    internal class CalculateRequestBuilder : ICalculateRequestBuilder
    {
        private readonly List<CalculateRequestItem> _items = new List<CalculateRequestItem>();

        /// <inheritdoc />
        public ICalculateRequestBuilder AndSelection(ISelection selection)
        {
            if (selection == null)
            {
                throw new ArgumentNullException(nameof(selection));
            }

            _items.Add(new AndSelectionItem(selection));
            return this;
        }

        /// <inheritdoc />
        public ICalculateRequestBuilder AndAnyOfSelections(params ISelection[] selections)
        {
            if (selections == null || selections.Length == 0)
            {
                throw new ArgumentException("At least one selection must be provided for an OR group.", nameof(selections));
            }

            _items.Add(new OrSelectionGroupItem(selections));
            return this;
        }

        /// <summary>
        /// Builds and returns the <see cref="CalculateRequest"/> from the accumulated legs.
        /// </summary>
        internal CalculateRequest Build()
        {
            return new CalculateRequest(_items.AsReadOnly());
        }
    }
}
