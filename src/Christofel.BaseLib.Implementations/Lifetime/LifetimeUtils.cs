using System;
using System.Threading;
using System.Threading.Tasks;

namespace Christofel.BaseLib.Lifetime
{
    public static class LifetimeUtils
    {
        /// <summary>
        /// Waits for specified timeout for the given state
        /// </summary>
        /// <remarks>
        /// Because specific state may not be captured in time,
        /// actually looks for the given state or any latter one
        /// </remarks>
        /// <param name="lifetime"></param>
        /// <param name="state"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        public static Task<bool> WaitForAsync(this ILifetime lifetime, LifetimeState state, int timeout)
        {
            CancellationTokenSource tokenSource = new CancellationTokenSource();
            tokenSource.CancelAfter(timeout);

            return WaitForAsync(lifetime, state, tokenSource.Token);
        }

        /// <summary>
        /// Wait for state until token is canceled
        /// </summary>
        /// <param name="lifetime"></param>
        /// <param name="state"></param>
        /// <param name="token"></param>
        /// <returns></returns>
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