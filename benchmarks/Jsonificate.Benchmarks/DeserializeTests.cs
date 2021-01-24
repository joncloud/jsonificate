using System.Text.Json;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;

namespace Jsonificate.Benchmarks
{
    [GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)]
    [CategoriesColumn]
    [MemoryDiagnoser]
    public class DeserializeTests
    {
        NoPoolingDeserializeTest _noPooling;
        PoolingDeserializeTest _pooling;

        [GlobalSetup]
        public void Setup()
        {
            var simpleJson = JsonSerializer.Serialize(new Simple
            {
                Int32 = int.MaxValue,
            });

            var complexJson = JsonSerializer.Serialize(new Complex
            {
                One = new Simple { Int32 = int.MaxValue },
                Two = new Simple { Int32 = int.MinValue },
                Three = new Simple { Int32 = int.MaxValue },
            });

            _noPooling = new NoPoolingDeserializeTest(simpleJson, complexJson);
            _pooling = new PoolingDeserializeTest(simpleJson, complexJson);
        }

        [Benchmark(Baseline = true)]
        [BenchmarkCategory(nameof(Complex))]
        public int Complex_NoPool()
        {
            return _noPooling.Complex();
        }

        [Benchmark]
        [BenchmarkCategory(nameof(Complex))]
        public int Complex_Pool()
        {
            return _pooling.Complex();
        }

        [Benchmark(Baseline = true)]
        [BenchmarkCategory(nameof(Simple))]
        public int Simple_NoPool()
        {
            return _noPooling.Simple();
        }

        [Benchmark]
        [BenchmarkCategory(nameof(Simple))]
        public int Simple_Pool()
        {
            return _pooling.Simple();
        }
    }
}
