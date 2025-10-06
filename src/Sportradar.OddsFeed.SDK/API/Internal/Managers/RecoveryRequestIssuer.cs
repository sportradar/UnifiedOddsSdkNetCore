// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using System.Net.Http;
using System.Threading.Tasks;
using Dawn;
using Microsoft.Extensions.Logging;
using Sportradar.OddsFeed.SDK.Api.Config;
using Sportradar.OddsFeed.SDK.Api.EventArguments;
using Sportradar.OddsFeed.SDK.Api.Internal.ApiAccess;
using Sportradar.OddsFeed.SDK.Api.Internal.Recovery;
using Sportradar.OddsFeed.SDK.Api.Managers;
using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Common.Exceptions;
using Sportradar.OddsFeed.SDK.Common.Extensions;
using Sportradar.OddsFeed.SDK.Common.Internal;
using Sportradar.OddsFeed.SDK.Common.Internal.Telemetry;

namespace Sportradar.OddsFeed.SDK.Api.Internal.Managers
{
    /// <summary>
    /// Used to issue recovery requests to the feed
    /// </summary>
    internal class RecoveryRequestIssuer : IEventRecoveryRequestIssuer, IRecoveryRequestIssuer
    {
        /// <summary>
        /// A <see cref="ILogger"/> used for execution logging
        /// </summary>
        private static readonly ILogger ExecutionLog = SdkLoggerFactory.GetLoggerForExecution(typeof(RecoveryRequestIssuer));

        /// <summary>
        /// A <see cref="ILogger"/> used for execution logging
        /// </summary>
        private static readonly ILogger LogInt = SdkLoggerFactory.GetLoggerForClientInteraction(typeof(RecoveryRequestIssuer));

        /// <summary>
        /// A format of the URL used to request a recovery of state after the specified timestamp
        /// </summary>
        private const string AfterTimestampRecoveryUrlFormat = @"{0}recovery/initiate_request?after={1}&request_id={2}";

        /// <summary>
        /// A format of the URL used to request full odds recovery
        /// </summary>
        private const string FullOddsRecoveryUrlFormat = @"{0}recovery/initiate_request?request_id={1}";

        /// <summary>
        /// A url format used to request recovery of all messages for a specific event
        /// </summary>
        private const string EventMessagesRecoveryUrlFormat = @"{0}odds/events/{1}/initiate_request?request_id={2}";

        /// <summary>
        /// A url format used to request recovery of stateful messages for a specific event
        /// </summary>
        private const string EventStatefulMessagesRecoveryUrlFormat = @"{0}stateful_messages/events/{1}/initiate_request?request_id={2}";

        /// <summary>
        /// A <see cref="IDataPoster"/> instance used to issue the request to the feed API
        /// </summary>
        private readonly IDataPoster _dataPoster;

        /// <summary>
        /// A <see cref="ISequenceGenerator"/> used to generate the sequence numbers
        /// </summary>
        private readonly ISequenceGenerator _sequenceGenerator;

        /// <summary>
        /// Used to invoke RequestInitiated event
        /// </summary>
        private readonly ProducerManager _producerManager;

        /// <summary>
        /// The node identifier
        /// </summary>
        private readonly int _nodeId;

        /// <summary>
        /// Initializes a new instance of the <see cref="RecoveryRequestIssuer"/> class
        /// </summary>
        /// <param name="dataPoster">A <see cref="IDataPoster"/> instance used to issue the request to the feed API</param>
        /// <param name="sequenceGenerator">A <see cref="ISequenceGenerator"/> used to generate the sequence numbers</param>
        /// <param name="config">The <see cref="IUofConfiguration"/> used to get nodeId</param>
        /// <param name="producerManager">The <see cref="IProducerManager"/> used to invoke RequestInitiated event</param>
        public RecoveryRequestIssuer(IDataPoster dataPoster, ISequenceGenerator sequenceGenerator, IUofConfiguration config, IProducerManager producerManager)
        {
            Guard.Argument(dataPoster, nameof(dataPoster)).NotNull();
            Guard.Argument(sequenceGenerator, nameof(sequenceGenerator)).NotNull();
            Guard.Argument(config, nameof(config)).NotNull();
            Guard.Argument(producerManager, nameof(producerManager)).NotNull();

            _dataPoster = dataPoster;
            _sequenceGenerator = sequenceGenerator;
            _producerManager = (ProducerManager)producerManager;
            _nodeId = config.NodeId;
        }

