using System;
using System.IO;
using System.Threading.Tasks;
using Christofel.Messages.JsonConverters;
using Christofel.Messages.Options;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace Christofel.Messages.Services
{
    public class EmbedsProvider : IDisposable, IAsyncDisposable
    {
        private EmbedsOptions _embedsOptions;
        private readonly IDisposable _embedsOptionsUpdateToken;
        private readonly JsonSerializerSettings _jsonSerializerSettings;
        
        public EmbedsProvider(IOptionsMonitor<EmbedsOptions> options)
        {
            _embedsOptionsUpdateToken = options.OnChange(c => _embedsOptions = c);
            _embedsOptions = options.CurrentValue;

            _jsonSerializerSettings = new JsonSerializerSettings();
            _jsonSerializerSettings.Converters.Add(new EmbedColorConverter());
        }

        public string EmbedsFolder => _embedsOptions.Folder;

        public Embed GetEmbedFromString(string embedString)
        {
            Embed embed = JsonConvert
                .DeserializeObject<EmbedBuilder>(embedString, _jsonSerializerSettings)
                .Build();

            return embed;
        }

        public async Task<Embed> GetEmbedFromFile(string embedName)
        {
            return GetEmbedFromString(await File.ReadAllTextAsync(Path.Join(EmbedsFolder, embedName + ".json")));
        }

        public void Dispose()
        {
            _embedsOptionsUpdateToken.Dispose();
        }

        public ValueTask DisposeAsync()
        {
            _embedsOptionsUpdateToken.Dispose();
            return ValueTask.CompletedTask;
        }
    }
}