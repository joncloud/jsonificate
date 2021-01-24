using System;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.ObjectPool;

namespace Jsonificate
{
    class JsonObjectReader<TInstance>
        where TInstance : class
    {
        readonly SpanDictionary<IJsonPropertyReader<TInstance>> _properties;
        readonly Type _type;
        readonly ObjectPool<TInstance> _pool;

        public JsonObjectReader(ObjectPool<TInstance> pool, JsonSerializerOptions options)
        {
            _pool = pool;
            _type = typeof(TInstance);
            _properties = new SpanDictionary<IJsonPropertyReader<TInstance>>();

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
                var value = (IJsonPropertyReader<TInstance>)Activator.CreateInstance(importerType, new[] { property });
                _properties.Add(key, value);
            }
        }

        public TInstance ReadObject(ref Utf8JsonReader reader, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.StartObject)
            {
                throw new JsonException($"Type mismatch expected: {JsonTokenType.StartObject}, actual: {reader.TokenType}");
            }

            var instance = _pool.Get();
            while (reader.Read() && reader.TokenType == JsonTokenType.PropertyName)
            {
                if (_properties.TryGetValue(reader.ValueSpan, out var property))
                {
                    property.ReadProperty(instance, ref reader, options);
                }
                else
                {
                    var propertyName = reader.GetString();
                    throw new InvalidOperationException($"Unable to find {propertyName} for {_type}, with instance {instance}");
                }
            }

            if (reader.TokenType != JsonTokenType.EndObject)
            {
                throw new JsonException($"Type mismatch expected: {JsonTokenType.EndObject}, actual: {reader.TokenType}");
            }

            return instance;
        }
    }
}
