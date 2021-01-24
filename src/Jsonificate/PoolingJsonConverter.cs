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
            var value = _pool.Get();
            Populate(ref reader, typeToConvert, value, options);
            return value;
        }

        protected virtual void Populate(ref Utf8JsonReader reader, Type typeToConvert, T value, JsonSerializerOptions options)
        {
            _importer.ReadObject(ref reader, value, options);
        }

        public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
        {
            _exporter.WriteObject(writer, value, options);
        }
    }
}
