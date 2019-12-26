/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
namespace Sportradar.OddsFeed.SDK.API
{
    /// <summary>
    /// Status of the replay player
    /// </summary>
    public enum ReplayPlayerStatus
    {
        /// <summary>
        /// Player was never playing
        /// </summary>
        NotStarted = 0,

        /// <summary>
        /// Player is currently playing
        /// </summary>
        Playing = 1,

        /// <summary>
        /// Player is stopped
        /// </summary>
        Stopped = 2,

        /// <summary>
        /// The setting up
        /// </summary>
        Setting_up = 3
    }
}
