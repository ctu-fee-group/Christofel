//
//   LifetimeUtils.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Threading;
using System.Threading.Tasks;
using Christofel.Plugins.Lifetime;

namespace Christofel.Plugins.Extensions
{
    /// <summary>
    /// Class containing extensions for waiting for <see cref="ILifetime"/> to change into some state.
    /// </summary>
    public static class LifetimeUtils
    {
        /// <summary>
        /// Waits for the specified state.
        /// </summary>
        /// <remarks>
        /// Waits for one of:
        ///  1. Lifetime reached the given (or latter) state.
        ///  2. Cancellation token was cancelled.
        ///  3. Timeout was reached.
        /// </remarks>
        /// <param name="lifetime">The lifetime to check.</param>
        /// <param name="state">State to wait for.</param>
        /// <param name="timeout">Timeout after which to return even if the timeout was not reached.</param>
        /// <param name="token">Cancellation token used for canceling the task.</param>
        /// <returns>Whether the desired state was reached.</returns>
        public static async Task<bool> WaitForAsync
        (
            this ILifetime lifetime,
            LifetimeState state,
            int timeout,
            CancellationToken token = default
        )
        {
            CancellationTokenSource tokenSource = new CancellationTokenSource();
            tokenSource.CancelAfter(timeout);

            CancellationTokenSource cancellationTokenSource =
                CancellationTokenSource.CreateLinkedTokenSource(token, tokenSource.Token);

            try
            {
                return await WaitForAsync
                (
                    lifetime,
                    state,
                    cancellationTokenSource.Token
                );
            }
            finally
            {
                tokenSource.Dispose();
                cancellationTokenSource.Dispose();
            }
        }

        /// <summary>
        /// Waits for the specified state.
        /// </summary>
        /// <remarks>
        /// Waits for one of:
        ///  1. Lifetime reached the given (or latter) state.
        ///  2. Cancellation token was cancelled.
        /// </remarks>
        /// <param name="lifetime">The lifetime to check.</param>
        /// <param name="state">State to wait for.</param>
        /// <param name="token">Cancellation token used for canceling the task.</param>
        /// <returns>Whether the desired state was reached.</returns>
        public static async Task<bool> WaitForAsync
        (
            this ILifetime lifetime,
            LifetimeState state,
            CancellationToken token
        )
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