using System;
using System.Diagnostics.CodeAnalysis;

namespace Jsonificate
{
    [ExcludeFromCodeCoverage]
    class ReflectionReferenceAttribute : Attribute
    {
        public ReflectionReferenceAttribute(Type type) { }
    }
}
