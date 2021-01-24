using System.Text.Json;
using Microsoft.Extensions.ObjectPool;
using Xunit;

namespace Jsonificate.Tests
{
    public class PoolingJsonConverterTests
    {
        [Fact]
        public void Serialize_ShouldReuseFromPool()
        {
            var provider = new DefaultObjectPoolProvider();
            var pool = provider.Create(new DefaultPooledObjectPolicy<TestClass>());
            var options = new JsonSerializerOptions()
                .AddPoolingConverter(pool);

            var expected = new Context(options);

            pool.Return(expected.Instance);

            var actual = new Context(options);

            Assert.NotEqual(expected.Value, actual.Value);
            Assert.Same(expected.Instance, actual.Instance);
        }

        class Context
        {
            public string Json { get; }
            public TestClass Instance { get; }
            public string Value { get; }

            public Context(JsonSerializerOptions options)
            {
                Json = JsonSerializer.Serialize(
                    TestClass.Random(),
                    options
                );
                Instance = JsonSerializer.Deserialize<TestClass>(
                    Json,
                    options
                );
                Value = JsonSerializer.Serialize(Instance, options);
            }
        }
    }
}