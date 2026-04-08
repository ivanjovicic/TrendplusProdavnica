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
            v => string.IsNullOrEmpty(JsonSerializer.Serialize(v, (JsonSerializerOptions?)null)) ? string.Empty : JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
            v => string.IsNullOrEmpty(v) ? default : Deserialize(v)
        ) { }

        private static T? Deserialize(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return default;
            }

            var deserialized = JsonSerializer.Deserialize<T>(value, (JsonSerializerOptions?)null);
            return deserialized;
        }
    }
}
