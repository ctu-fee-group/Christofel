using System.Threading.Tasks;

namespace Christofel.BaseLib.Plugins
{
    /// <summary>
    /// Symbols an object that can be refreshed
    /// </summary>
    public interface IRefreshable
    {
        public Task RefreshAsync();
    }
    
    /// <summary>
    /// Symbols an object that can be refreshed
    /// </summary>
    public interface IStoppable
    {
        public Task StopAsync();
    }
    
    /// <summary>
    /// Symbols an object that can be refreshed
    /// </summary>
    public interface IStartable
    {
        public Task StartAsync();
    }
}