using System;

namespace Christofel.BaseLib.Exceptions
{
    /// <summary>
    /// Exception thrown when specified value could not be found in config
    /// </summary>
    public class ConfigValueNotFoundException : Exception
    {
        public ConfigValueNotFoundException(string name)
        : base(@$"Config value of {name} could not be found")
        {
            Name = name;
        }
        
        public string Name { get; }
    }
}