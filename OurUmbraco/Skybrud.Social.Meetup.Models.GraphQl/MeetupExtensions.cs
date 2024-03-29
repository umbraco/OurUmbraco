using System;
using System.Globalization;
using Newtonsoft.Json.Linq;
using Skybrud.Essentials.Enums;
using Skybrud.Essentials.Json.Extensions;
using Skybrud.Essentials.Strings;
using Skybrud.Essentials.Time;
using Skybrud.Essentials.Time.Xml;

namespace Skybrud.Social.Meetup.Extensions
{
    internal static class MeetupExtensions
    {
        internal static bool TryParse(string iso8601, out DateTimeOffset result)
        {
            return DateTimeOffset.TryParseExact(iso8601, DateTimeFormats, CultureInfo.InvariantCulture,
                DateTimeStyles.None, out result);
        }

        internal static readonly string[] DateTimeFormats =
        {
            "yyyy-MM-ddTHH:mm:ssZ",
            "yyyy-MM-ddTHH:mm:ssK",
            "yyyy-MM-ddTHH:mm:ss.fffZ",
            "yyyy-MM-ddTHH:mm:ss.fffK",
            "yyyy-MM-ddTHH:mmZ",
            "yyyy-MM-ddTHH:mmK"
        };

        internal static EssentialsTime GetEssentialsTime(this JObject obj, string propertyName)
        {
            JToken value = obj?.GetValue(propertyName);

            switch (value?.Type)
            {
                case JTokenType.String:
                    return TryParse((string)value, out DateTimeOffset result) ? new EssentialsTime(result) : null;

                default:
                    return null;
            }
        }

        internal static TimeSpan? GetTimeSpanNullable(this JObject obj, string propertyName)
        {
            JToken value = obj?.GetValue(propertyName);

            switch (value?.Type)
            {
                case JTokenType.String:
                    return XmlSchemaUtils.ParseDuration((string)value);

                default:
                    return null;
            }
        }

        internal static T? GetEnumNullable<T>(this JObject obj, string propertyName) where T : struct
        {
            return ParseEnumNullable<T>(obj.GetString(propertyName));
        }

        internal static bool? GetBooleanNullable(this JObject obj, string propertyName)
        {
            JToken value = obj?.GetValue(propertyName);

            switch (value?.Type)
            {
                case null:
                case JTokenType.Null:
                case JTokenType.None:
                    return null;

                case JTokenType.Integer:
                    switch ((long)value)
                    {
                        case 0: return false;
                        case 1: return true;
                        default: return null;
                    }

                case JTokenType.String:
                    return StringUtils.TryParseBoolean((string)value, out bool result) ? result : (bool?)null;

                default:
                    return null;
            }
        }

        /// <summary>
        /// Parses the specified the specified <paramref name="value"/> into an instance of <typeparamref name="T"/>, or <c>null</c> if empty.
        /// </summary>
        /// <typeparam name="T">The type of the enum.</typeparam>
        /// <param name="value">The value to parse.</param>
        /// <returns>An instance of <typeparamref name="T"/> of <paramref name="value"/> is not empty; otherwise; <c>null</c>.</returns>
        internal static T? ParseEnumNullable<T>(string value) where T : struct
        {
            if (string.IsNullOrWhiteSpace(value)) return null;
            return EnumUtils.ParseEnum<T>(value);
        }

        static T ParseEnum<T>(string str) where T : Enum
        {
            if (TryParseEnum(str, out T value)) return value;
            throw new EnumParseException(typeof(T), str);
        }

        static bool TryParseEnum<T>(string str, out T value) where T : Enum
        {
            // Check whether the type of T is an enum
            if (EnumUtils.IsEnum<T>() == false) throw new ArgumentException("Generic type T must be an enum.");

            // Initialize "value"
            value = default;

            // Check whether the specified string is NULL (or white space)
            if (string.IsNullOrWhiteSpace(str)) return false;

            // Convert "str" to camel case and then lowercase
            string modified = StringUtils.ToCamelCase(str + string.Empty).ToLowerInvariant();

            // Parse the enum
            foreach (T v in GetEnumValues<T>())
            {
                string ordinal = Convert.ChangeType(v, typeof(int)) + string.Empty;
                string name = v.ToString().ToLowerInvariant();
                if (ordinal == modified || name == modified)
                {
                    value = v;
                    return true;
                }
            }

            return false;
        }

        static T[] GetEnumValues<T>() where T : Enum
        {
            return (T[])Enum.GetValues(typeof(T));
        }
    }
}