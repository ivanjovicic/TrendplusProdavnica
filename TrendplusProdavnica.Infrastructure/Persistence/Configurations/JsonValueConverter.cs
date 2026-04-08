#nullable enable
using System;
using System.Text.Json;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace TrendplusProdavnica.Infrastructure.Persistence.Configurations
{
    public class JsonValueConverter<T> : ValueConverter<T?, string>
        where T : class
    {
        public JsonValueConverter() : base(
            v => Serialize(v),
            v => Deserialize(v)
        ) { }

        public static ValueConverter<T, string> NonNullable()
        {
            return new ValueConverter<T, string>(
                v => Serialize(v),
                v => DeserializeNonNullable(v));
        }

        private static string Serialize(T? value)
        {
            if (value is null)
            {
                return string.Empty;
            }

            return JsonSerializer.Serialize(value, (JsonSerializerOptions?)null);
        }

        private static T? Deserialize(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return default;
            }

            return JsonSerializer.Deserialize<T>(value, (JsonSerializerOptions?)null);
        }

        private static T DeserializeNonNullable(string value)
        {
            var deserialized = Deserialize(value);
            if (deserialized is null)
            {
                throw new InvalidOperationException("Deserialization returned null for non-nullable value.");
            }

            return deserialized;
        }
    }
}
