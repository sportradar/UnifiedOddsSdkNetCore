/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System.Diagnostics.Contracts;
using System.IO;
using System.Threading.Tasks;

namespace Sportradar.OddsFeed.SDK.Test.Shared
{
    public static class FileHelper
    {
        public static Stream OpenFile(string dirPath, string fileName)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(dirPath));
            Contract.Requires(!string.IsNullOrWhiteSpace(fileName));
            Contract.Ensures(Contract.Result<Stream>() != null);

            var filePath = dirPath.TrimEnd('/') + "/" + fileName.TrimStart('/');
            return OpenFile(filePath);
        }

        public static Stream OpenFile(string filePath)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(filePath));
            return File.OpenRead(filePath);
        }

        public static Task<Stream> OpenFileAsync(string filePath)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(filePath));
            return Task.Factory.StartNew(() => OpenFile(filePath));
        }

        public static string ReadFile(string dirPath, string fileName)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(dirPath));
            Contract.Requires(!string.IsNullOrWhiteSpace(fileName));
            Contract.Ensures(Contract.Result<string>() != null);

            var stream = OpenFile(dirPath, fileName);
            var reader = new StreamReader(stream);
            var result = reader.ReadToEnd();
            return result;
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
}
