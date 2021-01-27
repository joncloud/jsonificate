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
                        FieldInfo f => CreateFieldWriter(name, f, options),
                        PropertyInfo p => CreatePropertyWriter(name, p, options),
                        _ => null,
                    };
                })
                .Where(value => value is not null)
                .ToArray();
        }

        static IJsonPropertyWriter<T> CreateFieldWriter(string name, FieldInfo field, JsonSerializerOptions options)
        {
            var writerType = typeof(DelegatingJsonPropertyWriter<,>)
                .MakeGenericType(typeof(T), field.FieldType);

            var method = writerType.GetMethod("ForField", BindingFlags.Public | BindingFlags.Static);
            return (IJsonPropertyWriter<T>)method.Invoke(null, new object[] { name, field, options });
        }

        static IJsonPropertyWriter<T> CreatePropertyWriter(string name, PropertyInfo property, JsonSerializerOptions options)
        {
            var writerType = typeof(DelegatingJsonPropertyWriter<,>)
                .MakeGenericType(typeof(T), property.PropertyType);

            var method = writerType.GetMethod("ForProperty", BindingFlags.Public | BindingFlags.Static);
            return (IJsonPropertyWriter<T>)method.Invoke(null, new object[] { name, property, options });
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
