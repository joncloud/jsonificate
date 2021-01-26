using System.Text.Json;

namespace Jsonificate.Tests
{
    class Context<T>
    {
        public string Json { get; }
        public T Instance { get; }
        public string Value { get; }

        public Context(JsonSerializerOptions options, T instance)
        {
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