/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/

using System;
using System.Threading.Tasks;
using Sportradar.OddsFeed.SDK.Entities.REST.Caching.Exportable;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Internal.EntitiesImpl
{
    /// <summary>
    /// Represents a streaming channel
    /// </summary>
    internal class StreamingChannel : EntityPrinter, IStreamingChannel
    {
        /// <summary>
        /// The <see cref="Id"/> property backing field
        /// </summary>
        private readonly int _id;

        /// <summary>
        /// The <see cref="Name"/> property backing field
        /// </summary>
        private readonly string _name;

        /// <summary>
        /// Initializes a new instance of the <see cref="StreamingChannel"/> class.
        /// </summary>
        /// <param name="id">a value uniquely identifying the current streaming channel</param>
        /// <param name="name">the name of the streaming channel represented by the current instance</param>
        internal StreamingChannel(int id, string name)
        {
            _id = id;
            _name = name;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StreamingChannel"/> class.
        /// </summary>
        /// <param name="exportable">A <see cref="ExportableStreamingChannelCI"/> representing the current item</param>
        internal StreamingChannel(ExportableStreamingChannelCI exportable)
        {
            if (exportable == null)
                throw new ArgumentNullException(nameof(exportable));

            _id = exportable.Id;
            _name = exportable.Name;
        }

        /// <summary>
        /// Gets a value uniquely identifying the current streaming channel
        /// </summary>
        public int Id => _id;

        /// <summary>
        /// Gets the name of the streaming channel represented by the current instance
        /// </summary>
        public string Name => _name;

        protected override string PrintI()
        {
            return $"Id={_id}";
        }

        protected override string PrintC()
        {
            return $"Id={_id}, Name={_name}";
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
        public Task<ExportableStreamingChannelCI> ExportAsync()
        {
            return Task.FromResult(new ExportableStreamingChannelCI
            {
                Id = _id,
                Name = _name
            });
        }
    }
}
