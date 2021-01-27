using System;
using System.Text.Json;
using Xunit;

namespace Jsonificate.Tests
{
    partial class PoolingJsonConverterTests
    {
        [Fact]
        public void Serialize_ShouldIgnoreProperty_GivenDifferingCaseAndCaseInsensitiveFalse()
        {
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = false,
            };
            var instance = CaseInsensitive.Random();

            var context = new Context<CaseInsensitive>(
                instance,
                options
            );

            var json = "{\"vAlUe\":123,\"REALLYlongPropertyNameThatGoesBeyond256Characters________________________________________________________________________________________________________________________________________________________________________________________________________________\":456}";
            var expected = (0, 0);
            var result = JsonSerializer.Deserialize<CaseInsensitive>(json, options);
            var actual = (result.Value, result.ReallyLongPropertyNameThatGoesBeyond256Characters________________________________________________________________________________________________________________________________________________________________________________________________________________);

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Serialize_ShouldIncludeProperty_GivenDifferingCaseAndCaseInsensitiveTrue()
        {
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
            };
            var instance = CaseInsensitive.Random();

            var context = new Context<CaseInsensitive>(
                instance,
                options
            );

            var json = "{\"vAlUe\":123,\"REALLYlongPropertyNameThatGoesBeyond256Characters________________________________________________________________________________________________________________________________________________________________________________________________________________\":456}";
            var expected = (123, 456);
            var result = JsonSerializer.Deserialize<CaseInsensitive>(json, options);
            var actual = (result.Value, result.ReallyLongPropertyNameThatGoesBeyond256Characters________________________________________________________________________________________________________________________________________________________________________________________________________________);

            Assert.Equal(expected, actual);
        }

        class CaseInsensitive
        {
            public int Value { get; set; }
            public int ReallyLongPropertyNameThatGoesBeyond256Characters________________________________________________________________________________________________________________________________________________________________________________________________________________ { get; set; }

            public static CaseInsensitive Random()
            {
                return new CaseInsensitive
                {
                    Value = new Random().Next(),
                    ReallyLongPropertyNameThatGoesBeyond256Characters________________________________________________________________________________________________________________________________________________________________________________________________________________ = new Random().Next(),
                };
            }
        }
    }
}