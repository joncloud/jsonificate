using System;
using System.Text.Json;
using Xunit;

namespace Jsonificate.Tests
{
    public class JsonClonerTests
    {
        [Fact]
        public void Clone_ShouldDeepClone_GivenNonNull()
        {
            var options = new JsonSerializerOptions();
            var target = options.CreateCloner();
            var expected = TestClass.Random();

            var actual = target.Clone(expected);

            Assert.Equal(expected, actual);
            Assert.NotSame(expected, actual);
        }

        [Fact]
        public void Clone_ShouldReturnNull_GivenNull()
        {
            var options = new JsonSerializerOptions();
            var target = options.CreateCloner();
            var expected = default(TestClass);

            var actual = target.Clone(expected);

            Assert.Null(actual);
        }

        [Fact]
        public void Ctor_ShouldThrowArgumentNullException_GivenNullOptions()
        {
            var options = default(JsonSerializerOptions);

            var ex = Assert.Throws<ArgumentNullException>(
                () => new JsonCloner(options)
            );
            Assert.Equal("options", ex.ParamName);
        }
    }
}
