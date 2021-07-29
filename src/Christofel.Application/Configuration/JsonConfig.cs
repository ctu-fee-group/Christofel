using System.Threading.Tasks;
using Christofel.BaseLib.Configuration;
using Christofel.BaseLib.Configuration.Converters;
using Christofel.BaseLib.Exceptions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Christofel.Application.Configuration
{
    /// <summary>
    /// Readable config loaded from json
    /// </summary>
    public sealed class JsonConfig : ConvertersConfig, IReadableConfig
    {
        private JObject? _jObject;
        
        public JsonConfig(string file, IConfigConverterResolver resolver)
            : base(resolver)
        {
            File = file;
        }
        
        public string File { get; }

        public Task<T> GetAsync<T>(string name)
        {
            if (!JObject.ContainsKey(name))
            {
                throw new ConfigValueNotFoundException(name);
            }

            return Task.FromResult(Convert<T>(JObject[name].ToString()));
        }

        private JObject JObject
        {
            get
            {
                if (_jObject == null)
                {
                    _jObject = JObject.Parse(System.IO.File.ReadAllText(File));
                }

                return _jObject;
            }
        }
    }
}