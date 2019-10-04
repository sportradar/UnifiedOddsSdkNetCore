/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Net.Http;
using System.Xml;
using Sportradar.OddsFeed.SDK.Common.Internal.Log;
using Sportradar.OddsFeed.SDK.Entities.REST;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal;
using Sportradar.OddsFeed.SDK.Messages;

namespace Sportradar.OddsFeed.SDK.API.Internal.Replay
{
    /// <summary>
    /// Implementation of the <see cref="IReplayManagerV1"/> for interaction with xReplay Server for doing integration tests against played matches that are older than 48 hours
    /// </summary>
    /// <seealso cref="IReplayManagerV1" />
    [Log(LoggerType.ClientInteraction)]
    public class ReplayManager : MarshalByRefObject, IReplayManagerV1
    {
        private readonly IDataRestful _dataRestful;

        private readonly string _apiHost;

        private readonly int _nodeId;

        /// <summary>
        /// Initializes a new instance of the <see cref="ReplayManager"/> class
        /// </summary>
        /// <param name="replayApiHost">The API host of the Replay Server</param>
        /// <param name="dataRestful">The <see cref="IDataRestful"/> used to make REST requests</param>
        /// <param name="nodeId">The node id used to connect to replay server</param>
        public ReplayManager(string replayApiHost, IDataRestful dataRestful, int nodeId)
        {
            _apiHost = replayApiHost;
            _dataRestful = dataRestful;
            _nodeId = nodeId;
        }

        /// <summary>
        /// Defined field invariants needed by code contracts
        /// </summary>
        [ContractInvariantMethod]
        private void ObjectInvariant()
        {
            Contract.Invariant(_dataRestful != null);
            Contract.Invariant(!string.IsNullOrEmpty(_apiHost));
        }

        /// <summary>
        /// Adds event {eventId} to the end of the replay queue
        /// </summary>
        /// <param name="eventId">The <see cref="URN" /> of the event</param>
        /// <param name="startTime">Minutes relative to event start time</param>
        /// <returns>A <see cref="IReplayResponse"/></returns>
        public IReplayResponse AddMessagesToReplayQueue(URN eventId, int? startTime = null)
        {
            var url = $"{_apiHost}/events/{eventId}{BuildNodeIdQuery("?")}";
            if (startTime != null)
            {
                url = $"{_apiHost}/events/{eventId}?start_time={startTime}{BuildNodeIdQuery("&")}";
            }
            var uri = new Uri(url);

            var response = _dataRestful.PutDataAsync(uri).Result;

            return HandleHttpResponseMessage(response);
        }

        /// <summary>
        /// Removes the event from replay queue
        /// </summary>
        /// <param name="eventId">The <see cref="URN"/> of the <see cref="IMatch"/></param>
        public IReplayResponse RemoveEventFromReplayQueue(URN eventId)
        {
            if (eventId.TypeGroup != ResourceTypeGroup.MATCH)
            {
                throw new ArgumentException("Wrong type of EventId. Only IMatch supported.");
            }

            var uri = new Uri($"{_apiHost}/events/{eventId}{BuildNodeIdQuery("?")}");

            var response = _dataRestful.DeleteDataAsync(uri).Result;

            return HandleHttpResponseMessage(response);
        }

