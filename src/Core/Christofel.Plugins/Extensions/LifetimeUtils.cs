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
    public static class LifetimeUtils
    {
        /// <summary>
        ///     Waits for specified timeout for the given state
        /// </summary>
        /// <remarks>
        ///     Because specific state may not be captured in time,
        ///     actually looks for the given state or any latter one
        /// </remarks>
        /// <param name="lifetime"></param>
        /// <param name="state"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
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
                    lifetime, state,
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
        ///     Wait for state until token is canceled
        /// </summary>
        /// <param name="lifetime"></param>
        /// <param name="state"></param>
        /// <param name="token"></param>
        /// <returns></returns>
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