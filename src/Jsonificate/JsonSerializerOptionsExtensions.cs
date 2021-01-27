using Jsonificate;
using Microsoft.Extensions.ObjectPool;

namespace System.Text.Json
{
    /// <summary>
    /// Extends <see cref="System.Text.Json.JsonSerializerOptions" /> with additional features.
    /// </summary>
    public static class JsonSerializerOptionsExtensions
    {
        /// <summary>
        /// Adds a converter that is capable of deserializing objects from a pool.
        /// </summary>
        public static JsonSerializerOptions AddPoolingConverter<T>(this JsonSerializerOptions options, ObjectPool<T> pool)
            where T : class
        {
            if (options is null) throw new ArgumentNullException(nameof(options));
            if (pool is null) throw new ArgumentNullException(nameof(pool));

            options.Converters.Add(new PoolingJsonConverter<T>(options, pool));
            return options;
        }

        /// <summary>
        /// Creates a new JSON cloner.
        /// </summary>
        public static JsonCloner CreateCloner(this JsonSerializerOptions options)
        {
            if (options is null) throw new ArgumentNullException(nameof(options));

            return new JsonCloner(options);
        }
    }
}
