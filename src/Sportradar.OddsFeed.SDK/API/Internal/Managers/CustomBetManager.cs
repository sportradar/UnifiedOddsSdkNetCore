// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Sportradar.OddsFeed.SDK.Api.Config;
using Sportradar.OddsFeed.SDK.Api.Internal.ApiAccess;
using Sportradar.OddsFeed.SDK.Api.Managers;
using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Common.Enums;
using Sportradar.OddsFeed.SDK.Common.Exceptions;
using Sportradar.OddsFeed.SDK.Entities.Rest.CustomBet;

namespace Sportradar.OddsFeed.SDK.Api.Internal.Managers
{
    /// <summary>
    /// The run-time implementation of the <see cref="ICustomBetManager"/> and <see cref="ICustomBetManagerV2"/> interfaces
    /// </summary>
    internal class CustomBetManager : ICustomBetManagerV2
    {
        private readonly ILogger _clientLog;
        private readonly ILogger _executionLog;

        private readonly IDataRouterManager _dataRouterManager;
        private readonly ICustomBetSelectionBuilderFactory _customBetSelectionBuilderFactory;
        private readonly ExceptionHandlingStrategy _exceptionHandlingStrategy;
        private readonly int _bookmakerId;

        public ICustomBetSelectionBuilder CustomBetSelectionBuilder => _customBetSelectionBuilderFactory.CreateBuilder();

        public IPrebuiltBetsRequestBuilder PrebuiltBetsRequestBuilder => new PrebuiltBetsRequestBuilder(_bookmakerId);

        public CustomBetManager(IDataRouterManager dataRouterManager,
            IUofConfiguration config,
            ICustomBetSelectionBuilderFactory customBetSelectionBuilderFactory,
            ILogger clientLog,
            ILogger executionLog)
        {
            _clientLog = clientLog ?? throw new ArgumentNullException(nameof(clientLog));
            _executionLog = executionLog ?? throw new ArgumentNullException(nameof(executionLog));

            _dataRouterManager = dataRouterManager ?? throw new ArgumentNullException(nameof(dataRouterManager));
            _customBetSelectionBuilderFactory = customBetSelectionBuilderFactory ?? throw new ArgumentNullException(nameof(customBetSelectionBuilderFactory));
            _exceptionHandlingStrategy = config.ExceptionHandlingStrategy;
            _bookmakerId = config.BookmakerDetails.BookmakerId;
        }

        public ICalculateRequestBuilder GetCalculateRequestBuilder()
        {
            return new CalculateRequestBuilder();
        }

        public Task<IAvailableSelections> GetAvailableSelectionsAsync(Urn eventId)
        {
            if (eventId == null)
            {
                throw new ArgumentNullException(nameof(eventId));
            }

            return GetAvailableSelectionsInternalAsync(eventId);
        }

        private async Task<IAvailableSelections> GetAvailableSelectionsInternalAsync(Urn eventId)
        {
            try
            {
                _clientLog.LogInformation("Invoking CustomBetManager.GetAvailableSelectionsAsync({SportEventId})", eventId);
                return await _dataRouterManager.GetAvailableSelectionsAsync(eventId).ConfigureAwait(false);
            }
            catch (CommunicationException ce)
            {
                _executionLog.LogWarning("Event[{SportEventId}] getting available selections failed. API response code: {ErrorResponseCode}, message: {ErrorMessage}", eventId, ce.ResponseCode, ce.Message);
                if (_exceptionHandlingStrategy == ExceptionHandlingStrategy.Throw)
                {
                    throw;
                }
            }
            catch (Exception e)
            {
                _executionLog.LogWarning(e, "Event[{SportEventId}] getting available selections failed", eventId);
                if (_exceptionHandlingStrategy == ExceptionHandlingStrategy.Throw)
                {
                    throw;
                }
            }
            return null;
        }

        public async Task<IPrebuiltBets> GetPrebuiltBets(IPrebuiltBetsRequest prebuiltBetsRequest)
        {
            try
            {
                _clientLog.LogInformation("Invoking CustomBetManager.GetPrebuiltBets({SportEventId})", prebuiltBetsRequest.EventId);
                return await _dataRouterManager.RequestCustomBetPrebuiltBets(prebuiltBetsRequest).ConfigureAwait(false);
            }
            catch (CommunicationException ce)
            {
                _executionLog.LogError("Getting prebuilt bets failed. API response code: {ErrorResponseCode}, message: {ErrorMessage}", ce.ResponseCode, ce.Message);
                if (_exceptionHandlingStrategy == ExceptionHandlingStrategy.Throw)
                {
                    throw;
                }
            }
            catch (Exception e)
            {
                _executionLog.LogError("Getting prebuilt bets failed with message: {ErrorMessage}", e.Message);
                if (_exceptionHandlingStrategy == ExceptionHandlingStrategy.Throw)
                {
                    throw;
                }
            }

            return null;
        }

