using System.Threading;
using System.Threading.Tasks;
using Christofel.BaseLib.Plugins;

namespace Christofel.CommandsLib.Commands
{
    public interface ICommandsRegistrator : IStartable, IStoppable, IRefreshable
    {
        public Task RegisterCommandsAsync(ICommandHolder holder, CancellationToken token = default);
        
        public Task UnregisterCommandsAsync(ICommandHolder holder, CancellationToken token = default);

        public Task RefreshCommandsAndPermissionsAsync(ICommandHolder holder, CancellationToken token = default);
    }
}