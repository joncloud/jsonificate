using System.Text.Json;
using Microsoft.Extensions.ObjectPool;

namespace Jsonificate
{
    public static class JsonSerializerOptionsExtensions
    {
        public static JsonSerializerOptions AddPoolingConverter<T>(this JsonSerializerOptions options, ObjectPool<T> pool)
            where T : class
        {
            options.Converters.Add(new PoolingJsonConverter<T>(pool, options));
            return options;
        }

        public static JsonCloner CreateCloner(this JsonSerializerOptions options)
        {
            return new JsonCloner(options);
        }
    }
}
