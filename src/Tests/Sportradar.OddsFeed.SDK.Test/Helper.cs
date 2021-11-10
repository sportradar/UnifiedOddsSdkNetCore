using System;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace Sportradar.OddsFeed.SDK.Test
{
    public static class Helper
    {
        /// <summary>
        /// Convert long epoch time to DateTime
        /// </summary>
        /// <param name="epochTime">The UNIX time</param>
        /// <returns>DateTime</returns>
        public static DateTime FromEpochTime(long epochTime)
        {
            var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            return epoch.AddMilliseconds(epochTime).ToLocalTime();
        }

        /// <summary>
        /// Convert DateTime to the epoch time (in seconds)
        /// </summary>
        /// <param name="date">The date</param>
        /// <returns>System.Int64</returns>
        public static long ToEpochTime(DateTime date)
        {
            var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            return Convert.ToInt64((date.ToUniversalTime() - epoch).TotalMilliseconds);
        }

        /// <summary>
        /// Return the fixed length of the string. If input to short it adds spaces, if too long, it is truncated
        /// </summary>
        /// <param name="value">¸Input string</param>
        /// <param name="length">Length of the returned string</param>
        /// <param name="postfix">If the spaces are added pre- or post- string</param>
        /// <returns>Fixed length string</returns>
        public static string FixedLength(this string value, int length, bool postfix = true)
        {
            var result = string.IsNullOrEmpty(value)
                ? string.Empty
                : value.Length >= length
                    ? value.Substring(0, length)
                    : value;
            var dif = length - result.Length;

            var space = string.Empty;
            for (var i = 0; i < dif; i++)
            {
                space += " ";
            }

            return postfix
                ? result + space
                : space + result;
        }

        public static string Serialize<T>(T dataToSerialize)
        {
            try
            {
                //var stringWriter = new Utf8StringWriter();
                ////XmlWriterSettings settings = new XmlWriterSettings
                ////                             {
                ////                                 Indent = false,
                ////                                 OmitXmlDeclaration = true
                ////                             };
                //XmlSerializerNamespaces emptyNamespaces = new XmlSerializerNamespaces(new[] { XmlQualifiedName.Empty });
                //var serializer = new XmlSerializer(typeof(T));
                //serializer.Serialize(stringWriter, dataToSerialize, emptyNamespaces);
                //return stringWriter.ToString();

                var emptyNamespaces = new XmlSerializerNamespaces(new[] { XmlQualifiedName.Empty });
                var serializer = new XmlSerializer(typeof(T));
                var settings = new XmlWriterSettings
                               {
                                   Indent = false,
                                   OmitXmlDeclaration = false
                               };

                using (var stream = new Utf8StringWriter())
                {
                    using (var writer = XmlWriter.Create(stream, settings))
                    {
                        serializer.Serialize(writer, dataToSerialize, emptyNamespaces);
                        return stream.ToString();
                    }

                }
            }
            catch (Exception ex)
            {
                WriteToOutput(ex.Message);
            }

            return null;
        }

        public class Utf8StringWriter : StringWriter
        {
            public override Encoding Encoding => Encoding.UTF8;
        }

        public static void WriteToOutput(string message)
        {
            Console.WriteLine($"{DateTime.Now.ToLongTimeString()}\t{message}");
        }
    }
}
