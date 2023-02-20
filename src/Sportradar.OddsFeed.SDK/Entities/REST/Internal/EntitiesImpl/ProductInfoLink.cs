/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using System.Threading.Tasks;
using Sportradar.OddsFeed.SDK.Entities.REST.Caching.Exportable;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Internal.EntitiesImpl
{
    /// <summary>
    /// Represents a product info link
    /// </summary>
    internal class ProductInfoLink : EntityPrinter, IProductInfoLink
    {
        /// <summary>
        /// The <see cref="Reference"/> property backing field.
        /// </summary>
        private readonly string _reference;

        /// <summary>
        /// The <see cref="Name"/> property backing field.
        /// </summary>
        private readonly string _name;

        /// <summary>
        /// Initializes a new instance of the <see cref="ProductInfoLink"/> class.
        /// </summary>
        /// <param name="reference">the reference to the product info represented by the current instance</param>
        /// <param name="name">the name of the product link represented by the current instance</param>
        public ProductInfoLink(string reference, string name)
        {
            _reference = reference;
            _name = name;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ProductInfoLink"/> class.
        /// </summary>
        /// <param name="exportable">A <see cref="ExportableProductInfoLinkCI"/> representing the current item</param>
        public ProductInfoLink(ExportableProductInfoLinkCI exportable)
        {
            if (exportable == null)
            {
                throw new ArgumentNullException(nameof(exportable));
            }

            _reference = exportable.Reference;
            _name = exportable.Name;
        }

        /// <summary>
        /// Gets the reference to the product info represented by the current instance
        /// </summary>
        public string Reference => _reference;

        /// <summary>
        /// Gets the name of the product link represented by the current instance
        /// </summary>
        public string Name => _name;

        protected override string PrintI()
        {
            return $"Name={_name}";
        }

        protected override string PrintC()
        {
            return $"Name={_name}, Reference={_reference}";
        }

        protected override string PrintF()
        {
            return PrintC();
        }

        protected override string PrintJ()
        {
            return PrintJ(GetType(), this);
        }

        /// <summary>
        /// Asynchronous export item's properties
        /// </summary>
        /// <returns>An <see cref="ExportableCI"/> instance containing all relevant properties</returns>
        public Task<ExportableProductInfoLinkCI> ExportAsync()
        {
            return Task.FromResult(new ExportableProductInfoLinkCI
            {
                Name = _name,
                Reference = _reference
            });
        }
    }
}
