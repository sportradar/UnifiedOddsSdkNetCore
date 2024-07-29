// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Dawn;
using Sportradar.OddsFeed.SDK.Common.Internal.Extensions;

namespace Sportradar.OddsFeed.SDK.Tests.Common;

public static class FileHelper
{
    public static string GetFileContent(string name)
    {
        var stream = GetResource(name);
        if (stream == null)
        {
            return null;
        }

        using var reader = new StreamReader(stream);

        return reader.ReadToEnd();
    }

    public static Stream GetStreamFromString(string content)
    {
        var stream = new MemoryStream();
        var writer = new StreamWriter(stream, Encoding.UTF8);

        writer.Write(content);
        writer.Flush();
        stream.Position = 0;

        return stream;
    }

    public static Stream GetResource(string name)
    {
        //Debug.WriteLine($"Executing {Assembly.GetExecutingAssembly().FullName}: {Assembly.GetExecutingAssembly().GetManifestResourceNames().Length}");
        //Debug.WriteLine($"Entry {Assembly.GetEntryAssembly()?.FullName}: {Assembly.GetEntryAssembly()?.GetManifestResourceNames().Length}");
        //Debug.WriteLine($"Calling {Assembly.GetCallingAssembly().FullName}: {Assembly.GetCallingAssembly().GetManifestResourceNames().Length}");
        var execResources = Assembly.GetExecutingAssembly().GetManifestResourceNames().Distinct().ToList();
        //if (resources.Count > 0)
        //{
        //    foreach (var s in resources)
        //    {
        //        Debug.WriteLine(s);
        //    }
        //}
        var execResource = execResources.FirstOrDefault(x => x.Equals(name, StringComparison.InvariantCultureIgnoreCase));
        if (execResource != null)
        {
            var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(execResource);
            return stream;
        }

        var entryResources = Assembly.GetEntryAssembly()?.GetManifestResourceNames();
        var entryResource = entryResources?.FirstOrDefault(x => x.Equals(name, StringComparison.InvariantCultureIgnoreCase));
        if (entryResource != null)
        {
            var stream = Assembly.GetEntryAssembly()?.GetManifestResourceStream(entryResource);
            return stream;
        }

        var commonResources = Assembly.GetAssembly(typeof(FileHelper))?.GetManifestResourceNames();
        var commonResource = commonResources?.FirstOrDefault(x => x.Equals(name, StringComparison.InvariantCultureIgnoreCase));
        if (commonResource != null)
        {
            var stream = Assembly.GetEntryAssembly()?.GetManifestResourceStream(commonResource);
            return stream;
        }

        var fileStream = OpenFile(FindFile(name));
        if (fileStream != null)
        {
            return fileStream;
        }

        return null;
    }

    public static bool ResourceExists(string name)
    {
        var resources = Assembly.GetExecutingAssembly().GetManifestResourceNames();
        var resource = resources.FirstOrDefault(x => x.EndsWith(name, StringComparison.InvariantCultureIgnoreCase));
        return resource != null;
    }

    public static Stream OpenFile(string dirPath, string fileName)
    {
        Guard.Argument(dirPath, nameof(dirPath)).NotNull().NotEmpty();
        Guard.Argument(fileName, nameof(File)).NotNull().NotEmpty();

        var filePath = dirPath?.TrimEnd('/') + "/" + fileName?.TrimStart('/');
        return OpenFile(filePath);
    }

    public static Stream OpenFile(string filePath)
    {
        Guard.Argument(filePath, nameof(filePath)).NotNull().NotEmpty();
        if (filePath.IsNullOrEmpty() || !File.Exists(filePath))
        {
            return null;
        }
        return File.OpenRead(filePath);
    }

    public static string FindFile(string fileName)
    {
        var fi = new FileInfo(fileName);
        if (fi.Exists)
        {
            return fi.FullName;
        }

        var currentDir = new DirectoryInfo(Directory.GetCurrentDirectory());
        foreach (var di in currentDir.GetDirectories())
        {
            var file = FindFile(fileName, di);
            if (!fileName.Equals(file))
            {
                return file;
            }
        }

        return fileName;
    }

    public static string FindFile(string fileName, DirectoryInfo directory)
    {
        if (directory == null)
        {
            return fileName;
        }

        var files = directory.GetFiles();
        foreach (var f in files)
        {
            if (f.Name == fileName)
            {
                return f.FullName;
            }
        }

        var dirs = directory.GetDirectories();
        foreach (var dir in dirs)
        {
            var tmp = FindFile(fileName, dir);
            if (!fileName.Equals(tmp))
            {
                return tmp;
            }
        }

        return fileName;
    }
}
