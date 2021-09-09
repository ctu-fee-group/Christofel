using Christofel.Plugins.Runtime;
using Christofel.Remora;

namespace Christofel.BaseLib.Plugins
{
    public abstract class ChristofelDIPlugin : DIRuntimePlugin<IChristofelState, IPluginContext>
    {
        protected override IPluginContext InitializeContext()
        {
            return new PluginContext();
        }
    }
}