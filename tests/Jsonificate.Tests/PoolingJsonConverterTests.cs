using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Microsoft.Extensions.ObjectPool;
using Xunit;

namespace Jsonificate.Tests
{
    public partial class PoolingJsonConverterTests
    {
        [Fact]
        public void Deserialize_ShouldThrowJsonException_GivenMissingStartObject()
        {
            var pool = new DefaultObjectPoolProvider().Create<TestClass>();
            var options = new JsonSerializerOptions().AddPoolingConverter(
                pool
            );
            var json = JsonSerializer.Serialize(TestClass.Random(), options);
            json = "[" + json.Substring(1);

            var ex = Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<TestClass>(json, options));

            var expected = $"Type mismatch expected: {JsonTokenType.StartObject}, actual: {JsonTokenType.StartArray}";
            var actual = ex.Message;

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Serialize_ShouldReuseFromPool()
        {
            var expected = new Context<TestClass>(TestClass.Random());

            expected.Pool.Return(expected.Instance);

            var actual = new Context<TestClass>(TestClass.Random(), expected.Pool);

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
            var context = new Context<TestClass>(TestClass.Random());

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
            };

            var context = new Context<TestClass>(TestClass.Random(), options);

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
            var context = new Context<TestClass>(TestClass.Random());

            using var document = JsonDocument.Parse(context.Json);

            var rootElement = document.RootElement;

            Assert.False(
                rootElement.TryGetProperty(nameof(TestClass.Ignored), out var _),
                $"Document should **not** have a property of {nameof(TestClass.Ignored)}"
            );

            var json = context.Json.Substring(0, context.Json.Length - 1) + ",\"Ignored\":123}";

            var result = JsonSerializer.Deserialize<TestClass>(json, context.Options);
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
            var context = new Context<TestClass>(TestClass.Random());

            var json = context.Json.Substring(0, context.Json.Length - 1) + additionalJson;

            var result = JsonSerializer.Deserialize<TestClass>(json, context.Options);
            Assert.Equal(0, result.Ignored);
        }
    }
}