//
//   Program.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Threading;
using System.Threading.Tasks;

namespace Christofel.Application
{
    /// <summary>
    /// The entry point class of Christofel.
    /// </summary>
    public class Program
    {
        /// <summary>
        /// The main entry point of Christofel.
        /// </summary>
        /// <param name="args">Command line arguments.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
        public static async Task Main(string[] args)
        {
            try
            {
                ChristofelApp application = new ChristofelApp(args);
                EventWaitHandle exitEvent = new EventWaitHandle(false, EventResetMode.ManualReset);

                AppDomain.CurrentDomain.ProcessExit += (sender, e) =>
                {
                    application.Lifetime.RequestStop();
                    exitEvent.WaitOne();
                };

                Console.CancelKeyPress += (sender, e) =>
                {
                    e.Cancel = true;
                    application.Lifetime.RequestStop();
                };

                // App lifetime cycle
                await application.InitAsync();
                await application.RunAsync(); // blocks until Bot.QuitBot is called

                await application.RunBlockAsync();

                exitEvent.Set();
                Console.WriteLine("Goodbye!");
            }
            catch (Exception e)
            {
                Console.WriteLine("Caught an exception in Main.");
                Console.WriteLine(e);
            }
        }
    }
}