using System;

namespace Christofel.BaseLib.Configuration.Converters
{
    public interface IConfigConverterResolver
    {
        public void RegisterConverter(IConfigConverter converter);
        public void RemoveConverter(IConfigConverter converter);
        
        public IConfigConverter GetConverter(Type type);
    }
}