        /// <summary>
        /// Asynchronously requests messages for the specified sport event and returns a request number used when issuing the request
        /// </summary>
        /// <param name="producer">An <see cref="IProducer"/> for which to make the recovery</param>
        /// <param name="eventId">A <see cref="Urn"/> specifying the sport event for which to request the messages</param>
        /// <returns> <see cref="Task{HttpStatusCode}"/> representing the async operation</returns>
        public async Task<long> RecoverEventMessagesAsync(IProducer producer, Urn eventId)
        {
            Guard.Argument(producer, nameof(producer)).NotNull();
            Guard.Argument(eventId, nameof(eventId)).NotNull();

            if (!producer.IsAvailable || producer.IsDisabled)
            {
                throw new ArgumentException($"Producer {producer} is not available or is disabled", nameof(producer));
            }

            LogInt.LogInformation("Invoked RecoverEventMessagesAsync: [Producer={ProducerId}, Urn={SportEventId}]", producer.Id, eventId);

            var requestNumber = _sequenceGenerator.GetNext();
            var myProducer = (Producer)producer;
            var url = string.Format(EventMessagesRecoveryUrlFormat, myProducer.ApiUrl, eventId, requestNumber);
            if (_nodeId != 0)
            {
                url = $"{url}&node_id={_nodeId}";
            }

            // await the result in case an exception is thrown
            var responseMessage = await _dataPoster.PostDataAsync(new Uri(url)).ConfigureAwait(false);
            if (!responseMessage.IsSuccessStatusCode)
            {
                ExecutionLog.LogError("Recovery request of event messages for Event={SportEventId}, RequestId={RequestId}, failed with StatusCode={ResponseStatusCode}", eventId, requestNumber, responseMessage.StatusCode);
                var message = $"Recovery request of event messages for Event={eventId}, RequestId={requestNumber}, failed with StatusCode={responseMessage.StatusCode}";
                _producerManager.InvokeRecoveryInitiated(new RecoveryInitiatedEventArgs(requestNumber, producer.Id, 0, eventId, message));
                throw new CommunicationException(message, url, responseMessage.StatusCode, null);
            }

            ExecutionLog.LogInformation("Recovery request of event messages for Event={SportEventId}, RequestId={RequestId} succeeded", eventId, requestNumber);
            myProducer.EventRecoveries.TryAdd(requestNumber, eventId);
            _producerManager.InvokeRecoveryInitiated(new RecoveryInitiatedEventArgs(requestNumber, producer.Id, 0, eventId, string.Empty));
            return requestNumber;
        }

