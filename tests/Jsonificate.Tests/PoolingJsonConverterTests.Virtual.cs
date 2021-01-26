using System;
using Xunit;

namespace Jsonificate.Tests
{
    partial class PoolingJsonConverterTests
    {
        [Fact]
        public void Serialize_ShouldCallVirt_GivenOverriddenProperty()
        {
            var instance = Derived.Random();
            var context = new Context<Derived>(
                instance
            );

            Assert.Equal(context.Json, context.Value);
            Assert.Equal(instance.Base, context.Instance.Base);
            Assert.Equal(instance.Overridden, context.Instance.Overridden);
        }

        class Virtual
        {
            public virtual int Base { get; set; }

            public virtual int Overridden
            {
                get => 0;
                set { }
            }
        }

        class Derived : Virtual
        {
            int _overridden;
            public override int Overridden
            {
                get => _overridden;
                set => _overridden = value;
            }

            public static Derived Random()
            {
                return new Derived
                {
                    Base = new Random().Next(),
                    Overridden = new Random().Next(),
                };
            }
        }
    }
}