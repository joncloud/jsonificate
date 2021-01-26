using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Microsoft.Extensions.ObjectPool;
using Xunit;

namespace Jsonificate.Tests
{
    public class PoolingJsonConverterTests
    {
        readonly DefaultObjectPoolProvider _provider;
        readonly ObjectPool<TestClass> _pool;
        readonly JsonSerializerOptions _options;

        public PoolingJsonConverterTests()
        {
            _provider = new DefaultObjectPoolProvider();
            _pool = _provider.Create(new DefaultPooledObjectPolicy<TestClass>());
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
            var expected = new Context<TestClass>(_options, TestClass.Random());

            _pool.Return(expected.Instance);

            var actual = new Context<TestClass>(_options, TestClass.Random());

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
            var context = new Context<TestClass>(_options, TestClass.Random());

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

            var context = new Context<TestClass>(options, TestClass.Random());

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
            var context = new Context<TestClass>(_options, TestClass.Random());

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
            var context = new Context<TestClass>(_options, TestClass.Random());

            var json = context.Json.Substring(0, context.Json.Length - 1) + additionalJson;

            var result = JsonSerializer.Deserialize<TestClass>(json, _options);
            Assert.Equal(0, result.Ignored);
        }

        [Fact]
        public void Serialize_ShouldIgnoreField_GivenIncludeFieldsFalse()
        {
            var context = new Context<TestClass>(_options, TestClass.Random());

            using var document = JsonDocument.Parse(context.Json);

            var rootElement = document.RootElement;

            Assert.False(
                rootElement.TryGetProperty(nameof(TestClass.Int32Field), out var _),
                $"Document should **not** have a property of {nameof(TestClass.Int32Field)}"
            );

            var json = context.Json.Substring(0, context.Json.Length - 1) + ",\"Int32Field\":123}";

            var result = JsonSerializer.Deserialize<TestClass>(json, _options);
            Assert.Equal(0, result.Int32Field);
        }

        [Fact]
        public void Serialize_ShouldIncludeField_GivenIncludeFieldsTrue()
        {
            var options = new JsonSerializerOptions {
                IncludeFields = true,
            }.AddPoolingConverter(_pool);

            var testClass = TestClass.Random();
            testClass.Int32Field = new Random().Next();

            var context = new Context<TestClass>(options, testClass);

            Assert.Equal(testClass.Int32Field, context.Instance.Int32Field);
        }

        [Fact]
        public void Serialize_ShouldIgnoreEvents()
        {
            var testClass = TestClass.Random();
            testClass.NothingHappened += () => { };
            var context = new Context<TestClass>(_options, testClass);

            using var document = JsonDocument.Parse(context.Json);

            var rootElement = document.RootElement;

            Assert.False(
                rootElement.TryGetProperty(nameof(TestClass.NothingHappened), out var _),
                $"Document should **not** have a property of {nameof(TestClass.Int32Field)}"
            );
        }

        [Fact]
        public void Serialize_ShouldIncludeReadOnlyFields_GivenIgnoreReadOnlyFieldsFalse()
        {
            var pool = _provider.Create<ReadonlyFields>();
            var options = new JsonSerializerOptions {
                IncludeFields = true,
                IgnoreReadOnlyFields = false,
            }.AddPoolingConverter(pool);

            var item = new ReadonlyFields(123);

            var expected = "{\"Int32\":123}";
            var actual = JsonSerializer.Serialize(item, options);

            Assert.Equal(expected, actual);

            var read = JsonSerializer.Deserialize<ReadonlyFields>(actual, options);

            Assert.Equal(0, read.Int32);
        }

        [Fact]
        public void Serialize_ShouldIncludeReadOnlyFields_GivenIgnoreReadOnlyFieldsTrue()
        {
            var pool = _provider.Create<ReadonlyFields>();
            var options = new JsonSerializerOptions {
                IncludeFields = true,
                IgnoreReadOnlyFields = true,
            }.AddPoolingConverter(pool);

            var item = new ReadonlyFields(123);

            var expected = "{}";
            var actual = JsonSerializer.Serialize(item, options);

            Assert.Equal(expected, actual);
        }

        class ReadonlyFields
        {
            public readonly int Int32;

            public ReadonlyFields()
            {
                
            }
            public ReadonlyFields(int int32)
            {
                Int32 = int32;
            }
        }
    }
}