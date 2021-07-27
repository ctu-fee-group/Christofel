using System;

namespace Christofel.BaseLib.Exceptions
{
    public class ConverterException : Exception
    {
        public ConverterException(string value, Type converterType)
        : base($@"Could not convert value {value} using {converterType.FullName}")
        {
            
        }
    }
}