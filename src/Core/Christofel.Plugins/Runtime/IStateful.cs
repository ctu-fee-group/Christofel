using System.Threading;
using System.Threading.Tasks;

namespace Christofel.Plugins.Runtime
{
    /// <summary>
    /// Symbols an object that can be refreshed
    /// </summary>
    public interface IRefreshable
    {
        public Task RefreshAsync(CancellationToken token = new CancellationToken());
    }
    
    /// <summary>
    /// Symbols an object that can be refreshed
    /// </summary>
    public interface IStoppable
    {
        public Task StopAsync(CancellationToken token = new CancellationToken());
    }
    
    /// <summary>
    /// Symbols an object that can be refreshed
    /// </summary>
    public interface IStartable
    {
        public Task StartAsync(CancellationToken token = new CancellationToken());
    }
}