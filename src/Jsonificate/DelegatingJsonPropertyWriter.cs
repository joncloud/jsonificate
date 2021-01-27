using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Jsonificate
{
    class DelegatingJsonPropertyWriter<TInstance, TValue> : IJsonPropertyWriter<TInstance>
    {
        static readonly Dictionary<JsonIgnoreCondition, Func<TValue, bool>> _ignoreConditions = new Dictionary<JsonIgnoreCondition, Func<TValue, bool>>
        {
            [JsonIgnoreCondition.Never] = _ => false,
            [JsonIgnoreCondition.Always] = _ => true,
            [JsonIgnoreCondition.WhenWritingDefault] = value => object.Equals(value, default),
            [JsonIgnoreCondition.WhenWritingNull] = value => value is null,
        };

        readonly Func<TInstance, TValue> _func;
        readonly string _name;
        readonly Func<TValue, bool> _ignore;

        public DelegatingJsonPropertyWriter(Func<TInstance, TValue> func, string name, JsonIgnoreCondition ignoreCondition)
        {
            _func = func;
            _name = name;
            _ignore = _ignoreConditions[ignoreCondition];
        }

        [ReflectionReference(typeof(Jsonificate.JsonObjectWriter<>))]
        public static DelegatingJsonPropertyWriter<TInstance, TValue> ForField(string name, FieldInfo field, JsonSerializerOptions options)
        {
            var method = new DynamicMethod(
                "GetValue",
                typeof(TValue),
                new[] { typeof(TInstance) },
                typeof(DelegatingJsonPropertyWriter<,>).Module
            );
            var il = method.GetILGenerator();
            il.Emit(OpCodes.Ldarg_0);

            il.Emit(OpCodes.Ldfld, field);
            il.Emit(OpCodes.Ret);

            var func = method.CreateDelegate<Func<TInstance, TValue>>();
            return new DelegatingJsonPropertyWriter<TInstance, TValue>(func, name, options.DefaultIgnoreCondition);
        }

        [ReflectionReference(typeof(Jsonificate.JsonObjectWriter<>))]
        public static DelegatingJsonPropertyWriter<TInstance, TValue> ForProperty(string name, PropertyInfo property, JsonSerializerOptions options)
        {
            var method = new DynamicMethod(
                "GetValue",
                typeof(TValue),
                new[] { typeof(TInstance) },
                typeof(DelegatingJsonPropertyWriter<,>).Module
            );
            var il = method.GetILGenerator();
            il.Emit(OpCodes.Ldarg_0);

            var getMethod = property.GetGetMethod();
            il.Emit(getMethod.IsVirtual ? OpCodes.Callvirt : OpCodes.Call, getMethod);
            il.Emit(OpCodes.Ret);

            var func = method.CreateDelegate<Func<TInstance, TValue>>();
            return new DelegatingJsonPropertyWriter<TInstance, TValue>(func, name, options.DefaultIgnoreCondition);
        }

        public void WriteProperty(Utf8JsonWriter writer, TInstance instance, JsonSerializerOptions options)
        {
            var value = _func(instance);
            if (!_ignore(value))
            {
                writer.WritePropertyName(_name);
                JsonSerializer.Serialize(writer, value, options);
            }
        }
    }
}
