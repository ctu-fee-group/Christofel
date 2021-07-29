using System;
using System.Threading.Tasks;
using System.Xml.Schema;

namespace Christofel.Application
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            ChristofelApp application = new ChristofelApp();

            // App lifetime cycle
            await application.InitAsync();
            await application.RunAsync(); // blocks until Bot.QuitBot is called
            await application.StopAsync();
            await application.DestroyAsync();
            
            Console.WriteLine("Goodbye!");
        }
    }
}