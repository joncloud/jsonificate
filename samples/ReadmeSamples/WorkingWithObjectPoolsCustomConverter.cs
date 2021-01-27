using System;
using System.Text.Json;
using Microsoft.Extensions.ObjectPool;

namespace ReadmeSamples
{
    class WorkingWithObjectPoolsCustomConverter : ISample
    {
        public void Run()
        {
            var provider = new DefaultObjectPoolProvider();

            // Create your own pools
            ObjectPool<Point> pool = provider.Create<Point>();

            var options = new JsonSerializerOptions();
            options.Converters.Add(new PointPoolingJsonConverter(options, pool));

            string json = "[10,53]";

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
