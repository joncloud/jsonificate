using System.Text.Json;
using Xunit;

namespace Jsonificate.Tests
{
    partial class PoolingJsonConverterTests
    {
        [Fact]
        public void Serialize_ShouldIncludeReadOnlyFields_GivenIgnoreReadOnlyFieldsFalse()
        {
            var options = new JsonSerializerOptions {
                IncludeFields = true,
                IgnoreReadOnlyFields = false,
            };

            var item = new ReadonlyFields(123);

            var context = new Context<ReadonlyFields>(item, options);
            Assert.Equal("{\"Int32\":123}", context.Json);
            Assert.Equal("{\"Int32\":0}", context.Value);
        }

        [Fact]
        public void Serialize_ShouldIncludeReadOnlyFields_GivenIgnoreReadOnlyFieldsTrue()
        {
            var options = new JsonSerializerOptions {
                IncludeFields = true,
                IgnoreReadOnlyFields = true,
            };

            var item = new ReadonlyFields(123);

            var context = new Context<ReadonlyFields>(item, options);

            var expected = "{}";
            var actual = JsonSerializer.Serialize(item, options);

            Assert.Equal(expected, actual);
        }

        class ReadonlyFields
        {
            public readonly int Int32;

            public ReadonlyFields() { }
            public ReadonlyFields(int int32)
            {
                Int32 = int32;
            }
        }
    }
}