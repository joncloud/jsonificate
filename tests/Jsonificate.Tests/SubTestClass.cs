using System;

namespace Jsonificate.Tests
{
    class SubTestClass : IEquatable<SubTestClass>
    {
        public Guid Guid { get; set; }

        public override bool Equals(object obj)
        {
            return obj is SubTestClass other &&
                Equals(other);
        }

        public bool Equals(SubTestClass other)
        {
            return Guid == other.Guid;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Guid);
        }
    }
}
