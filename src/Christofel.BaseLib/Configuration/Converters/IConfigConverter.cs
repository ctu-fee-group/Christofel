using System;

namespace Christofel.BaseLib.Configuration.Converters
{
    public interface IConfigConverter
    {
        public object? Convert(string value);
        
        public Type ConvertType { get; }
    }
    
    public interface IConfigConverter<T> : IConfigConverter
    {
        public new T Convert(string value);
    }

    public abstract class ConfigConverter<T> : IConfigConverter<T>
    {
        object? IConfigConverter.Convert(string value)
        {
            return Convert(value);
        }

        public abstract T Convert(string value);

        public Type ConvertType => typeof(T);
    }
}