using System.Buffers;
using System.Text.Json;
using Microsoft.Extensions.ObjectPool;

namespace Jsonificate
{
    public class JsonCloner : IJsonCloner
    {
        static readonly DefaultObjectPoolProvider _provider = new DefaultObjectPoolProvider();
        readonly ObjectPool<Cloner> _clonerPool;
        readonly JsonSerializerOptions _options;
        
        public JsonCloner(JsonSerializerOptions options)
        {
            _options = options;
            _clonerPool = _provider.Create(new DelegatingPooledObjectPolicy<Cloner>(CreateCloner));
        }

        Cloner CreateCloner()
        {
            return new Cloner(_options);
        }

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
