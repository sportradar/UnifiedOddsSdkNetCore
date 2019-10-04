/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using System.Globalization;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.DTO;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Internal.EventArguments
{
    /// <summary>
    /// An event argument used by events raised when a message from the feed is received
    /// </summary>
    public class TournamentReceivedEventArgs : EventArgs
    {
        /// <summary>
        /// Gets a <see cref="string"/> representing deserialized message
        /// </summary>
        public TournamentDTO Tournament { get; }

        /// <summary>
        /// Culture of the tournament data
        /// </summary>
        public CultureInfo Culture { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="TournamentReceivedEventArgs"/> class
        /// </summary>
        /// <param name="tournament">a <see cref="TournamentDTO"/> representing the received tournament</param>
        /// <param name="culture">Culture of the tournament data</param>
        public TournamentReceivedEventArgs(TournamentDTO tournament, CultureInfo culture)
        {
            //Contract.Requires(tournament != null);

            Tournament = tournament;
            Culture = culture;
        }
    }
}
