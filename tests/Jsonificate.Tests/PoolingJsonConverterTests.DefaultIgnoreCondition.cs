using System.Text.Json;
using System.Text.Json.Serialization;
using Xunit;

namespace Jsonificate.Tests
{
    partial class PoolingJsonConverterTests
    {
        [Theory]
        [InlineData(JsonIgnoreCondition.WhenWritingDefault)]
        [InlineData(JsonIgnoreCondition.WhenWritingNull)]
        [InlineData(JsonIgnoreCondition.Never)]
        public void Serialize_ShouldRespectJsonIgnoreCondition(JsonIgnoreCondition condition)
        {
            var options = new JsonSerializerOptions
            {
                DefaultIgnoreCondition = condition,
            };
            var instance = new DefaultIgnoreCondition();
            var context = new Context<DefaultIgnoreCondition>(
                instance,
                new JsonSerializerOptions(options)
            );

            var expected = JsonSerializer.Serialize(instance, options);
            var actual = context.Json;
            Assert.Equal(expected, actual);
        }

        class DefaultIgnoreCondition
        {
            public int? WithoutIgnore { get; set; }
            [JsonIgnore]
            public int? WithIgnore { get; set; }
        }
    }
}
