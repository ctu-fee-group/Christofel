using System;
using Christofel.BaseLib.Configuration;
using Christofel.BaseLib.Configuration.Converters;

namespace Christofel.BaseLib.Extensions
{
    public static class ConfigConverterResolverExtensions
    {
        /// <summary>
        /// Add common converters to the config using ConvertConfigConverter
        /// The types that are added: bool, byte, char, decimal, float, dlouble, short, int, long, ushort, uint, ulong and string
        /// </summary>
        /// <param name="resolver"></param>
        public static void AddConvertConverters(this IConfigConverterResolver resolver)
        {
            resolver.RegisterConverter(new ConvertConfigConverter<bool>());
            resolver.RegisterConverter(new ConvertConfigConverter<byte>());
            resolver.RegisterConverter(new ConvertConfigConverter<char>());
            resolver.RegisterConverter(new ConvertConfigConverter<decimal>());
            resolver.RegisterConverter(new ConvertConfigConverter<float>());
            resolver.RegisterConverter(new ConvertConfigConverter<double>());
            resolver.RegisterConverter(new ConvertConfigConverter<short>());
            resolver.RegisterConverter(new ConvertConfigConverter<int>());
            resolver.RegisterConverter(new ConvertConfigConverter<long>());
            resolver.RegisterConverter(new ConvertConfigConverter<ushort>());
            resolver.RegisterConverter(new ConvertConfigConverter<uint>());
            resolver.RegisterConverter(new ConvertConfigConverter<ulong>());
            resolver.RegisterConverter(new ConvertConfigConverter<string>());
        }

        public static void AddIEnumerableConverter<T>(this IConfigConverterResolver resolver)
        {
            resolver.RegisterConverter(new IEnumerableConverter<T>());
        }
        
        public static void AddEnumConverter<T>(this IConfigConverterResolver resolver)
            where T : Enum
        {
            resolver.RegisterConverter(new EnumConverter<T>());
        }

        public static IConfigConverter GetConverter<T>(this IConfigConverterResolver resolver)
        {
            return resolver.GetConverter(typeof(T));
        }
    }
}