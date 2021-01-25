using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace Jsonificate.Tests
{
    class TestClass : IEquatable<TestClass>
    {
        public string String { get; set; }
        public DateTime DateTime { get; set; }
        public SubTestClass Sub { get; set; }
        public List<int> Int32s { get; set; }

        public const string JsonPropertyRenameName = "_jsonPropertyRename";

        [JsonPropertyName(JsonPropertyRenameName)]
        public int JsonPropertyRename { get; set; }

        public override bool Equals(object obj)
        {
            return obj is TestClass other &&
                Equals(other);
        }

        public bool Equals(TestClass other)
        {
            var result = String == other.String &&
                DateTime == other.DateTime &&
                Sub.Equals(other.Sub) &&
                Int32s.Count == other.Int32s.Count &&
                JsonPropertyRename == other.JsonPropertyRename;

            if (result)
            {
                var count = Int32s.Count;
                for (var i = 0; i < count; i++)
                {
                    if (Int32s[i] != other.Int32s[i])
                    {
                        return false;
                    }
                }
            }
            return result;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(
                String,
                DateTime,
                Sub,
                Int32s,
                JsonPropertyRename
            );
        }

        public static TestClass Random()
        {
            return new TestClass
            {
                String = Guid.NewGuid().ToString(),
                DateTime = DateTime.UtcNow,
                Sub = new SubTestClass
                {
                    Guid = Guid.NewGuid(),
                },
                Int32s = Enumerable.Repeat(new Random(), 10)
                    .Select(x => x.Next())
                    .ToList(),
                JsonPropertyRename = new Random().Next(),
            };
        }
    }
}
