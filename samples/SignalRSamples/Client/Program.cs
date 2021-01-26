using System;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.ObjectPool;
using Shared;

namespace Client
{
    class Program
    {
        static async Task<int> Main(string[] args)
        {
            var arg = args.ElementAtOrDefault(0) ?? "";

            if (!Enum.TryParse<Command>(arg, ignoreCase: true, out var command))
            {
                return 1;
            }

            var provider = new DefaultObjectPoolProvider();
            var pool = provider.Create<Message<double>>();

            var jsonOptions = new JsonSerializerOptions()
                .AddPoolingConverter(pool);

            var builder = new HubConnectionBuilder()
                .WithUrl("http://localhost:5000/hubs/messaging")
                .AddJsonProtocol(options =>
                {
                    options.PayloadSerializerOptions = jsonOptions;
                });

            var connection = builder.Build();
            await connection.StartAsync();

            var cts = new CancellationTokenSource();
            Console.CancelKeyPress += (sender, e) =>
            {
                if (cts.IsCancellationRequested)
                {
                    return;
                }

                Console.WriteLine("Stopping...");
                e.Cancel = true;
                cts.Cancel();
            };

            ICommandHandler handler = command switch
            {
                Command.Receive => new Receiver(pool, connection),
                Command.Send => new Sender(pool, connection),
                _ => throw new InvalidOperationException("Invalid command"),
            };

            Console.WriteLine($"Starting {handler}...");
            handler.Start(cts.Token);

            cts.Token.WaitHandle.WaitOne();

            return 0;
        }
    }
}
