# Migrate to UOF .NET SDK version 4.0.0

This guide explains how to move an application from the *Unified Odds Feed (UOF)* .NET SDK version 3 to version 4.0.0.

Version 4 adds private key *JSON Web Token (JWT)* authentication and removes the temporary V2 interfaces, `SelectReplay()`, and the separate messaging username and password setters. Access token authentication still works. If you use client authentication, don't also set an access token.

The following sections describe the version 4 changes and the updates they require.

## Review SDK dependencies

These packages are dependencies of the SDK. If you manage package versions centrally, or if your project references any of them directly, use the versions in the following table.

| Package | Version | Purpose |
| --- | --- | --- |
| `System.IdentityModel.Tokens.Jwt` | 8.23.0 | Client-authentication JWT signing |
| `Polly` | 8.8.0 | Transient-error retries and circuit breakers |
| `Microsoft.Bcl.TimeProvider` | 10.0.12 | `TimeProvider` for time-based operations |
| `ZiggyCreatures.FusionCache` | 2.9.0 | Caching |
| `Microsoft.Bcl.AsyncInterfaces` | 10.0.12 | Async interfaces for `netstandard2.0` |
| `Microsoft.Extensions.Caching.Memory` | 10.0.12 | Memory cache |
| `Microsoft.Extensions.Configuration` | 10.0.12 | Configuration abstractions |
| `Microsoft.Extensions.DependencyInjection` | 10.0.12 | Dependency injection |
| `Microsoft.Extensions.Diagnostics.HealthChecks` | 10.0.12 | Health checks |
| `Microsoft.Extensions.Http` | 10.0.12 | HTTP client factory |
| `System.Configuration.ConfigurationManager` | 10.0.12 | Configuration files |
| `OpenTelemetry` | 1.19.1 | Telemetry API |
| `OpenTelemetry.Exporter.OpenTelemetryProtocol` | 1.19.1 | OTLP export |
| `OpenTelemetry.Extensions.Hosting` | 1.19.1 | OpenTelemetry hosting |
| `OpenTelemetry.Instrumentation.Runtime` | 1.19.0 | Runtime metrics |

The SDK targets `netstandard2.0` and `net8.0`. NuGet restores the build that matches your project's target framework. Choose a build only if you pin the package to a specific framework.

## Optional: Authenticate with a private key JWT

Version 4.0.0 adds private key JWT client authentication. The SDK uses the OAuth 2.0 client credentials grant and a signed JWT.

Use client authentication instead of an access token. The two options are mutually exclusive.

### Register the public key

Register the public key that matches your RSA private key with Sportradar. After registration, Sportradar gives you a key ID and a service ID. Use both values in the configuration below. For help with registration, contact Sportradar support.

> **Important:** Register the public key in the same environment that you select in the SDK, either production or integration. The service ID belongs to that environment. To validate the credentials, see `CredentialCheckExample` in `src/Sportradar.OddsFeed.SDK.DemoProject/Example/CredentialCheckExample.cs`.

### Configure private key JWT authentication

The private key must be an `RsaSecurityKey`. The SDK rejects any other `AsymmetricSecurityKey`.

The following example configures client authentication for the integration environment. `rsa` is an `RSA` instance that you load from your private key.

```csharp
using Microsoft.IdentityModel.Tokens;

// The private key must be an RsaSecurityKey.
var yourRsaPrivateKey = new RsaSecurityKey(rsa);

var clientAuth = UofClientAuthentication
    .PrivateKeyJwt()
    .SetClientId("your-client-id") // Service ID from Sportradar.
    .SetSigningKeyId("your-key-id") // Key ID from public-key registration.
    .SetPrivateKey(yourRsaPrivateKey)
    .Build();

// Configure client authentication instead of an access token.
var config = UofSdk.GetConfigurationBuilder()
    .SetClientAuthentication(clientAuth)
    .SelectEnvironment(SdkEnvironment.Integration)
    .Build();
```

To use a custom environment, set the client authentication server details after `SelectCustom()`:

```csharp
var customConfig = UofSdk.GetConfigurationBuilder()
    .SetClientAuthentication(clientAuth)
    .SelectCustom()
    // Overwrite the default host, port, SSL usage and tenant of the client authentication server if needed, e.g. for testing.
    .SetClientAuthenticationHost("auth.example.com")
    .SetClientAuthenticationPort(443)
    .SetClientAuthenticationUseSsl(true)
    .SetClientAuthenticationTenant("https://auth.example.com/") // Optional, defaults to the authentication server URL.
    // ... other configuration options
    .Build();
```

