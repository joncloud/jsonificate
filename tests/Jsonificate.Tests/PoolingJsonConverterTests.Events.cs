using System;
using System.Text.Json;
using Xunit;

namespace Jsonificate.Tests
{
    partial class PoolingJsonConverterTests
    {
        [Fact]
        public void Serialize_ShouldIgnoreEvents()
        {
            var instance = new EventHolder();
            instance.NothingHappened += () => { };
            var context = new Context<EventHolder>(instance);

            using var document = JsonDocument.Parse(context.Json);

            var rootElement = document.RootElement;

            Assert.False(
                rootElement.TryGetProperty(nameof(EventHolder.NothingHappened), out var _),
                $"Document should **not** have a property of {nameof(EventHolder.NothingHappened)}"
            );
        }

        class EventHolder
        {
            public event Action NothingHappened;
        }
    }
}