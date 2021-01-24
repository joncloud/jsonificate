using System.Linq;
using BenchmarkDotNet.Running;

namespace Jsonificate.Benchmarks
{
    class Program
    {
        static void Main(string[] args)
        {
            switch (args.ElementAtOrDefault(0))
            {
                case nameof(ClonerTests):
                    {
                        BenchmarkRunner.Run<ClonerTests>();
                        break;
                    }
                case nameof(DeserializeTests):
                    {
                        BenchmarkRunner.Run<DeserializeTests>();
                        break;
                    }
                default:
                    {
                        BenchmarkRunner.Run<ClonerTests>();
                        BenchmarkRunner.Run<DeserializeTests>();
                        break;
                    }
            }
        }
    }
}
