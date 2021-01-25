using System.Collections.Generic;
using System.Linq;
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

        [Fact]
        public void Serialize_ShouldRenameProperty_GivenJsonPropertyNameAttribute()
        {
            var context = new Context(_options);

            using var document = JsonDocument.Parse(context.Json);

            var rootElement = document.RootElement;

            Assert.False(
                rootElement.TryGetProperty(nameof(TestClass.JsonPropertyRename), out var _),
                $"Document should **not** have a property of {nameof(TestClass.JsonPropertyRename)}"
            );

            Assert.True(
                rootElement.TryGetProperty(TestClass.JsonPropertyRenameName, out var property),
                $"Document should have a property of {nameof(TestClass.JsonPropertyRename)}"
            );

            Assert.Equal(context.Instance.JsonPropertyRename, property.GetInt32());
        }

        [Fact]
        public void Serialize_ShouldRenameProperty_GivenPropertyNamingConvention()
        {
            var options = new JsonSerializerOptions {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            }.AddPoolingConverter(_pool);

            var context = new Context(options);

            using var document = JsonDocument.Parse(context.Json);

            var rootElement = document.RootElement;

            var expected = typeof(TestClass).GetProperties()
                .Select(prop => prop.Name)
                .Where(name => name != nameof(TestClass.JsonPropertyRename))
                .Where(name => name != nameof(TestClass.Ignored))
                .Select(name => options.PropertyNamingPolicy.ConvertName(name))
                .ToList();

            var actual = expected
                .Where(name => rootElement.TryGetProperty(name, out var _))
                .ToList();

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Serialize_ShouldIgnoreProperty_GivenJsonIgnoreAttribute()
        {
            var context = new Context(_options);

            using var document = JsonDocument.Parse(context.Json);

            var rootElement = document.RootElement;

            Assert.False(
                rootElement.TryGetProperty(nameof(TestClass.Ignored), out var _),
                $"Document should **not** have a property of {nameof(TestClass.Ignored)}"
            );

            var json = context.Json.Substring(0, context.Json.Length - 1) + ",\"Ignored\":123}";

            var result = JsonSerializer.Deserialize<TestClass>(json, _options);
            Assert.Equal(0, result.Ignored);
        }
        
        [Theory]
        [InlineData(",\"Obj\":{\"Prop\":123}}")]
        [InlineData(",\"Arr\":[1,2,3]}")]
        [InlineData(",\"Obj\":{\"Arr\":[1,2,3]}}")]
        [InlineData(",\"Arr\":[{\"Prop\":123}]}")]
        [InlineData(",\"Level\":{\"One\":{\"Two\":2}}}")]
        public void Serialize_ShouldKeepReading_GivenAdditionalComplexProperty(string additionalJson)
        {
            var context = new Context(_options);

            var json = context.Json.Substring(0, context.Json.Length - 1) + additionalJson;

            var result = JsonSerializer.Deserialize<TestClass>(json, _options);
            Assert.Equal(0, result.Ignored);
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