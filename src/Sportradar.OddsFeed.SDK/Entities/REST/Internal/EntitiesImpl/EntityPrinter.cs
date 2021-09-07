/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Internal.EntitiesImpl
{
    /// <summary>
    /// Printer used to format ToString display for entities
    /// </summary>
    [DataContract]
    internal abstract class EntityPrinter : IFormattable, IEntityPrinter
    {
        /// <summary>Returns a string that represents the current objects identifier.</summary>
        /// <returns>A string that represents the current object.</returns>
        public override string ToString()
        {
            return ToString("I", null);
        }

        /// <summary>
        /// Returns a string that represents the current object
        /// </summary>
        /// <param name="formatProvider">A format provider used to format the output string</param>
        /// <returns>A string that represents the current object.</returns>
        public string ToString(IFormatProvider formatProvider)
        {
            return ToString(null, formatProvider);
        }

        /// <summary>Formats the value of the current instance using the specified format.</summary>
        /// <returns>The value of the current instance in the specified format.</returns>
        /// <param name="format">The format to use.-or- A null reference (Nothing in Visual Basic) to use the default format defined for the type of the <see cref="T:System.IFormattable" /> implementation. </param>
        public string ToString(string format)
        {
            // ReSharper disable once IntroduceOptionalParameters.Global
            return ToString(format, null);
        }

        /// <summary>Formats the value of the current instance using the specified format.</summary>
        /// <returns>The value of the current instance in the specified format.</returns>
        /// <param name="format">The format to use.-or- A null reference (Nothing in Visual Basic) to use the default format defined for the type of the <see cref="T:System.IFormattable" /> implementation. </param>
        /// <param name="formatProvider">The provider to use to format the value.-or- A null reference (Nothing in Visual Basic) to obtain the numeric format information from the current locale setting of the operating system. </param>
        public string ToString(string format, IFormatProvider formatProvider)
        {
            //Supported formats: C - compact, F - full, I - only id, J - json
            if (format == null)
            {
                format = "G";
            }
            format = format.ToLower();

            ICustomFormatter formatter = formatProvider?.GetFormat(GetType()) as ICustomFormatter;
            if (formatter != null)
            {
                return formatter.Format(format, this, formatProvider);
            }

            switch (format)
            {
                case "c":
                    return PrintC();
                case "f":
                    return PrintF();
                case "j":
                    return PrintJ();
                //case "i":
                //case "g":
                default:
                    return PrintI();

            }
        }

        /// <summary>
        /// Constructs and returns a <see cref="string"/> containing the id of the current instance
        /// </summary>
        /// <returns>A <see cref="string"/> containing the id of the current instance.</returns>
        protected abstract string PrintI();

        /// <summary>
        /// Constructs and return a <see cref="string"/> containing details of the current instance
        /// </summary>
        /// <returns>A <see cref="string"/> containing details of the current instance.</returns>
        protected virtual string PrintF()
        {
            return PrintI();
        }

        /// <summary>
        /// Constructs and returns a <see cref="string"/> containing compacted representation of the current instance
        /// </summary>
        /// <returns>A <see cref="string"/> containing compacted representation of the current instance.</returns>
        protected virtual string PrintC()
        {
            return PrintI();
        }

        /// <summary>
        /// Constructs and returns a <see cref="string" /> containing a JSON representation of the current instance
        /// </summary>
        /// <param name="type">A <see cref="Type"/> specifying the type of the instance whose representation to create.</param>
        /// <param name="item">A <see cref="object"/> representing the instance whose representation to create.</param>
        /// <returns>a <see cref="string" /> containing a JSON representation of the current instance.</returns>
        protected virtual string PrintJ(Type type, object item)
        {
            return PrintJson(type, item);
        }

        /// <summary>
        /// Constructs and returns a <see cref="string" /> containing a JSON representation of the current instance
        /// </summary>
        /// <returns>a <see cref="string" /> containing a JSON representation of the current instance.</returns>
        protected abstract string PrintJ();

        private static string PrintJson(Type type, object item)
        {
            MemoryStream stream1 = new MemoryStream();
            DataContractJsonSerializer ser = new DataContractJsonSerializer(type);
            ser.WriteObject(stream1, item);
            StreamReader sr = new StreamReader(stream1);
            stream1.Position = 0;
            string json = sr.ReadToEnd();
            return json;
        }
    }
}
