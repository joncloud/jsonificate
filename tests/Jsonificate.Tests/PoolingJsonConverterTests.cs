using System;
using System.Collections.Generic;
using System.Text.Json;
using Microsoft.Extensions.ObjectPool;
using Xunit;

namespace Jsonificate.Tests
{
    public partial class PoolingJsonConverterTests
    {
        [Fact]
        public void Ctor_ShouldThrowArgumentNullException_GivenNullOptions()
        {
            var options = default(JsonSerializerOptions);
            var pool = new DefaultObjectPoolProvider().Create<TestClass>();

            var ex = Assert.Throws<ArgumentNullException>(
                () => new PoolingJsonConverter<TestClass>(options, pool)
            );
            Assert.Equal("options", ex.ParamName);
        }

        [Fact]
        public void Ctor_ShouldThrowArgumentNullException_GivenNullPool()
        {
            var options = new JsonSerializerOptions();
            var pool = default(ObjectPool<TestClass>);

            var ex = Assert.Throws<ArgumentNullException>(
                () => new PoolingJsonConverter<TestClass>(options, pool)
            );
            Assert.Equal("pool", ex.ParamName);
        }

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
    }
}