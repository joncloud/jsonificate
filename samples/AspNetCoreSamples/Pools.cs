using Microsoft.Extensions.ObjectPool;

namespace AspNetCoreSamples
{
    public class Pools
    {
        public ObjectPool<Celsius> Celsius { get; }
        public ObjectPool<Fahrenheit> Fahrenheit { get; }
        public Pools(ObjectPool<Celsius> celsius, ObjectPool<Fahrenheit> fahrenheit)
        {
            Celsius = celsius;
            Fahrenheit = fahrenheit;
        }
    }
}
