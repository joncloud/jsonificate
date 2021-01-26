using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Xunit;

namespace Jsonificate.Tests
{
    partial class PoolingJsonConverterTests
    {
        [Fact]
        public void Serialize_ShouldIgnoreProperty_GivenJsonIgnoreAttribute()
        {
            var context = new Context<Ignore>(Ignore.Random());

            using var document = JsonDocument.Parse(context.Json);

            var rootElement = document.RootElement;

            Assert.False(
                rootElement.TryGetProperty(nameof(Ignore.Ignored), out var _),
                $"Document should **not** have a property of {nameof(Ignore.Ignored)}"
            );

            var json = "{\"Ignored\":123}";

            var result = JsonSerializer.Deserialize<Ignore>(json, context.Options);
            Assert.Equal(0, result.Ignored);
        }
        
        [Theory]
        [InlineData("{\"Obj\":{\"Prop\":123}}")]
        [InlineData("{\"Arr\":[1,2,3]}")]
        [InlineData("{\"Obj\":{\"Arr\":[1,2,3]}}")]
        [InlineData("{\"Arr\":[{\"Prop\":123}]}")]
        [InlineData("{\"Level\":{\"One\":{\"Two\":2}}}")]
        public void Serialize_ShouldKeepReading_GivenAdditionalComplexProperty(string json)
        {
            var context = new Context<Ignore>(Ignore.Random());

            var result = JsonSerializer.Deserialize<Ignore>(json, context.Options);
            Assert.Equal(0, result.Ignored);
        }

        class Ignore
        {
            [JsonIgnore]
            public int Ignored { get; set; }

            public static Ignore Random()
            {
                return new Ignore
                {
                    Ignored = new Random().Next(),
                };
            }
        }
    }
}