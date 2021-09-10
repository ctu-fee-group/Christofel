//
//   Program.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Threading;
using System.Threading.Tasks;

namespace Christofel.Api
{
    /// <summary>
    /// The entry point class of the Api.
    /// </summary>
    public class Program
    {
        // Api can run either as standalone service or as a plugin

        /// <summary>
        /// The entry point of the Api.
        /// </summary>
        /// <param name="args">The command line arguments.</param>
        /// <returns>A <see cref="Task"/> that represents the command line arguments.</returns>
        public static async Task Main(string[] args)
        {
            var app = new ApiApp();

            EventWaitHandle exitEvent = new EventWaitHandle(false, EventResetMode.ManualReset);

            AppDomain.CurrentDomain.ProcessExit += (sender, e) =>
            {
                // ReSharper disable once AccessToModifiedClosure
                // ReSharper disable once AccessToDisposedClosure
                app?.Lifetime.RequestStop();
                exitEvent.WaitOne();
            };

            Console.CancelKeyPress += (sender, e) =>
            {
                e.Cancel = true;

                // ReSharper disable once AccessToModifiedClosure
                // ReSharper disable once AccessToDisposedClosure
                app?.Lifetime.RequestStop();
            };

            app.Init();
            await app.RunAsync();
            app.Dispose();
            app = null;

            exitEvent.Set();
            Console.WriteLine("Goodbye!");
        }
    }
}