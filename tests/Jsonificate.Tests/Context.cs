using System.Text.Json;
using Microsoft.Extensions.ObjectPool;

namespace Jsonificate.Tests
{
    class Context<T>
        where T : class, new()
    {
        static readonly DefaultObjectPoolProvider _poolProvider = new DefaultObjectPoolProvider();
        
        public JsonSerializerOptions Options { get; }
        public ObjectPool<T> Pool { get; }
        public string Json { get; }
        public T Instance { get; }
        public string Value { get; }

        public Context(T instance)
            : this(instance, _poolProvider.Create<T>(), new JsonSerializerOptions())
        {

        }

        public Context(T instance, JsonSerializerOptions options)
            : this(instance, _poolProvider.Create<T>(), options)
        {
        }

        public Context(T instance, ObjectPool<T> pool)
            : this(instance, pool, new JsonSerializerOptions())
        {
        }

        public Context(T instance, ObjectPool<T> pool, JsonSerializerOptions options)
        {
            Options = options.AddPoolingConverter(pool);
            Pool = pool;
            Json = JsonSerializer.Serialize(
                instance,
                options
            );
            Instance = JsonSerializer.Deserialize<T>(
                Json,
                options
            );
            Value = JsonSerializer.Serialize(Instance, options);
        }
    }
}