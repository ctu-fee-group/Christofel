using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Christofel.BaseLib.Configuration;
using Christofel.BaseLib.Configuration.Converters;
using Christofel.BaseLib.Exceptions;

namespace Christofel.Application.Configuration
{
    /// <summary>
    /// Config combining multiple configs
    /// Priority of reading depends on the order in the ienumerable
    /// Only one config that can be written to can be used
    /// </summary>
    public sealed class CompositeConfig : IWRConfig
    {
        public CompositeConfig(IEnumerable<IReadableConfig> read, IWritableConfig write)
        {
            ReadConfigs = read.ToList();
            WriteConfig = write;
        }
        
        public List<IReadableConfig> ReadConfigs { get; }
        
        public IWritableConfig WriteConfig { get; }
        
        public Task SetAsync<T>(string name, T value)
        {
            return WriteConfig.SetAsync(name, value);
        }

        public async Task<T> GetAsync<T>(string name)
        {
            foreach (IReadableConfig readable in ReadConfigs)
            {
                try
                {
                    return await readable.GetAsync<T>(name);
                }
                catch (ConfigValueNotFoundException)
                {
                    // continue reading another one
                }
            }

            throw new ConfigValueNotFoundException(name);
        }

        public void RegisterConverter(IConfigConverter converter)
        {
            foreach (IReadableConfig readableConfig in ReadConfigs)
            {
                readableConfig.RegisterConverter(converter);
            }
            
            WriteConfig.RegisterConverter(converter);
        }
    }
}