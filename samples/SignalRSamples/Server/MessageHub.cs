using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.ObjectPool;
using Shared;

namespace Server
{
    class MessageHub : Hub
    {
        readonly ObjectPool<Message<double>> _pool;

        public MessageHub(ObjectPool<Message<double>> pool)
        {
            _pool = pool;
        }

        public async Task SendMessage(Message<double> message)
        {
            await Clients.All.SendAsync("ReceiveMessage", message);

            _pool.Return(message);
        }
    }
}
