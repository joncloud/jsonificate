using System.Text.Json;

namespace Jsonificate
{
    interface IJsonPropertyWriter<TInstance>
    {
        void WriteProperty(Utf8JsonWriter reader, TInstance instance, JsonSerializerOptions options);
    }
}
