using System;
using System.Reflection;
using System.Reflection.Emit;
using System.Text.Json;

namespace Jsonificate
{
    class FieldJsonPropertyReader<TInstance, TField> : IJsonPropertyReader<TInstance>
        where TInstance : class
    {
        readonly Action<TInstance, TField> _func;

        [ReflectionReference(typeof(Jsonificate.JsonObjectReader<>))]
        public FieldJsonPropertyReader(FieldInfo field)
        {
            var method = new DynamicMethod(
                "SetValue",
                typeof(void),
                new[] { typeof(TInstance), typeof(TField) },
                typeof(FieldJsonPropertyReader<,>).Module
            );

            var il = method.GetILGenerator();
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldarg_1);

            il.Emit(OpCodes.Stfld, field);
            il.Emit(OpCodes.Ret);

            _func = method.CreateDelegate<Action<TInstance, TField>>();
        }

        public void ReadProperty(TInstance instance, ref Utf8JsonReader reader, JsonSerializerOptions options)
        {
            var property = JsonSerializer.Deserialize<TField>(ref reader, options);
            _func(instance, property);
        }
    }
}