        /// <summary>
        /// Asynchronously requests stateful messages for the specified sport event and returns a request number used when issuing the request
        /// </summary>
        /// <param name="producer">An <see cref="IProducer"/> for which to make the recovery</param>
        /// <param name="eventId">A <see cref="Urn"/> specifying the sport event for which to request the messages</param>
        /// <returns> <see cref="Task{HttpStatusCode}"/> representing the async operation</returns>
        public async Task<long> RecoverEventStatefulMessagesAsync(IProducer producer, Urn eventId)
        {
            Guard.Argument(producer, nameof(producer)).NotNull();
            Guard.Argument(eventId, nameof(eventId)).NotNull();

            if (!producer.IsAvailable || producer.IsDisabled)
            {
                throw new ArgumentException($"Producer {producer} is disabled in the SDK", nameof(producer));
            }

            LogInt.LogInformation("Invoked RecoverEventStatefulMessagesAsync: [Producer={ProducerId}, Urn={SportEventId}]", producer.Id, eventId);

            var requestNumber = _sequenceGenerator.GetNext();
            var myProducer = (Producer)producer;
            var url = string.Format(EventStatefulMessagesRecoveryUrlFormat, myProducer.ApiUrl, eventId, requestNumber);
            if (_nodeId != 0)
            {
                url = $"{url}&node_id={_nodeId}";
            }

            // await the result in case an exception is thrown
            var responseMessage = await _dataPoster.PostDataAsync(new Uri(url)).ConfigureAwait(false);
            if (!responseMessage.IsSuccessStatusCode)
            {
                ExecutionLog.LogError("Recovery of stateful event messages for Event={SportEventId}, RequestId={RequestId}, failed with StatusCode={ResponseStatusCode}", eventId, requestNumber, responseMessage.StatusCode);
                var message = $"Recovery of stateful event messages for Event={eventId}, RequestId={requestNumber}, failed with StatusCode={responseMessage.StatusCode}";
                _producerManager.InvokeRecoveryInitiated(new RecoveryInitiatedEventArgs(requestNumber, producer.Id, 0, eventId, message));
                throw new CommunicationException(message, url, responseMessage.StatusCode, null);
            }

            ExecutionLog.LogInformation("Recovery of stateful event messages for Event={SportEventId}, RequestId={RequestId} succeeded", eventId, requestNumber);
            myProducer.EventRecoveries.TryAdd(requestNumber, eventId);
            _producerManager.InvokeRecoveryInitiated(new RecoveryInitiatedEventArgs(requestNumber, producer.Id, 0, eventId, string.Empty));
            return requestNumber;
        }

        /// <summary>
        /// Asynchronously requests a recovery for the specified producer for changes which occurred after specified time
        /// </summary>
        /// <param name="producer">An <see cref="IProducer"/> for which to make the recovery</param>
        /// <param name="dateAfter">Specifies the time after which the changes should be recovered.</param>
        /// <param name="nodeId">The node id where recovery message will be processed</param>
        /// <returns><see cref="Task{Long}" /> representing a asynchronous method.
        /// Once the execution is complete it provides the request id associated with the recovery</returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task<long> RequestRecoveryAfterTimestampAsync(IProducer producer, DateTime dateAfter, int nodeId)
        {
            Guard.Argument(producer, nameof(producer)).NotNull();

            if (!producer.IsAvailable || producer.IsDisabled)
            {
                throw new ArgumentException($"Producer {producer} is disabled in the SDK", nameof(producer));
            }
            if (dateAfter > DateTime.Now)
            {
                throw new ArgumentException("Value of parameter dateAfter is not valid. It can not be in the future.", nameof(dateAfter));
            }
            if (dateAfter < DateTime.Now.Subtract(producer.MaxAfterAge()))
            {
                throw new ArgumentException($"Value of parameter dateAfter is not valid. It can not be older then {producer.MaxAfterAge()}", nameof(dateAfter));
            }

            LogInt.LogInformation("Invoked RequestRecoveryAfterTimestampAsync: [Producer={ProducerId}, DateAfter={DateAfter}, NodeId={NodeId}]", producer.Id, dateAfter, nodeId);

            var timestampAfter = SdkInfo.ToEpochTime(dateAfter);
            var requestNumber = _sequenceGenerator.GetNext();
            var url = string.Format(AfterTimestampRecoveryUrlFormat, ((Producer)producer).ApiUrl, timestampAfter, requestNumber);
            if (nodeId != 0)
            {
                url = $"{url}&node_id={nodeId}";
            }

            var timestampRequested = SdkInfo.ToEpochTime(DateTime.Now);
            HttpResponseMessage responseMessage = null;

            try
            {
                responseMessage = await _dataPoster.PostDataAsync(new Uri(url)).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                // ignored
                var _ = ex.Message;
            }
            finally
            {
                var responseCode = responseMessage == null ? 0 : (int)responseMessage.StatusCode;
                var responseMsg = responseMessage == null ? string.Empty : responseMessage.ReasonPhrase;
                var producerV1 = (Producer)producer;
                producerV1.RecoveryInfo = new RecoveryInfo(timestampAfter, timestampRequested, requestNumber, nodeId, responseCode, responseMsg);
            }

            if (responseMessage == null || !responseMessage.IsSuccessStatusCode)
            {
                ExecutionLog.LogError("Recovery since after timestamp for Producer={ProducerId}, RequestId={RequestId}, After={DateAfter} failed with StatusCode={ResponseStatusCode}",
                                      producer.Id,
                                      requestNumber,
                                      dateAfter,
                                      responseMessage?
                                         .StatusCode);
                var message = $"Recovery since after timestamp for Producer={producer.Id}, RequestId={requestNumber}, After={dateAfter} failed with StatusCode={responseMessage?.StatusCode}";
                _producerManager.InvokeRecoveryInitiated(new RecoveryInitiatedEventArgs(requestNumber, producer.Id, timestampAfter, null, message));
                throw new CommunicationException(message, url, responseMessage?.StatusCode ?? 0, null);
            }

            ExecutionLog.LogInformation("Recovery since after timestamp for Producer={Producer}, RequestId={RequestId}, After={DateAfter} succeeded", producer.Id, requestNumber, dateAfter);
            _producerManager.InvokeRecoveryInitiated(new RecoveryInitiatedEventArgs(requestNumber, producer.Id, timestampAfter, null, string.Empty));
            return requestNumber;
        }

