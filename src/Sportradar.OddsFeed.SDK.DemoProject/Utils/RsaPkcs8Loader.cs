// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using Microsoft.IdentityModel.Tokens;

namespace Sportradar.OddsFeed.SDK.DemoProject.Utils;

public static class RsaPkcs8Loader
{
    private const string BeginPkcs8 = "-----BEGIN PRIVATE KEY-----";
    private const string EndPkcs8 = "-----END PRIVATE KEY-----";

    public static AsymmetricSecurityKey LoadPkcs8RsaKey(string pemPath, string keyId = null)
    {
        var pem = File.ReadAllText(pemPath);
        return ParsePkcs8RsaKey(pem, keyId);
    }

    public static AsymmetricSecurityKey ParsePkcs8RsaKey(string pemContent, string keyId = null)
    {
        if (string.IsNullOrWhiteSpace(pemContent))
        {
            throw new ArgumentException("PEM content is empty.");
        }

        var text = pemContent.Trim();

        var begin = text.IndexOf(BeginPkcs8, StringComparison.Ordinal);
        var end = text.IndexOf(EndPkcs8, StringComparison.Ordinal);
        if (begin < 0 || end < 0 || end <= begin)
        {
            throw new ArgumentException("Not a PKCS#8 PEM: missing BEGIN/END PRIVATE KEY markers.");
        }

        var base64 = text.Substring(begin + BeginPkcs8.Length, end - (begin + BeginPkcs8.Length));
        base64 = new string(base64.Where(c => !char.IsWhiteSpace(c)).ToArray());

        var der = Convert.FromBase64String(base64);

        var rsa = RSA.Create();
        try
        {
            rsa.ImportPkcs8PrivateKey(der, out _);

            var key = new RsaSecurityKey(rsa);
            if (!string.IsNullOrWhiteSpace(keyId))
            {
                key.KeyId = keyId;
            }

            return key;
        }
        catch
        {
            rsa.Dispose();
            throw;
        }
    }
}
