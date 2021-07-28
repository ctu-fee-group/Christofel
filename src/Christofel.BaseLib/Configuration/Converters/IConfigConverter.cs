using System;

namespace Christofel.BaseLib.Configuration.Converters
{
    /// <summary>
    /// ConfigConverter is supposed to convert string value to another type.
    /// </summary>
    public interface IConfigConverter
    {
        public object? Convert(string value);
        
        public Type ConvertType { get; }
    }
    
    public interface IConfigConverter<T> : IConfigConverter
    {
        public new T Convert(string value);
    }

    /// <summary>
    /// Base abstract config converter class to inherit from,
    /// only generic Convert method implementation is needed
    /// </summary>
    /// <typeparam name="T"></typeparam>
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