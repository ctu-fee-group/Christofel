using System.Threading;
using System.Threading.Tasks;
using Christofel.Plugins.Data;

namespace Christofel.Plugins.Services
{
    public interface IPluginLifetimeService
    {
        public bool ShouldHandle(IPlugin plugin);

        public Task<bool> InitializeAsync(AttachedPlugin plugin, CancellationToken token = default);

        public Task<bool> DestroyAsync(AttachedPlugin plugin, CancellationToken token = default);
    }
}