using System.Text.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.ObjectPool;

namespace AspNetCoreSamples
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            var provider = new DefaultObjectPoolProvider();
            var celsius = provider.Create<Celsius>();
            var fahrenheit = provider.Create<Fahrenheit>();

            services.AddSingleton<ObjectPoolProvider>(provider);
            services.AddSingleton(celsius);
            services.AddSingleton(fahrenheit);
            services.AddSingleton<Pools>();

            var options = new JsonSerializerOptions()
                .AddPoolingConverter(celsius)
                .AddPoolingConverter(fahrenheit);

            services.AddSingleton(options);

            services.AddSingleton<TemperatureConverter>();
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseDeveloperExceptionPage();
            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGet("/", async context =>
                {
                    await context.Response.WriteAsync(@"# Try one of the following cURL commands:
curl -H ""Content-Type: application/json"" -X POST -d '{""Temperature"":0}' http://localhost:5000/celsius
curl -H ""Content-Type: application/json"" -X POST -d '{""Temperature"":32}' http://localhost:5000/fahrenheit
");
                });

                endpoints.MapPost("/celsius", context =>
                    context.RequestServices
                        .GetRequiredService<TemperatureConverter>()
                        .FromCelsiusAsync(context.Request, context.Response)
                );

                endpoints.MapPost("/fahrenheit", context =>
                    context.RequestServices
                        .GetRequiredService<TemperatureConverter>()
                        .FromFahrenheitAsync(context.Request, context.Response)
                );
            });
        }
    }
}
