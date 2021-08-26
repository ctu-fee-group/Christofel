using System;
using System.Threading;
using System.Threading.Tasks;

namespace Christofel.Application
{
    public class Program
    {
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