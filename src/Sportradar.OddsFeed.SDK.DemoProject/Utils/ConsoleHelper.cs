// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using System.IO;

namespace Sportradar.OddsFeed.SDK.DemoProject.Utils;

internal static class ConsoleHelper
{
    public static string ReadNonEmpty(string prompt)
    {
        while (true)
        {
            Console.Write(prompt);
            var value = Console.ReadLine()?.Trim();
            if (!string.IsNullOrWhiteSpace(value))
            {
                return value;
            }

            Console.WriteLine("  Value cannot be empty. Please try again.");
        }
    }

    public static string ReadExistingPemPath(string prompt)
    {
        while (true)
        {
            Console.Write(prompt);
            var privateKeyFilePath = (Console.ReadLine() ?? string.Empty).Trim();

            if (string.IsNullOrWhiteSpace(privateKeyFilePath))
            {
                Console.WriteLine("  Path cannot be empty. Please try again.");
                continue;
            }

            if (File.Exists(privateKeyFilePath))
            {
                return privateKeyFilePath;
            }

            Console.WriteLine("  File not found. Please enter a valid path to an existing .pem file.");
        }
    }
}
