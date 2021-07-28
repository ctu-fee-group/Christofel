using System;

namespace Christofel.BaseLib.Exceptions
{
    /// <summary>
    /// Exception thrown when converter for the type requested could not be found
    /// </summary>
    public class ConverterNotFoundException : Exception
    {
        public ConverterNotFoundException(Type converterType)
        : base(@$"Could not find converter for type {converterType.FullName}")
        {
            ConverterType = converterType;
        }
        
        public Type ConverterType { get; }
    }
}