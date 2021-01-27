using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.ObjectPool;

namespace Jsonificate
{
    /// <summary>
    /// Converts an object to or from JSON using <see cref="ObjectPool{T}" />.
    /// </summary>
    public class PoolingJsonConverter<T> : JsonConverter<T>
        where T : class
    {
        readonly ObjectPool<T> _pool;
        readonly JsonObjectReader<T> _importer;
        readonly JsonObjectWriter<T> _exporter;

        /// <summary>
        /// Initializes a new <see cref="PoolingJsonConverter{T}" /> instance.
        /// </summary>
        public PoolingJsonConverter(JsonSerializerOptions options, ObjectPool<T> pool)
        {
            if (options is null) throw new ArgumentNullException(nameof(options));
            
            _pool = pool ?? throw new ArgumentNullException(nameof(pool));
            _importer = new JsonObjectReader<T>(pool, options);
            _exporter = new JsonObjectWriter<T>(options);
        }

        /// <summary>
        /// Reads and converts the JSON into an existing instance provided by the object pool.
        /// </summary>
        public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var value = _pool.Get();
            Populate(ref reader, typeToConvert, value, options);
            return value;
        }

        /// <summary>
        /// Populates the specified value's data from JSON.
        /// </summary>
        protected virtual void Populate(ref Utf8JsonReader reader, Type typeToConvert, T value, JsonSerializerOptions options)
        {
            _importer.ReadObject(ref reader, value, options);
        }

        /// <summary>
        /// Writes a specified value as JSON.
        /// </summary>
        public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
        {
            _exporter.WriteObject(writer, value, options);
        }
    }
}
