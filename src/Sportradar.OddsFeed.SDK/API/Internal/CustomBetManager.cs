/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Common.Exceptions;
using Sportradar.OddsFeed.SDK.Entities.REST.CustomBet;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal;
using Sportradar.OddsFeed.SDK.Messages;

namespace Sportradar.OddsFeed.SDK.API.Internal
{
    /// <summary>
    /// The run-time implementation of the <see cref="ICustomBetManager"/> interface
    /// </summary>
    internal class CustomBetManager : ICustomBetManager
    {
        private readonly ILogger _clientLog = SdkLoggerFactory.GetLoggerForClientInteraction(typeof(CustomBetManager));
        private readonly ILogger _executionLog = SdkLoggerFactory.GetLoggerForExecution(typeof(CustomBetManager));

        private readonly IDataRouterManager _dataRouterManager;
        private readonly ExceptionHandlingStrategy _exceptionHandlingStrategy;

        public ICustomBetSelectionBuilder CustomBetSelectionBuilder { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="CustomBetManager"/> class
        /// </summary>
        /// <param name="dataRouterManager">A <see cref="IDataRouterManager"/> used to make custom bet API requests</param>
        /// <param name="customBetSelectionBuilder">A <see cref="ICustomBetSelectionBuilder"/> used to build selections</param>
        /// <param name="exceptionHandlingStrategy">The <see cref="ExceptionHandlingStrategy"/> used to handle exceptions</param>
        public CustomBetManager(IDataRouterManager dataRouterManager, ICustomBetSelectionBuilder customBetSelectionBuilder, ExceptionHandlingStrategy exceptionHandlingStrategy)
        {
            _dataRouterManager = dataRouterManager ?? throw new ArgumentNullException(nameof(dataRouterManager));
            CustomBetSelectionBuilder = customBetSelectionBuilder ?? throw new ArgumentNullException(nameof(customBetSelectionBuilder));
            _exceptionHandlingStrategy = exceptionHandlingStrategy;
        }

        public Task<IAvailableSelections> GetAvailableSelectionsAsync(URN eventId)
        {
            if (eventId == null)
            {
                throw new ArgumentNullException(nameof(eventId));
            }

            return GetAvailableSelectionsInternalAsync(eventId);
        }

        private async Task<IAvailableSelections> GetAvailableSelectionsInternalAsync(URN eventId)
        {
            try
            {
                _clientLog.LogInformation($"Invoking CustomBetManager.GetAvailableSelectionsAsync({eventId})");
                return await _dataRouterManager.GetAvailableSelectionsAsync(eventId).ConfigureAwait(false);
            }
            catch (CommunicationException ce)
            {
                _executionLog.LogWarning($"Event[{eventId}] getting available selections failed. API response code: {ce.ResponseCode}, message: {ce.Message}");
                if (_exceptionHandlingStrategy == ExceptionHandlingStrategy.THROW)
                {
                    throw;
                }
            }
            catch (Exception e)
            {
                _executionLog.LogWarning(e, $"Event[{eventId}] getting available selections failed.");
                if (_exceptionHandlingStrategy == ExceptionHandlingStrategy.THROW)
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
                _clientLog.LogInformation($"Invoking CustomBetManager.CalculateProbability({selectionStr})");
                return await _dataRouterManager.CalculateProbabilityAsync(selections).ConfigureAwait(false);
            }
            catch (CommunicationException ce)
            {
                _executionLog.LogWarning($"Calculating probabilities failed. API response code: {ce.ResponseCode}, message: {ce.Message}");
                if (_exceptionHandlingStrategy == ExceptionHandlingStrategy.THROW)
                {
                    throw;
                }
            }
            catch (Exception e)
            {
                _executionLog.LogWarning(e, "Calculating probabilities failed.");
                if (_exceptionHandlingStrategy == ExceptionHandlingStrategy.THROW)
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
                _clientLog.LogInformation($"Invoking CustomBetManager.CalculateProbabilityFilter({selectionStr})");
                return await _dataRouterManager.CalculateProbabilityFilteredAsync(selections).ConfigureAwait(false);
            }
            catch (CommunicationException ce)
            {
                _executionLog.LogWarning($"Calculating probabilities filtered failed. API response code: {ce.ResponseCode}, message: {ce.Message}");
                if (_exceptionHandlingStrategy == ExceptionHandlingStrategy.THROW)
                {
                    throw;
                }
            }
            catch (Exception e)
            {
                _executionLog.LogWarning(e, "Calculating probabilities filtered failed.");
                if (_exceptionHandlingStrategy == ExceptionHandlingStrategy.THROW)
                {
                    throw;
                }
            }

            return null;
        }
    }
}
