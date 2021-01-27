using System;
using System.Collections.Generic;
using System.Text;

namespace Jsonificate
{
    class SpanDictionary<TValue>
    {
        readonly GetHashCode _getHashCode;
        readonly Dictionary<int, TValue> _inner = new Dictionary<int, TValue>();

        public SpanDictionary(GetHashCode getHashCode)
        {
            _getHashCode = getHashCode;
        }

        public void Add(string key, TValue value)
        {
            var chars = key.AsSpan();
            var byteCount = Encoding.UTF8.GetByteCount(chars);

            Span<byte> bytes = byteCount <= 256
                ? stackalloc byte[256].Slice(0, byteCount)
                : new byte[byteCount];
            
            var realByteCount = Encoding.UTF8.GetBytes(chars, bytes);

            _inner.Add(_getHashCode(bytes.Slice(0, realByteCount)), value);
        }

        public bool TryGetValue(ReadOnlySpan<byte> key, out TValue value)
        {
            return _inner.TryGetValue(_getHashCode(key), out value);
        }
    }
}
