/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Common.Logging;
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
        private readonly ILog _clientLog = SdkLoggerFactory.GetLoggerForClientInteraction(typeof(CustomBetManager));
        private readonly ILog _executionLog = SdkLoggerFactory.GetLoggerForExecution(typeof(CustomBetManager));

        private readonly IDataRouterManager _dataRouterManager;
        private readonly ICustomBetSelectionBuilder _customBetSelectionBuilder;

        /// <summary>
        /// Initializes a new instance of the <see cref="CustomBetManager"/> class
        /// </summary>
        /// <param name="dataRouterManager">A <see cref="IDataRouterManager"/> used to make custom bet API requests</param>
        /// <param name="customBetSelectionBuilder">A <see cref="ICustomBetSelectionBuilder"/> used to build selections</param>
        public CustomBetManager(IDataRouterManager dataRouterManager, ICustomBetSelectionBuilder customBetSelectionBuilder)
        {
            if (dataRouterManager == null)
                throw new ArgumentNullException(nameof(dataRouterManager));
            if (customBetSelectionBuilder == null)
                throw new ArgumentNullException(nameof(customBetSelectionBuilder));

            _dataRouterManager = dataRouterManager;
            CustomBetSelectionBuilder = customBetSelectionBuilder;
        }

        public async Task<IAvailableSelections> GetAvailableSelectionsAsync(URN eventId)
        {
            if (eventId == null)
                throw new ArgumentNullException(nameof(eventId));

            try
            {
                _clientLog.Info($"Invoking CustomBetManager.GetAvailableSelectionsAsync({eventId})");
                return await _dataRouterManager.GetAvailableSelectionsAsync(eventId).ConfigureAwait(false);
            }
            catch (CommunicationException ce)
            {
                _executionLog.Warn($"Event[{eventId}] getting available selections failed, CommunicationException: {ce.Message}");
                throw;
            }
            catch (Exception e)
            {
                _executionLog.Warn($"Event[{eventId}] getting available selections failed.", e);
                throw;
            }
        }

        public async Task<ICalculation> CalculateProbabilityAsync(IEnumerable<ISelection> selections)
        {
            if (selections == null)
                throw new ArgumentNullException(nameof(selections));

            try
            {
                _clientLog.Info($"Invoking CustomBetManager.CalculateProbability({selections})");
                return await _dataRouterManager.CalculateProbability(selections).ConfigureAwait(false);
            }
            catch (CommunicationException ce)
            {
                _executionLog.Warn($"Calculating probabilities failed, CommunicationException: {ce.Message}");
                throw;
            }
            catch (Exception e)
            {
                _executionLog.Warn($"Calculating probabilities failed.", e);
                throw;
            }
        }

        public ICustomBetSelectionBuilder CustomBetSelectionBuilder { get; }
    }
}
