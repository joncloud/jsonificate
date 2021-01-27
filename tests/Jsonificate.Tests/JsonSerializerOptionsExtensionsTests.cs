using System;
using System.Text.Json;
using Microsoft.Extensions.ObjectPool;
using Xunit;

namespace Jsonificate.Tests
{
    public class JsonSerializerOptionsExtensionsTests
    {
        [Fact]
        public void AddPoolingConverter_ShouldThrowArgumentNullException_GivenNullOptions()
        {
            var options = default(JsonSerializerOptions);
            var pool = new DefaultObjectPoolProvider().Create<TestClass>();

            var ex = Assert.Throws<ArgumentNullException>(
                () => JsonSerializerOptionsExtensions.AddPoolingConverter(options, pool)
            );
            Assert.Equal("options", ex.ParamName);
        }

        [Fact]
        public void AddPoolingConverter_ShouldThrowArgumentNullException_GivenNullPool()
        {
            var options = new JsonSerializerOptions();
            var pool = default(ObjectPool<TestClass>);

            var ex = Assert.Throws<ArgumentNullException>(
                () => JsonSerializerOptionsExtensions.AddPoolingConverter(options, pool)
            );
            Assert.Equal("pool", ex.ParamName);
        }

        [Fact]
        public void CreateCloner_ShouldThrowArgumentNullException_GivenNullOptions()
        {
            var options = default(JsonSerializerOptions);

            var ex = Assert.Throws<ArgumentNullException>(
                () => JsonSerializerOptionsExtensions.CreateCloner(options)
            );
            Assert.Equal("options", ex.ParamName);
        }
    }
}