using System;
using System.Collections.Generic;
using System.Linq;

namespace Sportradar.OddsFeed.SDK.Common.Internal.Telemetry
{
    internal sealed class LogTagTransformer
    {
        private LogTagTransformer()
        {
        }

        public static LogTagTransformer Instance { get; } = new LogTagTransformer();

        protected string TransformIntegralTag(string key, long value) => $"{key}: {value.ToString()}";

        protected string TransformFloatingPointTag(string key, double value) => $"{key}: {value.ToString()}";

        protected string TransformBooleanTag(string key, bool value) => $"{key}: {(value ? "true" : "false")}";

        protected string TransformStringTag(string key, string value) => $"{key}: {value}";

        protected string TransformArrayTag(string key, Array array) => TransformStringTag(key, System.Text.Json.JsonSerializer.Serialize(array));

        public bool TryTransformTag(KeyValuePair<string, object> tag, out string result, int? maxLength = null)
        {
            if (tag.Value == null)
            {
                result = default;
                return false;
            }

            switch (tag.Value)
            {
                case char:
                case string:
                    result = TransformStringTag(tag.Key, TruncateString(Convert.ToString(tag.Value), maxLength));
                    break;
                case bool b:
                    result = TransformBooleanTag(tag.Key, b);
                    break;
                case byte:
                case sbyte:
                case short:
                case ushort:
                case int:
                case uint:
                case long:
                    result = TransformIntegralTag(tag.Key, Convert.ToInt64(tag.Value));
                    break;
                case float:
                case double:
                    result = TransformFloatingPointTag(tag.Key, Convert.ToDouble(tag.Value));
                    break;
                case Array array:
                    try
                    {
                        result = TransformArrayTagInternal(tag.Key, array, maxLength);
                    }
                    catch
                    {
                        // If an exception is thrown when calling ToString
                        // on any element of the array, then the entire array value
                        // is ignored.
                        ///////////////////////////////////OpenTelemetrySdkEventSource.Log.UnsupportedAttributeType(tag.Value.GetType().ToString(), tag.Key);
                        result = default;
                        return false;
                    }

                    break;

                // All other types are converted to strings including the following
                // built-in value types:
                // case nint:    Pointer type.
                // case nuint:   Pointer type.
                // case ulong:   May throw an exception on overflow.
                // case decimal: Converting to double produces rounding errors.
                default:
                    try
                    {
                        result = TransformStringTag(tag.Key, TruncateString(tag.Value.ToString(), maxLength));
                    }
                    catch
                    {
                        // If ToString throws an exception then the tag is ignored.
                        //////////////////////////////////////OpenTelemetrySdkEventSource.Log.UnsupportedAttributeType(tag.Value.GetType().ToString(), tag.Key);
                        result = default;
                        return false;
                    }

                    break;
            }

            return true;
        }

        private string TransformArrayTagInternal(string key, Array array, int? maxStringValueLength)
        {
            // This switch ensures the values of the resultant array-valued tag are of the same type.
            return array switch
            {
                char[] => TransformArrayTag(key, array),
                string[] => ConvertToStringArrayThenTransformArrayTag(key, array, maxStringValueLength),
                bool[] => TransformArrayTag(key, array),
                byte[] => TransformArrayTag(key, array),
                sbyte[] => TransformArrayTag(key, array),
                short[] => TransformArrayTag(key, array),
                ushort[] => TransformArrayTag(key, array),
                int[] => TransformArrayTag(key, array),
                uint[] => TransformArrayTag(key, array),
                long[] => TransformArrayTag(key, array),
                float[] => TransformArrayTag(key, array),
                double[] => TransformArrayTag(key, array),
                _ => ConvertToStringArrayThenTransformArrayTag(key, array, maxStringValueLength),
            };
        }

        private string ConvertToStringArrayThenTransformArrayTag(string key, Array array, int? maxStringValueLength)
        {
            string[] stringArray;

            if (array is string[] arrayAsStringArray && (!maxStringValueLength.HasValue || !arrayAsStringArray.Any(s => s?.Length > maxStringValueLength)))
            {
                stringArray = arrayAsStringArray;
            }
            else
            {
                stringArray = new string[array.Length];
                for (var i = 0; i < array.Length; ++i)
                {
                    stringArray[i] = TruncateString(array.GetValue(i)?.ToString(), maxStringValueLength);
                }
            }

            return TransformArrayTag(key, stringArray);
        }

        private string TruncateString(string input, int? maxLength)
        {
            return input;
        }
    }
}
