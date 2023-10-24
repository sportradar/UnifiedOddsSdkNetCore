/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/

using System;
using System.IO;
using System.Text;

namespace Sportradar.OddsFeed.SDK.Common.Extensions
{
    /// <summary>
    /// Defines extension methods for stream used by the sdk
    /// </summary>
    public static class StreamExtensions
    {
        /// <summary>
        /// Gets a <see cref="string"/> representation of the provided <see cref="Stream"/>
        /// </summary>
        /// <param name="stream">The <see cref="Stream"/> whose content to get.</param>
        /// <returns>A <see cref="string"/> representation of the <see cref="Stream"/> content.</returns>
        public static string GetData(this Stream stream)
        {
            if (stream == null)
            {
                throw new ArgumentNullException(nameof(stream));
            }

            using (var memoryStream = new MemoryStream())
            {
                stream.CopyTo(memoryStream);
                return Encoding.UTF8.GetString(memoryStream.ToArray());
            }
        }
    }
}
