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
    public class Program
    {
        // Api can run either as standalone service or as a plugin

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