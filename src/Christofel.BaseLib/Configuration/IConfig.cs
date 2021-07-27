using System.Threading.Tasks;
using Christofel.BaseLib.Configuration.Converters;

namespace Christofel.BaseLib.Configuration
{
    /// <summary>
    /// Configuration linking names/keys to values
    /// </summary>
    public interface IConfig
    {
        /// <summary>
        /// Register type converter to correctly convert to specified type
        /// Default converters are used for common types, custom can be added
        /// </summary>
        /// <param name="converter"></param>
        public void RegisterConverter(IConfigConverter converter);
    }
    
    public interface IReadableConfig : IConfig
    {
        /// <summary>
        /// Get entry with that name and convert it to specified type
        /// </summary>
        /// <param name="name"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public Task<T> GetAsync<T>(string name);
    }
    
    public interface IWritableConfig : IConfig
    {
        /// <summary>
        /// Set config name to value
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public Task SetAsync<T>(string name, T value);
    }

    /// <summary>
    /// Write, Read config
    /// </summary>
    public interface IWRConfig
    : IWritableConfig, IReadableConfig
    {
        
    }
}