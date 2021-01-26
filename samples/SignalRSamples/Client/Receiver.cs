using System;
using System.Threading;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.ObjectPool;
using Shared;

namespace Client
{
    class Receiver : ICommandHandler
    {
        readonly ObjectPool<Message<double>> _pool;
        readonly HubConnection _connection;

        public Receiver(ObjectPool<Message<double>> pool, HubConnection connection)
        {
            _pool = pool;
            _connection = connection;
        }

        IDisposable _subscription;

        public void Start(CancellationToken cancellationToken)
        {
            _subscription = _connection.On<Message<double>>("ReceiveMessage", message => {
                Console.WriteLine($"Received: {message.Content}");
                _pool.Return(message);
            });

            cancellationToken.Register(_subscription.Dispose);
        }
    }
}
