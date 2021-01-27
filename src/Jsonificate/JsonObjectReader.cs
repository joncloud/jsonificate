using System;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.ObjectPool;

namespace Jsonificate
{
    class JsonObjectReader<T>
        where T : class
    {
        readonly SpanDictionary<IJsonPropertyReader<T>> _properties;
        readonly Type _type;
        readonly ObjectPool<T> _pool;

        public JsonObjectReader(ObjectPool<T> pool, JsonSerializerOptions options)
        {
            _pool = pool;
            _type = typeof(T);
            _properties = new SpanDictionary<IJsonPropertyReader<T>>();

            foreach (var member in _type.GetMembers(BindingFlags.Public | BindingFlags.Instance))
            {
                if (member is PropertyInfo property)
                {
                    if (!property.CanWrite)
                    {
                        continue;
                    }
                }
                else if (member is FieldInfo field)
                {
                    if (!options.IncludeFields)
                    {
                        continue;
                    }
                    if (field.IsInitOnly)
                    {
                        continue;
                    }
                }
                if (member.GetCustomAttribute<JsonIgnoreAttribute>() is not null)
                {
                    continue;
                }
                
                var name = member.Name;

                // TODO options.PropertyNameCaseInsensitive
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

                var value = member switch
                {
                    PropertyInfo p => CreatePropertyReader(p),
                    FieldInfo f => CreateFieldReader(f),
                    _ => null,
                };
                if (value is null)
                {
                    continue;
                }
                
                _properties.Add(name, value);
            }
        }

        IJsonPropertyReader<T> CreateFieldReader(FieldInfo field)
        {
            var readerType = typeof(FieldJsonPropertyReader<,>)
                .MakeGenericType(_type, field.FieldType);
            return (IJsonPropertyReader<T>)Activator.CreateInstance(readerType, new[] { field });
        }

        IJsonPropertyReader<T> CreatePropertyReader(PropertyInfo property)
        {
            var readerType = typeof(PropertyJsonPropertyReader<,>)
                .MakeGenericType(_type, property.PropertyType);
            return (IJsonPropertyReader<T>)Activator.CreateInstance(readerType, new[] { property });
        }

        public void ReadObject(ref Utf8JsonReader reader, T value, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.StartObject)
            {
                throw new JsonException($"Type mismatch expected: {JsonTokenType.StartObject}, actual: {reader.TokenType}");
            }

            while (reader.Read() && reader.TokenType == JsonTokenType.PropertyName)
            {
                if (_properties.TryGetValue(reader.ValueSpan, out var property))
                {
                    property.ReadProperty(value, ref reader, options);
                }
                else
                {
                    if (reader.Read())
                    {
                        int level = reader.TokenType == JsonTokenType.StartArray ||
                            reader.TokenType == JsonTokenType.StartObject
                            ? 1
                            : 0;

                        while (level > 0 && reader.Read())
                        {
                            level += reader.TokenType switch
                            {
                                JsonTokenType.StartArray => 1,
                                JsonTokenType.StartObject => 1,
                                JsonTokenType.EndArray => -1,
                                JsonTokenType.EndObject => -1,
                                _ => 0,
                            };
                        }
                    }
                }
            }

            if (reader.TokenType != JsonTokenType.EndObject)
            {
                throw new JsonException($"Type mismatch expected: {JsonTokenType.EndObject}, actual: {reader.TokenType}");
            }
        }
    }
}
