// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Xml;
using Dawn;
using Microsoft.Extensions.Logging;
using Sportradar.OddsFeed.SDK.Api.Config;
using Sportradar.OddsFeed.SDK.Api.Replay;
using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Common.Enums;
using Sportradar.OddsFeed.SDK.Common.Internal.Telemetry;
using Sportradar.OddsFeed.SDK.Entities.Rest;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal;

namespace Sportradar.OddsFeed.SDK.Api.Internal.Replay
{
    /// <summary>
    /// Implementation of the <see cref="IReplayManager"/> for interaction with xReplay Server for doing integration tests against played matches that are older than 48 hours
    /// </summary>
    /// <seealso cref="IReplayManager" />
    internal class ReplayManager : IReplayManager
    {
        private static readonly ILogger LogInt = SdkLoggerFactory.GetLoggerForClientInteraction(typeof(ReplayManager));

        private readonly IDataRestful _dataRestful;

        private readonly string _apiHost;

        private readonly int _nodeId;

        /// <summary>
        /// Initializes a new instance of the <see cref="ReplayManager"/> class
        /// </summary>
        /// <param name="config">The configuration used to extract replay Api host and node id</param>
        /// <param name="dataRestful">The <see cref="IDataRestful"/> used to make REST requests</param>
        public ReplayManager(IUofConfiguration config, IDataRestful dataRestful)
        {
            Guard.Argument(config, nameof(config)).NotNull();
            Guard.Argument(dataRestful, nameof(dataRestful)).NotNull();

            _apiHost = config.Api.ReplayBaseUrl;
            _dataRestful = dataRestful;
            _nodeId = config.NodeId;
        }

        /// <summary>
        /// Adds event {eventId} to the end of the replay queue
        /// </summary>
        /// <param name="eventId">The <see cref="Urn" /> of the event</param>
        /// <param name="startTime">Minutes relative to event start time</param>
        /// <returns>A <see cref="IReplayResponse"/></returns>
        public IReplayResponse AddMessagesToReplayQueue(Urn eventId, int? startTime = null)
        {
            LogInt.LogInformation("Invoked AddMessagesToReplayQueue: [Urn={EventId}, StartTime={StartTime}]", eventId, startTime?.ToString());
            var url = $"{_apiHost}/events/{eventId}{BuildNodeIdQuery("?")}";
            if (startTime != null)
            {
                url = $"{_apiHost}/events/{eventId}?start_time={startTime.ToString()}{BuildNodeIdQuery("&")}";
            }
            var uri = new Uri(url);

            var response = _dataRestful.PutDataAsync(uri).GetAwaiter().GetResult();

            return HandleHttpResponseMessage(response);
        }

        /// <summary>
        /// Removes the event from replay queue
        /// </summary>
        /// <param name="eventId">The <see cref="Urn"/> of the <see cref="IMatch"/></param>
        public IReplayResponse RemoveEventFromReplayQueue(Urn eventId)
        {
            if (eventId.TypeGroup != ResourceTypeGroup.Match)
            {
                throw new ArgumentException("Wrong type of EventId. Only IMatch supported.");
            }

            LogInt.LogInformation("Invoked RemoveEventFromReplayQueue: [Urn={EventId}]", eventId);

            var uri = new Uri($"{_apiHost}/events/{eventId}{BuildNodeIdQuery("?")}");

            var response = _dataRestful.DeleteDataAsync(uri).GetAwaiter().GetResult();

            return HandleHttpResponseMessage(response);
        }

        /// <summary>
        /// Gets the events in replay queue.
        /// </summary>
        public IEnumerable<Urn> GetEventsInQueue()
        {
            LogInt.LogInformation("Invoked GetEventsInQueue");
            var result = GetReplayEventsInQueueInternal();
            return result?.Select(e => e.Id).ToList();
        }

        /// <summary>
        /// Gets list of replay events in queue.
        /// </summary>
        /// <returns>Returns a list of replay events</returns>
        public IEnumerable<IReplayEvent> GetReplayEventsInQueue()
        {
            LogInt.LogInformation("Invoked GetReplayEventsInQueue");
            return GetReplayEventsInQueueInternal();
        }

