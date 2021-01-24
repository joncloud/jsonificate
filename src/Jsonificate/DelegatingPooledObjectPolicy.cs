using System;
using Microsoft.Extensions.ObjectPool;

namespace Jsonificate
{
    class DelegatingPooledObjectPolicy<T> : IPooledObjectPolicy<T>
    {
        readonly Func<T> _fn;

        public DelegatingPooledObjectPolicy(Func<T> fn)
        {
            _fn = fn;
        }

        public T Create() => _fn();

        public bool Return(T obj) => true;
    }
}
