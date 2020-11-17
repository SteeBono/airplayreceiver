using System;
using System.Threading;
using System.Threading.Tasks;

namespace AirPlay
{
    public abstract class BaseListener
    {
        public abstract Task StartAsync(CancellationToken cancellationToken);

        public abstract Task StopAsync();
    }
}
