using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Christofel.Messages.JsonConverters;
using Christofel.Messages.Options;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Remora.Discord.API.Objects;

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

        public Embed? GetEmbedFromString(string embedString)
        {
            return JsonConvert.DeserializeObject<Embed>(embedString, _jsonSerializerSettings);
        }

        public async Task<Embed?> GetEmbedFromFile(string embedName)
        {
            if (!Regex.IsMatch(embedName, @"^[a-zA-Z_\-\.]+$"))
            {
                throw new InvalidOperationException("File name cannot be accepted");
            }

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