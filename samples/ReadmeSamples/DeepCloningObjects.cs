using System;
using System.Text.Json;

namespace ReadmeSamples
{
    class DeepCloningObjects : ISample
    {
        public void Run()
        {
            var options = new JsonSerializerOptions();

            // Setup options your way.

            var cloner = options.CreateCloner();

            var original = new Point { X = 10, Y = 53 };
            var clone = cloner.Clone(original);

            Console.WriteLine($"Original: {original}");
            Console.WriteLine($"Clone: {clone}");
            Console.WriteLine(object.ReferenceEquals(original, clone));
        }
    }
}
