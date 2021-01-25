using System;
using System.Buffers;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Jsonificate
{
    class JsonObjectWriter<T>
        where T : class
    {
        readonly IJsonPropertyWriter<T>[] _properties;

        public JsonObjectWriter(JsonSerializerOptions options)
        {
            _properties = typeof(T).GetMembers(BindingFlags.Public | BindingFlags.Instance)
                .Where(member =>
                {
                    if (member is PropertyInfo property)
                    {
                        if (!property.CanRead)
                        {
                            return false;
                        }
                    }
                    else if (member is FieldInfo field)
                    {
                        if (!options.IncludeFields)
                        {
                            return false;
                        }
                        if (options.IgnoreReadOnlyFields && field.IsInitOnly)
                        {
                            return false;
                        }
                    }
                    if (member.GetCustomAttribute<JsonIgnoreAttribute>() is not null)
                    {
                        return false;
                    }
                    return true;
                }
                )
                .Select(member =>
                {
                    var name = member.Name;

                    // TODO options.JsonNumberHandling
                    // TODO options.IgnoreNullValues
                    // TODO options.JsonIgnoreCondition
                    // TODO options.AllowTrailingCommas
                    
                    var propertyName = member.GetCustomAttribute<JsonPropertyNameAttribute>();
                    if (propertyName is not null)
                    {
                        name = propertyName.Name;
                    }
                    else if (options.PropertyNamingPolicy is not null)
                    {
                        name = options.PropertyNamingPolicy.ConvertName(name);
                    }

                    return member switch
                    {
                        FieldInfo f => CreateFieldWriter(name, f),
                        PropertyInfo p => CreatePropertyWriter(name, p),
                        _ => null,
                    };
                })
                .Where(value => value is not null)
                .ToArray();
        }

        static IJsonPropertyWriter<T> CreateFieldWriter(string name, FieldInfo field)
        {
            var writerType = typeof(FieldJsonPropertyWriter<,>)
                .MakeGenericType(typeof(T), field.FieldType);
            return (IJsonPropertyWriter<T>)Activator.CreateInstance(writerType, new object[] { name, field });
        }

        static IJsonPropertyWriter<T> CreatePropertyWriter(string name, PropertyInfo property)
        {
            var writerType = typeof(PropertyJsonPropertyWriter<,>)
                .MakeGenericType(typeof(T), property.PropertyType);
            return (IJsonPropertyWriter<T>)Activator.CreateInstance(writerType, new object[] { name, property });
        }

        public void WriteObject(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
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
