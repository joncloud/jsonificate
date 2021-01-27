using System;
using System.Buffers;
using System.Text.Json;
using Microsoft.Extensions.ObjectPool;

namespace Jsonificate
{
    /// <summary>
    /// Creates deep cloned copies of objects using JSON.
    /// </summary>
    public class JsonCloner : IJsonCloner
    {
        static readonly DefaultObjectPoolProvider _provider = new DefaultObjectPoolProvider();
        readonly ObjectPool<Cloner> _clonerPool;
        readonly JsonSerializerOptions _options;

        /// <summary>
        /// Initializes a new <see cref="JsonCloner" /> instance.
        /// </summary>
        public JsonCloner(JsonSerializerOptions options)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _clonerPool = _provider.Create(new DelegatingPooledObjectPolicy<Cloner>(CreateCloner));
        }

        Cloner CreateCloner()
        {
            return new Cloner(_options);
        }

        /// <summary>
        /// Creates a deep clone of the input item.
        /// </summary>
        public T Clone<T>(T item)
        {
            var cloner = _clonerPool.Get();

            var clone = cloner.Clone(item);

            _clonerPool.Return(cloner);

            return clone;
        }

        class Cloner
        {
            readonly ArrayBufferWriter<byte> _bufferWriter;
            readonly Utf8JsonWriter _jsonWriter;
            readonly JsonSerializerOptions _options;

            public Cloner(JsonSerializerOptions options)
            {
                _options = options;
                _bufferWriter = new ArrayBufferWriter<byte>(256);
                _jsonWriter = new Utf8JsonWriter(_bufferWriter);
            }
            public T Clone<T>(T item)
            {
                _bufferWriter.Clear();
                _jsonWriter.Reset();

                JsonSerializer.Serialize(_jsonWriter, item, _options);

                return JsonSerializer.Deserialize<T>(_bufferWriter.WrittenSpan, _options);
            }
        }
    }
}
