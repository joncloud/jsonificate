using System;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using Xunit;

namespace Jsonificate.Tests
{
    partial class PoolingJsonConverterTests
    {
        [Fact]
        public void Serialize_ShouldRenameProperty_GivenJsonPropertyNameAttribute()
        {
            var context = new Context<Naming>(Naming.Random());

            using var document = JsonDocument.Parse(context.Json);

            var rootElement = document.RootElement;

            Assert.False(
                rootElement.TryGetProperty(nameof(Naming.Renamed), out var _),
                $"Document should **not** have a property of {nameof(Naming.Renamed)}"
            );

            Assert.True(
                rootElement.TryGetProperty(Naming.RenamedName, out var property),
                $"Document should have a property of {nameof(Naming.RenamedName)}"
            );

            Assert.Equal(context.Instance.Renamed, property.GetInt32());
        }

        [Fact]
        public void Serialize_ShouldRenameProperty_GivenPropertyNamingConvention()
        {
            var options = new JsonSerializerOptions {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            };

            var context = new Context<Naming>(Naming.Random(), options);

            using var document = JsonDocument.Parse(context.Json);

            var rootElement = document.RootElement;

            var expected = new [] {
                options.PropertyNamingPolicy.ConvertName(nameof(Naming.Plain)),
                Naming.RenamedName
            };

            var actual = expected
                .Where(name => rootElement.TryGetProperty(name, out var _))
                .ToArray();

            Assert.Equal(expected, actual);
        }

        class Naming : IEquatable<Naming>
        {
            public int Plain { get; set; }

            public const string RenamedName = "_renamed";

            [JsonPropertyName(RenamedName)]
            public int Renamed { get; set; }

            public override bool Equals(object obj)
            {
                return obj is Naming other &&
                    Equals(other);
            }

            public bool Equals(Naming other)
            {
                return Plain == other.Plain &&
                    Renamed == other.Renamed;
            }

            public override int GetHashCode()
            {
                return HashCode.Combine(Plain, Renamed);
            }

            public static Naming Random()
            {
                return new Naming
                {
                    Plain = new Random().Next(),
                    Renamed = new Random().Next(),
                };
            }
        }
    }
}