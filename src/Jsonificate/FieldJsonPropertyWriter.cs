using System;
using System.Reflection;
using System.Reflection.Emit;
using System.Text.Json;

namespace Jsonificate
{
    class FieldJsonPropertyWriter<TInstance, TField> : IJsonPropertyWriter<TInstance>
    {
        readonly Func<TInstance, TField> _func;
        readonly string _name;

        [ReflectionReference(typeof(Jsonificate.JsonObjectWriter<>))]
        public FieldJsonPropertyWriter(string name, FieldInfo field)
        {
            _name = name;

            var method = new DynamicMethod(
                "GetValue",
                typeof(TField),
                new[] { typeof(TInstance) },
                typeof(PropertyJsonPropertyWriter<,>).Module
            );
            var il = method.GetILGenerator();
            il.Emit(OpCodes.Ldarg_0);

            il.Emit(OpCodes.Ldfld, field);
            il.Emit(OpCodes.Ret);

            _func = method.CreateDelegate<Func<TInstance, TField>>();
        }

        public void WriteProperty(Utf8JsonWriter writer, TInstance instance, JsonSerializerOptions options)
        {
            writer.WritePropertyName(_name);
            var value = _func(instance);
            JsonSerializer.Serialize(writer, value, options);
        }
    }
}
