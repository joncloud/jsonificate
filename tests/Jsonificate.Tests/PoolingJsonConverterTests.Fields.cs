using System;
using System.Text.Json;
using Xunit;

namespace Jsonificate.Tests
{
    partial class PoolingJsonConverterTests
    {
        [Fact]
        public void Serialize_ShouldIgnoreField_GivenIncludeFieldsFalse()
        {
            var context = new Context<Fields>(Fields.Random());

            using var document = JsonDocument.Parse(context.Json);

            var rootElement = document.RootElement;

            Assert.False(
                rootElement.TryGetProperty(nameof(Fields.Int32), out var _),
                $"Document should **not** have a property of {nameof(Fields.Int32)}"
            );

            var json = "{\"Int32\":123}";

            var result = JsonSerializer.Deserialize<Fields>(json, context.Options);
            Assert.Equal(0, result.Int32);
        }

        [Fact]
        public void Serialize_ShouldIncludeField_GivenIncludeFieldsTrue()
        {
            var options = new JsonSerializerOptions {
                IncludeFields = true,
            };

            var expected = Fields.Random();
            var context = new Context<Fields>(expected, options);

            Assert.Equal(context.Json, context.Value);
            Assert.Equal(expected, context.Instance);
        }

        class Fields : IEquatable<Fields>
        {
            public int Int32;


            public override bool Equals(object obj)
            {
                return obj is Fields other &&
                    Equals(other);
            }

            public bool Equals(Fields other)
            {
                return Int32 == other.Int32;
            }

            public override int GetHashCode()
            {
                return HashCode.Combine(Int32);
            }

            public static Fields Random()
            {
                return new Fields
                {
                    Int32 = new Random().Next(),
                };
            }
        }
    }
}