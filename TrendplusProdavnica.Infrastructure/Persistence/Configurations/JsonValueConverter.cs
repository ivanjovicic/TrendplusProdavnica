#nullable enable
using System.Text.Json;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace TrendplusProdavnica.Infrastructure.Persistence.Configurations
{
    public class JsonValueConverter<T> : ValueConverter<T, string>
        where T : class
    {
        public JsonValueConverter() : base(
            v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
            v => Deserialize(v)
        ) { }

        private static T Deserialize(string value)
        {
            var deserialized = JsonSerializer.Deserialize<T>(value, (JsonSerializerOptions?)null);
            return deserialized ?? throw new InvalidOperationException("Deserialization returned null.");
        }
    }
}
