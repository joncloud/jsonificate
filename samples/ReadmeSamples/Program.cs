using System;
using System.Linq;

namespace ReadmeSamples
{
    class Program
    {
        static void Main(string[] args)
        {
            var samples = typeof(Program).Assembly
                .GetTypes()
                .Where(type => !type.IsInterface)
                .Where(type => type.IsAssignableTo(typeof(ISample)))
                .Select(x => Activator.CreateInstance(x))
                .Cast<ISample>();

            foreach (var sample in samples)
            {
                Console.WriteLine($"# {sample.GetType()}");
                Console.WriteLine();
                sample.Run();
                Console.WriteLine();
            }
        }
    }
}
