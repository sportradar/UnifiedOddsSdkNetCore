// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Sportradar.OddsFeed.SDK.Common.Internal;
using Xunit.Abstractions;

namespace Sportradar.OddsFeed.SDK.Tests.Common;

public class StubMessageHandler : HttpMessageHandler
{
    private readonly int _timeoutMs;
    private readonly int _timeoutVariablePercent;
    private readonly ITestOutputHelper _outputHelper;
    private HttpResponseMessage _wantedResponseMessage;
    private bool _useOnce;
    private Exception _wantedException;

    public StubMessageHandler(ITestOutputHelper outputHelper, int timeoutMs, int timeoutVariablePercent = 0)
    {
        _timeoutMs = timeoutMs;
        _timeoutVariablePercent = timeoutVariablePercent;
        _outputHelper = outputHelper;
        _wantedResponseMessage = null;
        _useOnce = false;
        _wantedException = null;
    }

    public void SetWantedResponse(HttpResponseMessage httpResponseMessage, bool useOnce = false)
    {
        _wantedResponseMessage = httpResponseMessage;
        _useOnce = useOnce;
    }

    public void SetWantedResponse(Exception exception)
    {
        _wantedException = exception;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var stopWatch = Stopwatch.StartNew();
        var timeout = _timeoutVariablePercent < 1 ? _timeoutMs : SdkInfo.GetVariableNumber(_timeoutMs, _timeoutVariablePercent);

        await Task.Delay(timeout, cancellationToken).ConfigureAwait(false);
        _outputHelper.WriteLine($"Request to {request.RequestUri} took {stopWatch.ElapsedMilliseconds} ms");

        if (_wantedException != null)
        {
            throw _wantedException;
        }

        var responseMessage = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.Accepted,
            ReasonPhrase = nameof(HttpStatusCode.Accepted),
            RequestMessage = request,
            Content = new StringContent("some text")
        };
        if (_wantedResponseMessage != null)
        {
            responseMessage = _wantedResponseMessage;
            if (_useOnce)
            {
                _wantedResponseMessage = null;
            }
        }

        return responseMessage;
    }
}
