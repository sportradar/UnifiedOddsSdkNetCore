// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using System.Collections.Generic;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Sportradar.OddsFeed.SDK.Tests.Api.FeedAccess;

internal class StubConnection : IConnection
{
    private bool _onCloseThrow = false;

    public int LocalPort { get; }
    public int RemotePort { get; }
    public void Dispose()
    {

    }

    public void PrepareForThrowOnClose()
    {
        _onCloseThrow = true;
    }

    public void UpdateSecret(string newSecret, string reason)
    {
        throw new NotImplementedException();
    }

    public void Abort()
    {
        CallbackException?.Invoke(this, new CallbackExceptionEventArgs(new TimeoutException("some-message")));
    }

    public void Abort(ushort reasonCode, string reasonText)
    {
        throw new NotImplementedException();
    }

    public void Abort(TimeSpan timeout)
    {
        throw new NotImplementedException();
    }

    public void Abort(ushort reasonCode, string reasonText, TimeSpan timeout)
    {
        throw new NotImplementedException();
    }

    public void Close()
    {
        if (_onCloseThrow)
        {
            throw new InvalidOperationException("Error closing connection");
        }
        ConnectionShutdown?.Invoke(this, new ShutdownEventArgs(ShutdownInitiator.Library, 111, ClientProvidedName));
    }

    public void Close(ushort reasonCode, string reasonText)
    {
        if (_onCloseThrow)
        {
            throw new InvalidOperationException("Error closing connection");
        }
        ConnectionShutdown?.Invoke(this, new ShutdownEventArgs(ShutdownInitiator.Library, reasonCode, reasonText));
    }

    public void Close(TimeSpan timeout)
    {
        throw new NotImplementedException();
    }

    public void Close(ushort reasonCode, string reasonText, TimeSpan timeout)
    {
        throw new NotImplementedException();
    }

    public IModel CreateModel()
    {
        return new StubChannel();
    }

    public void HandleConnectionBlocked(string reason)
    {
        ConnectionBlocked?.Invoke(this, new ConnectionBlockedEventArgs(reason));
    }

    public void HandleConnectionUnblocked()
    {
        ConnectionUnblocked?.Invoke(this, EventArgs.Empty);
    }

    public ushort ChannelMax { get; }
    public IDictionary<string, object> ClientProperties { get; }
    public ShutdownEventArgs CloseReason { get; }
    public AmqpTcpEndpoint Endpoint { get; }
    public uint FrameMax { get; }
    public TimeSpan Heartbeat { get; }
    public bool IsOpen { get; } = true;
    public AmqpTcpEndpoint[] KnownHosts { get; }
    public IProtocol Protocol { get; }
    public IDictionary<string, object> ServerProperties { get; }
    public IList<ShutdownReportEntry> ShutdownReport { get; }
    public string ClientProvidedName { get; }
    public event EventHandler<CallbackExceptionEventArgs> CallbackException;
    public event EventHandler<ConnectionBlockedEventArgs> ConnectionBlocked;
    public event EventHandler<ShutdownEventArgs> ConnectionShutdown;
    public event EventHandler<EventArgs> ConnectionUnblocked;
}
