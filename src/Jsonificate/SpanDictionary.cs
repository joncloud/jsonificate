using System;
using System.Collections.Generic;
using System.Text;

namespace Jsonificate
{
    class SpanDictionary<TValue>
    {
        readonly Dictionary<int, TValue> _inner = new Dictionary<int, TValue>();
        static int GetHashCode(ReadOnlySpan<byte> span)
        {
            unchecked // Overflow is fine, just wrap
            {
                int hash = 17;
                int index = span.Length;
                while (--index >= 0)
                {
                    hash = hash * 23 + span[index];
                }
                return hash;
            }
        }

        public void Add(string key, TValue value)
        {
            // TODO probably could make this no allocation
            _inner.Add(GetHashCode(Encoding.UTF8.GetBytes(key)), value);
        }

        public bool TryGetValue(ReadOnlySpan<byte> key, out TValue value)
        {
            return _inner.TryGetValue(GetHashCode(key), out value);
        }
    }
}