        /// <summary>
        /// Asynchronously requests a recovery of current odds for specific producer
        /// </summary>
        /// <param name="producer">An <see cref="IProducer"/> for which to make the recovery</param>
        /// <param name="nodeId">The node id where recovery message will be processed</param>
        /// <returns><see cref="Task{Long}" /> representing a asynchronous method.
        /// Once the execution is complete it provides the request id associated with the recovery</returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task<long> RequestFullOddsRecoveryAsync(IProducer producer, int nodeId)
        {
            Guard.Argument(producer, nameof(producer)).NotNull();

            if (!producer.IsAvailable || producer.IsDisabled)
            {
                throw new ArgumentException($"Producer {producer} is disabled in the SDK", nameof(producer));
            }

            LogInt.LogInformation("Invoked RequestFullOddsRecoveryAsync: [Producer={ProducerId}, NodeId={NodeId}]", producer.Id, nodeId);

            var requestNumber = _sequenceGenerator.GetNext();
            var url = string.Format(FullOddsRecoveryUrlFormat, ((Producer)producer).ApiUrl, requestNumber);
            if (nodeId != 0)
            {
                url = $"{url}&node_id={nodeId}";
            }

            var timestampRequested = SdkInfo.ToEpochTime(DateTime.Now);
            HttpResponseMessage responseMessage = null;

            try
            {
                responseMessage = await _dataPoster.PostDataAsync(new Uri(url)).ConfigureAwait(false);
            }
            catch (Exception)
            {
                // ignored
            }
            finally
            {
                var responseCode = responseMessage == null ? 0 : (int)responseMessage.StatusCode;
                var responseMsg = responseMessage == null ? string.Empty : responseMessage.ReasonPhrase;
                var producerV1 = (Producer)producer;
                producerV1.RecoveryInfo = new RecoveryInfo(0, timestampRequested, requestNumber, nodeId, responseCode, responseMsg);
            }

            if (responseMessage == null || !responseMessage.IsSuccessStatusCode)
            {
                ExecutionLog.LogError("Full odds recovery for Producer={ProducerId}, RequestId={RequestId}, failed with StatusCode={ResponseStatusCode}", producer.Id, requestNumber, responseMessage?.StatusCode);
                var message = $"Full odds recovery for Producer={producer.Id}, RequestId={requestNumber}, failed with StatusCode={responseMessage?.StatusCode}";
                _producerManager.InvokeRecoveryInitiated(new RecoveryInitiatedEventArgs(requestNumber, producer.Id, 0, null, message));
                throw new CommunicationException(message, url, responseMessage?.StatusCode ?? 0, null);
            }

            ExecutionLog.LogInformation("Full odds recovery for Producer={ProducerId}, RequestId={RequestId} succeeded", producer.Id, requestNumber);
            _producerManager.InvokeRecoveryInitiated(new RecoveryInitiatedEventArgs(requestNumber, producer.Id, 0, null, string.Empty));
            return requestNumber;
        }
    }
}