        /// <summary>
        /// Gets list of replay events in queue.
        /// </summary>
        /// <returns>Returns a list of replay events</returns>
        private IEnumerable<IReplayEvent> GetReplayEventsInQueueInternal()
        {
            var uri = new Uri($"{_apiHost}/{BuildNodeIdQuery("?")}");

            var response = _dataRestful.GetDataAsync(uri).GetAwaiter().GetResult();

            if (response == null)
            {
                HandleHttpResponseMessage(null);
                return new List<IReplayEvent>();
            }

            var xml = new XmlDocument { XmlResolver = null };
            xml.Load(response);

            var result = new List<IReplayEvent>();
            var xmlNodeList = xml.DocumentElement?.SelectNodes("event");
            if (xmlNodeList == null)
            {
                return result;
            }

            foreach (XmlNode node in xmlNodeList)
            {
                if (node.Attributes == null || node.Attributes.Count == 0)
                {
                    continue;
                }

                var idAttribute = node.Attributes["id"];
                if (idAttribute == null)
                {
                    continue;
                }
                if (!Urn.TryParse(idAttribute.Value, out var id))
                {
                    continue;
                }

                int? position = null;
                var positionAttribute = node.Attributes["position"];
                if (positionAttribute != null)
                {
                    position = int.TryParse(positionAttribute.Value, out var outValue) ? (int?)outValue : null;
                }

                int? startTime = null;
                var startTimeAttribute = node.Attributes["start_time"];
                if (startTimeAttribute != null)
                {
                    startTime = int.TryParse(startTimeAttribute.Value, out var outValue) ? (int?)outValue : null;
                }

                result.Add(new ReplayEvent(id, position, startTime));
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
            LogInt.LogInformation("Invoked StartReplay: [Speed={Speed}, MaxDelay={MaxDelay}, ProducerId={ProducerId}, RewriteTimestamps={RewriteTimestamps}, RunParallel={RunParallel}]", speed, maxDelay, producerId, rewriteTimestamps, runParallel);

            var paramProducerId = string.Empty;
            if (producerId != null)
            {
                paramProducerId = $"&product={producerId}";
            }
            var paramRewriteTimestamps = string.Empty;

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

            var response = _dataRestful.PostDataAsync(uri).GetAwaiter().GetResult();

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
            LogInt.LogInformation("Invoked StartReplayScenario: [ScenarioId={ScenarioId}, Speed={Speed}, MaxDelay={MaxDelay}, ProducerId={ProducerId}, RewriteTimestamps={RewriteTimestamps}]",
                                  scenarioId,
                                  speed,
                                  maxDelay,
                                  producerId,
                                  rewriteTimestamps);

            var paramProducerId = string.Empty;
            if (producerId != null)
            {
                paramProducerId = $"&product={producerId}";
            }
            var paramRewriteTimestamps = string.Empty;
            if (rewriteTimestamps != null)
            {
                paramRewriteTimestamps = $"&use_replay_timestamp={rewriteTimestamps}";
            }

            var uri = new Uri($"{_apiHost}/scenario/play/{scenarioId}?speed={speed}&max_delay={maxDelay}{BuildNodeIdQuery("&")}{paramProducerId}{paramRewriteTimestamps}");

            var response = _dataRestful.PostDataAsync(uri).GetAwaiter().GetResult();

            return HandleHttpResponseMessage(response);
        }

        /// <summary>
        /// Stop the player if it is currently playing. If player is already stopped, nothing will happen.
        /// </summary>
        public IReplayResponse StopReplay()
        {
            LogInt.LogInformation("Invoked StopReplay");

            var uri = new Uri($"{_apiHost}/stop{BuildNodeIdQuery("?")}");

            var response = _dataRestful.PostDataAsync(uri).GetAwaiter().GetResult();

            return HandleHttpResponseMessage(response);
        }

        /// <summary>
        /// Stop the player if it is currently playing and clear the replay queue. If player is already stopped, the queue is cleared.
        /// </summary>
        public IReplayResponse StopAndClearReplay()
        {
            LogInt.LogInformation("Invoked StopAndClearReplay");

            var uri = new Uri($"{_apiHost}/reset{BuildNodeIdQuery("?")}");

            var response = _dataRestful.PostDataAsync(uri).GetAwaiter().GetResult();

            return HandleHttpResponseMessage(response);
        }

        /// <summary>
        /// Return the status of player. Possible values are: player is playing, player is stopped, player was never playing.
        /// </summary>
        /// <returns>A <see cref="IReplayStatus"/></returns>
        public IReplayStatus GetStatusOfReplay()
        {
            LogInt.LogInformation("Invoked GetStatusOfReplay");

            var uri = new Uri($"{_apiHost}/status{BuildNodeIdQuery("?")}");

            var response = _dataRestful.GetDataAsync(uri).GetAwaiter().GetResult();

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
            var eventId = string.IsNullOrEmpty(urn) ? null : Urn.Parse(urn);
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
            LogInt.LogInformation("Invoked GetAvailableScenarios");

            var uri = new Uri($"{_apiHost}/scenario{BuildNodeIdQuery("?")}");

            var response = _dataRestful.GetDataAsync(uri).GetAwaiter().GetResult();

            if (response == null)
            {
                HandleHttpResponseMessage(null);
                return new List<IReplayScenario>();
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

                    var associatedEventIds = new List<Urn>();
                    var xmlEventNodes = replayScenarioNode.SelectNodes("event");
                    if (xmlEventNodes != null)
                    {
                        foreach (XmlNode eventNode in xmlEventNodes)
                        {
                            if (eventNode.Attributes != null)
                            {
                                var urn = eventNode.Attributes["id"].Value;
                                associatedEventIds.Add(Urn.Parse(urn));
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
                xml.Load(response.Content.ReadAsStreamAsync().GetAwaiter().GetResult());

                responseMsg = xml.DocumentElement?.SelectSingleNode("action")?.InnerText;
                if (responseMsg != null && !responseMsg.EndsWith(".", StringComparison.InvariantCultureIgnoreCase))
                {
                    responseMsg += ".";
                }
                responseMsg += " " + xml.DocumentElement?.SelectSingleNode("message")?.InnerText;

                if (!response.IsSuccessStatusCode)
                {
                    success = false;
                    errorMsg = $"Request was not successfully completed. Response: {(int)response.StatusCode}-{response.ReasonPhrase}";
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
