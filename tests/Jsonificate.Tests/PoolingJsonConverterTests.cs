using System.Collections.Generic;
using System.Text.Json;
using Microsoft.Extensions.ObjectPool;
using Xunit;

namespace Jsonificate.Tests
{
    public class PoolingJsonConverterTests
    {
        readonly ObjectPool<TestClass> _pool;
        readonly JsonSerializerOptions _options;

        public PoolingJsonConverterTests()
        {
            var provider = new DefaultObjectPoolProvider();
            _pool = provider.Create(new DefaultPooledObjectPolicy<TestClass>());
            _options = new JsonSerializerOptions()
                .AddPoolingConverter(_pool);
        }

        [Fact]
        public void Deserialize_ShouldThrowJsonException_GivenMissingStartObject()
        {
            var json = JsonSerializer.Serialize(TestClass.Random(), _options);
            json = "[" + json.Substring(1);

            var ex = Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<TestClass>(json, _options));

            var expected = $"Type mismatch expected: {JsonTokenType.StartObject}, actual: {JsonTokenType.StartArray}";
            var actual = ex.Message;

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Deserialize_ShouldThrowJsonException_GivenMissingProperty()
        {
            var json = JsonSerializer.Serialize(TestClass.Random(), _options);
            json = json.Substring(0, json.Length - 1) + ",\"anotherProperty\":\"\"}";

            var ex = Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<TestClass>(json, _options));

            var expected = $"Unable to find anotherProperty for {typeof(TestClass)}, with instance Jsonificate.Tests.TestClass";
            var actual = ex.Message;

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Serialize_ShouldReuseFromPool()
        {
            var expected = new Context(_options);

            _pool.Return(expected.Instance);

            var actual = new Context(_options);

            Assert.NotEqual(expected.Value, actual.Value);
            Assert.Same(expected.Instance, actual.Instance);

            var withDefaultValues = new TestClass
            {
                DateTime = default,
                Int32s = new List<int>(),
                String = default,
                Sub = new SubTestClass
                {
                    Guid = default,
                },
            };
            Assert.NotEqual(expected.Instance, withDefaultValues);
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