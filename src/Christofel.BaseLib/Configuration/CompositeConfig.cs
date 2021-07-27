using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Christofel.BaseLib.Configuration.Converters;
using Christofel.BaseLib.Exceptions;

namespace Christofel.BaseLib.Configuration
{
    /// <summary>
    /// Config combining two configs, load data from first one and then from second one, if data weren't found in the first one
    /// </summary>
    public sealed class CompositeConfig : IConfig, IWRConfig
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