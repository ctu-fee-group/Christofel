using System;
using Christofel.BaseLib.Exceptions;

namespace Christofel.BaseLib.Configuration.Converters
{
    public class EnumConverter<T> : ConfigConverter<T>
        where T : Enum
    {
        public override string? GetString(T value, IConfigConverterResolver resolver)
        {
            return Enum.GetName(typeof(T), value);
        }

        public override T Convert(string value, IConfigConverterResolver resolver)
        {
            try
            {
                return (T)Enum.Parse(typeof(T), value, true);
            }
            catch (Exception e)
            {
                throw new ConverterException(value, GetType(), e);
            }
        }
    }
}