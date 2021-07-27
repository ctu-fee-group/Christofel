using System.Threading.Tasks;
using System.Xml.Schema;

namespace Christofel.Application
{
    public class Program
    {
        public static void Main(string[] args)
            => MainAsync(args).GetAwaiter().GetResult();

        public static async Task MainAsync(string[] args)
        {
            ChristofelApp application = new ChristofelApp();

            await application.InitAsync();
            await application.RunAsync();
            await application.DestroyAsync();
        }
    }
}