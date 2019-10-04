/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System.Collections.Generic;
using Sportradar.OddsFeed.SDK.Entities.REST;
using Sportradar.OddsFeed.SDK.Messages;

namespace Sportradar.OddsFeed.SDK.API
{
    /// <summary>
    /// Defines a contract implemented by classes capable of interacting with Replay Server
    /// </summary>
    public interface IReplayManager
    {
        /// <summary>
        /// Adds event {eventId} to the end of the replay queue
        /// </summary>
        /// <param name="eventId">The <see cref="URN" /> of the <see cref="IMatch" /></param>
        /// <param name="starttime">The minutes relative to event start time</param>
        /// <returns>Returns an <see cref="IReplayResponse"/></returns>
        IReplayResponse AddMessagesToReplayQueue(URN eventId, int? starttime = null);

        /// <summary>
        /// Removes the event from replay queue
        /// </summary>
        /// <param name="eventId">The <see cref="URN"/> of the <see cref="IMatch"/></param>
        /// <returns>Returns an <see cref="IReplayResponse"/></returns>
        IReplayResponse RemoveEventFromReplayQueue(URN eventId);

        /// <summary>
        /// Gets the events in replay queue.
        /// </summary>
        /// <returns>Returns an array of event ids</returns>
        IEnumerable<URN> GetEventsInQueue();

        /// <summary>
        /// Start replay the event from replay queue. Events are played in the order they were played in reality
        /// </summary>
        /// <param name="speed">The speed factor of the replay</param>
        /// <param name="maxDelay">The maximum delay between messages in milliseconds</param>
        /// <param name="producerId">The id of the producer from which we want to get messages, or null for messages from all producers</param>
        /// <param name="rewriteTimestamps">Should the timestamp in messages be rewritten with current time</param>
        /// <remarks>Start replay the event from replay queue. Events are played in the order they were played in reality, e.g. if there are some events that were played simultaneously in reality, they will be played in parallel as well here on replay server. If not specified, default values speed = 10 and max_delay = 10000 are used. This means that messages will be sent 10x faster than in reality, and that if there was some delay between messages that was longer than 10 seconds it will be reduced to exactly 10 seconds/10 000 ms (this is helpful especially in pre-match odds where delay can be even a few hours or more). If player is already in play, nothing will happen</remarks>
        /// <returns>Returns an <see cref="IReplayResponse"/></returns>
        IReplayResponse StartReplay(int speed = 10, int maxDelay = 10000, int? producerId = null, bool? rewriteTimestamps = null);

        /// <summary>
        /// Starts playing a predefined scenario
        /// </summary>
        /// <param name="scenarioId">The identifier of the scenario that should be played</param>
        /// <param name="speed">The speed factor of the replay</param>
        /// <param name="maxDelay">The maximum delay between messages in milliseconds</param>
        /// <param name="producerId">The id of the producer from which we want to get messages, or null for messages from all producers</param>
        /// <param name="rewriteTimestamps">Should the timestamp in messages be rewritten with current time</param>
        /// <remarks>Start replay the event from replay queue. Events are played in the order they were played in reality, e.g. if there are some events that were played simultaneously in reality, they will be played in parallel as well here on replay server. If not specified, default values speed = 10 and max_delay = 10000 are used. This means that messages will be sent 10x faster than in reality, and that if there was some delay between messages that was longer than 10 seconds it will be reduced to exactly 10 seconds/10 000 ms (this is helpful especially in pre-match odds where delay can be even a few hours or more). If player is already in play, nothing will happen</remarks>
        /// <returns>Returns an <see cref="IReplayResponse"/></returns>
        IReplayResponse StartReplayScenario(int scenarioId, int speed = 10, int maxDelay = 10000, int? producerId = null, bool? rewriteTimestamps = null);

        /// <summary>
        /// Stop the player if it is currently playing. If player is already stopped, nothing will happen.
        /// </summary>
        /// <returns>Returns an <see cref="IReplayResponse"/></returns>
        IReplayResponse StopReplay();

        /// <summary>
        /// Stop the player if it is currently playing and clear the replay queue. If player is already stopped, the queue is cleared.
        /// </summary>
        /// <returns>Returns an <see cref="IReplayResponse"/></returns>
        IReplayResponse StopAndClearReplay();

        /// <summary>
        /// Return the status of player. Possible values are: player is playing, player is stopped, player was never playing.
        /// </summary>
        /// <returns>Returns an <see cref="IReplayResponse"/></returns>
        IReplayStatus GetStatusOfReplay();

        /// <summary>
        /// Gets a list of available replay scenarios
        /// </summary>
        /// <returns>A list of available replay scenarios</returns>
        IEnumerable<IReplayScenario> GetAvailableScenarios();
    }
}
