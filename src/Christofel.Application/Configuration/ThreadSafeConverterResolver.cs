using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Christofel.BaseLib.Configuration.Converters;
using Christofel.BaseLib.Exceptions;

namespace Christofel.Application.Configuration
{
    public class ThreadSafeConverterResolver : IConfigConverterResolver
    {
        private ConcurrentDictionary<Type, IConfigConverter> _converters;

        public ThreadSafeConverterResolver()
        {
            _converters = new ConcurrentDictionary<Type, IConfigConverter>();
        }
        
        public void RegisterConverter(IConfigConverter converter)
        {
            _converters.AddOrUpdate(converter.ConvertType, (type) =>
            {
                if (type != converter.ConvertType)
                {
                    throw new InvalidOperationException("There was an error. What is this?");
                }

                return converter;
            }, (type, _) =>
            {
                if (type != converter.ConvertType)
                {
                    throw new InvalidOperationException("There was an error. What is this?");
                }

                return converter;
            });
        }

        public void RemoveConverter(IConfigConverter converter)
        {
            IConfigConverter retrievedConverter;
            try
            {
                retrievedConverter = GetConverter(converter.ConvertType);
            } catch (ConverterNotFoundException)
            {
                // ConverterNotFound does not matter
                return;
            }

            if (retrievedConverter == converter)
            {
                // This if should always be true,
                // but if converter was overwritten
                // using another one
                // then do not remove it as someone may still
                // be using it
                _converters.TryRemove(converter.ConvertType, out _);
            }
        }

        public IConfigConverter GetConverter(Type type)
        {
            if (!_converters.TryGetValue(type, out IConfigConverter? converter))
            {
                throw new ConverterNotFoundException(type);
            }

            return converter;
        }
    }
}