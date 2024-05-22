// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace Sportradar.OddsFeed.SDK.Common.Internal.Extensions
{
    internal static class SerializeExtensions
    {
        public static byte[] SerializeToByteArray(this object obj)
        {
            if (obj == null)
            {
                return null;
            }
            var bf = new BinaryFormatter();
            using (var ms = new MemoryStream())
            {
                bf.Serialize(ms, obj);
                return ms.ToArray();
            }
        }

        public static T Deserialize<T>(this byte[] byteArray) where T : class
        {
            if (byteArray == null)
            {
                return null;
            }
            using (var memStream = new MemoryStream())
            {
                var binForm = new BinaryFormatter();
                binForm.Binder = new SdkTypeBinder();
                memStream.Write(byteArray, 0, byteArray.Length);
                memStream.Seek(0, SeekOrigin.Begin);
                var obj = (T)binForm.Deserialize(memStream);
                return obj;
            }
        }

        sealed class SdkTypeBinder : SerializationBinder
        {
            private const string SdkNamespace = "Sportradar.OddsFeed.SDK";

            public override Type BindToType(string assemblyName, string typeName)
            {
                if (!typeName.StartsWith(SdkNamespace, StringComparison.InvariantCultureIgnoreCase))
                {
                    throw new SerializationException("Only sdk classes are allowed"); // Compliant
                }
                return Assembly.Load(assemblyName).GetType(typeName);
            }
        }
    }
}
