using System;
using System.Text.Json;
using Jsonificate;
using Microsoft.Extensions.ObjectPool;

namespace ReadmeSamples
{
    public class PointPoolingJsonConverter : PoolingJsonConverter<Point>
    {
        public PointPoolingJsonConverter(ObjectPool<Point> pool, JsonSerializerOptions options)
          : base(pool, options)
        {
        }

        protected override void Populate(ref Utf8JsonReader reader, Type typeToConvert, Point value, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.StartArray)
            {
                throw new JsonException();
            }

            value.X = reader.Read() ? reader.GetInt32() : throw new JsonException();
            value.Y = reader.Read() ? reader.GetInt32() : throw new JsonException();

            if (!reader.Read() || reader.TokenType != JsonTokenType.EndArray)
            {
                throw new JsonException();
            }
        }

        public override void Write(Utf8JsonWriter writer, Point value, JsonSerializerOptions options)
        {
            writer.WriteStartArray();
            writer.WriteNumberValue(value.X);
            writer.WriteNumberValue(value.Y);
            writer.WriteEndArray();
        }
    }
}
