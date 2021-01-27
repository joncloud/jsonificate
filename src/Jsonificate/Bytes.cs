using System;
using System.Text;

namespace Jsonificate
{
    delegate int GetHashCode(ReadOnlySpan<byte> bytes);

    static class Bytes
    {
        public static int Exact(ReadOnlySpan<byte> bytes)
        {
            unchecked // Overflow is fine, just wrap
            {
                int hash = 17;
                int index = bytes.Length;
                while (--index >= 0)
                {
                    hash = hash * 23 + bytes[index];
                }
                return hash;
            }
        }

        public static int CaseInsensitive(ReadOnlySpan<byte> bytes)
        {
            var charLength = Encoding.UTF8.GetCharCount(bytes);

            Span<char> chars = charLength <= 256
                ? stackalloc char[256].Slice(0, charLength)
                : new char[charLength];

            var realCharLength = Encoding.UTF8.GetChars(bytes, chars);

            var hashCode = string.GetHashCode(
                chars.Slice(0, realCharLength),
                StringComparison.OrdinalIgnoreCase
            );

            return hashCode;
        }
    }
}
