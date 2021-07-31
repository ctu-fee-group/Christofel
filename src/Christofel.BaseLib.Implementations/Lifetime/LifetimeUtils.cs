using System;
using System.Threading;
using System.Threading.Tasks;

namespace Christofel.BaseLib.Lifetime
{
    public static class LifetimeUtils
    {
        public static Task<bool> WaitForAsync(this ILifetime lifetime, LifetimeState state, int timeout)
        {
            CancellationTokenSource tokenSource = new CancellationTokenSource();
            tokenSource.CancelAfter(timeout);

            return WaitForAsync(lifetime, state, tokenSource.Token);
        }

        public static async Task<bool> WaitForAsync(this ILifetime lifetime, LifetimeState state, CancellationToken token)
        {
            while (lifetime.State < state)
            {
                try
                {
                    await Task.Delay(10, token);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
            }

            return lifetime.State >= state;
        }
    }
}