using System;
using System.Reflection;
using System.Reflection.Emit;
using System.Text.Json;

namespace Jsonificate
{
    class JsonPropertyWriter<TInstance, TProperty> : IJsonPropertyWriter<TInstance>
    {
        readonly Func<TInstance, TProperty> _func;
        readonly string _name;

        [ReflectionReference(typeof(Jsonificate.JsonObjectWriter<>))]
        public JsonPropertyWriter(string name, PropertyInfo property)
        {
            _name = name;

            var method = new DynamicMethod(
                "GetValue",
                typeof(TProperty),
                new[] { typeof(TInstance) },
                typeof(JsonPropertyWriter<,>).Module
            );
            var il = method.GetILGenerator();
            il.Emit(OpCodes.Ldarg_0);

            var getMethod = property.GetGetMethod();
            il.Emit(getMethod.IsVirtual ? OpCodes.Callvirt : OpCodes.Call, getMethod);
            il.Emit(OpCodes.Ret);

            _func = method.CreateDelegate<Func<TInstance, TProperty>>();
        }

        public void WriteProperty(Utf8JsonWriter writer, TInstance instance, JsonSerializerOptions options)
        {
            writer.WritePropertyName(_name);
            var value = _func(instance);
            JsonSerializer.Serialize(writer, value, options);
        }
    }
}
