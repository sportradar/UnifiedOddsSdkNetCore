using System;
using System.Collections.Generic;
using RabbitMQ.Client;

namespace Sportradar.OddsFeed.SDK.Tests.Api.FeedAccess;
internal class StubConnectionFactory : IConnectionFactory
{
    public IAuthMechanismFactory AuthMechanismFactory(IList<string> mechanismNames)
    {
        throw new NotImplementedException();
    }

    public IConnection CreateConnection()
    {
        return new StubConnection();
    }

    public IConnection CreateConnection(string clientProvidedName)
    {
        throw new NotImplementedException();
    }

    public IConnection CreateConnection(IList<string> hostnames)
    {
        throw new NotImplementedException();
    }

    public IConnection CreateConnection(IList<string> hostnames, string clientProvidedName)
    {
        throw new NotImplementedException();
    }

    public IConnection CreateConnection(IList<AmqpTcpEndpoint> endpoints)
    {
        throw new NotImplementedException();
    }

    public IConnection CreateConnection(IList<AmqpTcpEndpoint> endpoints, string clientProvidedName)
    {
        throw new NotImplementedException();
    }

    public IDictionary<string, object> ClientProperties { get; set; } = new Dictionary<string, object>();
    public string Password { get; set; }
    public ushort RequestedChannelMax { get; set; }
    public uint RequestedFrameMax { get; set; }
    public TimeSpan RequestedHeartbeat { get; set; }
    public bool UseBackgroundThreadsForIO { get; set; }
    public string UserName { get; set; }
    public string VirtualHost { get; set; }
    public Uri Uri { get; set; }
    public string ClientProvidedName { get; set; }
    public TimeSpan HandshakeContinuationTimeout { get; set; }
    public TimeSpan ContinuationTimeout { get; set; }
}
