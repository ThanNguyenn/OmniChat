using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace OmniChat.Infrastructure.Extensions
{
    public class NullableUtcDateTimeJsonConverter : JsonConverter<DateTime?>
    {
        private const string Format = "yyyy-MM-ddTHH:mm:ss.fffZ";

        public override DateTime? Read(
            ref Utf8JsonReader reader,
            Type typeToConvert,
            JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Null)
                return null;

            var value = reader.GetString();

            if (string.IsNullOrWhiteSpace(value))
                return null;

            var dto = DateTimeOffset.Parse(value);
            return dto.UtcDateTime;
        }

        public override void Write(
            Utf8JsonWriter writer,
            DateTime? value,
            JsonSerializerOptions options)
        {
            if (value == null)
            {
                writer.WriteNullValue();
                return;
            }

            writer.WriteStringValue(
                value.Value.ToUniversalTime().ToString(Format)
            );
        }
    }
}
