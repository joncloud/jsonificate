using System.Text.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.ObjectPool;
using Shared;

namespace Server
{

    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            var provider = new DefaultObjectPoolProvider();
            var pool = provider.Create<Message<double>>();

            var jsonOptions = new JsonSerializerOptions()
                .AddPoolingConverter(pool);

            services.AddSingleton<ObjectPoolProvider>(provider);
            services.AddSingleton(pool);

            services
                .AddSignalR(options => {
                    
                })
                .AddJsonProtocol(options => {
                    options.PayloadSerializerOptions = jsonOptions;
                });
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseDeveloperExceptionPage();

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGet("/", async context =>
                {
                    await context.Response.WriteAsync(@"# Try running the Client app to communicate with SignalR:
dotnet run Receive  # This starts a program to listen and receive messages
dotnet run Send     # This starts a program to send messages
");
                });
                
                endpoints.MapHub<MessageHub>("/hubs/messaging");
            });
        }
    }
}
