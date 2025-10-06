// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using System.Net;
using Dawn;
using Sportradar.OddsFeed.SDK.Messages.Rest;

namespace Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Dto
{
    /// <summary>
    /// A data-access-object representing a bookmaker details
    /// </summary>
    internal class BookmakerDetailsDto
    {
        /// <summary>
        /// Gets the bookmaker id
        /// </summary>
        public int Id { get; }

        /// <summary>
        /// Gets the specific virtual host of the bookmaker
        /// </summary>
        /// <value>The virtual host</value>
        public string VirtualHost { get; }

        /// <summary>
        /// Gets the expire date of the token
        /// </summary>
        /// <value>The expires</value>
        public DateTime? Expires { get; }

        /// <summary>
        /// Gets the message of the request
        /// </summary>
        public string Message { get; }

        /// <summary>
        /// Gets the <see cref="HttpStatusCode"/> specifying the server's response code;
        /// </summary>
        /// <value>The response code</value>
        public HttpStatusCode? ResponseCode { get; }

        /// <summary>
        /// Gets the server time difference
        /// </summary>
        /// <value>The server time difference</value>
        public TimeSpan ServerTimeDifference { get; internal set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="BookmakerDetailsDto"/> class
        /// </summary>
        /// <param name="msg">The MSG</param>
        /// <param name="serverTimeDifference">The server time difference</param>
        public BookmakerDetailsDto(bookmaker_details msg, TimeSpan serverTimeDifference)
        {
            Guard.Argument(msg, nameof(msg)).NotNull();

            Id = msg.bookmaker_id;
            VirtualHost = msg.virtual_host;
            Expires = msg.expire_atSpecified ? (DateTime?)msg.expire_at : null;
            ResponseCode = RestMapperHelper.MapResponseCode(msg.response_code, msg.response_codeSpecified);
            Message = msg.message;
            ServerTimeDifference = serverTimeDifference;
        }
    }
}
