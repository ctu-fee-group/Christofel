using System;

namespace Christofel.BaseLib.Exceptions
{
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