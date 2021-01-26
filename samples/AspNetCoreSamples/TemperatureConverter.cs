using Microsoft.AspNetCore.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace AspNetCoreSamples
{
    public class TemperatureConverter
    {
        readonly Pools _pools;
        readonly JsonSerializerOptions _options;

        public TemperatureConverter(Pools pools, JsonSerializerOptions options)
        {
            _pools = pools;
            _options = options;
        }

        public async Task FromCelsiusAsync(HttpRequest req, HttpResponse res)
        {
            var celsius = await req.ReadFromJsonAsync<Celsius>(_options);

            var fahrenheit = _pools.Fahrenheit.Get();
            fahrenheit.Temperature = celsius.ToFahrenheit();

            _pools.Celsius.Return(celsius);

            res.Headers["Content-Type"] = "application/json";
            await res.WriteAsJsonAsync(fahrenheit, _options);

            _pools.Fahrenheit.Return(fahrenheit);
        }

        public async Task FromFahrenheitAsync(HttpRequest req, HttpResponse res)
        {
            var fahrenheit = await req.ReadFromJsonAsync<Fahrenheit>(_options);

            var celsius = _pools.Celsius.Get();
            celsius.Temperature = fahrenheit.ToCelsius();

            _pools.Fahrenheit.Return(fahrenheit);

            res.Headers["Content-Type"] = "application/json";
            await res.WriteAsJsonAsync(celsius);

            _pools.Celsius.Return(celsius);
        }
    }
}
