/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using System.Collections.Generic;
using Dawn;
using System.IO;
using RabbitMQ.Client.Events;
using Sportradar.OddsFeed.SDK.Common.Internal;
using Sportradar.OddsFeed.SDK.Entities.Internal;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal;
using Sportradar.OddsFeed.SDK.Messages.Feed;

// ReSharper disable UnusedMember.Local
// ReSharper disable UnassignedGetOnlyAutoProperty
// ReSharper disable UnusedMember.Global

namespace Sportradar.OddsFeed.SDK.Entities.Test
{
    internal class FakeRabbitMqChannel : IRabbitMqChannel
    {
        private readonly IDeserializer<FeedMessage> _deserializer;

        private readonly IDataFetcher _dataFetcher;

        private readonly string _dirPath;

        public FakeRabbitMqChannel(IDataFetcher dataFetcher, IDeserializer<FeedMessage> deserializer, string dirPath)
        {
            Guard.Argument(deserializer, nameof(deserializer)).NotNull();
            Guard.Argument(_dataFetcher, nameof(_dataFetcher)).NotNull();
            Guard.Argument(dirPath, nameof(dirPath)).NotNull().NotEmpty();

            if (!Directory.Exists(dirPath))
            {
                throw new ArgumentException(nameof(dirPath), $"Directory {dirPath} does not exist");
            }
            _deserializer = deserializer;
            _dataFetcher = dataFetcher;
            _dirPath = dirPath;
        }

        private async void StartDispatching()
        {
            var files = Directory.GetFiles(_dirPath);
            foreach (var file in files)
            {
                var stream = await _dataFetcher.GetDataAsync(new Uri(file, UriKind.Absolute));
                var unused = _deserializer.Deserialize(stream);
            }
        }

        public bool IsOpened { get; }

        public void Open(MessageInterest interest, IEnumerable<string> routingKeys)
        {

        }

        public void Close()
        {
            throw new NotImplementedException();
        }

        public event EventHandler<BasicDeliverEventArgs> Received;

        protected virtual void OnReceived(BasicDeliverEventArgs e)
        {
            Received?.Invoke(this, e);
        }
    }
}
