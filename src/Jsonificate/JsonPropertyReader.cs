using System;
using System.Reflection;
using System.Reflection.Emit;
using System.Text.Json;

namespace Jsonificate
{
    class JsonPropertyReader<TInstance, TProperty> : IJsonPropertyReader<TInstance>
        where TInstance : class
    {
        readonly Action<TInstance, TProperty> _func;

        [ReflectionReference(typeof(Jsonificate.JsonObjectReader<>))]
        public JsonPropertyReader(PropertyInfo property)
        {
            var method = new DynamicMethod(
                "SetValue",
                typeof(void),
                new[] { typeof(TInstance), typeof(TProperty) },
                typeof(JsonPropertyReader<,>).Module
            );
            var il = method.GetILGenerator();
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldarg_1);

            var setMethod = property.GetSetMethod();
            il.Emit(setMethod.IsVirtual ? OpCodes.Callvirt : OpCodes.Call, setMethod);
            il.Emit(OpCodes.Ret);

            _func = method.CreateDelegate<Action<TInstance, TProperty>>();
        }

        public void ReadProperty(TInstance instance, ref Utf8JsonReader reader, JsonSerializerOptions options)
        {
            var property = JsonSerializer.Deserialize<TProperty>(ref reader, options);
            _func(instance, property);
        }
    }
}
