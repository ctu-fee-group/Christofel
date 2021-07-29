using System;
using Christofel.BaseLib.Exceptions;

namespace Christofel.BaseLib.Configuration.Converters
{
    /// <summary>
    /// ConfigConverter is supposed to convert string value to another type.
    /// </summary>
    public interface IConfigConverter
    {
        public object? Convert(string value, IConfigConverterResolver resolver);
        public string? GetString(object? value, IConfigConverterResolver resolver);
        
        public Type ConvertType { get; }
    }
    
    public interface IConfigConverter<T> : IConfigConverter
    {
        public new T Convert(string value, IConfigConverterResolver resolver);
        public new string? GetString(T value, IConfigConverterResolver resolver);
    }

    /// <summary>
    /// Base abstract config converter class to inherit from,
    /// only generic Convert method implementation is needed
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class ConfigConverter<T> : IConfigConverter<T>
    {
        object? IConfigConverter.Convert(string value, IConfigConverterResolver resolver)
        {
            return Convert(value, resolver);
        }
        
        string? IConfigConverter.GetString(object? value, IConfigConverterResolver resolver)
        {
            if (value?.GetType().IsAssignableFrom(typeof(T)) ?? false)
            {
                return GetString((T)value, resolver);
            }

            throw new ConverterException("Could not assign to T", typeof(ConfigConverter<T>));
        }

        public abstract string? GetString(T value, IConfigConverterResolver resolver);

        public abstract T Convert(string value, IConfigConverterResolver resolver);

        public Type ConvertType => typeof(T);
    }
}