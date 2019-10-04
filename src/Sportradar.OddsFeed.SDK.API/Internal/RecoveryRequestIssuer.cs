/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using System.Diagnostics.Contracts;
using System.Net.Http;
using System.Threading.Tasks;
using Sportradar.OddsFeed.SDK.Common.Exceptions;
using Sportradar.OddsFeed.SDK.Common.Internal;
using Sportradar.OddsFeed.SDK.Common.Internal.Log;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal;
using Sportradar.OddsFeed.SDK.Messages;
using Sportradar.OddsFeed.SDK.Messages.Internal;

namespace Sportradar.OddsFeed.SDK.API.Internal
{
    /// <summary>
    /// Used to issue recovery requests to the feed
    /// </summary>
    [Log(LoggerType.ClientInteraction)]
    internal class RecoveryRequestIssuer : MarshalByRefObject, IEventRecoveryRequestIssuer, IRecoveryRequestIssuer
    {
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
        /// The node identifier
        /// </summary>
        private readonly int _nodeId;

        /// <summary>
        /// Initializes a new instance of the <see cref="RecoveryRequestIssuer"/> class
        /// </summary>
        /// <param name="dataPoster">A <see cref="IDataPoster"/> instance used to issue the request to the feed API</param>
        /// <param name="sequenceGenerator">A <see cref="ISequenceGenerator"/> used to generate the sequence numbers</param>
        /// <param name="config">The <see cref="IOddsFeedConfiguration"/> used to get nodeId</param>
        public RecoveryRequestIssuer(IDataPoster dataPoster, ISequenceGenerator sequenceGenerator, IOddsFeedConfiguration config)
        {
            Contract.Requires(dataPoster != null);
            Contract.Requires(sequenceGenerator != null);
            Contract.Requires(config != null);

            _dataPoster = dataPoster;
            _sequenceGenerator = sequenceGenerator;
            _nodeId = config.NodeId;
        }

        /// <summary>
        /// Defined field invariants needed by code contracts
        /// </summary>
        [ContractInvariantMethod]
        private void ObjectInvariant()
        {
            Contract.Invariant(_sequenceGenerator != null);
            Contract.Invariant(_dataPoster != null);
        }

        /// <summary>
        /// Asynchronously requests messages for the specified sport event and returns a request number used when issuing the request
        /// </summary>
        /// <param name="producer">An <see cref="IProducer"/> for which to make the recovery</param>
        /// <param name="eventId">A <see cref="URN"/> specifying the sport event for which to request the messages</param>
        /// <returns> <see cref="Task{HttpStatusCode}"/> representing the async operation</returns>
        public async Task<long> RecoverEventMessagesAsync(IProducer producer, URN eventId)
        {
            if (!producer.IsAvailable || producer.IsDisabled)
            {
                throw new ArgumentException($"Producer {producer} is disabled in the SDK", nameof(producer));
            }
            var requestNumber = _sequenceGenerator.GetNext();
            var myProducer = (Producer) producer;
            var url = string.Format(EventMessagesRecoveryUrlFormat, myProducer.ApiUrl, eventId, requestNumber);
            if (_nodeId != 0)
            {
                url = $"{url}&node_id={_nodeId}";
            }

            // await the result in case an exception is thrown
            var responseMessage = await _dataPoster.PostDataAsync(new Uri(url)).ConfigureAwait(false);
            if (!responseMessage.IsSuccessStatusCode)
            {
                throw new CommunicationException($"Recovery of event messages for Event={eventId}, RequestId={requestNumber}, failed with StatusCode={responseMessage.StatusCode}",
                                                url,
                                                responseMessage.StatusCode,
                                                null);
            }

            myProducer.EventRecoveries.TryAdd(requestNumber, eventId);
            return requestNumber;
        }

        /// <summary>
        /// Asynchronously requests stateful messages for the specified sport event and returns a request number used when issuing the request
        /// </summary>
        /// <param name="producer">An <see cref="IProducer"/> for which to make the recovery</param>
        /// <param name="eventId">A <see cref="URN"/> specifying the sport event for which to request the messages</param>
        /// <returns> <see cref="Task{HttpStatusCode}"/> representing the async operation</returns>
        public async Task<long> RecoverEventStatefulMessagesAsync(IProducer producer, URN eventId)
        {
            if (!producer.IsAvailable || producer.IsDisabled)
            {
                throw new ArgumentException($"Producer {producer} is disabled in the SDK", nameof(producer));
            }

            var requestNumber = _sequenceGenerator.GetNext();
            var myProducer = (Producer) producer;
            var url = string.Format(EventStatefulMessagesRecoveryUrlFormat, myProducer.ApiUrl, eventId, requestNumber);
            if (_nodeId != 0)
            {
                url = $"{url}&node_id={_nodeId}";
            }

            // await the result in case an exception is thrown
            var responseMessage = await _dataPoster.PostDataAsync(new Uri(url)).ConfigureAwait(false);
            if (!responseMessage.IsSuccessStatusCode)
            {
                throw new CommunicationException($"Recovery of stateful event messages for Event={eventId}, RequestId={requestNumber}, failed with StatusCode={responseMessage.StatusCode}",
                                                url,
                                                responseMessage.StatusCode,
                                                null);
            }

            myProducer.EventRecoveries.TryAdd(requestNumber, eventId);
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
            catch (Exception)
            {
                // ignored
            }
            finally
            {
                var responseCode = responseMessage == null ? 0 : (int) responseMessage.StatusCode;
                var responseMsg = responseMessage == null ? string.Empty : responseMessage.ReasonPhrase;
                var producerV1 = (Producer) producer;
                producerV1.RecoveryInfo = new RecoveryInfo(timestampAfter, timestampRequested, requestNumber, nodeId, responseCode, responseMsg);
            }

            if (responseMessage == null || !responseMessage.IsSuccessStatusCode)
            {
                throw new CommunicationException($"Recovery since after timestamp for Producer={producer}, RequestId={requestNumber}, After={dateAfter} failed with StatusCode={responseMessage?.StatusCode}",
                                                url,
                                                responseMessage?.StatusCode ?? 0,
                                                null);
            }
            return requestNumber;
        }

        /// <summary>
        /// Asynchronously requests a recovery of current odds for specific producer
        /// </summary>
        /// <param name="producer">An <see cref="IProducer"/> for which to make the recovery</param>
        /// <param name="nodeId">The node id where recovery message will be processed</param>
        /// <returns><see cref="Task{Long}" /> representing a asynchronous method.
        /// Once the execution is complete it provides the request id associated with the recovery</returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public async Task<long> RequestFullOddsRecoveryAsync(IProducer producer, int nodeId)
        {
            if (!producer.IsAvailable || producer.IsDisabled)
            {
                throw new ArgumentException($"Producer {producer} is disabled in the SDK", nameof(producer));
            }

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
                var responseCode = responseMessage == null ? 0 : (int) responseMessage.StatusCode;
                var responseMsg = responseMessage == null ? string.Empty : responseMessage.ReasonPhrase;
                var producerV1 = (Producer) producer;
                producerV1.RecoveryInfo = new RecoveryInfo(0, timestampRequested, requestNumber, nodeId, responseCode, responseMsg);
            }

            if (responseMessage == null || !responseMessage.IsSuccessStatusCode)
            {
                throw new CommunicationException($"Recovery since after timestamp for Producer={producer}, RequestId={requestNumber}, failed with StatusCode={responseMessage?.StatusCode}",
                                                url,
                                                responseMessage?.StatusCode ?? 0,
                                                null);
            }

            return requestNumber;
        }
    }
}