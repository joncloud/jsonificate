using System;
using System.Text.Json;
using Microsoft.Extensions.ObjectPool;

namespace ReadmeSamples
{
    class WorkingWithObjectPools : ISample
    {
        public void Run()
        {
            var provider = new DefaultObjectPoolProvider();

            // Create your own pools
            ObjectPool<Point> pool = provider.Create<Point>();

            var options = new JsonSerializerOptions()
                .AddPoolingConverter(pool);

            string json = "{\"X\":10,\"Y\":53}";

            var p = JsonSerializer.Deserialize<Point>(json, options);

            DoWork(p);

            pool.Return(p);
        }

        void DoWork(Point point)
        {
            Console.WriteLine(point);
        }
    }
}
