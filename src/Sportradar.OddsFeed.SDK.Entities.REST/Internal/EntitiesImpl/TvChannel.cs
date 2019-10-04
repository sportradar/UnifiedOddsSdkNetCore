/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using System.Threading.Tasks;
using Sportradar.OddsFeed.SDK.Entities.REST.Caching.Exportable;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Internal.EntitiesImpl
{
    /// <summary>
    /// Represents a TV channel
    /// </summary>
    /// <seealso cref="ITvChannel" />
    internal class TvChannel : EntityPrinter, ITvChannelV1
    {
        /// <summary>
        /// The <see cref="Name"/> property backing field
        /// </summary>
        private readonly string _name;

        /// <summary>
        /// The <see cref="StartTime"/> property backing field
        /// </summary>
        private readonly DateTime? _startTime;

        /// <summary>
        /// The stream URL
        /// </summary>
        private readonly string _streamUrl;

        /// <summary>
        /// Initializes a new instance of the <see cref="TvChannel"/> class
        /// </summary>
        /// <param name="name">a name of the channel represented by the current <see cref="ITvChannel" /> instance</param>
        /// <param name="startTime">a <see cref="DateTime" /> specifying when the coverage on the channel represented by the current <see cref="ITvChannel" /> starts</param>
        /// <param name="streamUrl">The stream url</param>
        public TvChannel(string name, DateTime? startTime, string streamUrl)
        {
            _name = name;
            _startTime = startTime;
            _streamUrl = streamUrl;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TvChannel"/> class
        /// </summary>
        /// <param name="exportable">A <see cref="ExportableTvChannelCI"/> instance specifying the current item</param>
        public TvChannel(ExportableTvChannelCI exportable)
        {
            if (exportable == null)
                throw new ArgumentNullException(nameof(exportable));

            _name = exportable.Name;
            _startTime = exportable.StartTime;
            _streamUrl = exportable.StreamUrl;
        }

        /// <summary>
        /// Gets a name of the channel represented by the current <see cref="ITvChannel" /> instance
        /// </summary>
        public string Name => _name;

        /// <summary>
        /// Gets a <see cref="DateTime" /> specifying when the coverage on the channel represented by the
        /// current <see cref="ITvChannel" /> starts, or a null reference if the time is not known.
        /// </summary>
        /// <value>The start time.</value>
        public DateTime? StartTime => _startTime;

        /// <summary>
        /// Gets the stream url of the channel represented by the current <see cref="ITvChannelV1"/> instance
        /// </summary>
        public string StreamUrl => _streamUrl;

        /// <summary>
        /// Constructs and returns a <see cref="string"/> containing the id of the current instance
        /// </summary>
        /// <returns>A <see cref="string"/> containing the id of the current instance.</returns>
        protected override string PrintI()
        {
            return $"Name={_name}";
        }

        /// <summary>
        /// Constructs and returns a <see cref="string"/> containing compacted representation of the current instance
        /// </summary>
        /// <returns>A <see cref="string"/> containing compacted representation of the current instance.</returns>
        protected override string PrintC()
        {
            return $"Name={_name}, StartTime={_startTime}, StreamUrl={_streamUrl}";
        }

        /// <summary>
        /// Constructs and return a <see cref="string"/> containing details of the current instance
        /// </summary>
        /// <returns>A <see cref="string"/> containing details of the current instance.</returns>
        protected override string PrintF()
        {
            return PrintC();
        }

        /// <summary>
        /// Constructs and returns a <see cref="string" /> containing a JSON representation of the current instance
        /// </summary>
        /// <returns>a <see cref="string" /> containing a JSON representation of the current instance.</returns>
        protected override string PrintJ()
        {
            return PrintJ(GetType(), this);
        }

        /// <summary>
        /// Asynchronous export item's properties
        /// </summary>
        /// <returns>An <see cref="ExportableCI"/> instance containing all relevant properties</returns>
        public Task<ExportableTvChannelCI> ExportAsync()
        {
            return Task.FromResult(new ExportableTvChannelCI
            {
                Name = _name,
                StartTime = _startTime,
                StreamUrl = _streamUrl
            });
        }
    }
}
