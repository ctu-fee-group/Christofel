using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Christofel.BaseLib.Configuration.Converters;
using Christofel.BaseLib.Exceptions;
using Christofel.BaseLib.Extensions;

namespace Christofel.BaseLib.Configuration
{
    /// <summary>
    /// Abstract class to use for converters to make registering converters easier
    /// </summary>
    public abstract class ConvertersConfig : IConfig
    {
        protected ConvertersConfig(IConfigConverterResolver resolver)
        {
            ExternalResolver = resolver;
        }

        protected string GetString<T>(T value)
        {
            IConfigConverter converter = ExternalResolver.GetConverter<T>();

            string? converted = converter.GetString(value, ExternalResolver);
            if (converted == null)
            {
                throw new ConverterException("Could not get string", converter.GetType());
            }
            
            return converted;
        }

        protected T Convert<T>(string value)
        {
            IConfigConverter converter = ExternalResolver.GetConverter<T>();

            object? converted = converter.Convert(value, ExternalResolver);
            if (converted == null)
            {
                throw new ConverterException(value, converter.GetType());
            }
            
            return (T)converted;
        }

        public IConfigConverterResolver ExternalResolver { get; }
    }
}