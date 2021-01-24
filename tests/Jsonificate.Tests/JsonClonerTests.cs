using System.Text.Json;
using Xunit;

namespace Jsonificate.Tests
{
    public class JsonClonerTests
    {
        [Fact]
        public void Clone_ShouldDeepClone()
        {
            var options = new JsonSerializerOptions();
            var target = options.CreateCloner();
            var expected = TestClass.Random();

            var actual = target.Clone(expected);

            Assert.Equal(expected, actual);
            Assert.NotSame(expected, actual);
        }
    }
}
