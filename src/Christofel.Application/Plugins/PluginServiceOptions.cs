using System;

namespace Christofel.Application.Plugins
{
    public class PluginServiceOptions
    {
        public PluginServiceOptions(string folder)
        {
            Folder = folder;
        }
        
        public string Folder { get; set; }
    }
}