### Configuration Options

The `UofClientAuthentication.IPrivateKeyJwtBuilder` provides the following configuration methods:

- **`SetSigningKeyId(string keyId)`** - Sets the key identifier used in JWT header ("kid" parameter)
- **`SetClientId(string clientId)`** - Sets the client identifier (Sportradar Service Id) for JWT "iss" and "sub" claims
- **`SetPrivateKey(AsymmetricSecurityKey privateKey)`** - Sets the RSA private key (`RsaSecurityKey`) for JWT signing

When using the custom environment (`SelectCustom()`), the following additional methods are available on the `ICustomConfigurationBuilder`:

- **`SetClientAuthenticationHost(string host)`** - Sets the host of the client authentication server
- **`SetClientAuthenticationPort(int port)`** - Sets the port of the client authentication server
- **`SetClientAuthenticationUseSsl(bool useSsl)`** - Sets whether HTTPS (`true`) or HTTP (`false`) is used to communicate with the client authentication server
- **`SetClientAuthenticationTenant(string tenant)`** - Sets the (optional) authentication tenant. The tenant is used as the audience (`aud` claim) of the signed JWT. When not set, it defaults to the full URL of the client authentication server (e.g. `https://auth.sportradar.com/`), which is what you want in almost all cases.

For the predefined environments (production, integration, global production/integration and the replay environments) the SDK selects the client authentication server automatically.

Configuration via configuration files is not supported for client authentication configuration. Configuration files used to configure SDK in version 3.x.x are still available, but the client authentication must be set programmatically.

The configured client authentication is exposed via `IUofConfiguration.Authentication` (`null` when the access token is used).

### Security Requirements

- **RSA Keys Only** - Only RSA SHA256 algorithm is supported for private keys at the moment
- **Key Registration** - The corresponding public key must be registered with Sportradar authentication service
- **SSL Recommended** - Use HTTPS for secure transmission of authentication credentials

## Interface changes

The temporary `V2` interfaces introduced on the 3.x line to keep 3.x releases backward compatible were merged into their base interfaces:

- _ICustomBetManagerV2_ is removed - its members (`PrebuiltBetsRequestBuilder`, `GetPrebuiltBets`, `GetCalculateRequestBuilder`, `CalculateProbabilityAsync(ICalculateRequestBuilder)` and `CalculateProbabilityFilterAsync(ICalculateRequestBuilder)`) are now part of `ICustomBetManager`
  - Client previously casting `(ICustomBetManagerV2)uofSdk.CustomBetManager` must switch to `uofSdk.CustomBetManager` directly
- _IMessageV2_ is removed - `MessageHeaders` (`IReadOnlyDictionary<string, string>`, never null) is now a member of `IMessage`
  - Client previously casting a message to `IMessageV2` can read `message.MessageHeaders` directly
- _IOutcomeSettlementV2_ is removed - `EachWayResult`, `EachWayPlaceFactor` and `DeadHeatFactorPlace` are now members of `IOutcomeSettlement`
  - Client previously casting a settled outcome to `IOutcomeSettlementV2` can read these properties directly from `IOutcomeSettlement`
  - Each-way result values the SDK does not recognize are reported as `EachWayResult.UnsupportedBySdk`

```csharp
// Before (v3)
var outcome = (IOutcomeSettlementV2)settledOutcome;
var eachWayResult = outcome.EachWayResult;
var headers = ((IMessageV2)message).MessageHeaders;

// After (v4)
var eachWayResult = settledOutcome.EachWayResult;
var headers = message.MessageHeaders;
```

If you provide your own implementations of `ICustomBetManager`, `IMessage` or `IOutcomeSettlement` (e.g. test doubles or wrappers), you must implement the added members. Code that merely consumes the SDK entities is not affected.

Other interface changes:

- `IRollbackBetSettlement<T>` now exposes its markets as `IMarketRollbackSettlement` (previously `IMarketCancel`). Rollback messages with in-play settlement markets no longer require casting the market to `IMarketRollbackSettlement`.
- `IUofConfiguration` has a new member `Authentication` of type `UofClientAuthentication.IPrivateKeyJwt` (`null` when the access token is used).
- `ITokenSetter` has a new method `SetClientAuthentication(UofClientAuthentication.IPrivateKeyJwtData)`.

## Custom configuration builder changes

