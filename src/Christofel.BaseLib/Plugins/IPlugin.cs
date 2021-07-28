using System.Threading.Tasks;

namespace Christofel.BaseLib.Plugins
{
    public interface IPlugin
    {
        public string Name { get; }
        public string Description { get; }
        public string Version { get; }

        public Task InitAsync(IChristofelState state);
        public Task RefreshAsync();
        public Task DestroyAsync();
    }
}