using System;
using Christofel.BaseLib.Configuration;
using Christofel.BaseLib.Configuration.Converters;

namespace Christofel.BaseLib.Extensions
{
    public static class ConfigExtensions
    {
        /// <summary>
        /// Add common converters to the config using ConvertConfigConverter
        /// The types that are added: bool, byte, char, decimal, float, dlouble, short, int, long, ushort, uint, ulong and string
        /// </summary>
        /// <param name="config"></param>
        public static void AddConvertConverters(this IConfig config)
        {
            config.RegisterConverter(new ConvertConfigConverter<bool>());
            config.RegisterConverter(new ConvertConfigConverter<byte>());
            config.RegisterConverter(new ConvertConfigConverter<char>());
            config.RegisterConverter(new ConvertConfigConverter<decimal>());
            config.RegisterConverter(new ConvertConfigConverter<float>());
            config.RegisterConverter(new ConvertConfigConverter<double>());
            config.RegisterConverter(new ConvertConfigConverter<short>());
            config.RegisterConverter(new ConvertConfigConverter<int>());
            config.RegisterConverter(new ConvertConfigConverter<long>());
            config.RegisterConverter(new ConvertConfigConverter<ushort>());
            config.RegisterConverter(new ConvertConfigConverter<uint>());
            config.RegisterConverter(new ConvertConfigConverter<ulong>());
            config.RegisterConverter(new ConvertConfigConverter<string>());
        }
    }
}