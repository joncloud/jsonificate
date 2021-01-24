using Jsonificate;
using Microsoft.Extensions.ObjectPool;

namespace System.Text.Json
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
