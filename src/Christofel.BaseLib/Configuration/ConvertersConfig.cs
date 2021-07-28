using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Christofel.BaseLib.Configuration.Converters;
using Christofel.BaseLib.Exceptions;

namespace Christofel.BaseLib.Configuration
{
    /// <summary>
    /// Abstract class to use for converters to make registering converters easier
    /// </summary>
    public abstract class ConvertersConfig : IConfig
    {
        private readonly List<IConfigConverter> _converters;

        protected ConvertersConfig()
        {
            _converters = new List<IConfigConverter>();
        }

        public void RegisterConverter(IConfigConverter converter)
        {
            _converters.Add(converter);
        }

        protected T Convert<T>(string value)
        {
            IConfigConverter? converter = _converters.FirstOrDefault(x => x.ConvertType == typeof(T));
            if (converter == null)
            {
                throw new ConverterNotFoundException(typeof(T));
            }

            object? converted = converter.Convert(value);
            if (converted == null)
            {
                throw new ConverterException(value, converter.GetType());
            }
            
            return (T)converted;
        }
    }
}