using System;
using System.Buffers;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Jsonificate
{
    class JsonObjectWriter<TInstance>
        where TInstance : class
    {
        readonly IJsonPropertyWriter<TInstance>[] _properties;

        public JsonObjectWriter(JsonSerializerOptions options)
        {
            _properties = typeof(TInstance).GetProperties()
                .Where(property =>
                    property.CanRead &&
                    property.GetCustomAttribute<JsonIgnoreAttribute>() is null
                )
                .Select(property =>
                {
                    var name = property.Name;

                    // TODO options.JsonNumberHandling
                    // TODO options.IgnoreReadOnlyFields
                    // TODO options.IgnoreNullValues
                    // TODO options.JsonIgnoreCondition
                    // TODO options.AllowTrailingCommas
                    
                    var propertyName = property.GetCustomAttribute<JsonPropertyNameAttribute>();
                    if (propertyName is not null)
                    {
                        name = propertyName.Name;
                    }
                    else if (options.PropertyNamingPolicy is not null)
                    {
                        name = options.PropertyNamingPolicy.ConvertName(name);
                    }

                    var type = typeof(JsonPropertyWriter<,>)
                        .MakeGenericType(typeof(TInstance), property.PropertyType);
                    return (IJsonPropertyWriter<TInstance>)Activator.CreateInstance(type, new object[] { name, property });
                })
                .ToArray();
        }

        public void WriteObject(Utf8JsonWriter writer, TInstance value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            foreach (var property in _properties)
            {
                property.WriteProperty(writer, value, options);
            }
            writer.WriteEndObject();
        }
    }
}
