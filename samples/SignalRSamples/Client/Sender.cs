using System;
using System.Threading;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.ObjectPool;
using Shared;

namespace Client
{
    class Sender : ICommandHandler
    {
        readonly ObjectPool<Message<double>> _pool;
        readonly HubConnection _connection;
        readonly Random _random;

        public Sender(ObjectPool<Message<double>> pool, HubConnection connection)
        {
            _pool = pool;
            _connection = connection;
            _random = new Random();
        }

        System.Timers.Timer _timer;

        public void Start(CancellationToken cancellationToken)
        {
            _timer = new System.Timers.Timer
            {
                Interval = 100,
            };
            _timer.Elapsed += async (sender, e) => {
                var message = _pool.Get();

                message.Content = _random.NextDouble();

                Console.WriteLine($"Sending {message.Content}");
                await _connection.SendAsync("SendMessage", message, cancellationToken);

                _pool.Return(message);
            };
            _timer.Start();
            cancellationToken.Register(_timer.Stop);
        }
    }
}
