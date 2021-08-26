using System;
using System.Threading;
using System.Threading.Tasks;
using Christofel.BaseLib;
using Christofel.BaseLib.Database;
using Christofel.BaseLib.Extensions;
using Christofel.BaseLib.Lifetime;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

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