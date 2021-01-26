using System;
using System.Text.Json;
using Xunit;

namespace Jsonificate.Tests
{
    partial class PoolingJsonConverterTests
    {
        [Fact]
        public void Serialize_ShouldRead_GivenWriteOnlyProperty()
        {
            var instance = ReadWriteProperties.Random();
            var context = new Context<ReadWriteProperties>(
                instance
            );

            var expected = instance.ReadOnlyProperty;
            var json = $"{{\"WriteOnlyProperty\":{expected}}}";
            var actual = JsonSerializer.Deserialize<ReadWriteProperties>(
                json,
                context.Options
            ).GetWriteOnlyProperty();

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Serialize_ShouldNotRead_GivenReadOnlyProperty()
        {
            var instance = ReadWriteProperties.Random();
            var context = new Context<ReadWriteProperties>(
                instance
            );

            var expected = 0;
            var actual = context.Instance.ReadOnlyProperty;

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Serialize_ShouldNotWrite_GivenWriteOnlyProperty()
        {
            var instance = ReadWriteProperties.Random();
            var context = new Context<ReadWriteProperties>(
                instance
            );

            var expected = $"{{\"ReadOnlyProperty\":{instance.ReadOnlyProperty}}}";
            var actual = context.Json;
            Assert.Equal(expected, actual);
        }

        class ReadWriteProperties
        {
            int _readOnlyProperty;
            public int ReadOnlyProperty
            {
                get => _readOnlyProperty;
            }
            public void SetReadOnlyProperty(int value) =>
                _readOnlyProperty = value;

            int _writeOnlyValue;
            public int WriteOnlyProperty
            {
                set => _writeOnlyValue = value;
            }
            public int GetWriteOnlyProperty() =>
                _writeOnlyValue;

            public static ReadWriteProperties Random()
            {
                var instance = new ReadWriteProperties {
                    WriteOnlyProperty = new Random().Next(),
                };
                instance.SetReadOnlyProperty(new Random().Next());
                return instance;
            }
        }
    }
}