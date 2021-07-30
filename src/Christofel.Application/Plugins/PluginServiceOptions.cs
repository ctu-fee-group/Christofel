using System;
using System.Collections.Generic;

namespace Christofel.Application.Plugins
{
    public class PluginServiceOptions
    {
        public string Folder { get; set; } = null!;

        public string[]? AutoLoad { get; set; } = null!;
    }
}