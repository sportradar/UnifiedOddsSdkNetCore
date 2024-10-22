// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using Sportradar.OddsFeed.SDK.Messages.Rest;

namespace Sportradar.OddsFeed.SDK.Tests.Common.Dsl.Api;

public class ErrorResponseEndpoint
{
    private readonly response _errorResponse = new response();

    public static ErrorResponseEndpoint Create()
    {
        return new ErrorResponseEndpoint();
    }

    public ErrorResponseEndpoint WithResponseCode(response_code responseCode)
    {
        _errorResponse.response_code = responseCode;
        _errorResponse.response_codeSpecified = true;
        return this;
    }

    public ErrorResponseEndpoint WithMessage(string message)
    {
        _errorResponse.message = message;
        return this;
    }

    public ErrorResponseEndpoint WithAction(string action)
    {
        _errorResponse.action = action;
        return this;
    }

    public response Build()
    {
        return _errorResponse;
    }
}
