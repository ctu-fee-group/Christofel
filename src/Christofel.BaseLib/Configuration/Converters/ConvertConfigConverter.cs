using System;
using Christofel.BaseLib.Exceptions;

namespace Christofel.BaseLib.Configuration.Converters
{
    /// <summary>
    /// Converter for Config made using Convert class to convert to basic types
    /// like short, int, long, bool, byte etc.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ConvertConfigConverter<T> : ConfigConverter<T>
    {
        public override T Convert(string value)
        {
            try
            {
                return (T) System.Convert.ChangeType(value, typeof(T));
            }
            catch (Exception e)
            {
                throw new ConverterException(value, GetType(), e);
            }
        }
    }
}