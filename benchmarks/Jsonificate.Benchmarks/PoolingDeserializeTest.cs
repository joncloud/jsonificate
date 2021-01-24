using System.Text.Json;
using Microsoft.Extensions.ObjectPool;

namespace Jsonificate.Benchmarks
{
    class PoolingDeserializeTest : IDeserializeTest
    {
        readonly string _simpleJson;
        readonly ObjectPool<Simple> _simplePool;
        readonly string _complexJson;
        readonly ObjectPool<Complex> _complexPool;
        readonly JsonSerializerOptions _options;

        public PoolingDeserializeTest(string simpleJson, string complexJson)
        {
            _simpleJson = simpleJson;
            _complexJson = complexJson;

            var provider = new DefaultObjectPoolProvider();
            _simplePool = provider.Create(new DefaultPooledObjectPolicy<Simple>());
            _complexPool = provider.Create(new DefaultPooledObjectPolicy<Complex>());

            _options = new JsonSerializerOptions()
                .AddPoolingConverter(_simplePool)
                .AddPoolingConverter(_complexPool);
        }

        public int Complex()
        {
            var result = JsonSerializer.Deserialize<Complex>(_complexJson, _options);
            var num = 0;
            unchecked
            {
                num += result.One.Int32;
                num += result.Two.Int32;
                num += result.Three.Int32;
            }
            _simplePool.Return(result.One);
            _simplePool.Return(result.Two);
            _simplePool.Return(result.Three);
            _complexPool.Return(result);
            return num;
        }

        public int Simple()
        {
            var result = JsonSerializer.Deserialize<Simple>(_simpleJson, _options);
            var num = result.Int32;
            _simplePool.Return(result);
            return num;
        }
    }
}