        public Task<ICalculation> CalculateProbabilityAsync(IEnumerable<ISelection> selections)
        {
            if (selections == null)
            {
                throw new ArgumentNullException(nameof(selections));
            }

            var builder = new CalculateRequestBuilder();
            foreach (var selection in selections)
            {
                builder.AndSelection(selection);
            }

            return CalculateProbabilityInternalAsync(builder.Build());
        }

        public Task<ICalculation> CalculateProbabilityAsync(ICalculateRequestBuilder request)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            var concreteBuilder = request as CalculateRequestBuilder;
            if (concreteBuilder == null)
            {
                throw new ArgumentException($"The provided {nameof(request)} must be an instance created by {nameof(GetCalculateRequestBuilder)}.", nameof(request));
            }

            return CalculateProbabilityInternalAsync(concreteBuilder.Build());
        }

        private async Task<ICalculation> CalculateProbabilityInternalAsync(CalculateRequest request)
        {
            try
            {
                var selectionStr = string.Join("|", request.Items.SelectMany(i => i.Selections));
                _clientLog.LogInformation("Invoking CustomBetManager.CalculateProbability({Selections})", selectionStr);
                return await _dataRouterManager.CalculateProbabilityAsync(request).ConfigureAwait(false);
            }
            catch (CommunicationException ce)
            {
                _executionLog.LogWarning("Calculating probabilities failed. API response code: {ErrorResponseCode}, message: {ErrorMessage}", ce.ResponseCode, ce.Message);
                if (_exceptionHandlingStrategy == ExceptionHandlingStrategy.Throw)
                {
                    throw;
                }
            }
            catch (Exception e)
            {
                _executionLog.LogWarning(e, "Calculating probabilities failed");
                if (_exceptionHandlingStrategy == ExceptionHandlingStrategy.Throw)
                {
                    throw;
                }
            }

            return null;
        }

        public Task<ICalculationFilter> CalculateProbabilityFilterAsync(IEnumerable<ISelection> selections)
        {
            if (selections == null)
            {
                throw new ArgumentNullException(nameof(selections));
            }

            var builder = new CalculateRequestBuilder();
            foreach (var selection in selections)
            {
                builder.AndSelection(selection);
            }

            return CalculateProbabilityFilterInternalAsync(builder.Build());
        }

        public Task<ICalculationFilter> CalculateProbabilityFilterAsync(ICalculateRequestBuilder request)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            var concreteBuilder = request as CalculateRequestBuilder;
            if (concreteBuilder == null)
            {
                throw new ArgumentException($"The provided {nameof(request)} must be an instance created by {nameof(GetCalculateRequestBuilder)}.", nameof(request));
            }

            return CalculateProbabilityFilterInternalAsync(concreteBuilder.Build());
        }

        private async Task<ICalculationFilter> CalculateProbabilityFilterInternalAsync(CalculateRequest request)
        {
            try
            {
                var selectionStr = string.Join("|", request.Items.SelectMany(i => i.Selections));
                _clientLog.LogInformation("Invoking CustomBetManager.CalculateProbabilityFilter({Selections})", selectionStr);
                return await _dataRouterManager.CalculateProbabilityFilteredAsync(request).ConfigureAwait(false);
            }
            catch (CommunicationException ce)
            {
                _executionLog.LogWarning("Calculating probabilities filtered failed. API response code: {ErrorResponseCode}, message: {ErrorMessage}", ce.ResponseCode, ce.Message);
                if (_exceptionHandlingStrategy == ExceptionHandlingStrategy.Throw)
                {
                    throw;
                }
            }
            catch (Exception e)
            {
                _executionLog.LogWarning(e, "Calculating probabilities filtered failed");
                if (_exceptionHandlingStrategy == ExceptionHandlingStrategy.Throw)
                {
                    throw;
                }
            }

            return null;
        }
    }
}
