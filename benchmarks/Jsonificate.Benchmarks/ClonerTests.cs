using System.Text.Json;
using BenchmarkDotNet.Attributes;
using Microsoft.Extensions.ObjectPool;

namespace Jsonificate.Benchmarks
{
    [MemoryDiagnoser]
    public class ClonerTests
    {
        JsonCloner _clonerPool;
        JsonCloner _clonerNoPool;
        ObjectPool<Simple> _simplePool;
        Simple _root;


        [GlobalSetup]
        public void Setup()
        {
            var provider = new DefaultObjectPoolProvider();
            _simplePool = provider.Create(new DefaultPooledObjectPolicy<Simple>());

            _clonerPool = new JsonCloner(
                new JsonSerializerOptions()
                    .AddPoolingConverter(_simplePool)
            );

            _clonerNoPool = new JsonCloner(
                new JsonSerializerOptions()
            );
            _root = new Simple { Int32 = int.MaxValue };
        }

        [Benchmark(Baseline = true)]
        public int Clone_NoPool()
        {
            return _clonerNoPool.Clone(_root).Int32;
        }

        [Benchmark]
        public int Clone_Pooled()
        {
            var item = _clonerPool.Clone(_root);
            var num = item.Int32;
            _simplePool.Return(item);
            return num;
        }
    }
}