`ICustomConfigurationBuilder.SetMessagingUsername(string)` and `ICustomConfigurationBuilder.SetMessagingPassword(string)` were replaced by a single method `SetMessagingCredentials(string username, string password)`. Both values are mandatory and must be non-empty.

```csharp
// Before (v3)
var config = UofSdk.GetConfigurationBuilder()
    .SetAccessToken("your access token")
    .SelectCustom()
    .SetMessagingHost("localhost")
    .SetMessagingUsername("guest")
    .SetMessagingPassword("guest")
    // ... other configuration options
    .Build();

// After (v4)
var config = UofSdk.GetConfigurationBuilder()
    .SetAccessToken("your access token")
    .SelectCustom()
    .SetMessagingHost("localhost")
    .SetMessagingCredentials("guest", "guest")
    // ... other configuration options
    .Build();
```

The same rule applies to the configuration file when the custom environment is used: the `username` and `password` attributes of the `uofSdkSection` element must now be either both set or both omitted. Setting only one of them makes building the configuration fail (previously the single value was silently accepted).

## Replay changes

Removed `IEnvironmentSelector.SelectReplay()` and `SdkEnvironment.Replay`. Use `SelectEnvironment(env)` instead.

Be specific about credentials and select either `SdkEnvironment.ReplayWithIntegrationCredentials` or `SdkEnvironment.ReplayWithProductionCredentials` depending on your use case.

When connecting to the Replay server, you must explicitly choose which credential environment your access token or Client Authentication configuration belongs to:

- **`ReplayWithIntegrationCredentials`** — for integration access tokens or CIAM credentials.
- **`ReplayWithProductionCredentials`** — for production access tokens or CIAM credentials.

Previously, `SelectReplay()` and `SdkEnvironment.Replay` attempted to auto-detect whether the access token was integration or production. This behavior is removed — you must now select the appropriate replay environment explicitly.

```csharp
// Before (v3)
var config = UofSdk.GetConfigurationBuilder()
    .SetAccessToken("your access token")
    .SelectReplay()
    .SetDefaultLanguage(CultureInfo.GetCultureInfo("en"))
    .Build();

// After (v4) — integration token
var config = UofSdk.GetConfigurationBuilder()
    .SetAccessToken("your integration access token")
    .SelectEnvironment(SdkEnvironment.ReplayWithIntegrationCredentials)
    .SetDefaultLanguage(CultureInfo.GetCultureInfo("en"))
    .Build();

// After (v4) — production token
var config = UofSdk.GetConfigurationBuilder()
    .SetAccessToken("your production access token")
    .SelectEnvironment(SdkEnvironment.ReplayWithProductionCredentials)
    .SetDefaultLanguage(CultureInfo.GetCultureInfo("en"))
    .Build();
```

If using configuration files, update the `environment` setting from `Replay` to `ReplayWithIntegrationCredentials` or `ReplayWithProductionCredentials`.

Note that the numeric values of the `SdkEnvironment` members changed (`Replay` was replaced by two members). Do not persist or compare the underlying integer values of the enum; use the member names instead.

### Removed Methods

- `IEnvironmentSelector.SelectReplay()`
- enum value `SdkEnvironment.Replay`
- `ICustomConfigurationBuilder.SetMessagingUsername(string)` (use `SetMessagingCredentials(string, string)`)
- `ICustomConfigurationBuilder.SetMessagingPassword(string)` (use `SetMessagingCredentials(string, string)`)
- interfaces `ICustomBetManagerV2`, `IMessageV2` and `IOutcomeSettlementV2` (merged into `ICustomBetManager`, `IMessage` and `IOutcomeSettlement`)

## Test Your Project

Thoroughly test your project after making the changes. Test all critical functionality to ensure that everything still works as expected. Pay special attention to any areas of your setup that interact with the SDK, as these are likely to be the most affected by the upgrade.

## Update the Documentation

Update your project's documentation and any training materials to reflect the changes introduced by the upgrade. This will help your team members understand and work with the new version.

## Deploy to Production

Once you are confident that your project works correctly with the upgraded SDK, you can deploy the updated version to
your production environment.

## Monitoring and Maintenance

After deployment, monitor your project closely for any unexpected issues or performance problems. Be prepared to address any post-upgrade issues promptly.

## Feedback and Reporting

If you encounter any bugs or issues in the SDK, consider reporting them to support@sportradar.com. Providing feedback
can help improve the SDK for future releases.
