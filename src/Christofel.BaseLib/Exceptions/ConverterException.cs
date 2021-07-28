using System;

namespace Christofel.BaseLib.Exceptions
{
    /// <summary>
    /// Exception thrown when value could not be converted by the converter 
    /// </summary>
    public class ConverterException : Exception
    {
        public ConverterException(string value, Type converterType, Exception? innerException = null)
        : base($@"Could not convert value {value} using {converterType.FullName}", innerException)
        {
            
        }
    }
}