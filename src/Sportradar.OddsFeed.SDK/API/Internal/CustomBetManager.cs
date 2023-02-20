/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using System.Collections.Generic;
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

        public ICustomBetSelectionBuilder CustomBetSelectionBuilder { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="CustomBetManager"/> class
        /// </summary>
        /// <param name="dataRouterManager">A <see cref="IDataRouterManager"/> used to make custom bet API requests</param>
        /// <param name="customBetSelectionBuilder">A <see cref="ICustomBetSelectionBuilder"/> used to build selections</param>
        public CustomBetManager(IDataRouterManager dataRouterManager, ICustomBetSelectionBuilder customBetSelectionBuilder)
        {
            _dataRouterManager = dataRouterManager ?? throw new ArgumentNullException(nameof(dataRouterManager));
            CustomBetSelectionBuilder = customBetSelectionBuilder ?? throw new ArgumentNullException(nameof(customBetSelectionBuilder));
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
                _executionLog.LogWarning(ce, $"Event[{eventId}] getting available selections failed, CommunicationException: {ce.Message}");
                throw;
            }
            catch (Exception e)
            {
                _executionLog.LogWarning(e, $"Event[{eventId}] getting available selections failed.");
                throw;
            }
        }

        public Task<ICalculation> CalculateProbabilityAsync(IEnumerable<ISelection> selections)
        {
            if (selections == null)
            {
                throw new ArgumentNullException(nameof(selections));
            }

            return CalculateProbabilityInternalAsync(selections);
        }

        private async Task<ICalculation> CalculateProbabilityInternalAsync(IEnumerable<ISelection> selections)
        {
            try
            {
                _clientLog.LogInformation($"Invoking CustomBetManager.CalculateProbability({selections})");
                return await _dataRouterManager.CalculateProbabilityAsync(selections).ConfigureAwait(false);
            }
            catch (CommunicationException ce)
            {
                _executionLog.LogWarning(ce, $"Calculating probabilities failed, CommunicationException: {ce.Message}");
                throw;
            }
            catch (Exception e)
            {
                _executionLog.LogWarning(e, "Calculating probabilities failed.");
                throw;
            }
        }

        public Task<ICalculationFilter> CalculateProbabilityFilterAsync(IEnumerable<ISelection> selections)
        {
            if (selections == null)
            {
                throw new ArgumentNullException(nameof(selections));
            }

            return CalculateProbabilityFilterInternalAsync(selections);
        }

        private async Task<ICalculationFilter> CalculateProbabilityFilterInternalAsync(IEnumerable<ISelection> selections)
        {
            try
            {
                _clientLog.LogInformation($"Invoking CustomBetManager.CalculateProbabilityFilter({selections})");
                return await _dataRouterManager.CalculateProbabilityFilteredAsync(selections).ConfigureAwait(false);
            }
            catch (CommunicationException ce)
            {
                _executionLog.LogWarning(ce, $"Calculating probabilities filtered failed, CommunicationException: {ce.Message}");
                throw;
            }
            catch (Exception e)
            {
                _executionLog.LogWarning(e, "Calculating probabilities filtered failed.");
                throw;
            }
        }
    }
}
