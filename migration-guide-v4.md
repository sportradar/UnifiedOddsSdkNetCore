# UOF .NET SDK - Migration Guide to Version 4.0.0

The UOF .NET SDK has been upgraded from your current v3 version to the v4. This migration guide provides a roadmap for a smooth transition from your current SDK version to the latest version. The upgrade is designed to elevate your experience and align the SDK more closely with your business needs.

This guide offers practical advice to ensure your transition is not only efficient but also enhances the performance and capabilities of your software.

## 1. Upgrade Dependencies

The following new dependency has been added:

* **System.IdentityModel.Tokens.Jwt 6.34.0** - required for the new client authentication feature

Make sure to update your project's dependency management to include this new library.

## 2. New Features

#### Client Authentication with Private Key JWT

Version 4.0.0 introduces a new client authentication feature that supports **Private Key JWT** authentication method. This feature implements OAuth 2.0 Client Credentials Grant with JWT assertion, providing enhanced security for API communications.

##### Migration Notes

- Ensure your RSA private key is properly formatted and the corresponding public key is registered with Sportradar
- Contact Sportradar support for assistance with key registration and authentication setup
- After registering your public key, you will receive `key id` and `service id` that must be used in the client authentication configuration

##### Basic Usage

To configure client authentication with Private Key JWT:

IMPORTANT: Client authentication and your access token needs to be configured for the same bookmaker id and same environment (i.e. either production or staging).

```csharp

// Create client authentication configuration
var clientAuth = UofClientAuthentication
    .PrivateKeyJwt()
    .SetClientId("your-client-id") // Sportradar Service Id
    .SetSigningKeyId("your-key-id") // Key id provided to you after registering the Public Key with Sportradar
    .SetPrivateKey(yourRSAPrivateKey)
    .Build()

// Use with SDK configuration
var config = UofSdk.GetConfigurationBuilder()
    // Client authentication, if configured, needs to be set before access token is set
    .SetClientAuthentication(clientAuth)
    // your access token is still required to access RabbitMQ    
    .SetAccessToken("your access token")
    // ... other configuration options
    .Build();

// Use with SDK configuration (custom)
var config = UofSdk.GetConfigurationBuilder()
        // Client authentication, if configured, needs to be set before access token is set
        .SetClientAuthentication(clientAuth)
        // your access token is still required to access RabbitMQ    
        .SetAccessToken("your access token")
        .SelectCustom()
        // overwriting default host and port for client authentication server if needed, e.g. for testing
        .SetClientAuthenticationHost("stg-auth.sportradar.com")
        .SetClientAuthenticationPort(443)
        .SetClientAuthenticationUseSsl(true)
        // ... other configuration options
        .build();
```

##### Configuration Options

The `PrivateKeyJwt.Builder` provides the following configuration methods:

- **`SetSigningKeyId(string keyId)`** - Sets the key identifier used in JWT header ("kid" parameter)
- **`SetClientId(string clientId)`** - Sets the client identifier (Sportradar Service Id) for JWT "iss" and "sub" claims
- **`SetPrivateKey(PrivateKey privateKey)`** - Sets the RSA private key for JWT signing

Configuration via configuration files is not supported for client authentication configuration. Configuration files used to configure SDK in version 3.x.x are still available to be used, but the client authentication must be set programmatically.

##### Security Requirements

- **RSA Keys Only** - Only RSA SHA256 algorithm is supported for private keys at the moment
- **Key Registration** - The corresponding public key must be registered with Sportradar authentication service
- **SSL Recommended** - Use HTTPS for secure transmission of authentication credentials

## 3. Release Candidate Constraints

- Access token is mandatory to be configured along with optional client authentication configuration.
- SetClientAuthentication must be set before SetAccessToken when configuring SDK.
- Client authentication only affects API calls, RabbitMQ still uses access token for authentication.
- SDK integrations via SSL-terminating proxies is not supported. This is because custom Client Authentication server fields (e.g.`.SetClientAuthenticationHost()`) works correctly only for staging and production URLs.
- Replay does not support Client Authentication.

## 4. Test Your Project

Thoroughly test your project after making the changes. Test all critical functionality to ensure that everything still works as expected. Pay special attention to any areas of your setup that interact with the SDK, as these are likely to be the most affected by the upgrade.

## 5. Update the Documentation

Update your project's documentation and any training materials to reflect the changes introduced by the upgrade. This will help your team members understand and work with the new version.

## 6. Deploy to Production

Once you are confident that your project works correctly with the upgraded SDK, you can deploy the updated version to
your production environment.

## 7. Monitoring and Maintenance

After deployment, monitor your project closely for any unexpected issues or performance problems. Be prepared to address any post-upgrade issues promptly.

## 8. Feedback and Reporting

If you encounter any bugs or issues in the SDK, consider reporting them to support@sportradar.com. Providing feedback
can help improve the SDK for future releases.