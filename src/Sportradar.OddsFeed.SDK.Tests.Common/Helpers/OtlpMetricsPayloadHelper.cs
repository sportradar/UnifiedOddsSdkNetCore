// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sportradar.OddsFeed.SDK.Tests.Common.Helpers;

internal static class OtlpMetricsPayloadHelper
{
    internal static IReadOnlyCollection<string> ParseExportedOtlpMetricsPayload(byte[] payload)
    {
        if (payload == null || payload.Length == 0)
        {
            throw new ArgumentException("OTLP metrics payload must not be null or empty.", nameof(payload));
        }

        var parsedStrings = ExtractUtf8StringsFromProtobuf(payload);
        return parsedStrings.Count == 0 ? throw new InvalidOperationException("OTLP protobuf payload should contain readable UTF-8 string fields.") : parsedStrings;
    }

    private static IReadOnlyCollection<string> ExtractUtf8StringsFromProtobuf(byte[] data)
    {
        var strings = new HashSet<string>(StringComparer.Ordinal);
        WalkProtobufMessage(data, 0, data.Length, strings);
        return strings;
    }

    private static void WalkProtobufMessage(byte[] data, int start, int end, ISet<string> strings)
    {
        var pos = start;
        while (pos < end)
        {
            if (!TryReadVarint(data, end, ref pos, out var tag))
            {
                return;
            }

            var wireType = (int)(tag & 0x7);
            switch (wireType)
            {
                case 0:
                    if (!TryReadVarint(data, end, ref pos, out _))
                    {
                        return;
                    }
                    break;
                case 1:
                    pos += 8;
                    break;
                case 2:
                    if (!TryReadVarint(data, end, ref pos, out var length) || pos + (int)length > end)
                    {
                        return;
                    }

                    var fieldStart = pos;
                    var fieldLength = (int)length;
                    pos += fieldLength;

                    var fieldBytes = data.AsSpan(fieldStart, fieldLength);
                    if (TryGetUtf8String(fieldBytes, out var utf8String))
                    {
                        strings.Add(utf8String);
                    }

                    WalkProtobufMessage(data, fieldStart, fieldStart + fieldLength, strings);
                    break;
                case 5:
                    pos += 4;
                    break;
                default:
                    return;
            }
        }
    }

    private static bool TryReadVarint(byte[] data, int end, ref int pos, out ulong value)
    {
        value = 0;
        var shift = 0;
        while (pos < end && shift < 64)
        {
            var b = data[pos++];
            value |= (ulong)(b & 0x7F) << shift;
            if ((b & 0x80) == 0)
            {
                return true;
            }

            shift += 7;
        }

        return false;
    }

    private static bool TryGetUtf8String(ReadOnlySpan<byte> bytes, out string value)
    {
        value = null;
        if (bytes.IsEmpty)
        {
            return false;
        }

        foreach (var b in bytes)
        {
            if (b is 0 or > 0x7F)
            {
                break;
            }
        }

        try
        {
            value = Encoding.UTF8.GetString(bytes);
        }
        catch (DecoderFallbackException)
        {
            return false;
        }

        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        if (!value.Any(c => char.IsControl(c) && c != '\t' && c != '\n' && c != '\r'))
        {
            return true;
        }

        value = null;
        return false;

    }
}
