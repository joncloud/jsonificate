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

            foreach (var property in _type.GetProperties())
            {
                if (!property.CanWrite)
                {
                    continue;
                }
                
                var name = property.Name;

                // TODO options.PropertyNameCaseInsensitive
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

                var key = name;

                var importerType = typeof(JsonPropertyReader<,>)
                    .MakeGenericType(_type, property.PropertyType);
                var value = (IJsonPropertyReader<T>)Activator.CreateInstance(importerType, new[] { property });
                _properties.Add(key, value);
            }
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
                    var propertyName = reader.GetString();
                    throw new JsonException($"Unable to find {propertyName} for {_type}, with instance {value}");
                }
            }

            if (reader.TokenType != JsonTokenType.EndObject)
            {
                throw new JsonException($"Type mismatch expected: {JsonTokenType.EndObject}, actual: {reader.TokenType}");
            }
        }
    }
}
