/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using System.Diagnostics.Contracts;
using System.Net;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.DTO;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Internal.EntitiesImpl
{
    /// <summary>
    /// Represents a bookmaker details
    /// </summary>
    internal class BookmakerDetails : EntityPrinter, IBookmakerDetails
    {
        /// <summary>
        /// Gets the bookmaker id
        /// </summary>
        public int BookmakerId { get; }

        /// <summary>
        /// Gets the specific virtual host of the bookmaker
        /// </summary>
        /// <value>The virtual host</value>
        public string VirtualHost { get; }

        /// <summary>
        /// Gets the server time difference
        /// </summary>
        /// <value>The server time difference</value>
        public TimeSpan ServerTimeDifference { get; }

        /// <summary>
        /// Gets the expire date of the token
        /// </summary>
        /// <value>The expires</value>
        public DateTime ExpireAt { get; }

        /// <summary>
        /// Gets the response code of the server's response or a null reference
        /// </summary>
        public HttpStatusCode? ResponseCode { get; }

        /// <summary>
        /// Gets the message of the request
        /// </summary>
        public string Message { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="BookmakerDetails"/> class
        /// </summary>
        /// <param name="dto">A <see cref="BookmakerDetailsDTO"/> to be used for constructing new instance</param>
        public BookmakerDetails(BookmakerDetailsDTO dto)
        {
            Contract.Requires(dto != null);

            BookmakerId = dto.Id;
            VirtualHost = dto.VirtualHost;
            ExpireAt = dto.Expires.GetValueOrDefault(DateTime.MinValue);
            ResponseCode = dto.ResponseCode;
            Message = dto.Message;
            ServerTimeDifference = dto.ServerTimeDifference;
        }

        [ContractInvariantMethod]
        private void ObjectInvariant()
        {
            Contract.Invariant(this.BookmakerId >= 0);
        }

        /// <summary>
        /// Constructs and returns a <see cref="string"/> containing the id of the current instance
        /// </summary>
        /// <returns>A <see cref="string"/> containing the id of the current instance</returns>
        protected override string PrintI()
        {
            return $"Id={BookmakerId}";
        }

        /// <summary>
        /// Constructs and returns a <see cref="string"/> containing compacted representation of the current instance
        /// </summary>
        /// <returns>A <see cref="string"/> containing compacted representation of the current instance</returns>
        protected override string PrintC()
        {
            return $"Id={BookmakerId}, VirtualHost={VirtualHost}, ExpireAt={ExpireAt}, ResponseCode={ResponseCode}, Message={Message}, ServerTimeDifference={ServerTimeDifference}";
        }

        /// <summary>
        /// Constructs and return a <see cref="string"/> containing details of the current instance
        /// </summary>
        /// <returns>A <see cref="string"/> containing details of the current instance</returns>
        protected override string PrintF()
        {
            return PrintC();
        }

        /// <summary>
        /// Constructs and returns a <see cref="string" /> containing a JSON representation of the current instance
        /// </summary>
        /// <returns>a <see cref="string" /> containing a JSON representation of the current instance</returns>
        protected override string PrintJ()
        {
            return PrintJ(GetType(), this);
        }
    }
}
