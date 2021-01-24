using System.Text.Json;

namespace Jsonificate
{
    interface IJsonPropertyReader<TInstance>
        where TInstance : class
    {
        void ReadProperty(TInstance instance, ref Utf8JsonReader reader, JsonSerializerOptions options);
    }
}
