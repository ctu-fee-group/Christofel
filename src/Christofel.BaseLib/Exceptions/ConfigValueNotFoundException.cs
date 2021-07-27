using System;

namespace Christofel.BaseLib.Exceptions
{
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