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
using Sportradar.OddsFeed.SDK.Common.Internal.Telemetry;
using Sportradar.OddsFeed.SDK.Entities.Rest.CustomBet;

namespace Sportradar.OddsFeed.SDK.Api.Internal.Managers
{
    /// <summary>
    /// The run-time implementation of the <see cref="ICustomBetManager"/> interface
    /// </summary>
    internal class CustomBetManager : ICustomBetManager
    {
        private readonly ILogger _clientLog = SdkLoggerFactory.GetLoggerForClientInteraction(typeof(CustomBetManager));
        private readonly ILogger _executionLog = SdkLoggerFactory.GetLoggerForExecution(typeof(CustomBetManager));

        private readonly IDataRouterManager _dataRouterManager;
        private readonly ICustomBetSelectionBuilderFactory _customBetSelectionBuilderFactory;
        private readonly ExceptionHandlingStrategy _exceptionHandlingStrategy;

        public ICustomBetSelectionBuilder CustomBetSelectionBuilder => _customBetSelectionBuilderFactory.CreateBuilder();

        public CustomBetManager(IDataRouterManager dataRouterManager, IUofConfiguration config, ICustomBetSelectionBuilderFactory customBetSelectionBuilderFactory)
        {
            _dataRouterManager = dataRouterManager ?? throw new ArgumentNullException(nameof(dataRouterManager));
            _customBetSelectionBuilderFactory = customBetSelectionBuilderFactory ?? throw new ArgumentNullException(nameof(customBetSelectionBuilderFactory));
            _exceptionHandlingStrategy = config.ExceptionHandlingStrategy;
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

        public Task<ICalculation> CalculateProbabilityAsync(IEnumerable<ISelection> selections)
        {
            if (selections == null)
            {
                throw new ArgumentNullException(nameof(selections));
            }

            return CalculateProbabilityInternalAsync(selections.ToList());
        }

        private async Task<ICalculation> CalculateProbabilityInternalAsync(IReadOnlyCollection<ISelection> selections)
        {
            try
            {
                var selectionStr = string.Join("|", selections);
                _clientLog.LogInformation("Invoking CustomBetManager.CalculateProbability({Selections})", selectionStr);
                return await _dataRouterManager.CalculateProbabilityAsync(selections).ConfigureAwait(false);
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

            return CalculateProbabilityFilterInternalAsync(selections.ToList());
        }

        private async Task<ICalculationFilter> CalculateProbabilityFilterInternalAsync(IReadOnlyCollection<ISelection> selections)
        {
            try
            {
                var selectionStr = string.Join("|", selections);
                _clientLog.LogInformation("Invoking CustomBetManager.CalculateProbabilityFilter({Selections})", selectionStr);
                return await _dataRouterManager.CalculateProbabilityFilteredAsync(selections).ConfigureAwait(false);
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