        /// <summary>
        /// Gets the events in replay queue.
        /// </summary>
        public IEnumerable<URN> GetEventsInQueue()
        {
            var uri = new Uri($"{_apiHost}/{BuildNodeIdQuery("?")}");

            var response = _dataRestful.GetDataAsync(uri).Result;

            if (response == null)
            {
                HandleHttpResponseMessage(null);
                return null;
            }

            var xml = new XmlDocument();
            xml.Load(response);

            var result = new List<URN>();
            var xmlNodeList = xml.DocumentElement?.SelectNodes("event");
            if (xmlNodeList != null)
            {
                foreach (XmlNode node in xmlNodeList)
                {
                    if (node.Attributes != null)
                    {
                        var urn = node.Attributes["id"].Value;
                        int position;
                        int.TryParse(node.Attributes["position"].Value, out position);
                        result.Add(URN.Parse(urn));
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// Start replay the event from replay queue. Events are played in the order they were played in reality
        /// </summary>
        /// <param name="speed">The speed factor of the replay</param>
        /// <param name="maxDelay">The maximum delay between messages (in milliseconds)</param>
        /// <param name="producerId">The id of the producer from which we want to get messages, or null for messages from all producers</param>
        /// <param name="rewriteTimestamps">Should the timestamp in messages be rewritten with current time</param>
        /// <returns>Returns an <see cref="IReplayResponse" /></returns>
        /// <remarks>Start replay the event from replay queue. Events are played in the order they were played in reality, e.g. if there are some events that were played simultaneously in reality, they will be played in parallel as well here on replay server. If not specified, default values speed = 10 and max_delay = 10000 are used. This means that messages will be sent 10x faster than in reality, and that if there was some delay between messages that was longer than 10 seconds it will be reduced to exactly 10 seconds/10 000 ms (this is helpful especially in pre-match odds where delay can be even a few hours or more). If player is already in play, nothing will happen</remarks>
        public IReplayResponse StartReplay(int speed = 10, int maxDelay = 10000, int? producerId = null, bool? rewriteTimestamps = null)
        {
            return StartReplay(speed, maxDelay, producerId, rewriteTimestamps, null);
        }

        /// <summary>
        /// Start replay the event from replay queue. Events are played in the order they were played in reality
        /// </summary>
        /// <param name="speed">The speed factor of the replay</param>
        /// <param name="maxDelay">The maximum delay between messages in milliseconds</param>
        /// <param name="producerId">The id of the producer from which we want to get messages, or null for messages from all producers</param>
        /// <param name="rewriteTimestamps">Should the timestamp in messages be rewritten with current time</param>
        /// <param name="runParallel">Should the events in queue replay independently</param>
        /// <remarks>Start replay the event from replay queue. Events are played in the order they were played in reality, e.g. if there are some events that were played simultaneously in reality, they will be played in parallel as well here on replay server. If not specified, default values speed = 10 and max_delay = 10000 are used. This means that messages will be sent 10x faster than in reality, and that if there was some delay between messages that was longer than 10 seconds it will be reduced to exactly 10 seconds/10 000 ms (this is helpful especially in pre-match odds where delay can be even a few hours or more). If player is already in play, nothing will happen</remarks>
        /// <returns>Returns an <see cref="IReplayResponse"/></returns>
        public IReplayResponse StartReplay(int speed, int maxDelay, int? producerId, bool? rewriteTimestamps, bool? runParallel)
        {
            //speed = CheckBoundaries(speed, 1, 1000);
            //maxDelay = CheckBoundaries(maxDelay, 100, 1000000);

            var paramProducerId = string.Empty;
            if (producerId != null)
            {
                paramProducerId = $"&product={producerId}";
            }
            var paramRewriteTimestamps = string.Empty;
            //if (rewriteTimestamps == null)
            //{
            //    rewriteTimestamps = true;
            //}
            if (rewriteTimestamps != null)
            {
                paramRewriteTimestamps = $"&use_replay_timestamp={rewriteTimestamps}";
            }
            var paramRunParallel = string.Empty;
            if (runParallel != null)
            {
                paramRunParallel = $"&run_parallel={runParallel}";
            }

            var uri = new Uri($"{_apiHost}/play?speed={speed}&max_delay={maxDelay}{BuildNodeIdQuery("&")}{paramProducerId}{paramRewriteTimestamps}{paramRunParallel}");

            var response = _dataRestful.PostDataAsync(uri).Result;

            return HandleHttpResponseMessage(response);
        }

        /// <summary>
        /// Starts playing a predefined scenario
        /// </summary>
        /// <param name="scenarioId">The identifier of the scenario that should be played</param>
        /// <param name="speed">The speed factor of the replay</param>
        /// <param name="maxDelay">The maximum delay between messages</param>
        /// <param name="producerId">The id of the producer from which we want to get messages, or null for messages from all producers</param>
        /// <param name="rewriteTimestamps">Should the timestamp in messages be rewritten with current time</param>
        /// <returns>Returns an <see cref="IReplayResponse" /></returns>
        /// <remarks>Start replay the event from replay queue. Events are played in the order they were played in reality, e.g. if there are some events that were played simultaneously in reality, they will be played in parallel as well here on replay server. If not specified, default values speed = 10 and max_delay = 10000 are used. This means that messages will be sent 10x faster than in reality, and that if there was some delay between messages that was longer than 10 seconds it will be reduced to exactly 10 seconds/10 000 ms (this is helpful especially in pre-match odds where delay can be even a few hours or more). If player is already in play, nothing will happen</remarks>
        public IReplayResponse StartReplayScenario(int scenarioId, int speed = 10, int maxDelay = 10000, int? producerId = null, bool? rewriteTimestamps = null)
        {
            //speed = CheckBoundaries(speed, 1, 1000);
            //maxDelay = CheckBoundaries(maxDelay, 100, 1000000);

            var paramProducerId = string.Empty;
            if (producerId != null)
            {
                paramProducerId = $"&product={producerId}";
            }
            var paramRewriteTimestamps = string.Empty;
            //if (rewriteTimestamps == null)
            //{
            //    rewriteTimestamps = true;
            //}
            if (rewriteTimestamps != null)
            {
                paramRewriteTimestamps = $"&use_replay_timestamp={rewriteTimestamps}";
            }

            var uri = new Uri($"{_apiHost}/scenario/play/{scenarioId}?speed={speed}&max_delay={maxDelay}{BuildNodeIdQuery("&")}{paramProducerId}{paramRewriteTimestamps}");

            var response = _dataRestful.PostDataAsync(uri).Result;

            return HandleHttpResponseMessage(response);
        }

        /// <summary>
        /// Stop the player if it is currently playing. If player is already stopped, nothing will happen.
        /// </summary>
        public IReplayResponse StopReplay()
        {
            var uri = new Uri($"{_apiHost}/stop{BuildNodeIdQuery("?")}");

            var response = _dataRestful.PostDataAsync(uri).Result;

            return HandleHttpResponseMessage(response);
        }

        /// <summary>
        /// Stop the player if it is currently playing and clear the replay queue. If player is already stopped, the queue is cleared.
        /// </summary>
        public IReplayResponse StopAndClearReplay()
        {
            var uri = new Uri($"{_apiHost}/reset{BuildNodeIdQuery("?")}");

            var response = _dataRestful.PostDataAsync(uri).Result;

            return HandleHttpResponseMessage(response);
        }

        /// <summary>
        /// Return the status of player. Possible values are: player is playing, player is stopped, player was never playing.
        /// </summary>
        /// <returns>A <see cref="IReplayStatus"/></returns>
        public IReplayStatus GetStatusOfReplay()
        {
            var uri = new Uri($"{_apiHost}/status{BuildNodeIdQuery("?")}");

            var response = _dataRestful.GetDataAsync(uri).Result;

            if (response == null)
            {
                HandleHttpResponseMessage(null);
                return new ReplayStatus(null, ReplayPlayerStatus.NotStarted);
            }

            var xml = new XmlDocument();
            xml.Load(response);

            string urn = null;
            string status = null;

            if (xml.DocumentElement != null && xml.DocumentElement.HasAttribute("last_msg_from_event"))
            {
                urn = xml.DocumentElement.Attributes["last_msg_from_event"].Value;
            }
            if (xml.DocumentElement != null && xml.DocumentElement.HasAttribute("status"))
            {
                status = xml.DocumentElement.Attributes["status"].Value;
            }
            var eventId = string.IsNullOrEmpty(urn) ? null : URN.Parse(urn);
            if (string.IsNullOrEmpty(status))
            {
                throw new HttpRequestException("Status value missing");
            }

            return new ReplayStatus(eventId, MessageMapperHelper.GetEnumValue(status, ReplayPlayerStatus.NotStarted));
        }

        /// <summary>
        /// Gets a list of available replay scenarios
        /// </summary>
        /// <returns>A list of available replay scenarios</returns>
        public IEnumerable<IReplayScenario> GetAvailableScenarios()
        {
            var uri = new Uri($"{_apiHost}/scenario{BuildNodeIdQuery("?")}");

            var response = _dataRestful.GetDataAsync(uri).Result;

            if (response == null)
            {
                HandleHttpResponseMessage(null);
                return null;
            }

            var xml = new XmlDocument();
            xml.Load(response);

            var result = new List<IReplayScenario>();
            var xmlReplayScenarios = xml.DocumentElement?.SelectNodes("replay_scenario");
            if (xmlReplayScenarios != null)
            {
                foreach (XmlNode replayScenarioNode in xmlReplayScenarios)
                {
                    int scenarioId;
                    string description;
                    bool runParallel;
                    if (replayScenarioNode.Attributes != null)
                    {
                        int.TryParse(replayScenarioNode.Attributes["id"].Value, out scenarioId);
                        description = replayScenarioNode.Attributes["description"].Value;
                        runParallel = replayScenarioNode.Attributes["run_parallel"].Value.Equals("true");
                    }
                    else
                    {
                        throw new HttpRequestException("Scenario data missing");
                    }

                    var associatedEventIds = new List<URN>();
                    var xmlEventNodes = replayScenarioNode.SelectNodes("event");
                    if (xmlEventNodes != null)
                    {
                        foreach (XmlNode eventNode in xmlEventNodes)
                        {
                            if (eventNode.Attributes != null)
                            {
                                var urn = eventNode.Attributes["id"].Value;
                                associatedEventIds.Add(URN.Parse(urn));
                            }
                        }
                    }

                    result.Add(new ReplayScenario(scenarioId, description, runParallel, associatedEventIds));
                }
            }
            return result;
        }

        private static IReplayResponse HandleHttpResponseMessage(HttpResponseMessage response)
        {
            if (response == null)
            {
                throw new HttpRequestException("No response received.");
            }
            var success = true;
            var responseMsg = string.Empty;
            string errorMsg = null;
            if (response.Content != null)
            {
                var xml = new XmlDocument();
                xml.Load(response.Content.ReadAsStreamAsync().Result);

                responseMsg = xml.DocumentElement?.SelectSingleNode("action")?.InnerText;
                if (responseMsg != null && !responseMsg.EndsWith("."))
                {
                    responseMsg += ".";
                }
                responseMsg += " " + xml.DocumentElement?.SelectSingleNode("message")?.InnerText;

                if (!response.IsSuccessStatusCode)
                {
                    success = false;
                    errorMsg = $"Request was not successfully completed. Response: {(int) response.StatusCode}-{response.ReasonPhrase}";
                }
            }

            return new ReplayResponse(success, responseMsg, errorMsg);
        }

        private string BuildNodeIdQuery(string prefix)
        {
            return _nodeId != 0 ? prefix + "node_id=" + _nodeId : "";
        }
    }
}
