using System.Threading;

namespace Client
{
    interface ICommandHandler
    {
        void Start(CancellationToken cancellationToken);
    }
}
