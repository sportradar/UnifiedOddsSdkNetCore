// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using System.Threading.Tasks;
using Sportradar.OddsFeed.SDK.Api.EventArguments;
using Sportradar.OddsFeed.SDK.Api.Internal;
using Sportradar.OddsFeed.SDK.Api.Internal.ApiAccess;

namespace Sportradar.OddsFeed.SDK.Entities.Rest.Internal
{
    internal class ExecutionPathDataProvider<TOut> : IExecutionPathDataProvider<TOut>, IDisposable where TOut : class
    {
        private readonly IDataProvider<TOut> _criticalDataProvider;

        private readonly IDataProvider<TOut> _nonCriticalDataProvider;

        public ExecutionPathDataProvider(IDataProvider<TOut> criticalDataProvider, IDataProvider<TOut> nonCriticalDataProvider)
        {
            _criticalDataProvider = criticalDataProvider;
            _nonCriticalDataProvider = nonCriticalDataProvider;

            _criticalDataProvider.RawApiDataReceived += DataProviderOnRawApiDataReceived;
            _nonCriticalDataProvider.RawApiDataReceived += DataProviderOnRawApiDataReceived;
        }

        public event EventHandler<RawApiDataEventArgs> RawApiDataReceived;

        public async Task<TOut> GetDataAsync(RequestOptions requestOptions, params string[] identifiers)
        {
            if (requestOptions.ExecutionPath == ExecutionPath.TimeCritical)
            {
                return await _criticalDataProvider.GetDataAsync(identifiers);
            }

            return await _nonCriticalDataProvider.GetDataAsync(identifiers);
        }

        public void Dispose()
        {
            _criticalDataProvider.RawApiDataReceived -= DataProviderOnRawApiDataReceived;
            _nonCriticalDataProvider.RawApiDataReceived -= DataProviderOnRawApiDataReceived;
        }

        private void DataProviderOnRawApiDataReceived(object sender, RawApiDataEventArgs e)
        {
            RawApiDataReceived?.Invoke(this, e);
        }
    }
}
