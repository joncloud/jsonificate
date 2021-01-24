using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.ObjectPool;

namespace Jsonificate
{
    public class PoolingJsonConverter<T> : JsonConverter<T>
        where T : class
    {
        readonly ObjectPool<T> _pool;
        readonly JsonObjectReader<T> _importer;
        readonly JsonObjectWriter<T> _exporter;

        public PoolingJsonConverter(ObjectPool<T> pool, JsonSerializerOptions options)
        {
            _pool = pool;
            _importer = new JsonObjectReader<T>(pool, options);
            _exporter = new JsonObjectWriter<T>(options);
        }

        public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            return _importer.ReadObject(ref reader, options);
        }

        public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
        {
            _exporter.WriteObject(writer, value, options);
        }
    }
}
