using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace OmniChat.Infrastructure.Extensions
{
    public class UtcDateTimeJsonConverter : JsonConverter<DateTime>
    {
        private const string Format = "yyyy-MM-ddTHH:mm:ss.fffZ";

        public override DateTime Read(
            ref Utf8JsonReader reader,
            Type typeToConvert,
            JsonSerializerOptions options)
        {
            var value = reader.GetString();
            return DateTime.SpecifyKind(DateTime.Parse(value), DateTimeKind.Utc);
        }

        public override void Write(
            Utf8JsonWriter writer,
            DateTime value,
            JsonSerializerOptions options)
        {
            writer.WriteStringValue(
                value.ToUniversalTime().ToString(Format)
            );
        }

        //set up datetime trong program.cs 
    //    builder.Services.AddControllers()
    //.AddJsonOptions(options =>
    //{
    //        options.JsonSerializerOptions.Converters.Add(
    //            new UtcDateTimeJsonConverter()
    //        );
    //    });
    }
}
